using System.Linq;

namespace Core
{
    /// <summary>
    /// static class which has all methods to work with a board, it can check for checkmate, stalemate, checks, draws, move pieces, check is move is valid, find possible moves, and more.
    /// </summary>
    public static class GameUtil
    {
        //Add these values to X and Y to move: Up, Down, Right, Left, UpRight, UpLeft, DownRight, DownLeft, and all 8 knight moves...
        public static readonly int[,] moveSet = {
                { 1, 0 }, { -1, 0 }, { 0, 1 },
                { 0, -1 }, { 1, 1 }, { 1, -1 },
                { -1, 1 }, { -1, -1 }, { 2, 1 },
                { 1, 2 }, { -1, 2 }, { -2, 1 },
                { -2, -1 }, { -1, -2 }, { 1, -2 },
                { 2, -1 } };
        /*public static bool IsDraw(BoardModel board)
        {
            //WIP          
        }*/
        public static bool IsInCheckmate(int colorId, BoardModel testModel)
        {
            return HasNoLegalMoves(colorId, testModel) && IsInCheck(colorId, testModel);
        }
        public static bool HasNoLegalMoves(int colorId, BoardModel testModel)
        {
            return testModel.Board
                .Where(sq => sq.Piece != null && sq.Piece.ColorId == colorId)
                .All(sq => !FindMoves(testModel, sq)
                .Any(m => IsLegalMove(testModel, new Move(sq.Position, m))));
        }
        public static bool IsInCheck(int colorId, BoardModel testModel) //bug- findmoves returns movement squares not attackable squares, problem is pawns. 
        {
            Position kingLocation = testModel.Board
                .Where(sq => sq.Piece != null && sq.Piece.ColorId == colorId && sq.Piece.GetType() == typeof(King))
                .Select(sq => sq.Position)
                .FirstOrDefault();

            List<Position> opponentPossibleMoves = testModel.Board
                .Where(sq => sq.Piece != null && sq.Piece.ColorId != colorId)
                .SelectMany(sq => FindAttacks(testModel, sq)).ToList();

            return opponentPossibleMoves.Contains(kingLocation);
        }
        public static bool IsLegalMove(BoardModel b, Move m)
        {
            Piece? p = b.GetSquare(m.From).Piece;

            if (p == null)
                return false;

            if (b.GetSquare(m.To).Piece is King)
                return false;

            if (!FindMoves(b, b.GetSquare(m.From)).Contains(m.To))
                return false;

            BoardModel boardCopy = DeepCopy(b);

            ApplyMove(m, boardCopy);

            return !IsInCheck(p.ColorId, boardCopy);
        }
        public static void ApplyMove(Move m, BoardModel b)
        {
            b.GetSquare(m.To).Piece = b.GetSquare(m.From).Piece;
            b.GetSquare(m.From).Piece = null;
        }
        public static BoardModel DeepCopy(BoardModel toCopy)
        {
            return new BoardModel
            {
                Board = toCopy.Board.Select(square => new Square(square)).ToArray()
            };
        }
        public static Position NextPosition(Position current)
        {
            if (current.Y > 7)
            {
                return new Position(current.X + 1, 1);
            }
            return new Position(current.X, current.Y + 1);

        }
        public static List<Position> FindAttacks(BoardModel board, Square square)
        {
            Piece? piece = square.Piece;

            if (piece == null)
                return [];

            if (piece is Pawn)
                return FindPawnAttacks(square.Position, piece.ColorId);

            return FindSquares(board, square, piece, true);
        }

        public static List<Position> FindMoves(BoardModel board, Square square)
        {
            Piece? piece = square.Piece;

            if (piece == null)
                return [];

            if (piece is Pawn)
                return FindPawnMoves(board, square, piece);

            return FindSquares(board, square, piece, false);
        }
        private static List<Position> FindSquares(BoardModel board, Square square, Piece piece, bool includeFriendly)
        {
            List<Position> results = [];

            for (int direction = piece.StartIndex; direction < piece.EndIndex; direction++)
            {
                Position current = square.Position;
                int reach = piece.FindReach(current, piece.ColorId);

                AddReachableSquares(board, piece, includeFriendly, results, direction, current, reach);
            }

            return results;
        }

        private static void AddReachableSquares(BoardModel board,Piece piece, bool includeFriendly,List<Position> results, int direction, Position current, int reach)
        {
            for (int i = 0; i < reach; i++)
            {
                current = new Position(
                    current.X + moveSet[direction, 0],
                    current.Y + moveSet[direction, 1]
                );

                if (!Position.IsValid(current))
                    break;

                Piece? target = board.GetSquare(current).Piece;
                if (target is not King || includeFriendly)
                {
                    if (target == null || includeFriendly || target.ColorId != piece.ColorId)
                        results.Add(current);
                }

                if (target != null)
                    break;
            }
        }

        private static List<Position> FindPawnAttacks(Position from, int colorId)
        {
            List<Position> attacks = [];
            int direction = colorId == 0 ? 1 : -1;

            for (int offset = -1; offset <= 1; offset += 2)
            {
                Position attack = new(
                    from.X + direction,
                    from.Y + offset
                );

                if (Position.IsValid(attack))
                    attacks.Add(attack);
            }

            return attacks;
        }
        private static List<Position> FindPawnMoves(BoardModel board, Square square, Piece pawn)
        {
            List<Position> moves = [];

            int direction = pawn.ColorId == 0 ? 1 : -1;
            int startingRank = pawn.ColorId == 0 ? 2 : 7;
            int reach = square.Position.X == startingRank ? 2 : 1;

            for (int i = 1; i <= reach; i++)
            {
                Position next = new(
                    square.Position.X + direction * i,
                    square.Position.Y
                );

                if (!Position.IsValid(next))
                    break;

                if (board.GetSquare(next).Piece != null)
                    break;

                moves.Add(next);
            }

            foreach (Position attack in FindPawnAttacks(square.Position, pawn.ColorId))
            {
                Piece? target = board.GetSquare(attack).Piece;

                if (target != null && target.ColorId != pawn.ColorId && target is not King)
                {
                    moves.Add(attack);
                }
            }

            return moves;
        }
        public static List<List<Piece>> GetDrawableCombinations()
        {
            return [
                [new Knight(1), new Knight(1), new King(1), new King(0)],
                [new Knight(0), new Knight(0), new King(0), new King(1)],
                [new King(0), new King(1)],
                [new Knight(0), new King(0), new King(1)],
                [new Knight(1), new King(0), new King(1)]];
        }
        public static int CTI(int x, int y)
        {
            return 8 * (x - 1) + y - 1;
        }
        public static (int, int) ITC(int index)
        {
            return ((index / 8) + 1, (index % 8) + 1); 
        }
    }
}