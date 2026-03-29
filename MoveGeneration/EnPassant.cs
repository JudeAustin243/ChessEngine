using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public sealed class EnPassant : Piece
    {

        public void two_squares(Move move, PieceCall cache)

        {

            if (move.piece == 0 || move.piece == 1)

            {

                if (move.piece == 0 && move.start - move.end == 16)

                {

                    cache.Global.white = move.end;

                }

                else if (move.piece == 1 && move.start - move.end == -16)

                {

                    cache.Global.black = move.end;

                }

                else

                {

                    cache.Global.black = -1;

                    cache.Global.white = -1;

                }

            }

            else

            {

                cache.Global.black = -1;

                cache.Global.white = -1;

            }

        }

        public ulong can_enpassant(int start_index, ulong legal_moves, int colour, Global global, Board board)
        {

            ulong start = 1ul << start_index;

            if (colour == 0)
            {

                if (!((start & board_parts[4]) != 0 && (1UL << global.black & board_parts[5]) != 0 || (start & board_parts[5]) != 0 && (1UL << global.black & board_parts[4]) != 0))

                {

                    if (Math.Abs(start_index - global.black) == 1)
                    {

                        legal_moves |= 1UL << global.black - 8;

                    }
                }

            }

            if (colour == 1)
            {

                if (!((start & board_parts[5]) != 0 && (1UL << global.white & board_parts[4]) != 0 || (start & board_parts[4]) != 0L && (1UL << global.white & board_parts[5]) != 0))
                {

                    if (Math.Abs(start_index - global.white) == 1)
                    {

                        legal_moves |= 1UL << global.white + 8;

                    }
                }
            }
            return legal_moves;
        }

        public void do_passant(Move move, ulong[] bitboards, bool update, int capture)
        {
            //If pawns have moved diagonally and have not "gone over" a piece. En passant has occured
            if ((move.end - move.start == -9 || move.end - move.start == -7) && capture == 12)

            {

                if (update)
                {
                    bitboards[1] ^= 1ul << move.end + 8;


                }
                else
                {
                    bitboards[1] |= 1ul << move.end + 8;

                }

            }




            //If pawns have moved diagonally and have not "gone over" a piece. En passant has occured
            if ((move.end - move.start == 9 || move.end - move.start == 7) && capture == 12)

            {

                if (update)
                {
                    bitboards[0] ^= 1ul << move.end - 8;

                }
                else
                {
                    bitboards[0] |= 1ul << move.end - 8;

                }
            }
        }

        public void do_passant2(Move move, ulong[] bitboards, bool update, int capture, ref ulong currentKey, ulong[,] piece_keys)
        {
            //If pawns have moved diagonally and have not "gone over" a piece. En passant has occured
            if ((move.end - move.start == -9 || move.end - move.start == -7) && capture == 12)

            {

                if (update)
                {

                    bitboards[1] ^= 1ul << move.end + 8;
                    currentKey ^= piece_keys[1, move.end + 8];

                }
                else
                {
                    bitboards[1] |= 1ul << move.end + 8;
                    currentKey ^= piece_keys[1, move.end + 8];

                }

            }




            //If pawns have moved diagonally and have not "gone over" a piece. En passant has occured
            if ((move.end - move.start == 9 || move.end - move.start == 7) && capture == 12)

            {

                if (update)
                {
                    bitboards[0] ^= 1ul << move.end - 8;
                    currentKey ^= piece_keys[0, move.end - 8];

                }
                else
                {
                    bitboards[0] |= 1ul << move.end - 8;
                    currentKey ^= piece_keys[0, move.end - 8];

                }
            }
        }

        public void two_squares2(int piece, int start, int end, PieceCall cache)

        {

            if (piece == 0 || piece == 1)

            {

                if (piece == 0 && start - end == 16)

                {

                    cache.Global.white = end;

                }

                else if (piece == 1 && start - end == -16)

                {

                    cache.Global.black = end;

                }

                else

                {

                    cache.Global.black = -1;

                    cache.Global.white = -1;

                }

            }

            else

            {

                cache.Global.black = -1;

                cache.Global.white = -1;

            }

        }

        public void do_passant2(int piece, int start, int end, ulong[] bitboards, bool update, int capture)
        {

            if ((end - start == -9 || end - start == -7) && capture == 12)

            {
                if (update)
                {
                    bitboards[1] ^= 1ul << end + 8;

                }
                else
                {
                    bitboards[1] |= 1ul << end + 8;

                }

            }





            if ((end - start == 9 || end - start == 7) && capture == 12)

            {

                if (update)
                {
                    bitboards[0] ^= 1ul << end - 8;

                }
                else
                {
                    bitboards[0] |= 1ul << end - 8;

                }
            }
        }

    }
}
