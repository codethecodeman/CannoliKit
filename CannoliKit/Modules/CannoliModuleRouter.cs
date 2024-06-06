using CannoliKit.Interfaces;
using CannoliKit.Models;
using CannoliKit.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace CannoliKit.Modules
{
    internal class CannoliModuleRouter : ICannoliModuleRouter
    {
        private readonly IServiceProvider _serviceProvider;

        public CannoliModuleRouter(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task RouteToModuleCallback(CannoliRoute route, object parameter)
        {

            var classType = ReflectionUtility.GetType(route.CallbackType)!;

            var callbackMethodInfo = ReflectionUtility.GetMethodInfo(classType, route.CallbackMethod)!;

            var target = (ICannoliModule)_serviceProvider.GetRequiredService(classType);

            await target.LoadModuleState(route);

            var callbackTask = (Task)callbackMethodInfo.Invoke(target, [parameter, route])!;
            await callbackTask;

            await target.SaveModuleState();
        }
    }
}
