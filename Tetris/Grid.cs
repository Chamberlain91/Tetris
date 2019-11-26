using System.Collections.Generic;

namespace Tetris
{
    /// <summary>
    /// Represents a grid-like data structure.
    /// </summary>
    public class Grid<T>
    {
        private readonly T[,] _data;

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;

            // 
            _data = new T[Width, Height];
        }

        #region Properties

        /// <summary>
        /// Width of the grid.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the grid.
        /// </summary>
        public int Height { get; }

        #endregion

        #region Indexer

        public ref T this[int x, int y] => ref _data[x, y];

        #endregion

        public bool Contains(int x, int y)
        {
            if (x < 0 || x >= Width) { return false; }
            if (y < 0 || y >= Height) { return false; }
            return true;
        }

        public void Set(int x, int y, T value)
        {
            _data[x, y] = value;
        }

        public T Get(int x, int y)
        {
            return _data[x, y];
        }

        public IEnumerable<IntVec2> GetCoordinates()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = Height - 1; y >= 0; y--)
                {
                    yield return new IntVec2(x, y);
                }
            }
        }
    }
}
