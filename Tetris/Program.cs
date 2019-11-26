using Heirloom.Terminal;

namespace Tetris
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            // Execute application
            TerminalApplication.Run(new Tetris());
        }
    }
}
