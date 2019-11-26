using System;
using System.Diagnostics;
using System.Threading;

namespace Tetris.Terminal
{
    public abstract class TerminalApplication
    {
        public int Width => Buffer.Width;

        public int Height => Buffer.Height;

        public ScreenBuffer Buffer { get; }

        private Stopwatch _timer;

        private bool _exit = false;

        private int _delta = 33;

        // 

        protected abstract void Input(ConsoleKey key, char symbol);

        protected abstract void Update(int delta);

        private static TerminalApplication _application;

        protected TerminalApplication()
        {
            Buffer = new ScreenBuffer();
        }

        public static void Run(TerminalApplication app)
        {
            if (_application != null)
            {
                throw new InvalidOperationException("Application already running.");
            }

            // 
            _application = app;
            _application.Execute();
        }

        public static void Terminate()
        {
            _application._exit = true;
        }

        private void Execute()
        {
            // 
            Process.GetCurrentProcess().Exited += RestoreTerminal;

            // 
            Console.TreatControlCAsInput = true;

            // 
            EnableSecondBuffer(true);

            // 
            Buffer.AllocateBufferStorage();

            // 
            _timer = Stopwatch.StartNew();

            // 
            _exit = false;

            // Render loop
            while (!_exit)
            {
                _timer.Restart();

                // Process application input
                while (Console.KeyAvailable)
                {
                    // Get key info
                    var info = Console.ReadKey(true);

                    if (info.Modifiers.HasFlag(ConsoleModifiers.Control) && info.Key == ConsoleKey.C)
                    {
                        Terminate();
                        break;
                    }

                    Input(info.Key, info.KeyChar);
                }

                // Terminate requested in input
                if (_exit) { continue; }

                // Process application update
                Update(_delta);

                // Terminate requested in update
                if (_exit) { continue; }

                // Draw screen buffer
                Buffer.Render();

                // Time took to complete frame
                _delta = (int) _timer.ElapsedMilliseconds;

                // Limit frame rate (30 fps)
                var wait = Math.Max(0, 33 - _delta);
                if (wait > 0)
                {
                    // Wait for remainder of time
                    Thread.Sleep(wait);

                    // Append wait time to delta
                    _delta += wait;
                }
            }

            // 
            RestoreTerminal(null, null);
        }

        private static void EnableSecondBuffer(bool useSecondBuffer)
        {
            if (IsAnsiPlatform())
            {
                if (useSecondBuffer) { Console.Write("\x01B[?1049h"); }
                else { Console.Write("\x1B[?1049l"); }
            }
        }

        private static bool IsAnsiPlatform()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
        }

        private void RestoreTerminal(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Exited -= RestoreTerminal;

            //  
            EnableSecondBuffer(false);

            // 
            Console.CursorVisible = true;
            Console.ResetColor();
        }

        #region Draw 

        public void DrawString(int x, int y, string text)
        {
            foreach (var c in text)
            {
                Buffer.SetCharacter(x++, y, c);
            }
        }

        private void DrawH(int x1, int x2, int y, char c) { for (var x = x1; x < x2; x++) { Buffer.SetCharacter(x, y, c); } }

        private void DrawV(int y1, int y2, int x, char c) { for (var y = y1; y < y2; y++) { Buffer.SetCharacter(x, y, c); } }

        public void DrawBorder(int x, int y, int width, int height, BorderKind border, char center = '\0')
        {
            var x1 = x;
            var y1 = y;
            var x2 = x + width;
            var y2 = y + height;

            // Edges
            DrawH(x1, x2, y1, border.Horizontal);
            DrawH(x1, x2, y2, border.Horizontal);
            DrawV(y1, y2, x1, border.Vertical);
            DrawV(y1, y2, x2, border.Vertical);

            // Corner
            Buffer.SetCharacter(x1, y1, border.TopLeft);
            Buffer.SetCharacter(x2, y1, border.TopRight);
            Buffer.SetCharacter(x1, y2, border.BottomLeft);
            Buffer.SetCharacter(x2, y2, border.BottomRight);

            // Center?
            if (center != '\0')
            {
                FillRect(x1 + 1, y1 + 1, width - 1, height - 1, center);
            }
        }

        public void FillRect(int x, int y, int width, int height, char value)
        {
            FillRect(x, y, width, height, $"{value}");
        }

        public void FillRect(int x, int y, int width, int height, string pattern)
        {
            var x1 = x;
            var y1 = y;
            var x2 = x + width;
            var y2 = y + height;

            var o = x % pattern.Length;

            for (y = y1; y < y2; y++)
            {
                for (x = x1; x < x2; x++)
                {
                    Buffer.SetCharacter(x, y, pattern[(x + o) % pattern.Length]);
                }
            }
        }

        #endregion
    }
}
