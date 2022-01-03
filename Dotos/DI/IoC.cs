using Dotos.Models;
using Dotos.Services;
using Dotos.Services.SystemCommands;
using Dotos.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Dotos.DI
{
    internal class IoC
    {
        private static readonly IServiceProvider _provider;

        static IoC()
        {
            var services = new ServiceCollection();

            services.AddSingleton<CommandHandler>();
            services.AddSingleton<StreamWorker>();
            services.AddSingleton<FileSystem>();
            services.AddSingleton<Superblock>();
            services.AddSingleton<Session>();
            services.AddTransient<ISystemCommand, NewUserCommand>();
            services.AddTransient<ISystemCommand, ReadUsersCommand>();
            services.AddTransient<ISystemCommand, SignInCommand>();
            services.AddTransient<ISystemCommand, SignOutCommand>();
            services.AddTransient<ISystemCommand, ReadDirCommand>();
            services.AddTransient<ISystemCommand, ReadFileCommand>();
            services.AddTransient<ISystemCommand, RemoveUserCommand>();
            services.AddTransient<ISystemCommand, CreateDirectoryCommand>();
            services.AddTransient<ISystemCommand, CreateFileCommand>();
            services.AddTransient<ISystemCommand, FormatDiskCommand>();
            services.AddTransient<ISystemCommand, OpenDirCommand>();
            services.AddTransient<ISystemCommand, WriteFileCommand>();
            services.AddTransient<ISystemCommand, RemoveFileCommand>();
            services.AddTransient<ISystemCommand, RemoveDirCommand>();
            services.AddTransient<ISystemCommand, RenameFileCommand>();
            services.AddTransient<ISystemCommand, RenameDirCommand>();
            services.AddTransient<ISystemCommand, CopyFileCommand>();
            services.AddTransient<ISystemCommand, MoveFileCommand>();
            services.AddTransient<ISystemCommand, ChangeFileAttributesCommand>();
            services.AddTransient<ISystemCommand, ChangeDirectoryAttributesCommand>();
            services.AddTransient<CopyFileCommand>();
            services.AddTransient<RemoveFileCommand>();

            _provider = services.BuildServiceProvider();
        }
        public static T Resolve<T>() => _provider.GetService<T>();
    }
}
