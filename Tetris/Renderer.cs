using System;
using System.Collections.Generic;

using Heirloom.Terminal;

namespace Tetris
{
    public class Renderer
    {
        public delegate bool DrawFunction(int x, int y, out ConsoleColor color);

        private readonly TerminalApplication _app;

        public Renderer(TerminalApplication app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public void Draw(int x, int y, Tetromino data, int orientation, char chr = '█')
        {
            // 
            Draw(x, y, data.Width, data.Height, (int xc, int yc, out ConsoleColor color) =>
            {
                color = data.Color;
                return data.IsSolid(xc, yc, orientation);
            }, chr);
        }

        public void DrawPreview(int x, int y, Tetromino data, char chr = '█')
        {
            // Computes offset to 'center' the preview
            var xShift = 4 - data.Width;
            var yShift = data.Width == 4 ? 0 : 1;

            // 
            // _app.DrawBorder(x - 1, y - 1, 1 + 4 * 2, 1 + 4, BorderKind.Thin);

            // 
            Draw(xShift + x, yShift + y, 4, 4, (int xc, int yc, out ConsoleColor color) =>
            {
                color = data.Color;
                return data.IsSolid(xc, yc, 0);
            }, chr);
        }

        public void Draw(int x, int y, Playfield field, char chr = '█')
        {
            Draw(x, y, field.Width, field.Height, (int xc, int yc, out ConsoleColor color) =>
            {
                color = field.GetColor(xc, yc);
                return field.IsBlockSolid(xc, yc);
            }, chr);
        }

        public void Draw(int x, int y, int width, int height, DrawFunction draw, char chr = '█')
        {
            void SetPixel(int xc, int yc, char ch, ConsoleColor co)
            {
                // A typical font aspect ratio is ~ 2x1
                // so we double up the drawn coordinate to make it ~ 2x2
                var xx = x + xc * 2;
                var yy = y + yc;

                //  
                _app.Buffer.SetColor(xx + 0, yy, co);
                _app.Buffer.SetColor(xx + 1, yy, co);
                _app.Buffer.SetCharacter(xx + 0, yy, ch);
                _app.Buffer.SetCharacter(xx + 1, yy, ch);
            }

            // 
            foreach (var (xc, yc) in RasterizeRectangle(width, height))
            {
                if (draw(xc, yc, out var col))
                {
                    SetPixel(xc, yc, chr, col);
                }
            }
        }

        public static IEnumerable<IntVec2> RasterizeRectangle(int width, int height)
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = height - 1; y >= 0; y--)
                {
                    yield return new IntVec2(x, y);
                }
            }
        }
    }
}
