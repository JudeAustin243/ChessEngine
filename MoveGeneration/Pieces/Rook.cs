using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public sealed class Rook : Piece
    {

        public Magic magic = new Magic();

        public Rook(int colour)
        {
            this.colour = colour;



            rook_lookup();

            //mask = pawn_mask();
        }



        public ulong rook_movement_bitboards(int s)
        {

            ulong left, right, up, down, start, rook_moves;

            int[] edge_squares = [15, 23, 31, 39, 47, 55];

            ulong right2;

            int[] bottom_squares = [56, 57, 58, 59, 60, 61, 62, 63];

            ulong m = 1;

            start = m << s;

            int rounddown = s & -8;

            int roundup = 8 + s & -8;

            int round64 = roundup;

            if (s > 55 && s < 64)
            {
                roundup = 63;
            }

            ulong floor8 = m << rounddown;

            ulong ceiling8 = m << roundup;

            left = start - floor8 ^ floor8;

            ulong left2 = start - floor8;

            left_move[s] = left2;

            if (bottom_squares.Contains(s))
            {
                right = ceiling8 - start ^ m << round64 - 1 ^ m << 63;

                right2 = ceiling8 - start << 1;
            }
            else
            {
                right = ceiling8 - start ^ m << round64 - 1;
                right2 = ceiling8 - start ^ start;
            }


            right_move[s] = right2;

            down = (array_column[s - rounddown] ^ start) >> s << s ^ m << 63 - (7 - (s - rounddown));

            ulong down2 = (array_column[s - rounddown] ^ start) >> s << s;

            down_move[s] = down2;

            up = (array_column[s - rounddown] ^ start) & start - 1 ^ m << s - rounddown;

            ulong up2 = (array_column[s - rounddown] ^ start) & start - 1;

            up_move[s] = up2;

            if (edge_squares.Contains(s))
            {
                rook_moves = left | right | down | up;
            }
            else
            {
                rook_moves = (left | right | down | up) ^ start;
            }

            return rook_moves;

        }

        // Calculates the legal moves of a rook from a given square, considering blockers
        public ulong rook_legal_moves(int s, ulong blocker)
        {
            // Define an array of squares at the bottom of the board (rank 8)
            int[] bottom_squares = { 56, 57, 58, 59, 60, 61, 62, 63 };

            ulong start = 1ul << s;  // Set the starting square as a bitmask
            int rounddown = s & -8;  // Calculate the nearest boundary for row alignment (rounding down to the nearest 8 multiple)
            int roundup = 8 + s & -8;  // Calculate the nearest boundary for the upper row (rounding up)

            // If the square is on the last row, adjust roundup to the maximum (63)
            if (s > 55 && s < 64)
            {
                roundup = 63;
            }

            // Create bitmasks for the bottom and top rows based on the calculated row boundaries
            ulong floor8 = 1ul << rounddown;
            ulong ceiling8 = 1ul << roundup;

            ulong left, left_wall, blocked_left, MSB, left_moves, right, right_wall, blocked_right, LSB, right_moves, down, bottom_wall, blocked_down, down_moves, up, top_wall, blocked_up, up_moves, legal_moves;

            // Calculate the legal left moves considering blockers and left boundary
            left = start - floor8;
            left_wall = floor8;
            blocked_left = left & blocker | left_wall;
            int shift = 63 - bit64(blocked_left).IndexOf('1');
            MSB = blocked_left >> shift << shift;
            left_moves = start - MSB;

            // Calculate the legal right moves considering blockers and right boundary
            if (bottom_squares.Contains(s))
            {
                right = ceiling8 - start << 1;
                right_wall = ceiling8;
            }
            else
            {
                right = ceiling8 - start;
                right_wall = 1ul << roundup - 1;
            }

            blocked_right = right & blocker | right_wall;
            LSB = blocked_right ^ blocked_right & (blocked_right - 1 ^ 1ul << 63);
            right_moves = LSB - start << 1;

            // Calculate the legal down moves considering blockers and bottom boundary
            down = (array_column[s - rounddown] ^ start) >> s << s;
            bottom_wall = 1ul << 63 - (7 - (s - rounddown));
            blocked_down = down & blocker | bottom_wall;
            LSB = blocked_down ^ blocked_down & (blocked_down - 1 ^ 1ul << 63);
            down_moves = LSB - start << 1 & array_column[s - rounddown];

            // Calculate the legal up moves considering blockers and top boundary
            up = (array_column[s - rounddown] ^ start) & start - 1;
            top_wall = 1ul << s - rounddown;
            blocked_up = up & blocker | top_wall;
            shift = 63 - bit64(blocked_up).IndexOf('1');
            MSB = blocked_up >> shift << shift;
            up_moves = start - MSB & array_column[s - rounddown];

            // Combine all legal moves (up, down, left, right) into one bitmask and return
            legal_moves = up_moves | down_moves | left_moves | right_moves;

            return legal_moves;  // Return the combined legal moves
        }


        // Initializes the rook lookup table by calculating the legal moves for each rook on the board
        public void rook_lookup()
        {
            int count = 0;  // Counter for iterating through blockers
            int extra = 0;  // Unused variable, possibly for future extensions or adjustments
            for (int i = 0; i < 64; i++)  // Iterate over all squares on the board
            {
                ulong rook_moves = rook_movement_bitboards(i);  // Get legal rook moves for the current square

                rook_mask[i] = rook_moves;  // Store the rook's movement mask for the square

                // Create a Magic object for the rook using the calculated moves
                magic = new Magic("rook", false, rook_moves);

                // Get all possible blockers for the rook
                ulong[] blockers = magic.all_blockers();

                count = 0;  // Reset the blocker count
                foreach (ulong blocker in blockers)  // Iterate through each blocker
                {
                    count++;  // Increment the blocker count

                    // Calculate the unique key for this blocker using the magic values
                    ulong key = blocker * magic.rook_magic_values[i] >> magic.rook_shift_values[i];

                    // Store the legal moves for the rook in the lookup table
                    rook_look[i, key] = rook_legal_moves(i, blocker);
                }
            }
        }

        // Returns the possible moves for a rook, applying a filter to exclude certain positions
        public ulong mask_moves(int s, Board board, ulong filter)
        {
            // Calculate the blockers for the rook on the given square
            ulong blockers = rook_mask[s] & board.all_pieces;

            // Calculate the unique key for the current blockers using magic values
            ulong key = blockers * magic.rook_magic_values[s] >> magic.rook_shift_values[s];

            // Return the legal moves from the lookup table, applying the filter to exclude positions
            return rook_look[s, key] & ~filter;
        }

        // Returns the legal moves for the rook, excluding moves that would place the player in check
        public override ulong moves(int s, Board board, PieceCall cache, Check info, ulong[] pins, ulong filter)
        {
            //Hash function used to find the legal moves from rook position and board
            ulong blockers = rook_mask[s] & board.all_pieces;

            // Calculate the unique key for the current blockers using magic values
            ulong key = blockers * magic.rook_magic_values[s] >> magic.rook_shift_values[s];

            // Return the legal moves for the rook, excluding moves that result in check or pins
            return rook_look[s, key] & ~board.colour[colour] & ~pins[s] & info.mask & ~filter;
        }

        // Returns the legal capture moves for the rook, considering check and filters
        public ulong capture_moves(int s, Board board, PieceCall cache, Check info, ulong[] pins, ulong check, ulong filter)
        {
            // Calculate the blockers for the rook on the given square
            ulong blockers = rook_mask[s] & board.all_pieces;

            // Calculate the unique key for the current blockers using magic values
            ulong key = blockers * magic.rook_magic_values[s] >> magic.rook_shift_values[s];

            // Calculate the legal moves for the rook, excluding pins and board color
            ulong legal = rook_look[s, key] & ~board.colour[colour] & ~pins[s] & info.mask;

            // Return the legal capture moves, applying the check and filter
            return legal & board.all_pieces & ~filter | legal & check;
        }


    }
}
