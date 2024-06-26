#region Copyright

// GPL v3 License
// 
// DatabaseSystem/DatabaseSystem
// Copyright (c) 2024 DatabaseSystem/Subject.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.ComponentModel;

namespace DatabaseSystem;

public abstract record Subject : BsonEntry
{
    public override string ToString() => Name;

    [Description("姓名")]
    public string Name { get; set; } = "";

    [Description("电话")]
    public ulong Tel { get; set; } = 0;

    [Description("地址")]
    public string Address { get; set; } = "";

    [Description("邮箱")]
    public string Email { get; set; } = "";
}
