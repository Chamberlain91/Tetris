using System;
using System.Collections.Generic;
using System.Diagnostics;

using Tetris.Terminal;

namespace Tetris
{
    public class Tetris : TerminalApplication
    {
        public Playfield Playfield;

        public Tetromino UserPiece;
        public Tetromino NextPiece => NextQueue.Peek();
        public Tetromino HoldPiece;

        public Queue<Tetromino> NextQueue;

        public Renderer Renderer;

        // Active Piece State
        public int Rotation;
        public int X;
        public int Y;

        // 
        public bool CanHoldSwap = true;
        public bool DidHardDrop = false;
        public bool IsLastGrace = false;

        //
        public int Score = 0;

        // Current Level State
        public int LevelLineCount = 0;
        public int Level = 0;

        public const int LevelLineCountGoal = 10;
        public const int MaxLevel = 10;

        // Drop Timer
        public int DropDuration = 0;
        public int DropTime = 0;

        // Milisecond tick rate (how fast the block drops)
        public const int MaxDropDuration = 500;
        public const int MinDropDuration = 75;

        // RNG!
        public Random Random = new Random();

        private readonly Queue<string> _debugMessages;

        public Tetris()
        {
            // Construct the renderer
            Renderer = new Renderer(this);

            // Construct the playfield
            Playfield = new Playfield();

            // Construct the next queue (the future)
            NextQueue = new Queue<Tetromino>();

            _debugMessages = new Queue<string>();

            // Select initial piece
            GiveUserNextPiece();
        }

        protected override void Input(ConsoleKey key, char symbol)
        {
            // Lock input after instant drop
            if (!DidHardDrop)
            {
                // Move Left
                if (key == ConsoleKey.A)
                {
                    if (ApplyMove(-1, 0))
                    {
                        IsLastGrace = false;
                    }
                }

                // Move Right
                if (key == ConsoleKey.D)
                {
                    if (ApplyMove(1, 0))
                    {
                        IsLastGrace = false;
                    }
                }

                // Hold / Swap
                if (key == ConsoleKey.Tab && CanHoldSwap)
                {
                    ApplySwapPiece();
                }

                // Fast Drop
                if (key == ConsoleKey.S)
                {
                    // Move piece down one unit
                    if (ComputeShadowDistance() > 0)
                    {
                        // Cause the piece to naturally drop next frame
                        DropTime = 0;
                    }
                }

                // Hard Drop
                if (key == ConsoleKey.W)
                {
                    // Force the piece to meet its shadow
                    var shadow = ComputeShadowDistance();
                    if (shadow > 0)
                    {
                        Y += shadow; // SMASH!
                        DidHardDrop = true;
                        DropTime = 0;
                    }
                }

                // Clockwise Rotation
                if (key == ConsoleKey.Q || key == ConsoleKey.Spacebar)
                {
                    var rotation = Rotation + 1;
                    if (rotation >= 4) { rotation -= 4; }

                    // Attempt to rotate, if sucessful reset drop timer
                    if (ApplyRotation(rotation))
                    {
                        IsLastGrace = false;
                    }
                }

                // Counter Clockwise rotation
                if (key == ConsoleKey.E)
                {
                    var rotation = Rotation - 1;
                    if (rotation < 0) { rotation += 4; }

                    // Attempt to rotate, if sucessful reset drop timer
                    if (ApplyRotation(rotation))
                    {
                        IsLastGrace = false;
                    }
                }
            }
        }

        private void ApplySwapPiece()
        {
            // No existing hold piece (first use)
            if (HoldPiece == null)
            {
                // Assign the hold piece
                HoldPiece = UserPiece;

                // Get the next piece in the queue
                GiveUserNextPiece();
            }
            else
            {
                // Swap piece and hold piece
                var temp = UserPiece;
                UserPiece = HoldPiece;
                HoldPiece = temp;

                // Reset piece to top
                ResetPiecePosition();
            }

            // Mark as unable to hold again
            CanHoldSwap = false;
        }

        public bool ApplyMove(int xOffset, int yOffset)
        {
            var oX = X;
            var oY = Y;

            X += xOffset;
            Y += yOffset;

            if (!IsObstructed())
            {
                // Accept
                return true;
            }

            // Obstructed, reset position
            X = oX;
            Y = oY;

            return false;
        }

        public bool ApplyRotation(int rotation)
        {
            var oRotation = Rotation;
            var oX = X;
            var oY = Y;

            // Set orientation
            Rotation = rotation;

            // Try each kick at this orientation
            foreach (var (kX, kY) in UserPiece.Kicks[new IntVec2(oRotation, rotation)])
            {
                X = oX + kX;
                Y = oY + kY;

                // If the piece is unobstructed at this kick
                if (!IsObstructed())
                {
                    return true; // Accept
                }
            }

            // Ultimately fails, reset to original state
            Rotation = oRotation;
            X = oX;
            Y = oY;

            return false;
        }

        #region Update

        protected override void Update(int delta)
        {
            // Accumulate drop time
            DropTime -= delta;

            // When enough time has passed to drop
            while (DropTime < 0)
            {
                // 
                if (!ApplyMove(0, 1))
                {
                    // Can't move down and already did touch, 
                    // we have waited the grace period
                    if (IsLastGrace)
                    {
                        // Render piece into field
                        ApplyPieceToField();
                        IsLastGrace = false;
                    }
                    else
                    {
                        // Mark as touching
                        IsLastGrace = true;

                        // Set the duration to half second and the tick time
                        // to quarter second, if input succeeds with a touch
                        // still detected, will set the time to half second.
                        // This is that small 'grace period' when a piece
                        // lands but does not lock allowing rotation or
                        DropDuration = 250;
                        DropTime = 250;

                        // 
                        DebugMessage("Touch!");
                    }
                }
                else if (IsLastGrace)
                {
                    // Reset drop duration
                    DropDuration = ComputeCurrentDuration();
                    DropTime = DropDuration;

                    // No longer touching ground as we were able to move down
                    IsLastGrace = false;
                }

                // Increment the time (schedule next tick)
                DropTime += DropDuration;
            }

            // Render Game State
            Render();
        }

        #endregion

        private void Render()
        {
            // Wipe background?
            Buffer.Clear();

            // Stage Draw Location
            var stageX = (Width - Playfield.Width * 2) / 2;
            var stageY = (Height - Playfield.Height) / 2;

            // Draw Game Field
            Renderer.Draw(stageX, stageY, Playfield);

            // Draw Next Piece
            Renderer.DrawPreview(stageX + (Playfield.Width * 2) + 2, stageY, NextPiece);

            // Draw Hold Piece
            if (HoldPiece != null)
            {
                Renderer.DrawPreview(stageX - 8 - 2, stageY, HoldPiece);
            }

            // Draw active piece
            Renderer.Draw(stageX + X * 2, stageY + Y + ComputeShadowDistance(), UserPiece, Rotation, '░');
            Renderer.Draw(stageX + X * 2, stageY + Y, UserPiece, Rotation);

            // Draw Stage Frame
            DrawBorder(stageX - 1, stageY - 1, 1 + Playfield.Width * 2, 1 + Playfield.Height, BorderKind.Pipe);
            DrawString(stageX, stageY - 2, $"Score: {Score.ToString("000000")}");
            DrawString(stageX, stageY - 3, $"Level: {Level} ({LevelLineCount}/10)");

#if DEBUG
            // Render debug messages
            var messageIndex = 0;
            foreach (var message in _debugMessages)
            {
                DrawString(2, 2 + messageIndex++, message);
            }
#endif
        }

        [Conditional("DEBUG")]
        private void DebugMessage(string message)
        {
            _debugMessages.Enqueue(message);

            // Limit total messages
            if (_debugMessages.Count > 20)
            {
                _debugMessages.Dequeue();
            }
        }

        private bool IsObstructed()
        {
            foreach (var (x, y) in GetPieceFieldCoordinates())
            {
                if (Playfield.IsBlockSolid(x, y))
                {
                    // Piece overlaps the field somewhere
                    return true;
                }
            }

            return false;
        }

        private void ApplyPieceToField()
        {
            // Render piece in place
            foreach (var (x, y) in GetPieceFieldCoordinates())
            {
                if (y < 0)
                {
                    // GAME OVER!
                    TriggerGameOver();
                    return;
                }
                else
                {
                    Playfield.SetColor(x, y, UserPiece.Color);
                    Playfield.SetBlock(x, y, true);
                }
            }

            // Detect cleared lins
            var lines = 0;
            foreach (var row in Playfield.DetectClears())
            {
                // Elimate row
                Playfield.ClearRow(row);

                // Count how many cleared lines
                lines++;
            }

            // Accumulate score for cleared lines
            if (lines > 0)
            {
                // Using the metrics from the original / famicom
                // https://tetris.wiki/Tetris_(BPS)

                switch (lines)
                {
                    // 
                    case 1: Score += 40; break;
                    case 2: Score += 100; break;
                    case 3: Score += 300; break;
                    case 4: Score += 1200; break;

                    default:
                        throw new InvalidOperationException($"Got an unusual amount of lines cleared: {lines}??");
                }

                // Small reward to use instant drops
                if (DidHardDrop)
                {
                    Score += 1;
                }

                // Advance levels
                LevelLineCount += lines;
                if (LevelLineCount > LevelLineCountGoal)
                {
                    LevelLineCount = 0;
                    if (Level < MaxLevel) { Level++; }
                }
            }

            // Move to next piece
            GiveUserNextPiece();

            // Mark as able to hard drop and hold again
            DidHardDrop = false;
            CanHoldSwap = true;
        }

        private void TriggerGameOver()
        {
            // TODO: Proper Launch and Game Over Screen

            Console.Clear();

            // 
            Console.WriteLine();
            Console.WriteLine($" GAME OVER!");
            Console.WriteLine();
            Console.WriteLine($" Score: {Score}");
            Console.WriteLine($" Level: {Level}");

            Console.WriteLine();
            Console.WriteLine($" Press Any Key To Exit Game");
            Console.WriteLine();
            Console.ReadKey();

            Terminate();
        }

        private void GiveUserNextPiece()
        {
            // We have a shortage of pieces in the queue
            if (NextQueue.Count <= 1)
            {
                // Populates with a full set of pieces in random order
                foreach (var piece in CreateRandomizedPieceSet())
                {
                    NextQueue.Enqueue(piece);
                }
            }

            // Get the next piece in the queue
            UserPiece = NextQueue.Dequeue();

            // Position top, center and w/ base rotation
            ResetPiecePosition();
        }

        private void ResetPiecePosition()
        {
            // Set base rotation
            Rotation = 0;

            // Top and center
            X = (Playfield.Width - UserPiece.Width) / 2;
            Y = -1;

            // Set the drop duration
            DropDuration = ComputeCurrentDuration();
        }

        /// <summary>
        /// Computes a drop / tick duration for the current level.
        /// </summary>
        private int ComputeCurrentDuration()
        {
            return Interpolate(MaxDropDuration, MinDropDuration, Level / (float) MaxLevel);

            int Interpolate(int a, int b, float t)
            {
                return a + (int) ((b - a) * t);
            }
        }

        private Tetromino[] CreateRandomizedPieceSet()
        {
            var pieces = new Tetromino[] {
                Tetromino.IPiece,
                Tetromino.JPiece,
                Tetromino.LPiece,
                Tetromino.SPiece,
                Tetromino.ZPiece,
                Tetromino.OPiece,
                Tetromino.TPiece
            };

            // Randomize sequence
            for (var i = 0; i < pieces.Length; i++)
            {
                // 
                var j = Random.Next(pieces.Length);

                // Swap ith and jth pieces
                var temp = pieces[i];
                pieces[i] = pieces[j];
                pieces[j] = temp;
            }

            return pieces;
        }

        private int ComputeShadowDistance()
        {
            var min = Playfield.Height;

            // For each row within the piece
            foreach (var (x, y) in GetPieceFieldCoordinates())
            {
                // If a shadow block
                if (UserPiece.IsShadowBlock(x - X, y - Y, Rotation))
                {
                    // Cast down to find nearest intercepting solid row
                    var row = Playfield.CastDown(x, y, true) - 1;

                    // 
                    var dist = row - y;

                    // Was underground
                    if (dist < 0)
                    {
                        // How much of a vertical is needed to push up?
                        dist = Playfield.CastUp(x, y, false) - y;
                    }

                    // Store minimum distance found
                    min = Math.Min(min, dist);
                }
            }

            return min;
        }

        private IEnumerable<IntVec2> GetPieceFieldCoordinates()
        {
            for (var x = 0; x < UserPiece.Width; x++)
            {
                for (var y = UserPiece.Height; y >= 0; y--)
                {
                    // If a valid block in the piece
                    if (UserPiece.IsSolid(x, y, Rotation))
                    {
                        // Yield coordinate
                        yield return new IntVec2(X + x, Y + y);
                    }
                }
            }
        }
    }
}
