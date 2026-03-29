using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ChessEngine
{
    public sealed class King : Piece
    {
        public readonly ulong[] mask = new ulong[64];

        public King(int colour)
        {
            this.colour = colour;

            king_legal_moves();

        }

        private void king_legal_moves()

        {

            for (int s = 0; s < 64; s++)
            {
                ulong start = 1ul << s;

                if ((start & board_parts[5]) == 0)
                {
                    mask[s] |= 1ul << s + 1;
                }

                if ((start & board_parts[4]) == 0)

                {
                    mask[s] |= 1ul << s - 1;
                }

                if ((start & board_parts[1]) == 0)

                {
                    mask[s] |= 1ul << s + 8;

                }

                if ((start & board_parts[0]) == 0)

                {

                    mask[s] |= 1ul << s - 8;
                }

                if ((start & (board_parts[1] | board_parts[5])) == 0)

                {
                    mask[s] |= 1ul << s + 9;
                }

                if ((start & (board_parts[1] | board_parts[4])) == 0)

                {
                    mask[s] |= 1ul << s + 7;
                }

                if ((start & (board_parts[0] | board_parts[4])) == 0)

                {
                    mask[s] |= 1ul << s - 9;
                }

                if ((start & (board_parts[0] | board_parts[5])) == 0)

                {
                    mask[s] |= 1ul << s - 7;
                }
            }
        }

        public override ulong moves(int start_index, Board board, PieceCall cache, Check info, ulong[] pins, ulong filter)
        {

            ulong legal_moves = mask[start_index];

            legal_moves ^= legal_moves & board.colour[colour];

            legal_moves ^= info.illegal & legal_moves;

            if (info.check == 0 && (start_index == 4 || start_index == 60))

            {
                if (colour == 0)
                {
                    if (cache.Global.Wkingcastle == true && (Wking_side & board.all_pieces) == 0 && (board.bitboards[2] & 1ul << 63) != 0 && (Wking_side & info.illegal) == 0)

                    {

                        legal_moves |= 1UL << 62;

                    }

                    if (cache.Global.Wqueencastle == true && (Wqueen_side & board.all_pieces) == 0 && (board.bitboards[2] & 1ul << 56) != 0 && (864691128455135232 & info.illegal) == 0)

                    {

                        legal_moves |= 1UL << 58;

                    }
                }
                else
                {


                    if (cache.Global.Bkingcastle == true && (Bking_side & board.all_pieces) == 0 && (board.bitboards[3] & 1ul << 7) != 0 && (Bking_side & info.illegal) == 0)

                    {

                        legal_moves |= 1UL << 6;

                    }

                    if (cache.Global.Bqueencastle == true && (Bqueen_side & board.all_pieces) == 0 && (board.bitboards[3] & 1ul << 0) != 0 && (12 & info.illegal) == 0)

                    {

                        legal_moves |= 1UL << 2;

                    }
                }

            }
            return legal_moves & ~filter;

        }

        public override ulong capture_moves(int start_index, Board board, PieceCall cache, Check info, ulong[] pins, ulong check, ulong filter)
        {
            ulong legal_moves = mask[start_index];

            legal_moves ^= legal_moves & board.colour[colour];

            legal_moves ^= info.illegal & legal_moves;

            return legal_moves & board.all_pieces & ~filter;

        }
    }

}
