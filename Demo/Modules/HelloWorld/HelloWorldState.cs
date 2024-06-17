using CannoliKit.Modules.States;

namespace Demo.Modules.HelloWorld
{
    public class HelloWorldState : CannoliModuleState
    {
        public DateTime? LastHelloOn { get; set; } = null;
    }
}
