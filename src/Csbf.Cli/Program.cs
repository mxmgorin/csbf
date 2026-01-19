// See https://aka.ms/new-console-template for more information

using Csbf.Cli;

var app = new App();
if (args.Length == 0)
{
    Console.WriteLine("Welcome to csbf REPL.");
    Console.WriteLine("Type 'help' for commands.");
    Console.WriteLine();
    app.RunRepl();
}
else
{
    app.ExecuteCmd(args);
}