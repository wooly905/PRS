using System;

namespace PRS.Display
{
    internal class LcdMonitor : IDisplay
    {
        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void ShowInfo(string message)
        {
            Console.WriteLine(message);
        }
    }
}
