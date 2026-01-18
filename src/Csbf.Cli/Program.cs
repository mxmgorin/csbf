// See https://aka.ms/new-console-template for more information

using Csbf.Cli;

Console.WriteLine("csbf cli. Type 'help' for commands.");

var app = new App();

if (args.Length > 0) app.Dispatch(args);

app.Run();