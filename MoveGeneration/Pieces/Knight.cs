using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public sealed class Knight : Piece
    {
        public readonly ulong[] mask = new ulong[64];

        public Knight(int colour)
        {
            this.colour = colour;

            mask = knight_mask();
        }
        //Precompute mask to save time
        public ulong[] knight_mask()
        {

            for (int s = 0; s < 64; s++)
            {
                ulong start = 1ul << s;

                ulong legal_moves = 0;

                if ((start & (board_parts[2] | board_parts[4])) == 0)

                {
                    legal_moves |= 1ul << s + 15;


                }

                if ((start & (board_parts[2] | board_parts[5])) == 0)

                {
                    legal_moves |= 1ul << s + 17;

                }

                if ((start & (board_parts[1] | board_parts[7])) == 0)

                {

                    legal_moves |= 1ul << s + 10;

                }

                if ((start & (board_parts[1] | board_parts[6])) == 0)

                {

                    legal_moves |= 1ul << s + 6;

                }

                if ((start & (board_parts[0] | board_parts[6])) == 0)

                {
                    legal_moves |= 1ul << s - 10;

                }

                if ((start & (board_parts[0] | board_parts[7])) == 0)

                {
                    legal_moves |= 1ul << s - 6;

                }

                if ((start & (board_parts[3] | board_parts[4])) == 0)

                {
                    legal_moves |= 1ul << s - 17;

                }

                if ((start & (board_parts[3] | board_parts[5])) == 0)

                {
                    legal_moves |= 1ul << s - 15;

                }
                mask[s] = legal_moves;
            }

            return mask;
        }

        //Moves
        public override ulong moves(int start_index, Board board, PieceCall cache, Check info, ulong[] pins, ulong filter)
        {
            //Shows how moves are made legal on smaller board sizes with the filter
            return mask[start_index] & ~board.colour[colour] & ~pins[start_index] & info.mask & ~filter;
        }

        public override ulong capture_moves(int start_index, Board board, PieceCall cache, Check info, ulong[] pins, ulong check, ulong filter)
        {

            ulong legal_moves = mask[start_index] & ~board.colour[colour] & ~pins[start_index] & info.mask;

            return legal_moves & board.all_pieces & ~filter | legal_moves & check;
        }

    }
}

