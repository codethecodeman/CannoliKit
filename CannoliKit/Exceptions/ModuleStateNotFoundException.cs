using CannoliKit.Modules.States;

namespace CannoliKit.Exceptions
{
    /// <summary>
    /// Exception that is thrown when a <see cref="CannoliModuleState"/> cannot be located in the database.
    /// </summary>
    public sealed class ModuleStateNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ModuleStateNotFoundException"/>.
        /// </summary>
        /// <param name="message">Message that describes the error.</param>
        public ModuleStateNotFoundException(string message) : base(message) { }
    }
}
