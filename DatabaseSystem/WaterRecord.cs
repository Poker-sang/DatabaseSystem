using System;
using System.ComponentModel;
using LiteDB;

namespace DatabaseSystem;

public record WaterRecord : BsonEntry
{
    public int MineralWaterId { get; set; }

    [BsonIgnore]
    [Description("矿泉水")]
    public MineralWater MineralWater => App.GetCollection<MineralWater>().FindById(MineralWaterId);

    public int WorkerId { get; set; }

    [BsonIgnore]
    [Description("员工")]
    public Worker Worker => App.GetCollection<Worker>().FindById(WorkerId);

    public int? CustomerId { get; set; }

    [BsonIgnore]
    [Description("顾客")]
    public Customer? Customer => CustomerId is null ? null : App.GetCollection<Customer>().FindById(CustomerId);

    [Description("数量")]
    public uint Quantity { get; set; }

    [Description("交易额")]
    public decimal Amount { get; set; }

    [Description("时间")]
    public DateTimeOffset Time { get; set; }
}
