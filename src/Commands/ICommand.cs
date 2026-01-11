namespace PRS.Commands;

internal interface ICommand
{
    public Task RunAsync(string[] args);
}
