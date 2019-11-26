using System;

namespace Tetris
{
    public struct IntVec2 : IEquatable<IntVec2>
    {
        public int X;

        public int Y;

        public IntVec2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is IntVec2 vec && Equals(vec);
        }

        public bool Equals(IntVec2 other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        internal void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public static bool operator ==(IntVec2 left, IntVec2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntVec2 left, IntVec2 right)
        {
            return !(left == right);
        }
    }
}
