using Scheduler.DI;
using Scheduler.Services;
using Serilog;

File.Delete("logs.txt");
Serilog.Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs.txt")
    .CreateLogger();
var scheduler = IoC.Resolve<ProcessScheduler>();
var commandHandler = IoC.Resolve<CommandHandler>();


scheduler.Handle();
while (true)
{
    Console.Write("command: ");
    await commandHandler.Handle(Console.ReadLine());
}



