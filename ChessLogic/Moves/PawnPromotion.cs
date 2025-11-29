//Tao chức năng phong hậu
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
//Tao chức năng phong hậu
namespace ChessLogic
{
    public class PawnPromotion : Move
    {
        public override MoveType Type => MoveType.PawnPromotion;
        public override Position FromPos { get; }
        public override Position ToPos { get; }

        public readonly PieceType newType;
        public PawnPromotion(Position from,Position to, PieceType newType)
        {
            FromPos = from;
            ToPos = to;
            this.newType=newType;
        }
        private Pieces CreatePromotionPiece(Player color)
        {
            return newType switch
            {
                PieceType.Knight => new Knight(color),
                PieceType.Bishop => new Bishop(color),
                PieceType.Rook => new Rook(color),
                _ => new Queen(color)
            };
        }
        //Tao chức năng phong hậu
        public override void Execute(Board board)
        {
            Pieces pawn = board[FromPos];
            board[FromPos] = null;

            Pieces promotionPiece = CreatePromotionPiece(pawn.Color);
            promotionPiece.HasMoved = true;
            board[ToPos] = promotionPiece;
        }
    }
}
