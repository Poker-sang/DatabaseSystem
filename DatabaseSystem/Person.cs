#region Copyright

// GPL v3 License
// 
// DatabaseSystem/DatabaseSystem
// Copyright (c) 2024 DatabaseSystem/Person.cs
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

public abstract record Person : Subject
{
    [Description("性别是男")] 
    public bool IsMale { get; set; }

    [Description("身份证号")]
    public string IdNumber { get; set; } = "";
    
    public override string ToString() => base.ToString();
}
