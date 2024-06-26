using System;
using System.ComponentModel;
using LiteDB;

namespace DatabaseSystem;

public record MineralWater : BsonEntry
{
    public override string ToString() => Name;

    [Description("名称")]
    public string Name { get; set; } = "";

    [Description("数量")]
    public uint Quantity { get; set; }

    public int SupplierId { get; set; }

    [Description("产地")]
    public string Origin { get; set; } = "";

    [Description("保质期")]
    public TimeSpan ShelfLife { get; set; }

    [BsonIgnore]
    [Description("供应商")]
    public string Supplier
    {
        get => App.GetCollection<Supplier>().FindById(SupplierId)?.ToString() ?? "";
        set => SupplierId = int.TryParse(value, out var i) ? i : SupplierId;
    }
}
