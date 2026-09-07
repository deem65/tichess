using static Core.GameUtil;

namespace Core
{
    public class GameState
    {
        public BoardModel Board { get; } 
        public int SideToMove { get; private set; }
        public GameResult Result { get; private set; } 

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

            if (!IsLegalMove(Board, m))
                return false;

            ApplyMove(m, Board);

            SwitchTurns();

            return true;
        }



        private void SwitchTurns()
        {
            SideToMove = SideToMove == 0 ? 1 : 0;
        }
    }
}
