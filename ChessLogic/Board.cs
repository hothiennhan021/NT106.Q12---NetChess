using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class Board
    {
        private readonly Pieces[,] pieces = new Pieces[8, 8];
        public Pieces this[int row, int col]
        {
            get {return pieces[row, col];}
            set { pieces[row, col] = value;}
        }
        public Pieces this [Position pos]
        {
            get { return this[pos.Row, pos.Column]; }
            set { this [pos.Row, pos.Column] = value;}
        }
        public static Board Initial()
        {
            Board board =new Board();
            board.AddStartPieces();
            return board;
        }
        private void AddStartPieces()
        {
            this[0, 0] = new Rook(Player.Black);
            this[0, 1] = new Knight(Player.Black);
            this[0, 2]=new Bishop(Player.Black);
            this[0, 3]=new Queen(Player.Black);
            this[0, 4]= new King(Player.Black);
            this[0, 5] = new Bishop(Player.Black);
            this[0, 6]=new Knight(Player.Black);
            this[0, 7] = new Rook(Player.Black);

            this[7, 0] = new Rook(Player.White);
            this[7, 1] = new Knight(Player.White);
            this[7, 2] = new Bishop(Player.White);
            this[7, 3] = new Queen(Player.White);
            this[7, 4] = new King(Player.White);
            this[7, 5] = new Bishop(Player.White);
            this[7, 6] = new Knight(Player.White);
            this[7, 7] = new Rook(Player.White);

            for(int i=0;i<8;i++)
            {
                this[1, i]=new Pawn(Player.Black);
                this[6, i]=new Pawn(Player.White);
            }
        }
        public static bool IsInside(Position pos)
        {
            return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
        }
        public bool IsEmty(Position pos)
        {
            return this[pos] == null;
        }

        public IEnumerable<Position> PiecePosition()
        {
            for (int r=0; r<8;r++)
            {
                for (int c= 0; c<8;c++)
                {
                    Position pos = new Position(r, c);

                    if(!IsEmty(pos))
                    {
                        yield return pos;
                    }
                }
            }
        }

        public IEnumerable<Position> PiecePositionsFor(Player player)
        {
            return PiecePosition().Where(pos => this[pos].Color == player);
        }

        public bool IsIncheck(Player player )
        {
            return PiecePositionsFor(player.Opponent()).Any(pos =>
            {
                Pieces pieces = this[pos];
                return pieces.CanCaptureOpponentKing(pos, this);
            });
        }

        public Board Copy()
        {
            Board copy = new Board();

            foreach (Position pos in PiecePosition())
            {
                copy[pos] = this[pos].Copy();
            }
            return copy;

        }
        // Thay thế hàm ToFenString cũ trong ChessLogic/Board.cs bằng hàm này:

        public string ToFenString(Player currentPlayer)
        {
            StringBuilder sb = new StringBuilder();

            // 1. Vị trí các quân cờ (Piece Placement)
            for (int r = 0; r < 8; r++)
            {
                int emptyCount = 0;
                for (int c = 0; c < 8; c++)
                {
                    Pieces p = this[r, c];
                    if (p == null)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            sb.Append(emptyCount);
                            emptyCount = 0;
                        }
                        sb.Append(GetPieceChar(p));
                    }
                }
                if (emptyCount > 0) sb.Append(emptyCount);
                if (r < 7) sb.Append('/');
            }

            // 2. Lượt đi (Active Color)
            sb.Append(currentPlayer == Player.White ? " w " : " b ");

            // 3. Quyền nhập thành (Castling Rights) - ĐOẠN SỬA QUAN TRỌNG
            string castling = "";

            // Kiểm tra Trắng: Vua ở (7,4), Xe ở (7,7) và (7,0)
            // Lưu ý: Logic này giả định Vua/Xe chưa di chuyển nếu chúng còn ở vị trí gốc
            if (this[7, 4]?.Type == PieceType.King && this[7, 4]?.Color == Player.White)
            {
                if (this[7, 7]?.Type == PieceType.Rook && this[7, 7]?.Color == Player.White) castling += "K";
                if (this[7, 0]?.Type == PieceType.Rook && this[7, 0]?.Color == Player.White) castling += "Q";
            }

            // Kiểm tra Đen: Vua ở (0,4), Xe ở (0,7) và (0,0)
            if (this[0, 4]?.Type == PieceType.King && this[0, 4]?.Color == Player.Black)
            {
                if (this[0, 7]?.Type == PieceType.Rook && this[0, 7]?.Color == Player.Black) castling += "k";
                if (this[0, 0]?.Type == PieceType.Rook && this[0, 0]?.Color == Player.Black) castling += "q";
            }

            // Nếu không ai được nhập thành thì dùng "-"
            if (string.IsNullOrEmpty(castling)) castling = "-";

            sb.Append(castling);

            // 4. En Passant, Halfmove, Fullmove (Mặc định để đơn giản)
            sb.Append(" - 0 1");

            return sb.ToString();
        }

        private char GetPieceChar(Pieces p)
        {
            char c = ' ';
            switch (p.Type)
            {
                case PieceType.Pawn: c = 'p'; break;
                case PieceType.Rook: c = 'r'; break;
                case PieceType.Knight: c = 'n'; break;
                case PieceType.Bishop: c = 'b'; break;
                case PieceType.Queen: c = 'q'; break;
                case PieceType.King: c = 'k'; break;
            }
            // Quân trắng viết hoa, quân đen viết thường
            return p.Color == Player.White ? char.ToUpper(c) : c;
        }
    }
}
    

