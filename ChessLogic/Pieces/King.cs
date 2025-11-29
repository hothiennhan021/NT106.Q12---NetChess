using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class King : Pieces
    {
        public override PieceType Type => PieceType.King;
        public override Player Color { get; }

        private static readonly Directions[] dirs = new Directions[]
        {
            Directions.North,
            Directions.South,
            Directions.East,
            Directions.West,
            Directions.NorthWest,
            Directions.NorthEast,
            Directions.SouthWest,
            Directions.SouthEast
        };
        public King(Player color)
        {
            Color = color;
        }
        //ham de castling
        private static bool IsUnmovedRook(Position pos,Board board)
        {
            if(board.IsEmty(pos))
            {
                return false;
            }
            Pieces piece = board[pos];
            return piece.Type == PieceType.Rook && !piece.HasMoved;
        }

        private static bool AllEmpty(IEnumerable<Position> positions,Board board)
        {
            return positions.All(pos => board.IsEmty(pos));
        }

        private bool CanCastleKingSide(Position from,Board board)
        {
            if(HasMoved)
            {
                return false;
            }

            Position rookPos = new Position(from.Row, 7);
            Position[] betweenPositions = new Position[] { new(from.Row, 5), new(from.Row, 6) };

            return IsUnmovedRook(rookPos,board) && AllEmpty(betweenPositions, board);
        }
        //ham Castling
        private bool CanCastleQueenSide(Position from,Board board)
        {
            if(HasMoved)
            {
                return false;
            }
            Position rookPos = new Position(from.Row, 0);
            Position[] betweenPositions = new Position[] {new(from.Row, 1),new(from.Row, 2), new(from.Row, 3) };

            return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
        }

        public override Pieces Copy()
        {
            King copy = new King(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private IEnumerable<Position> MovePositions(Position from, Board board)
        {
            foreach (Directions dir in dirs)
            {
                Position to = from + dir;

                if (!Board.IsInside(to))
                {
                    continue;
                }

                if (board.IsEmty(to) || board[to].Color != Color)
                {
                    yield return to;
                }
            }

        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            foreach (Position to in MovePositions(from, board))
            {
                yield return new NormalMove(from, to);
            }
            //Castling
            if(CanCastleKingSide(from,board))
            {
                yield return new Castle(MoveType.CastleKS,from);
            }
            //Castling
            if(CanCastleQueenSide(from,board))
            {
                yield return new Castle(MoveType.CastleQS,from);   
            }
        }

        public override bool CanCaptureOpponentKing(Position from, Board board)
        {
            return MovePositions(from, board).Any(to =>
            {
                Pieces piece = board[to];
                return piece != null && piece.Type == PieceType.King;
            });
        }
    }
}
