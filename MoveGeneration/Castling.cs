using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public class Castling : Piece
    {



        public void set_castling(Move move, PieceCall cache)
        {
            if (move.piece == 10)
            {

                cache.Global.Wkingcastle = false;

                cache.Global.Wqueencastle = false;

                return;

            }
            if (move.piece == 2 && move.start == 63)
            {

                cache.Global.Wkingcastle = false;

                return;
            }

            if (move.piece == 2 && move.start == 56)
            {

                cache.Global.Wqueencastle = false;

                return;
            }

            if (move.piece == 11)
            {

                cache.Global.Bkingcastle = false;

                cache.Global.Bqueencastle = false;

                return;
            }

            if (move.piece == 3 && move.start == 7)
            {

                cache.Global.Bkingcastle = false;

                return;
            }

            if (move.piece == 3 && move.start == 0)
            {

                cache.Global.Bqueencastle = false;

                return;
            }

        }

        public void do_castling(Move move, ulong[] bitboards, bool update, bool[] is_castle)
        {
            switch (move.end)
            {
                case 62:

                    if (move.start == 60 && move.piece == 10)

                    {
                        //white rook kingside
                        if (update)
                        {

                            bitboards[2] = bitboards[2] ^ 1ul << 63 | 1ul << 61;

                            is_castle[0] = true;

                        }
                        else
                        {

                            bitboards[2] = bitboards[2] ^ 1ul << 61 | 1ul << 63;

                            is_castle[0] = false;

                        }

                    }
                    break;
                case 58:
                    if (move.start == 60 && move.piece == 10)

                    {

                        //white rook queenside
                        if (update)
                        {
                            bitboards[2] = bitboards[2] ^ 1ul << 56 | 1ul << 59;

                            is_castle[0] = true;

                        }
                        else
                        {
                            bitboards[2] = bitboards[2] ^ 1ul << 59 | 1ul << 56;

                            is_castle[0] = false;
                        }
                    }
                    break;
                case 6:
                    if (move.start == 4 && move.piece == 11)

                    {
                        //black rook kingside
                        if (update)
                        {
                            bitboards[3] = bitboards[3] ^ 1ul << 7 | 1ul << 5;

                            is_castle[1] = true;

                        }
                        else
                        {

                            bitboards[3] = bitboards[3] ^ 1ul << 5 | 1ul << 7;

                            is_castle[1] = false;

                        }

                    }
                    break;
                case 2:
                    if (move.start == 4 && move.piece == 11)

                    {
                        //black rook queenside
                        if (update)
                        {
                            bitboards[3] = bitboards[3] ^ 1ul << 0 | 1ul << 3;

                            is_castle[1] = true;

                        }
                        else
                        {
                            bitboards[3] = bitboards[3] ^ 1ul << 3 | 1ul << 0;

                            is_castle[1] = false;

                        }
                    }
                    break;
            }

        }

        public void do_castling2(Move move, ulong[] bitboards, bool update, bool[] is_castle, ref ulong currentKey, ulong[,] piece_keys)
        {
            switch (move.end)
            {
                case 62: // White kingside castling
                    if (move.start == 60 && move.piece == 10)
                    {
                        int rookType = 2; // white rook type
                        int oldSquare = 63;
                        int newSquare = 61;
                        if (update)
                        {
                            // Update the incremental key for the rook's move:
                            currentKey ^= piece_keys[rookType, oldSquare];
                            currentKey ^= piece_keys[rookType, newSquare];

                            bitboards[2] = bitboards[2] ^ 1UL << oldSquare | 1UL << newSquare;
                            is_castle[0] = true;
                        }
                        else
                        {
                            // Reverse the castling: move rook back.
                            currentKey ^= piece_keys[rookType, newSquare];
                            currentKey ^= piece_keys[rookType, oldSquare];

                            bitboards[2] = bitboards[2] ^ 1UL << newSquare | 1UL << oldSquare;
                            is_castle[0] = false;
                        }
                    }
                    break;
                case 58: // White queenside castling
                    if (move.start == 60 && move.piece == 10)
                    {
                        int rookType = 2; // white rook type
                        int oldSquare = 56;
                        int newSquare = 59;
                        if (update)
                        {
                            currentKey ^= piece_keys[rookType, oldSquare];
                            currentKey ^= piece_keys[rookType, newSquare];

                            bitboards[2] = bitboards[2] ^ 1UL << oldSquare | 1UL << newSquare;
                            is_castle[0] = true;
                        }
                        else
                        {
                            currentKey ^= piece_keys[rookType, newSquare];
                            currentKey ^= piece_keys[rookType, oldSquare];

                            bitboards[2] = bitboards[2] ^ 1UL << newSquare | 1UL << oldSquare;
                            is_castle[0] = false;
                        }
                    }
                    break;
                case 6: // Black kingside castling
                    if (move.start == 4 && move.piece == 11)
                    {
                        int rookType = 3; // black rook type
                        int oldSquare = 7;
                        int newSquare = 5;
                        if (update)
                        {
                            currentKey ^= piece_keys[rookType, oldSquare];
                            currentKey ^= piece_keys[rookType, newSquare];

                            bitboards[3] = bitboards[3] ^ 1UL << oldSquare | 1UL << newSquare;
                            is_castle[1] = true;
                        }
                        else
                        {
                            currentKey ^= piece_keys[rookType, newSquare];
                            currentKey ^= piece_keys[rookType, oldSquare];

                            bitboards[3] = bitboards[3] ^ 1UL << newSquare | 1UL << oldSquare;
                            is_castle[1] = false;
                        }
                    }
                    break;
                case 2: // Black queenside castling
                    if (move.start == 4 && move.piece == 11)
                    {
                        int rookType = 3; // black rook type
                        int oldSquare = 0;
                        int newSquare = 3;
                        if (update)
                        {
                            currentKey ^= piece_keys[rookType, oldSquare];
                            currentKey ^= piece_keys[rookType, newSquare];

                            bitboards[3] = bitboards[3] ^ 1UL << oldSquare | 1UL << newSquare;
                            is_castle[1] = true;
                        }
                        else
                        {
                            currentKey ^= piece_keys[rookType, newSquare];
                            currentKey ^= piece_keys[rookType, oldSquare];

                            bitboards[3] = bitboards[3] ^ 1UL << newSquare | 1UL << oldSquare;
                            is_castle[1] = false;
                        }
                    }
                    break;
            }
        }



        public void set_castling2(int piece, int start, int end, PieceCall cache)
        {
            if (piece == 10)
            {

                cache.Global.Wkingcastle = false;

                cache.Global.Wqueencastle = false;

                return;

            }
            if (piece == 2 && start == 63)
            {

                cache.Global.Wkingcastle = false;

                return;
            }

            if (piece == 2 && start == 56)
            {

                cache.Global.Wqueencastle = false;

                return;
            }

            if (piece == 11)
            {

                cache.Global.Bkingcastle = false;

                cache.Global.Bqueencastle = false;

                return;
            }

            if (piece == 3 && start == 7)
            {

                cache.Global.Bkingcastle = false;

                return;
            }

            if (piece == 3 && start == 0)
            {

                cache.Global.Bqueencastle = false;

                return;
            }

        }

        public void do_castling2(int piece, int start, int end, ulong[] bitboards, bool update, bool[] is_castle)
        {
            switch (end)
            {
                case 62:

                    if (start == 60 && piece == 10)

                    {

                        if (update)
                        {

                            bitboards[2] = bitboards[2] ^ 1ul << 63 | 1ul << 61;

                            is_castle[0] = true;

                        }
                        else
                        {

                            bitboards[2] = bitboards[2] ^ 1ul << 61 | 1ul << 63;

                            is_castle[0] = false;

                        }

                    }
                    break;
                case 58:
                    if (start == 60 && piece == 10)

                    {


                        if (update)
                        {
                            bitboards[2] = bitboards[2] ^ 1ul << 56 | 1ul << 59;

                            is_castle[0] = true;

                        }
                        else
                        {
                            bitboards[2] = bitboards[2] ^ 1ul << 59 | 1ul << 56;

                            is_castle[0] = false;
                        }
                    }
                    break;
                case 6:
                    if (start == 4 && piece == 11)

                    {

                        if (update)
                        {
                            bitboards[3] = bitboards[3] ^ 1ul << 7 | 1ul << 5;

                            is_castle[1] = true;

                        }
                        else
                        {

                            bitboards[3] = bitboards[3] ^ 1ul << 5 | 1ul << 7;

                            is_castle[1] = false;

                        }

                    }
                    break;
                case 2:
                    if (start == 4 && piece == 11)

                    {

                        if (update)
                        {
                            bitboards[3] = bitboards[3] ^ 1ul << 0 | 1ul << 3;

                            is_castle[1] = true;

                        }
                        else
                        {
                            bitboards[3] = bitboards[3] ^ 1ul << 3 | 1ul << 0;

                            is_castle[1] = false;

                        }
                    }
                    break;
            }

        }

    }
}
