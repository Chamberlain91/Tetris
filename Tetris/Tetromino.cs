using System;
using System.Collections;
using System.Collections.Generic;

namespace Tetris
{
    /// <summary>
    /// Represents the tetraminos
    /// </summary>
    public class Tetromino
    {
        private readonly Grid<BlockState>[] _rotations;

        private Tetromino(string name, ConsoleColor color, Kick kicks, char[,,] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Data is assumed to be in an in code array initializer order
            var rotationCount = input.GetLength(0);
            Height = input.GetLength(1);
            Width = input.GetLength(2);

            Kicks = kicks ?? throw new ArgumentNullException(nameof(kicks));
            Color = color;
            Name = name;

            // Create storage for each rotation
            _rotations = new Grid<BlockState>[rotationCount];

            // Construct each rotation
            for (var o = 0; o < rotationCount; o++)
            {
                // Create grid to store current rotation
                var grid = _rotations[o] = new Grid<BlockState>(Width, Height);

                // Mark appropriate blocks as solid
                foreach (var (x, y) in grid.GetCoordinates())
                {
                    // Mark each block as solid
                    grid[x, y] = input[o, y, x] == ' '
                        ? BlockState.Empty
                        : BlockState.Solid;
                }

                // Detect blocks as shadow casters
                foreach (var (x, y) in grid.GetCoordinates())
                {
                    // If current block is solid
                    if (grid[x, y].HasFlag(BlockState.Solid))
                    {
                        // Valid coordinate and is not solid or is not valid (off bottom edge)
                        if ((grid.Contains(x, y + 1) && grid[x, y + 1] == BlockState.Empty) || !grid.Contains(x, y + 1))
                        {
                            grid[x, y] |= BlockState.Shadow;
                        }
                    }
                }
            }
        }

        #region Properties

        /// <summary>
        /// Width of the data in blocks.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the data in blocks.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Number of unique rotation states (all but square should have 4).
        /// </summary>
        public int NumberRotationStates => _rotations.Length;

        /// <summary>
        /// Name of the tetromino for debug purposes.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Color to use when rendering the tetromino.
        /// </summary>
        public ConsoleColor Color { get; }

        /// <summary>
        /// The kicks to try to use when rotating the tetromino.
        /// </summary>
        public IReadOnlyDictionary<IntVec2, IntVec2[]> Kicks { get; }

        #endregion

        /// <summary>
        /// Determines if a block is considered solid.
        /// </summary>
        public bool IsSolid(int x, int y, int orientation)
        {
            return GetBlock(x, y, orientation).HasFlag(BlockState.Solid);
        }

        /// <summary>
        /// Determines if a block is a shadow caster (bottom most block of the shape).
        /// </summary>
        public bool IsShadowBlock(int x, int y, int orientation)
        {
            return GetBlock(x, y, orientation).HasFlag(BlockState.Shadow);
        }

        /// <summary>
        /// Gets a block state.
        /// </summary>
        private BlockState GetBlock(int x, int y, int orientation)
        {
            if (y < 0 || x < 0 || x >= Width || y >= Height) { return BlockState.Empty; }
            else { return _rotations[orientation % NumberRotationStates][x, y]; }
        }

        #region Kick Data

        private static Kick JLSTZKicks { get; } = new Kick {
            { 0, 1, new IntVec2(0, 0), new IntVec2(-1, 0), new IntVec2(-1,  1), new IntVec2(0, -2), new IntVec2(-1, -2) },
            { 1, 2, new IntVec2(0, 0), new IntVec2( 1, 0), new IntVec2( 1, -1), new IntVec2(0,  2), new IntVec2( 1,  2) },
            { 2, 3, new IntVec2(0, 0), new IntVec2( 1, 0), new IntVec2( 1,  1), new IntVec2(0, -2), new IntVec2( 1, -2) },
            { 3, 0, new IntVec2(0, 0), new IntVec2(-1, 0), new IntVec2(-1, -1), new IntVec2(0,  2), new IntVec2(-1,  2) },
        };

        private static Kick IKicks { get; } = new Kick {
            { 0, 1, new IntVec2(0, 0), new IntVec2(-2, 0), new IntVec2( 1,  0), new IntVec2(-2, -1), new IntVec2( 1,  2) },
            { 1, 2, new IntVec2(0, 0), new IntVec2(-1, 0), new IntVec2( 2,  0), new IntVec2(-1,  2), new IntVec2( 2, -1) },
            { 2, 3, new IntVec2(0, 0), new IntVec2( 2, 0), new IntVec2(-1,  0), new IntVec2( 2,  1), new IntVec2(-1, -2) },
            { 3, 0, new IntVec2(0, 0), new IntVec2( 1, 0), new IntVec2(-2,  0), new IntVec2( 1, -2), new IntVec2(-2,  1) },
        };

        private static Kick OKicks { get; } = new Kick {
            { 0, 1, new IntVec2(0, 0) },
            { 1, 2, new IntVec2(0, 0) },
            { 2, 3, new IntVec2(0, 0) },
            { 3, 0, new IntVec2(0, 0) },
        };

        #endregion

        #region Tetrominos

        public static readonly Tetromino IPiece = new Tetromino("I", ConsoleColor.Cyan, IKicks, new char[,,] {
            {
                {' ',' ',' ',' ' },
                {'X','X','X','X' },
                {' ',' ',' ',' ' },
                {' ',' ',' ',' ' },
            },
            {
                {' ',' ','X',' ' },
                {' ',' ','X',' ' },
                {' ',' ','X',' ' },
                {' ',' ','X',' ' },
            },
            {
                {' ',' ',' ',' ' },
                {' ',' ',' ',' ' },
                {'X','X','X','X' },
                {' ',' ',' ',' ' },
            },
            {
                {' ','X',' ',' ' },
                {' ','X',' ',' ' },
                {' ','X',' ',' ' },
                {' ','X',' ',' ' },
            }
        });

        public static readonly Tetromino TPiece = new Tetromino("T", ConsoleColor.Magenta, JLSTZKicks, new char[,,] {
            {
                {' ','X',' '},
                {'X','X','X'},
                {' ',' ',' '},
            },
            {
                {' ','X',' '},
                {' ','X','X'},
                {' ','X',' '},
            },
            {
                {' ',' ',' '},
                {'X','X','X'},
                {' ','X',' '},
            },
            {
                {' ','X',' '},
                {'X','X',' '},
                {' ','X',' '},
            }
        });

        public static readonly Tetromino LPiece = new Tetromino("L", ConsoleColor.Gray /* Orange */, JLSTZKicks, new char[,,] {
            {
                {' ',' ','X'},
                {'X','X','X'},
                {' ',' ',' '},
            },
            {
                {' ','X',' '},
                {' ','X',' '},
                {' ','X','X'},
            },
            {
                {' ',' ',' '},
                {'X','X','X'},
                {'X',' ',' '},
            },
            {
                {'X','X',' '},
                {' ','X',' '},
                {' ','X',' '},
            }
        });

        public static readonly Tetromino JPiece = new Tetromino("J", ConsoleColor.Blue, JLSTZKicks, new char[,,] {
            {
                {'X',' ',' '},
                {'X','X','X'},
                {' ',' ',' '},
            },
            {
                {' ','X','X'},
                {' ','X',' '},
                {' ','X',' '},
            },
            {
                {' ',' ',' '},
                {'X','X','X'},
                {' ',' ','X'},
            },
            {
                {' ','X',' '},
                {' ','X',' '},
                {'X','X',' '},
            }
        });

        public static readonly Tetromino SPiece = new Tetromino("S", ConsoleColor.Green, JLSTZKicks, new char[,,] {
            {
                {' ','X','X'},
                {'X','X',' '},
                {' ',' ',' '},
            },
            {
                {' ','X',' '},
                {' ','X','X'},
                {' ',' ','X'},
            },
            {
                {' ',' ',' '},
                {' ','X','X'},
                {'X','X',' '},
            },
            {
                {'X',' ',' '},
                {'X','X',' '},
                {' ','X',' '},
            }
        });

        public static readonly Tetromino ZPiece = new Tetromino("Z", ConsoleColor.Red, JLSTZKicks, new char[,,] {
            {
                {'X','X',' '},
                {' ','X','X'},
                {' ',' ',' '},
            },
            {
                {' ',' ','X'},
                {' ','X','X'},
                {' ','X',' '},
            },
            {
                {' ',' ',' '},
                {'X','X',' '},
                {' ','X','X'},
            },
            {
                {' ','X',' '},
                {'X','X',' '},
                {'X',' ',' '},
            }
        });

        public static readonly Tetromino OPiece = new Tetromino("O", ConsoleColor.Yellow, OKicks, new char[,,] {
            {
                {'X','X'},
                {'X','X'},
            }
        });

        #endregion

        [Flags]
        private enum BlockState
        {
            Empty = 0,
            Solid = 1 << 0,
            Shadow = 1 << 1
        }

        private sealed class Kick : IReadOnlyDictionary<IntVec2, IntVec2[]>
        {
            private readonly Dictionary<IntVec2, IntVec2[]> _kicks;

            public Kick()
            {
                _kicks = new Dictionary<IntVec2, IntVec2[]>();
            }

            public void Add(int a, int b, params IntVec2[] kicks)
            {
                // Create inverted kick mapping
                var invKicks = new IntVec2[kicks.Length];
                for (var i = 0; i < invKicks.Length; i++)
                {
                    var (x, y) = kicks[i];
                    invKicks[i] = new IntVec2(-x, -y);
                }

                // Store
                _kicks[new IntVec2(a, b)] = kicks;
                _kicks[new IntVec2(b, a)] = invKicks;
            }

            public IntVec2[] Get(int a, int b)
            {
                return _kicks[new IntVec2(a, b)];
            }

            public bool ContainsKey(IntVec2 key)
            {
                return _kicks.ContainsKey(key);
            }

            public bool TryGetValue(IntVec2 key, out IntVec2[] value)
            {
                return _kicks.TryGetValue(key, out value);
            }

            public IntVec2[] this[IntVec2 key] => _kicks[key];

            public IEnumerable<IntVec2> Keys => ((IReadOnlyDictionary<IntVec2, IntVec2[]>) _kicks).Keys;

            public IEnumerable<IntVec2[]> Values => ((IReadOnlyDictionary<IntVec2, IntVec2[]>) _kicks).Values;

            public int Count => _kicks.Count;

            public IEnumerator<KeyValuePair<IntVec2, IntVec2[]>> GetEnumerator()
            {
                return ((IReadOnlyDictionary<IntVec2, IntVec2[]>) _kicks).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IReadOnlyDictionary<IntVec2, IntVec2[]>) _kicks).GetEnumerator();
            }
        }
    }
}
