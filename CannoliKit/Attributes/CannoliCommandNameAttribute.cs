namespace CannoliKit.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CannoliCommandNameAttribute : Attribute
    {
        public readonly string CommandName;
        public CannoliCommandNameAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }
}
