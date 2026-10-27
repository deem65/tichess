using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Move
    {
        public Position From { get; private set; }
        public Position To { get; private set; }
        public Piece? PromotionPieceType { get; private set; }
        public bool IsCastle { get; set; }
        public Move(Position from, Position to)
        {
            From = from;
            To = to;
        }
    }
}
