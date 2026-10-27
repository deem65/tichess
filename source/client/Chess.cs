using Core;

namespace Client
{
    public partial class Chess : Form
    {
        private GameState Game { get; set; } = new();
        private Button[] Squares { get; set; } = [];
        private Position? SelectedSquare { get; set; }
        private List<Position> LegalMoves { get; set; } = [];
        public Chess()
        {
            InitializeComponent();
        }
        private void Chess_Load(object sender, EventArgs e)
        {
            Squares = new Button[64];

            InitializeBoardButtons();
            CustomBoardButton.Enabled = false; //temp disabled
            ToggleLightButton_Click(new object(), new EventArgs());
            NewGame();
        }
        private void InitializeBoardButtons()
        {
            for (int i = 0; i < Squares.Length; i++)
            {
                Squares[i] = CreateSquareButton(i);
            }
        }
        private Button CreateSquareButton(int i)
        {
            Button button = Controls.Find($"Sq{i}", true).OfType<Button>().Single();

            button.Tag = i;
            button.Click += Sq_Click;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
        private void Sq_Click(object sender, EventArgs e)
        {
            if (sender is not Button)
                return;

            var button = (Button)sender;

            if (button.Tag is not int)
                return;

            int index = (int)button.Tag;

            if (IsGameOver())
                return;

            var clicked = new Position(GameUtil.ITC(index).Item1, GameUtil.ITC(index).Item2);

            if (SelectedSquare == null || CanSelectPiece(clicked))
            {
                Select(clicked);
                return;
            }

            if (SelectedSquare.Value.Equals(clicked))
            {
                Unselect();
                return;
            }

            TryMove(clicked);
        }

        private bool CanSelectPiece(Position position)
        {
            Piece? piece = Game.Board.GetSquare(position).Piece;

            return piece != null && piece.ColorId == Game.SideToMove;
        }

        private void Select(Position pos)
        {
            if (!CanSelectPiece(pos))
                return;

            SelectedSquare = pos;
            LegalMoves = GetLegalMoves(pos);

            RefreshSelection();
            RefreshPossibleMoves();
        }

        private void Unselect()
        {
            SelectedSquare = null;
            LegalMoves.Clear();

            RefreshSelection();
            RefreshPossibleMoves();
        }

        private void TryMove(Position to)
        {
            if (SelectedSquare == null)
                return;

            Position from = SelectedSquare.Value;
            Piece? piece = Game.Board.GetSquare(from).Piece;

            if (piece == null)
            {
                Unselect();
                return;
            }
            if (!Game.TryMove(new(from, to)))
            {
                AppendLog("illegal move");
                return;
            }

            string color = piece.ColorId == 0 ? "white" : "black";

            AppendLog($"{color}: {ToChessNotation(from)} > {ToChessNotation(to)}");

            RefreshAll();
        }
        private void RefreshAll()
        {
            Unselect();
            RefreshBoard();
            RefreshGameStatus();
            RefreshPossibleMoves();
        }
        private List<Position> GetLegalMoves(Position from)
        {
            if (!Position.IsValid(from))
                return [];

            Square sq = Game.Board.GetSquare(from);
            Piece? p = sq.Piece;

            if (p == null || p.ColorId != Game.SideToMove)
                return [];


            List<Position> ms =
                GameUtil.FindMoves(Game.Board, sq)
                .Where(to => GameUtil.IsLegalMove(Game.Board, new Move(from, to)))
                .ToList();


            return ms;
        }
        private bool HasLegalMoves()
        {
            return Game.Board.Board
                .Where(sq => sq.Piece != null &&
                             sq.Piece.ColorId == Game.SideToMove)
                .Any(sq => GetLegalMoves(sq.Position).Count > 0);
        }
        private bool IsGameOver()
        {
            return Game.Result != GameResult.Ongoing ||
                   !HasLegalMoves();
        }
        private void RefreshBoard()
        {
            for (int i = 0; i < Squares.Length; i++)
            {
                var sq = Game.Board.Board[i];
                var p = sq.Piece;

                if (p == null)
                    DisplayNull(i, sq);
                else
                    DisplayPiece(i, sq, p);
            }

            RefreshSelection();
        }

        private void DisplayPiece(int i, Square sq, Piece p)
        {
            string name = $"_{p.Id}{p.ColorId}{sq.Color}";
            Squares[i].Image = Properties.Resources.ResourceManager.GetObject(name) as Bitmap;
        }

        private void DisplayNull(int i, Square sq)
        {
            if (sq.Color == 0)
            {
                Squares[i].Image = Properties.Resources.WhiteEmpty;
                return;
            }
            Squares[i].Image = Properties.Resources.BlackEmpty;
        }

        private void RefreshSelection()
        {
            for (int i = 0; i < Squares.Length; i++)
            {
                Button button = Squares[i];
                (int x, int y) = GameUtil.ITC(i);
                Position position = new(x, y);

                button.FlatAppearance.BorderSize = 0;

                if (SelectedSquare != null &&
                    SelectedSquare.Value.Equals(position))
                {
                    button.FlatAppearance.BorderColor = Color.Goldenrod;
                    button.FlatAppearance.BorderSize = 3;
                }
                else if (LegalMoves.Contains(position))
                {
                    button.FlatAppearance.BorderColor = Color.SteelBlue;
                    button.FlatAppearance.BorderSize = 2;
                }
            }
        }

        private void RefreshPossibleMoves()
        {
            PossibleMoves.Clear();

            if (SelectedSquare == null)
            {
                return;
            }

            Position from = SelectedSquare.Value;
            Piece? p = Game.Board.GetSquare(from).Piece;

            PossibleMoves.Text =
                $"{p?.GetType().Name} on {ToChessNotation(from)}" +
                Environment.NewLine + Environment.NewLine + $"possible moves:" + Environment.NewLine;

            foreach (Position move in LegalMoves)
            {
                PossibleMoves.AppendText(ToChessNotation(move) + Environment.NewLine);
            }
        }

        private void RefreshGameStatus()
        {
            string status;

            if (Game.Result != GameResult.Ongoing)
            {
                status = Game.Result switch
                {
                    GameResult.WhiteWon => "white wins",
                    GameResult.BlackWon => "black wins",
                    GameResult.Draw => "draw",
                    _ => "game over"
                };
            }
            else
            {
                int side = Game.SideToMove;
                string player = side == 0 ? "white" : "black";
                bool isInCheck = GameUtil.IsInCheck(side, Game.Board);

                if (!HasLegalMoves())
                {
                    status = isInCheck
                        ? $"checkmate {OpponentName(side)} wins"
                        : "stalemate draw";
                }
                else
                {
                    status = isInCheck
                        ? $"{player} to move in check!"
                        : $"{player} to move";
                }
            }

            label1.Text = status;
            Text = $"modest-chess — {status}";
        }

        private string OpponentName(int colorId)
        {
            return colorId == 0 ? "black" : "white";
        }
        private void NewGame()
        {
            Game = new GameState();

            SelectedSquare = null;
            LegalMoves.Clear();

            RefreshLogs();
            RefreshBoard();
            RefreshGameStatus();
        }

        private void NewGameButton_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        private void RefreshLogs()
        {
            Log.Clear();
            Log.Text = "logs:";

            PossibleMoves.Clear();
        }

        private void AppendLog(string message)
        {
            Log.AppendText(Environment.NewLine + message);
        }

        private void CustomBoardButton_Click(object sender, EventArgs e)
        {

        }
        private string ToChessNotation(Position position)
        {
            char file = (char)('a' + position.Y - 1);
            return $"{file}{position.X}";
        }

        private void ToggleLightButton_Click(object sender, EventArgs e)
        {
            bool useLight = BackColor == Color.DarkSlateGray;

            Color background = useLight
                ? Color.LightSeaGreen
                : Color.DarkSlateGray;

            Color foreground = useLight
                ? Color.Black
                : Color.White;

            BackColor = background;

            PossibleMoves.BackColor = background;
            PossibleMoves.ForeColor = foreground;

            Log.BackColor = background;
            Log.ForeColor = foreground;

            Label[] labels =
            [
                I1Label, I2Label, I3Label, I4Label,
                I5Label, I6Label, I7Label, I8Label,
                IALabel, IBLabel, ICLabel, IDLabel,
                IELabel, IFLabel, IGLabel, IHLabel
            ];

            foreach (Label label in labels)
                label.ForeColor = foreground;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}