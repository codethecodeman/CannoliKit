namespace Sample
{
    internal class FoodItem
    {
        public string Name { get; }
        public string Emoji { get; }

        public FoodItem(string name, string emoji)
        {
            Name = name;
            Emoji = emoji;
        }
    }
}
