using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    //Example of aggregation where the Piece_call class "has-a" class of every important class to be stored but they can all exist independently
    public sealed class PieceCall
    {
        public readonly Pawn White_Pawn;
        public readonly Pawn Black_Pawn;
        public readonly Knight White_Knight;
        public readonly Knight Black_Knight;
        public readonly Rook White_Rook;
        public readonly Rook Black_Rook;
        public readonly Bishop White_Bishop;
        public readonly Bishop Black_Bishop;
        public readonly Queen White_Queen;
        public readonly Queen Black_Queen;
        public readonly King White_King;
        public readonly King Black_King;
        public readonly Castling Castle;
        public readonly EnPassant Passant;
        public Global Global;
        public Check Illegal;
        public Move Opponent_Move;

        public PieceCall(Pawn White_Pawn, Pawn Black_Pawn, Rook White_Rook, Rook Black_Rook, Knight White_Knight, Knight Black_Knight, Bishop White_Bishop, Bishop Black_Bishop, Queen White_Queen, Queen Black_Queen, King White_King, King Black_King, Castling Castle, EnPassant Passant, Global Global, Check Illegal, Move Opponent_Move)
        {
            this.White_Pawn = White_Pawn;
            this.Black_Pawn = Black_Pawn;
            this.White_Rook = White_Rook;
            this.Black_Rook = Black_Rook;
            this.White_Knight = White_Knight;
            this.Black_Knight = Black_Knight;
            this.White_Bishop = White_Bishop;
            this.Black_Bishop = Black_Bishop;
            this.White_Queen = White_Queen;
            this.Black_Queen = Black_Queen;
            this.White_King = White_King;
            this.Black_King = Black_King;
            this.Castle = Castle;
            this.Passant = Passant;
            this.Global = Global;
            this.Illegal = Illegal;
            this.Opponent_Move = Opponent_Move;
        }

        public PieceCall Clone()
        {
            Piece piece = new Piece();
            return piece.get_cache();
        }
    }
}
