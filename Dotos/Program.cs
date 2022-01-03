using Dotos.DI;
using Dotos.Services;
using Dotos.Utils;

var handler = IoC.Resolve<CommandHandler>();
var session = IoC.Resolve<Session>();

while (true)
{
    Console.Write(session.ToString());
    await handler.Handle(Console.ReadLine());
}