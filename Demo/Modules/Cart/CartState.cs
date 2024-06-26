﻿using CannoliKit.Modules.States;

namespace Demo.Modules.Cart
{
    public class CartState : CannoliModuleState
    {
        public string CartId { get; init; } = Guid.NewGuid().ToString();
        public List<int> Items { get; init; } = [];
    }
}
