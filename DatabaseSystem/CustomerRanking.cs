using System.ComponentModel;

namespace DatabaseSystem;

public record CustomerRanking : Customer
{
    [Description("数量")]
    public uint Quantity { get; set; }

    [Description("交易额")]
    public decimal Amount { get; set; }
}
