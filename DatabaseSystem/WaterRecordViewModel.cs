using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DatabaseSystem;

public partial class WaterRecordViewModel : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Validate))]
    private bool _isIncome;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Validate))]
    private MineralWater? _mineralWater;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Validate))]
    private Worker? _worker;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Validate))]
    private Customer? _customer;

#pragma warning disable CA1822
    public IEnumerable<MineralWater> MineralWaters => App.GetCollection<MineralWater>().FindAll();

    public IEnumerable<Worker> Workers => App.GetCollection<Worker>().FindAll();

    public IEnumerable<Customer> Customers => App.GetCollection<Customer>().FindAll();
#pragma warning restore CA1822

    public uint Quantity { get; set; }

    public int Amount { get; set; }

    public DateTimeOffset Time { get; set; } = DateTimeOffset.Now;

    [MemberNotNullWhen(true, nameof(MineralWater), nameof(Worker))]
    public bool Validate =>
        MineralWater is not null && Worker is not null &&
        IsIncome == Customer is null;
}
