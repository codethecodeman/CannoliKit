using CannoliKit.Modules.States;
using Demo.Models;

namespace Demo.Modules.Cart
{
    internal class CartState : CannoliModuleState
    {
        internal string CartId { get; init; } = Guid.NewGuid().ToString();
        internal List<FoodItem> Items { get; init; } = [];
    }
}
