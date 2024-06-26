using System.ComponentModel;
using LiteDB;

namespace DatabaseSystem;

public abstract record BsonEntry
{
    [BsonId(true)] public int Id { get; init; }

    [Description("备注")]
    public string Remark { get; set; } = "";
}
