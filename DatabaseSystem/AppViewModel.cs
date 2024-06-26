#region Copyright

// GPL v3 License
// 
// DatabaseSystem/DatabaseSystem
// Copyright (c) 2024 DatabaseSystem/AppViewModel.cs
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

using CommunityToolkit.Mvvm.ComponentModel;

namespace DatabaseSystem;

public partial class AppViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isWorker;

    [ObservableProperty]
    private bool _isSupplier;

    [ObservableProperty]
    private bool _isCustomer;

    [ObservableProperty]
    private int _id;

    public void SetAdmin(CollectionType type, int id)
    {
        IsAdmin = false;
        IsWorker = false;
        IsSupplier = false;
        IsCustomer = false;
        Id = id;
        switch (type)
        {
            case CollectionType.Worker:
                IsWorker = true;
                break;
            case CollectionType.Supplier:
                IsSupplier = true;
                break;
            case CollectionType.Customer:
                IsCustomer = true;
                break;
            case CollectionType.WaterInventoryRecord:
            case CollectionType.WaterDeliveryRecord:
                IsAdmin = IsWorker = IsSupplier = IsCustomer = true;
                break;
            default:
                break;
        }
    }
}
