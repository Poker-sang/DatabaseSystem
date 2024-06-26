using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI.Controls;
using LiteDB;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace DatabaseSystem;

[ObservableObject]
[DependencyProperty<bool>("IsAdmin", propertyChanged: nameof(OnIsAdminChanged))]
[DependencyProperty<bool>("IsWorker")]
[DependencyProperty<bool>("IsCustomer")]
[DependencyProperty<bool>("IsSupplier")]
[DependencyProperty<int>("Id")]
public sealed partial class DatabaseEditor : UserControl
{
    public DatabaseEditor() => InitializeComponent();

    private void DataGrid_OnAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.PropertyName is nameof(BsonEntry.Id))
        {
            e.Column.IsReadOnly = true;
            e.Column.DisplayIndex = 0;
        }
        else if (e.PropertyName.Contains(nameof(BsonEntry.Id))
                 || CurrentType is CollectionType.WaterInventoryRecord && e.PropertyName is nameof(WaterRecord.Customer))
        {
            e.Cancel = true;
            return;
        }
        e.Column.CanUserSort = true;
        e.Column.Header = CurrentTypeInfo.GetProperty(e.PropertyName)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? e.Column.Header;
    }

    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var temp = args.SelectedItem is NavigationViewItem { Tag: CollectionType type } ? type : CollectionType.None;
        DataGrid.ItemsSource = temp is CollectionType.None ? null : GetCurrentEnumerable2(temp);
        CurrentType = temp;
    }

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsReadonly), nameof(IsNotAddable))] private CollectionType _currentType;

    private bool IsNotAddable => CurrentType is CollectionType.WaterInventoryRecord or CollectionType.WaterDeliveryRecord || IsReadonly;

    private bool IsReadonly => CurrentType is CollectionType.WorkerStatistics or CollectionType.CustomerRanking || !IsAdmin;

    private static void OnIsAdminChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var databaseEditor = sender.To<DatabaseEditor>();
        databaseEditor.OnPropertyChanged(nameof(IsNotAddable));
        databaseEditor.OnPropertyChanged(nameof(IsReadonly));
    }

    private dynamic CurrentCollection => GetCurrentCollection(CurrentType);

    private dynamic GetCurrentCollection(CollectionType type) => type switch
    {
        CollectionType.Worker => App.GetCollection<Worker>(),
        CollectionType.Customer => App.GetCollection<Customer>(),
        CollectionType.MineralWater => App.GetCollection<MineralWater>(),
        CollectionType.Supplier => App.GetCollection<Supplier>(),
        CollectionType.WaterInventoryRecord or
        CollectionType.WaterDeliveryRecord or
        CollectionType.WorkerStatistics or
        CollectionType.CustomerRanking => GetWaterRecord,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private ILiteCollection<WaterRecord> GetWaterRecord => App.GetCollection<WaterRecord>();

    private IEnumerable CurrentEnumerable2 => GetCurrentEnumerable2(CurrentType);

    private IEnumerable CurrentEnumerable => GetCurrentEnumerable(CurrentType);

    private IEnumerable GetCurrentEnumerable(CollectionType type) => type switch
    {
        CollectionType.Worker or
        CollectionType.Customer or
        CollectionType.MineralWater or
        CollectionType.Supplier => GetCurrentCollection(type).FindAll(),
        CollectionType.WaterInventoryRecord => GetWaterRecord.Find(t => t.CustomerId == null),
        CollectionType.WaterDeliveryRecord => GetWaterRecord.Find(t => t.CustomerId != null),
        CollectionType.WorkerStatistics => GetWorkerStatistics(),
        CollectionType.CustomerRanking => GetCustomerRanking(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private IEnumerable GetCurrentEnumerable2(CollectionType type)
    {
        switch (type)
        {
            case var _ when IsAdmin:
                return GetCurrentEnumerable(type);
            case CollectionType.WaterInventoryRecord when IsWorker:
                return GetWaterRecord.Find(t => t.CustomerId == null && t.WorkerId == Id);
            case CollectionType.WaterInventoryRecord when IsCustomer:
                return GetWaterRecord.Find(t => t.CustomerId == null && t.CustomerId == Id);
            case CollectionType.WaterDeliveryRecord when IsWorker:
                return GetWaterRecord.Find(t => t.CustomerId != null && t.WorkerId == Id);
            case CollectionType.WaterDeliveryRecord when IsCustomer:
                return GetWaterRecord.Find(t => t.CustomerId != null && t.CustomerId == Id);
            case CollectionType.Worker:
            {
                var r = (IEnumerable<Worker>)GetCurrentCollection(type).FindAll();
                return Id is 0 ? r : r.Where(t => t.Id == Id);
            }
            case CollectionType.Supplier:
            {
                var r = (IEnumerable<Supplier>)GetCurrentCollection(type).FindAll();
                return Id is 0 ? r : r.Where(t => t.Id == Id);
            }
            case CollectionType.Customer:
            {
                var r = (IEnumerable<Customer>)GetCurrentCollection(type).FindAll();
                return Id is 0 ? r : r.Where(t => t.Id == Id);
            }
            default:
                return GetCurrentEnumerable(type);
        }
    }

    private dynamic CurrentNewEntry => CurrentType switch
    {
        CollectionType.Worker => new Worker(),
        CollectionType.Customer => new Customer(),
        CollectionType.MineralWater => new MineralWater(),
        CollectionType.Supplier => new Supplier(),
        CollectionType.WaterInventoryRecord or
        CollectionType.WaterDeliveryRecord => new WaterRecord(),
        _ => throw new ArgumentOutOfRangeException(nameof(CurrentType))
    };

    private Type CurrentTypeInfo => CurrentType switch
    {
        CollectionType.Worker => typeof(Worker),
        CollectionType.Customer => typeof(Customer),
        CollectionType.MineralWater => typeof(MineralWater),
        CollectionType.Supplier => typeof(Supplier),
        CollectionType.WaterInventoryRecord or
        CollectionType.WaterDeliveryRecord => typeof(WaterRecord),
        CollectionType.WorkerStatistics => typeof(WorkerStatistics),
        CollectionType.CustomerRanking => typeof(CustomerRanking),
        _ => throw new ArgumentOutOfRangeException(nameof(CurrentType))
    };

    private void DataGrid_OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
    {
        dynamic rowDataContext = e.Row.DataContext;
        CurrentCollection.Update(rowDataContext);
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        CurrentCollection.Insert(CurrentNewEntry);
        Refresh();
    }

    private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataGrid.SelectedItem is BsonEntry entry)
        {
            if (Validate() && await ConfirmDialog.ShowAsync() is not ContentDialogResult.Primary)
                return;
            CurrentCollection.Delete(entry.Id);
            Refresh();
        }

        bool Validate()
        {
            switch (entry)
            {
                case MineralWater m:
                    if (GetWaterRecord.FindOne(t => t.MineralWaterId == m.Id) is not null)
                        return true;
                    break;
                case Worker m:
                    if (GetWaterRecord.FindOne(t => t.WorkerId == m.Id) is not null)
                        return true;
                    break;
                case Customer m:
                    if (GetWaterRecord.FindOne(t => t.CustomerId == m.Id) is not null)
                        return true;
                    break;
                case Supplier m:
                    if (App.GetCollection<MineralWater>().FindOne(t => t.SupplierId == m.Id) is not null)
                        return true;
                    break;
            }
            return false;
        }
    }

    private static WaterRecord[] GetOutlay => App.GetCollection<WaterRecord>().Find(t => t.CustomerId != null).ToArray();

    public void Refresh() => DataGrid.ItemsSource = CurrentEnumerable2;

    private static List<WorkerStatistics> GetWorkerStatistics()
    {
        var workers = App.GetCollection<Worker>().FindAll();
        var result = new List<WorkerStatistics>();
        foreach (var worker in workers)
        {
            var workerStatistics = GetOutlay.Where(t => t.WorkerId == worker.Id).ToArray();
            var quantity = (uint)workerStatistics.Sum(t => t.Quantity);
            var amount = workerStatistics.Sum(t => t.Amount);
            result.Add(new WorkerStatistics
            {
                Id = worker.Id,
                Name = worker.Name,
                Quantity = quantity,
                Amount = amount
            });
        }
        result.Sort((x, y) =>
        {
            var cmp = y.Amount.CompareTo(x.Amount);
            return cmp is 0 ? y.Quantity.CompareTo(x.Quantity) : cmp;
        });
        return result;
    }

    public static IEnumerable<CustomerRanking> GetCustomerRanking()
    {
        var customers = App.GetCollection<Customer>().FindAll();
        var result = new List<CustomerRanking>();
        foreach (var customer in customers)
        {
            var customerRanking = GetOutlay.Where(t => t.CustomerId == customer.Id).ToArray();
            var quantity = (uint)customerRanking.Sum(t => t.Quantity);
            var amount = customerRanking.Sum(t => t.Amount);
            result.Add(new CustomerRanking
            {
                Id = customer.Id,
                Name = customer.Name,
                Quantity = quantity,
                Amount = amount
            });
        }
        result.Sort((x, y) =>
        {
            var cmp = y.Amount.CompareTo(x.Amount);
            return cmp is 0 ? y.Quantity.CompareTo(x.Quantity) : cmp;
        });
        return result.Take(10);
    }

    private bool Or(bool a, bool b) => a || b;
}
