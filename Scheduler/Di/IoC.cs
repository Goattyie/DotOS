using Microsoft.Extensions.DependencyInjection;
using Scheduler.Services;
using Scheduler.Services.Commands;

namespace Scheduler.DI
{
    internal class IoC
    {
        private static readonly IServiceProvider _provider;

        static IoC()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ProcessScheduler>();
            services.AddSingleton<CommandHandler>();
            services.AddTransient<ISystemCommand, PrintProcessesCommand>();
            services.AddTransient<ISystemCommand, CreateProcessCommand>();
            services.AddTransient<ISystemCommand, ChangePriorityCommand>();
            services.AddTransient<ISystemCommand, RemoveProcessCommand>();

            _provider = services.BuildServiceProvider();
        }
        public static T Resolve<T>() => _provider.GetService<T>();
    }
}
