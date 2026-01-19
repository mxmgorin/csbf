// See https://aka.ms/new-console-template for more information

using Csbf.Cli;

Console.WriteLine("Welcome to csbf cli.");
var app = new App();
if (args.Length == 0)
{
    Console.WriteLine("Type 'help' for commands.");
    Console.WriteLine();
    app.RunRepl();
}
else
{
    app.ExecuteCmd(args);
}