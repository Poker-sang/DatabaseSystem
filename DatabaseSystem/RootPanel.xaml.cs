using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace DatabaseSystem;

[ObservableObject]
[DependencyProperty<Window>("Window")]
[DependencyProperty<bool>("IsAdmin")]
[DependencyProperty<bool>("IsWorker")]
[DependencyProperty<bool>("IsCustomer")]
[DependencyProperty<bool>("IsSupplier")]
[DependencyProperty<int>("Id")]
public sealed partial class RootPanel
{
    public RootPanel() => InitializeComponent();

    private async void OpenDialog_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _ = await sender.To<FrameworkElement>().GetTag<ContentDialog>().ShowAsync();
    }

    private void ManageInventoryDialog_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs e)
    {
        NewRecord = new();
    }

    [ObservableProperty]
    private WaterRecordViewModel _newRecord = new();

    private void ManageInventoryDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs e)
    {
        if (!NewRecord.Validate)
        {
            e.Cancel = true;
            return;
        }

        var newEntry = new WaterRecord
        {
            MineralWaterId = NewRecord.MineralWater.Id,
            WorkerId = NewRecord.Worker.Id,
            CustomerId = NewRecord.Customer?.Id,
            Amount = NewRecord.Amount,
            Quantity = NewRecord.Quantity,
            Time = NewRecord.Time
        };
        var waters = App.GetCollection<MineralWater>();
        var water = waters.FindById(NewRecord.MineralWater.Id);
        if (NewRecord.IsIncome)
            water.Quantity += NewRecord.Quantity;
        else
            water.Quantity -= NewRecord.Quantity;
        _ = App.GetCollection<WaterRecord>().Insert(newEntry);
        _ = App.GetCollection<MineralWater>().Update(water);
        if (DatabaseEditor.CurrentType is CollectionType.WaterInventoryRecord && NewRecord.IsIncome
            || DatabaseEditor.CurrentType is CollectionType.WaterDeliveryRecord && !NewRecord.IsIncome)
            DatabaseEditor.Refresh();
    }

    private async void Backup_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await Window.PickSingleFolderAsync() is { } folder)
        {
            App.Database.Dispose();
            File.Copy(App.DatabasePath, Path.Combine(folder.Path, App.DatabaseName));
            App.Database = new(App.DatabasePath);
        }
    }

    private async void Recover_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (await Window.PickSingleFileAsync() is not { } file)
            return;
        App.Database.Dispose();
        File.Delete(App.DatabasePath);
        File.Copy(file.Path, App.DatabasePath);
        App.Database = new(App.DatabasePath);
    }

    private void RootPanel_OnUnloaded(object sender, RoutedEventArgs e)
    {
        App.Database.Dispose();
    }

    private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (e.QueryText is "admin")
        {
            App.AppViewModel.SetAdmin(CollectionType.WaterDeliveryRecord, 0);
            return;
        }

        if (!int.TryParse(e.QueryText[^1].ToString(), out var i))
        {
            App.AppViewModel.SetAdmin(CollectionType.Worker, 0);
            return;
        }

        if (e.QueryText.StartsWith('w'))
            App.AppViewModel.SetAdmin(CollectionType.Worker, i);
        else if (e.QueryText.StartsWith('c'))
            App.AppViewModel.SetAdmin(CollectionType.Customer, i);
        else if (e.QueryText.StartsWith('s'))
            App.AppViewModel.SetAdmin(CollectionType.Supplier, i);
    }

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        ResetMaximum();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ResetMaximum();
    }

    private void ResetMaximum()
    {
        if (NewRecord.MineralWater is null)
            QuantityNumberBox.Maximum = uint.MaxValue;
        else
        {
            var q = App.GetCollection<MineralWater>().FindById(NewRecord.MineralWater.Id).Quantity;
            QuantityNumberBox.Maximum = NewRecord.IsIncome ? uint.MaxValue - q : q;
        }
    }

    private bool Or(bool a, bool b) => a || b;
}
