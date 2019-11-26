using System;

namespace Tetris.Terminal
{
    public sealed class BorderKind
    {
        public readonly char TopLeft;

        public readonly char TopRight;

        public readonly char BottomLeft;

        public readonly char BottomRight;

        public readonly char Vertical;

        public readonly char Horizontal;

        public static readonly BorderKind Thin = new BorderKind("┌┐└┘│─");

        public static readonly BorderKind Pipe = new BorderKind("╔╗╚╝║═");

        public static readonly BorderKind Solid = new BorderKind("██████");

        public BorderKind(string border)
        {
            if (border.Length != 6)
            {
                // `TR TL BR BL V H`
                throw new ArgumentException($"{nameof(BorderKind)} created with incorrectly formatted string. Must be 6 characters in length.");
            }

            TopLeft = border[0];
            TopRight = border[1];
            BottomLeft = border[2];
            BottomRight = border[3];

            Vertical = border[4];
            Horizontal = border[5];
        }

        public BorderKind(char tl, char tr, char bl, char br, char v, char h)
            : this($"{tl}{tr}{bl}{br}{v}{h}")
        { }
    }
}
