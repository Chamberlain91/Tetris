using System;
using System.Collections.Generic;

namespace Tetris
{
    /// <summary>
    /// Represents the tetris playfield / stack / well.
    /// </summary>
    public class Playfield
    {
        private readonly Grid<Block> _this;

        public int Width => _this.Width;

        public int Height => _this.Height;

        /// <summary>
        /// Construct a new standard game field.
        /// </summary>
        public Playfield()
            : this(10, 20)
        { }

        /// <summary>
        /// Construct a new game field.
        /// </summary>
        public Playfield(int width, int height)
        {
            _this = new Grid<Block>(width, height);

            // Construct each block
            foreach (var (x, y) in Renderer.RasterizeRectangle(Width, Height))
            {
                _this[x, y] = new Block(false, ConsoleColor.Black);
            }
        }

        public ConsoleColor GetColor(int x, int y)
        {
            return _this[x, y].Color;
        }

        public void SetColor(int x, int y, ConsoleColor color)
        {
            _this[x, y].Color = color;
        }

        public bool IsBlockSolid(int x, int y)
        {
            if (_this.Contains(x, y))
            {
                return _this.Get(x, y).Solid;
            }
            else
            {
                // Left, Right and Bottom edges are considered solid
                if (x < 0 || x >= Width) { return true; }
                if (y >= Height) { return true; }
                return false;
            }
        }

        public void SetBlock(int x, int y, bool solid)
        {
            if (_this.Contains(x, y))
            {
                _this[x, y].Solid = solid;
            }
        }

        /// <summary>
        /// Projects from the specified coordinate down until the desired state is found.
        /// </summary>
        public int CastDown(int x, int y, bool state)
        {
            // Initial coordinate matches the expected state
            if (IsBlockSolid(x, y) == state) { return y; }

            // Scan from current y coordinate down
            for (; y < Height; y++)
            {
                if (IsBlockSolid(x, y) == state)
                {
                    return y;
                }
            }

            // No hit
            return Height;
        }

        /// <summary>
        /// Projects from the specified coordinate up until the desired state is found.
        /// </summary>
        public int CastUp(int x, int y, bool state)
        {
            // Initial coordinate matches the expected state
            if (IsBlockSolid(x, y) == state) { return y; }

            // Scan from current y coordinate up
            for (; y >= 0; y--)
            {
                if (IsBlockSolid(x, y) == state)
                {
                    return y;
                }
            }

            // No hit
            return 0;
        }

        /// <summary>
        /// Determines if the given row is full (enumerated top down)
        /// </summary>
        public bool CheckRowForClear(int y)
        {
            for (var x = 0; x < Width; x++)
            {
                // If any block on the row is not solid, then its not a full row.
                if (!IsBlockSolid(x, y)) { return false; }
            }

            // All blocks are solid, a clear is detected
            return true;
        }

        /// <summary>
        /// Detects full rows top down, so it can be used to elimated lines in one pass.
        /// </summary>
        public IEnumerable<int> DetectClears()
        {
            // Starting at the bottom, check clears and migrate blocks down
            for (var y = 0; y < Height; y++)
            {
                if (CheckRowForClear(y))
                {
                    // Emit that a clear happened on this row
                    yield return y;
                }
            }
        }

        /// <summary>
        /// Shifts all blocks above the cleared row, assumes that the program correctly dectected a full line before calling.
        /// </summary>
        public void ClearRow(int row)
        {
            // From the desired row to the top
            for (var y = row; y > 0; y--)
            {
                // For each block in the row
                for (var x = 0; x < Width; x++)
                {
                    var block = _this[x, y];

                    // Does the current block have a block above it?
                    if (_this.Contains(x, y - 1))
                    {
                        var above = _this[x, y - 1];

                        // Copy the values of the block above it
                        block.Solid = above.Solid;
                        block.Color = above.Color;
                    }
                    else
                    {
                        // Above the stage
                        block.Color = ConsoleColor.Black;
                        block.Solid = false;
                    }
                }
            }
        }

        private class Block
        {
            public ConsoleColor Color;

            public bool Solid;

            public Block(bool solid, ConsoleColor color)
            {
                Solid = solid;
                Color = color;
            }
        }
    }
}
