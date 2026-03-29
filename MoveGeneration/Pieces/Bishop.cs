using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public sealed class Bishop : Piece
    {

        private static Magic magic = new Magic();

        public Bishop(int colour)
        {
            this.colour = colour;

            bishop_lookup();

        }

        // Calculates the possible bishop movements based on the given starting index on the board
        private ulong bishop_movement_bitboards(int start_index)
        {
            // Define board edge patterns for bitwise operations
            ulong[] board_parts = new ulong[8] {
        255, 18374686479671623680, 18446462598732840960, 65535,
        72340172838076673, 9259542123273814144, 217020518514230019, 13889313184910721216
    };

            // Declare variables for bishop's movement directions and legal moves
            ulong SE, SE_wall, blocked_SE, MSB, LSB, SE_moves, NW, NW_wall, blocked_NW, NW_moves, SW, SW_wall, blocked_SW, SW_moves, NE, NE_wall, blocked_NE, NE_moves, legal_moves, start;

            // List of edge squares for quick checks during movement
            int[] edge_squares = { 0, 1, 2, 3, 4, 5, 6, 7, 56, 57, 58, 59, 60, 61, 62, 63, 8, 16, 24, 32, 40, 48, 15, 23, 31, 39, 47, 55 };

            // Set the starting position as a bitmask
            start = 1ul << start_index;

            // Determine the closest boundary to the left of the start position
            int rounddown = start_index & -8;

            // Determine the closest boundary to the right of the start position
            int roundup = 8 + start_index & -8;

            // Calculate South-East diagonal moves from the start position
            SE = (SE_NW[-(start_index - 9 * (rounddown / 8)) + 7] ^ start) >> start_index << start_index;
            SE_move[start_index] = SE;

            // Determine walls in the South-East direction (blocked squares)
            SE_wall = (SE | start) & (board_parts[1] | board_parts[5]);
            SE ^= SE_wall;

            // Calculate North-West diagonal moves from the start position
            NW = (SE_NW[-(start_index - 9 * (rounddown / 8)) + 7] ^ start) & start - 1;
            NW_move[start_index] = NW;

            // Determine walls in the North-West direction
            NW_wall = (NW | start) & (board_parts[0] | board_parts[4]);
            NW ^= NW_wall;

            // Calculate South-West diagonal moves from the start position
            SW = (SW_NE[start_index - 7 * (roundup / 8) + 7] ^ start) >> start_index << start_index;
            SW_move[start_index] = SW;

            // Determine walls in the South-West direction
            SW_wall = (SW | start) & (board_parts[1] | board_parts[4]);
            SW ^= SW_wall;

            // Calculate North-East diagonal moves from the start position
            NE = (SW_NE[start_index - 7 * (roundup / 8) + 7] ^ start) & start - 1;
            NE_move[start_index] = NE;

            // Determine walls in the North-East direction
            NE_wall = (NE | start) & (board_parts[0] | board_parts[5]);
            NE ^= NE_wall;

            // If the start index is an edge square, limit the legal moves
            if (edge_squares.Contains(start_index))
            {
                legal_moves = (SE | SW | NE | NW) ^ start;  // Exclude the start square itself
                return legal_moves;  // Return the valid bishop moves on the edges
            }
            else
            {
                // Return all valid bishop moves
                legal_moves = SE | SW | NE | NW;
                return legal_moves;
            }
        }


        private ulong bishop_legal_moves(int start_index, ulong all_pieces)
        {

            ulong start = 1ul << start_index;

            int rounddown = start_index & -8;

            int roundup = 8 + start_index & -8;

            ulong[] board_parts = new ulong[8] { 255, 18374686479671623680, 18446462598732840960, 65535, 72340172838076673, 9259542123273814144, 217020518514230019, 13889313184910721216 };

            ulong SE, SE_wall, blocked_SE, MSB, LSB, SE_moves, NW, NW_wall, blocked_NW, NW_moves, SW, SW_wall, blocked_SW, SW_moves, NE, NE_wall, blocked_NE, NE_moves, legal_moves;

            SE = (SE_NW[-(start_index - 9 * (rounddown / 8)) + 7] ^ start) >> start_index << start_index;

            SE_wall = (SE | start) & (board_parts[1] | board_parts[5]);

            blocked_SE = SE & all_pieces | SE_wall;

            LSB = blocked_SE ^ blocked_SE & (blocked_SE - 1 ^ 1ul << 63 - 1);

            SE_moves = LSB - start << 1 & SE_NW[-(start_index - 9 * (rounddown / 8)) + 7];

            NW = (SE_NW[-(start_index - 9 * (rounddown / 8)) + 7] ^ start) & start - 1;

            NW_wall = (NW | start) & (board_parts[0] | board_parts[4]);

            blocked_NW = NW & all_pieces | NW_wall;

            int shift;

            shift = 63 - bit64(blocked_NW).IndexOf('1');

            MSB = blocked_NW >> shift << shift;

            NW_moves = start - MSB & SE_NW[-(start_index - 9 * (rounddown / 8)) + 7];

            SW = (SW_NE[start_index - 7 * (roundup / 8) + 7] ^ start) >> start_index << start_index;

            SW_wall = (SW | start) & (board_parts[1] | board_parts[4]);

            blocked_SW = SW & all_pieces | SW_wall;

            LSB = blocked_SW ^ blocked_SW & (blocked_SW - 1 ^ 1ul << 63 - 1);

            SW_moves = LSB - start << 1 & SW_NE[start_index - 7 * (roundup / 8) + 7];

            NE = (SW_NE[start_index - 7 * (roundup / 8) + 7] ^ start) & start - 1;

            NE_wall = (NE | start) & (board_parts[0] | board_parts[5]);

            blocked_NE = NE & all_pieces | NE_wall;

            shift = 63 - bit64(blocked_NE).IndexOf('1');

            MSB = blocked_NE >> shift << shift;

            NE_moves = start - MSB & SW_NE[start_index - 7 * (roundup / 8) + 7];

            legal_moves = NE_moves | NW_moves | SW_moves | SE_moves;

            return legal_moves;
        }

        private void bishop_lookup()
        {
            for (int start_square = 0; start_square < 64; start_square++)
            {
                ulong bishop_moves = bishop_movement_bitboards(start_square);

                bishop_mask[start_square] = bishop_moves;

                magic = new Magic("bishop", false, bishop_moves);

                ulong[] blockers = magic.all_blockers();

                foreach (ulong blocker in blockers)
                {

                    ulong key = blocker * magic.bishop_magic_values[start_square] >> magic.bishop_shift_values[start_square];

                    bishop_look[start_square, key] = bishop_legal_moves(start_square, blocker);

                }

            }

        }

        public ulong mask_moves(int startIndex, Board board, ulong filter)
        {

            ulong blockers = bishop_mask[startIndex] & board.all_pieces;

            ulong key = blockers * magic.bishop_magic_values[startIndex] >> magic.bishop_shift_values[startIndex];

            return bishop_look[startIndex, key] & ~filter;
        }

        public override ulong moves(int startIndex, Board board, PieceCall cache, Check info, ulong[] pins, ulong filter)
        {

            ulong blockers = bishop_mask[startIndex] & board.all_pieces;

            ulong key = blockers * magic.bishop_magic_values[startIndex] >> magic.bishop_shift_values[startIndex];

            return bishop_look[startIndex, key] & ~board.colour[colour] & ~pins[startIndex] & info.mask & ~filter;
        }

        public override ulong capture_moves(int startIndex, Board board, PieceCall cache, Check info, ulong[] pins, ulong check, ulong filter)
        {

            ulong blockers = bishop_mask[startIndex] & board.all_pieces;

            ulong key = blockers * magic.bishop_magic_values[startIndex] >> magic.bishop_shift_values[startIndex];

            ulong legal = bishop_look[startIndex, key] & ~board.colour[colour] & ~pins[startIndex] & info.mask;

            return legal & board.all_pieces & ~filter | legal & check;

        }
    }

}
