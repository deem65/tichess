
namespace Core
{
    public class GameState
    {
        public BoardModel Board { get; } 
        public int SideToMove { get; private set; } 
        public GameResult Result { get; private set; } 
        public bool WhiteKingCastling { get; set; }
        public bool WhiteQueenCastling { get; set; }
        public bool BlackKingCastling { get; set; }
        public bool BlackQueenCastling { get; set; }
        public bool HasWhiteKingMoved { get; set; }
        public bool HasWhiteRookMoved { get; set; }
        public bool HasBlackKingMoved { get; set; }
        public bool HasBlackRookMoved { get; set; }
        public GameState()
        {
            Board = new();
            SideToMove = 0;
            Result = GameResult.Ongoing;
        }
        public bool TryMove(Move m)
        {
            Piece? piece = Board.GetSquare(m.From).Piece;

            if (piece == null)
                return false;

            if (piece.ColorId != SideToMove)
                return false;

            if (!GameUtil.IsLegalMove(this, Board, m))
                return false;

            if (!CheckCastle(m))
                return false;
            
            GameUtil.ApplyMove(m, Board);

            if (m.IsCastle)
                UpdateCastlingFlags(m.To.X, m.To.Y);

            SwitchTurns();

            return true;
        }
        private void SwitchTurns()
        {
            SideToMove = SideToMove == 0 ? 1 : 0;
        }
        private bool CheckCastle(Move m)
        {
            if (m.IsCastle)
            {
                if (
                    (m.To.X == 6 && m.To.Y == 0 && !WhiteKingCastling) ||
                    (m.To.X == 3 && m.To.Y == 0 && !WhiteQueenCastling) ||
                    (m.To.X == 6 && m.To.Y == 8 && !BlackKingCastling) ||
                    (m.To.X == 3 && m.To.Y == 8 && !BlackQueenCastling)
                   )
                    return false;
                return true;
            }
            return true;
        }
        private void UpdateCastlingFlags(int x, int y)
        {
            if (x == 6 && y == 0) 
                WhiteKingCastling = false;
            if (x == 3 && y == 0)
                WhiteQueenCastling = false;
            if (x == 6 && y == 8)
                BlackKingCastling = false;
            if (x == 3 && y == 8)
                BlackQueenCastling = false;
        }
    }
}
