using System;
using System.Text;

namespace Tetris.Terminal
{
    public class ScreenBuffer
    {
        public static int WindowWidth => Console.WindowWidth - 1;

        public static int WindowHeight => Console.WindowHeight - 1;

        public int Width => _ch.GetLength(0);

        public int Height => _ch.GetLength(1);

        private ConsoleColor[,] _fg;
        private ConsoleColor[,] _bg;
        private char[,] _ch;

        public ScreenBuffer()
        {
            AllocateBufferStorage();
        }

        internal void AllocateBufferStorage()
        {
            // Allocation storage
            _fg = new ConsoleColor[WindowWidth, WindowHeight];
            _bg = new ConsoleColor[WindowWidth, WindowHeight];
            _ch = new char[WindowWidth, WindowHeight];

            //  
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            // Console.Clear();

            // Set to default
            Clear();
        }

        public void Clear()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    _fg[x, y] = ConsoleColor.White;
                    _bg[x, y] = ConsoleColor.Black;
                    _ch[x, y] = ' ';
                }
            }
        }

        public void Render()
        {
            // Was window resized?
            if (Width != WindowWidth || Height != WindowHeight)
            {
                // 
                AllocateBufferStorage();

                // Content will have been lost, so quit.
                return;
            }

            var text = new char[Width];

            // For each row
            for (var y = 0; y < Height; y++)
            {
                var ref_fg = _fg[0, y];
                var ref_bg = _bg[0, y];
                var _x = 0;

                // 
                var count = 0;

                // For each column
                for (var x = 0; x < Width; x++)
                {
                    var fg = _fg[x, y];
                    var bg = _bg[x, y];

                    // If the color is inconsistent (incompatible state)
                    if (fg != ref_fg || bg != ref_bg)
                    {
                        // Render line segment
                        Output(_x, y, ref_fg, ref_bg, text, count);
                        count = 0;
                        _x = x;

                        // Assign new reference color
                        ref_fg = fg;
                        ref_bg = bg;
                    }

                    // Append to string
                    text[count++] = _ch[x, y];
                }

                // If we have anything left in the line, render that too
                if (count > 0)
                {
                    Output(_x, y, ref_fg, ref_bg, text, count);
                }
            }
        }

        private void Output(int x, int y, ConsoleColor fg, ConsoleColor bg, char[] text, int count)
        {
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.SetCursorPosition(x, y);
            Console.Write(text, 0, count);
        }

        public bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        #region Character

        public void SetCharacter(int x, int y, char c)
        {
            if (Contains(x, y)) { _ch[x, y] = c; }
            else { /* Throw? */ }
        }

        public char GetCharacter(int x, int y)
        {
            if (Contains(x, y)) { return _ch[x, y]; }
            else { return '\0'; }
        }

        #endregion

        #region Color

        public void SetColor(int x, int y, ConsoleColor c)
        {
            if (Contains(x, y)) { _fg[x, y] = c; }
            else { /* Throw? */ }
        }

        public ConsoleColor GetColor(int x, int y)
        {
            if (Contains(x, y)) { return _fg[x, y]; }
            else { return ConsoleColor.Black; }
        }

        public void SetBackground(int x, int y, ConsoleColor c)
        {
            if (Contains(x, y)) { _bg[x, y] = c; }
            else { /* Throw? */ }
        }

        public ConsoleColor GetBackground(int x, int y)
        {
            if (Contains(x, y)) { return _bg[x, y]; }
            else { return ConsoleColor.Black; }
        }

        #endregion
    }
}
