using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public sealed class Illegal : Piece
    {

        PieceCall cache;

        public Check info;

        public readonly Magic magic = new Magic("", false, 0ul);

        public readonly Rook rook;

        public readonly Bishop bishop;

        public Illegal()
        {

        }

        // Calculates the sliding ray between the king and the attack position on the board
        private static ulong sliding_ray(Board Board, int king, int attack)
        {
            ulong ray;

            // Get the bit index (position) of the king using the trailing zero count
            int king_place = BitOperations.TrailingZeroCount(Board.bitboards[king]);

            // Calculate the down and up positions relative to the king's position
            int down_place = king_place & -8;  // Nearest lower boundary (rank start)
            int up_place = down_place + 8;    // Nearest upper boundary (rank end)

            // If the king is positioned between the attack and its down position, calculate the ray
            if (down_place <= attack && down_place <= king_place && up_place > king_place && up_place > attack && king_place > attack)
            {
                // Return the bitmask for the ray between the king and attack
                ray = (1ul << king_place) - (1ul << attack);
                return ray;
            }

            // If the king is positioned between the attack and its up position, calculate the ray
            if (down_place <= attack && down_place <= king_place && up_place > king_place && up_place > attack && king_place < attack)
            {
                // Return the bitmask for the ray between the king and attack (adjusted for up direction)
                ray = (1ul << attack) - (1ul << king_place) ^ (1ul << attack | 1ul << king_place);
                return ray;
            }

            // If the king and attack are aligned vertically, calculate the ray
            if ((king_place - attack) % 8 == 0 && king_place > attack)
            {
                // Return the vertical ray, adjusted for the board's column structure
                ray = (1ul << king_place) - (1ul << attack) & array_column[king_place - down_place];
                return ray;
            }

            // If the king and attack are aligned vertically in the opposite direction, calculate the ray
            if ((king_place - attack) % 8 == 0 && king_place < attack)
            {
                // Return the vertical ray with a shift for upward movement
                ray = ((1ul << attack) - (1ul << king_place) & array_column[king_place - down_place]) << 8;
                return ray;
            }

            // If the king and attack are aligned diagonally (South-East to North-West), calculate the ray
            if ((king_place - attack) % 9 == 0 && king_place < attack)
            {
                // Return the diagonal ray from the king to the attack position (SE to NW)
                ray = ((1ul << attack) - (1ul << king_place) & SE_NW[-(king_place - 9 * (down_place / 8)) + 7]) << 9;
                return ray;
            }

            // If the king and attack are aligned diagonally in the opposite direction, calculate the ray
            if ((king_place - attack) % 9 == 0 && king_place > attack)
            {
                // Return the diagonal ray from the king to the attack position (NW to SE)
                ray = (1ul << king_place) - (1ul << attack) & SE_NW[-(king_place - 9 * (down_place / 8)) + 7];
                return ray;
            }

            // If the king and attack are aligned diagonally (South-West to North-East), calculate the ray
            if ((king_place - attack) % 7 == 0 && king_place > attack)
            {
                // Return the diagonal ray from the king to the attack position (SW to NE)
                ray = (1ul << king_place) - (1ul << attack) & SW_NE[king_place - 7 * (up_place / 8) + 7];
                return ray;
            }

            // If the king and attack are aligned diagonally in the opposite direction, calculate the ray
            if ((king_place - attack) % 7 == 0 && king_place < attack)
            {
                // Return the diagonal ray from the king to the attack position (NE to SW)
                ray = ((1ul << attack) - (1ul << king_place) & SW_NE[king_place - 7 * (up_place / 8) + 7]) << 7;
                return ray;
            }

            return 0;  // Return 0 if no valid ray could be calculated
        }


        // Calculates the capture squares and check status for a given player's king, updating the check status if necessary
        public Check capture_squares(Board Board, int mod, PieceCall cache)
        {
            // Initialize variables to track the check status and illegal move positions
            Starts check_code = new Starts(-1, -1);  // Default starting position for check
            int num_checkers = 0;  // Number of pieces attacking the king
            ulong king = Board.bitboards[mod + 10];  // Get the king's position based on the mod (0 for white, 1 for black)
            ulong illegal = 0;  // Variable to store illegal moves
            ulong move = 0;  // Temporary variable to store the calculated moves for a piece
            ulong check_mask = ulong.MaxValue;  // Mask for checking valid squares for the king
            ulong pawn = 0;  // Stores pawn attack squares

            // Iterate through the pieces of the opponent (mod ^ 1 switches between 0 and 1)
            for (int i = mod ^ 1; i < 12; i += 2)
            {
                ulong board = Board.bitboards[i];  // Get the bitboard of the current piece
                int name = i;

                // Adjust name to match the correct piece (mod ^ 1)
                if (i > 1)
                {
                    name -= mod ^ 1;
                }

                // Loop through each piece of the current type (i.e., pawn, rook, knight, etc.)
                while (board > 0)
                {
                    // Get the position of the piece by counting trailing zeros (bit index)
                    int num = BitOperations.TrailingZeroCount(board);

                    // Switch case to handle different piece types
                    switch (name)
                    {
                        case 0:  // White Pawn
                            move = cache.White_Pawn.mask[num];
                            pawn |= move;
                            break;

                        case 1:  // Black Pawn
                            move = cache.Black_Pawn.mask[num];
                            pawn |= move;
                            break;

                        case 2:  // White Rook
                            ulong blockers = cache.White_Rook.rook_mask[num] & (Board.all_pieces ^ king);  // Exclude the king from blockers
                            ulong key = blockers * magic.rook_magic_values[num] >> magic.rook_shift_values[num];
                            move = cache.White_Rook.rook_look[num, key];
                            break;

                        case 4:  // White Knight
                            move = cache.White_Knight.mask[num];
                            break;

                        case 6:  // White Bishop
                            blockers = cache.White_Bishop.bishop_mask[num] & (Board.all_pieces ^ king);  // Exclude the king from blockers
                            key = blockers * magic.bishop_magic_values[num] >> magic.bishop_shift_values[num];
                            move = cache.Black_Bishop.bishop_look[num, key];
                            break;

                        case 8:  // White Queen (combination of rook and bishop)
                            blockers = cache.White_Rook.rook_mask[num] & (Board.all_pieces ^ king);
                            key = blockers * magic.rook_magic_values[num] >> magic.rook_shift_values[num];
                            move = cache.White_Rook.rook_look[num, key];

                            blockers = cache.White_Bishop.bishop_mask[num] & (Board.all_pieces ^ king);
                            key = blockers * magic.bishop_magic_values[num] >> magic.bishop_shift_values[num];
                            move |= cache.White_Bishop.bishop_look[num, key];
                            break;

                        case 10:  // White King
                            move = cache.White_King.mask[num];
                            break;
                    }

                    // Update the illegal moves bitboard by OR'ing the current move
                    illegal |= move;

                    // If the move puts the king in check, update the check status
                    if ((king & move) != 0)
                    {
                        num_checkers++;  // Increment the number of checkers
                        check_code = new Starts(name, num);  // Store the piece and position of the attacker

                        // If the attacker is a pawn or knight, set the check mask to a specific square
                        if (name == 0 || name == 4)
                        {
                            check_mask = 1UL << num;
                        }
                        else
                        {
                            // For sliding pieces (rooks, bishops, queens), calculate the sliding ray
                            check_mask = sliding_ray(Board, 10 + mod, num);
                        }
                    }

                    // Remove the processed piece from the bitboard
                    board &= board - 1;
                }
            }

            // If the king is in check, update the check status on the board
            if (num_checkers > 0)
            {
                if (mod == 0)
                {
                    Board.white_in_check = true;  // White is in check
                }
                if (mod == 1)
                {
                    Board.black_in_check = true;  // Black is in check
                }

                // If there are multiple checkers, reset the check mask
                if (num_checkers > 1)
                {
                    check_mask = 0;
                }
            }
            else
            {
                // If no checkers, reset the check status
                if (mod == 0)
                {
                    Board.white_in_check = false;  // White is not in check
                }
                if (mod == 1)
                {
                    Board.black_in_check = false;  // Black is not in check
                }
            }

            // Return a new Check object containing the captured squares and check status
            info = new Check(illegal, check_code, num_checkers, check_mask, pawn);
            return info;
        }


        // This method calculates and returns the pin masks for a given king position, identifying pins based on the king's position and other pieces' interactions.
        public ulong[] pin_masks(ulong[] all_bitboards, int king, int mod, ulong all_pieces, ulong colour, PieceCall cache, out bool can_passant)
        {
            ulong[] pins = new ulong[64];  // Array to store pin information for each square

            // Get the right move vector for the rook
            ulong right = cache.White_Rook.right_move[king];

            // Get the orthogonal pieces (opponent's rooks and queens) that block or attack in orthogonal directions
            ulong orthogonal = all_bitboards[8 + mod ^ 1] | all_bitboards[2 + mod ^ 1];

            can_passant = true;  // Initialize en passant to true

            // Check if there is a pin in the right direction (right move for the rook)
            if ((right & orthogonal) != 0)
            {
                ulong attack = right & orthogonal;  // Identify the attack line

                // Get the valid right moves for the rook
                ulong right_moves = right & cache.White_Rook.rook_mask[king];

                // Identify blockers for the rook's right move path
                ulong start_blockers = right_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Rook.rook_mask[king];

                // Calculate the rook's moves based on the blockers
                ulong key = magic.rook_magic_values[king] * blockers >> magic.rook_shift_values[king];
                ulong all_moves = cache.White_Rook.rook_look[king, key];

                // Update the right move vector with valid moves
                right &= all_moves;

                ulong passant;
                ulong blocker = right & (all_pieces ^ attack);  // Check for blocker pieces

                int piece = BitOperations.TrailingZeroCount(blocker);

                // Handle en passant logic for black side
                if (mod == 0 && cache.Global.black == piece + 1)
                {
                    ulong to_king = attack - (1ul << king) ^ 1ul << king;
                    ulong temp = right_moves & (all_pieces ^ attack);
                    temp &= to_king;
                    temp ^= blocker;

                    if (temp == 0)  // If there are no valid squares, en passant is not possible
                    {
                        can_passant = false;
                    }
                }

                // Handle en passant logic for white side
                if (mod == 1 && cache.Global.white == piece + 1)
                {
                    ulong to_king = attack - (1ul << king) ^ 1ul << king;
                    ulong temp = right_moves & (all_pieces ^ attack);
                    temp &= to_king;
                    temp ^= blocker;

                    if (temp == 0)  // If there are no valid squares, en passant is not possible
                    {
                        can_passant = false;
                    }
                }

                // If there is a blocker, check if it is the only piece in the line and set the pin mask
                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        if (mod == 0 && cache.Global.black == piece + 1)
                        {
                            can_passant = false;
                        }
                        if (mod == 1 && cache.Global.white == piece + 1)
                        {
                            can_passant = false;
                        }
                        pins[piece] = ~right;  // Set the pin mask for the piece
                    }
                }
            }

            // Check for pins in the up direction (up move for the rook)
            ulong up = cache.White_Rook.up_move[king];

            if ((up & orthogonal) != 0)
            {
                ulong attack = up & orthogonal;

                ulong up_moves = up & cache.White_Rook.rook_mask[king];

                ulong start_blockers = up_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Rook.rook_mask[king];

                ulong key = magic.rook_magic_values[king] * blockers >> magic.rook_shift_values[king];
                ulong all_moves = cache.White_Rook.rook_look[king, key];

                up &= all_moves;

                ulong blocker = up & (all_pieces ^ attack);

                int piece = BitOperations.TrailingZeroCount(blocker);

                // If there is a blocker and it is the only piece in the line, set the pin mask
                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        pins[piece] = ~up;  // Set the pin mask for the piece
                    }
                }
            }

            // Check for pins in the down direction (down move for the rook)
            ulong down = cache.White_Rook.down_move[king];

            if ((down & orthogonal) != 0)
            {
                ulong attack = down & orthogonal;

                ulong down_moves = down & cache.White_Rook.rook_mask[king];

                ulong start_blockers = down_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Rook.rook_mask[king];

                ulong key = magic.rook_magic_values[king] * blockers >> magic.rook_shift_values[king];
                ulong all_moves = cache.White_Rook.rook_look[king, key];

                down &= all_moves;

                ulong blocker = down & (all_pieces ^ attack);

                int piece = BitOperations.TrailingZeroCount(blocker);

                // If there is a blocker and it is the only piece in the line, set the pin mask
                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        pins[piece] = ~down;  // Set the pin mask for the piece
                    }
                }
            }

            // Check for pins in the left direction (left move for the rook)
            ulong left = cache.White_Rook.left_move[king];

            if ((left & orthogonal) != 0)
            {
                ulong attack = left & orthogonal;

                ulong left_moves = left & cache.White_Rook.rook_mask[king];

                ulong start_blockers = left_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Rook.rook_mask[king];

                ulong key = magic.rook_magic_values[king] * blockers >> magic.rook_shift_values[king];
                ulong all_moves = cache.White_Rook.rook_look[king, key];

                left &= all_moves;

                ulong blocker = left & (all_pieces ^ attack);

                int piece = BitOperations.TrailingZeroCount(blocker);

                // Handle en passant logic for both black and white sides
                if (mod == 0 && cache.Global.black == piece)
                {
                    ulong temp = left_moves & (all_pieces ^ attack ^ blocker);
                    temp &= temp - 1;
                    if (temp == 0)
                    {
                        can_passant = false;
                    }
                }

                if (mod == 1 && cache.Global.white == piece)
                {
                    ulong temp = left_moves & (all_pieces ^ attack ^ blocker);
                    temp &= temp - 1;
                    if (temp == 0)
                    {
                        can_passant = false;
                    }
                }

                // If there is a blocker and it is the only piece in the line, set the pin mask
                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        pins[piece] = ~left;  // Set the pin mask for the piece
                    }
                }
            }

            // Check for diagonal pins (using bishop moves)
            ulong SE = cache.White_Bishop.SE_move[king];
            ulong diagonal = all_bitboards[8 + mod ^ 1] | all_bitboards[6 + mod ^ 1];

            if ((SE & diagonal) != 0)
            {
                ulong attack = SE & diagonal;
                ulong SE_moves = SE & cache.White_Bishop.bishop_mask[king];

                ulong start_blockers = SE_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Bishop.bishop_mask[king];

                ulong key = magic.bishop_magic_values[king] * blockers >> magic.bishop_shift_values[king];
                ulong all_moves = cache.White_Bishop.bishop_look[king, key];

                SE &= all_moves;

                ulong blocker = SE & (all_pieces ^ attack);
                int piece = BitOperations.TrailingZeroCount(blocker);

                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        pins[piece] = ~SE;  // Set the pin mask for the piece
                    }
                }
            }

            // Similar checks for other diagonal directions (SW, NW, NE)
            // These follow the same logic as above for setting the pin masks.
            ulong SW = cache.White_Bishop.SW_move[king];
            diagonal = all_bitboards[8 + mod ^ 1] | all_bitboards[6 + mod ^ 1];

            if ((SW & diagonal) != 0)
            {
                ulong attack = SW & diagonal;
                ulong SW_moves = SW & cache.White_Bishop.bishop_mask[king];

                ulong start_blockers = SW_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Bishop.bishop_mask[king];

                ulong key = magic.bishop_magic_values[king] * blockers >> magic.bishop_shift_values[king];
                ulong all_moves = cache.White_Bishop.bishop_look[king, key];

                SW &= all_moves;

                ulong blocker = SW & (all_pieces ^ attack);
                int piece = BitOperations.TrailingZeroCount(blocker);

                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        pins[piece] = ~SW;  // Set the pin mask for the piece
                    }
                }
            }

            ulong NW = cache.White_Bishop.NW_move[king];
            diagonal = all_bitboards[8 + mod ^ 1] | all_bitboards[6 + mod ^ 1];

            if ((NW & diagonal) != 0)
            {
                ulong attack = NW & diagonal;
                ulong NW_moves = NW & cache.White_Bishop.bishop_mask[king];

                ulong start_blockers = NW_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Bishop.bishop_mask[king];

                ulong key = magic.bishop_magic_values[king] * blockers >> magic.bishop_shift_values[king];
                ulong all_moves = cache.White_Bishop.bishop_look[king, key];

                NW &= all_moves;

                ulong blocker = NW & (all_pieces ^ attack);
                int piece = BitOperations.TrailingZeroCount(blocker);

                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        pins[piece] = ~NW;  // Set the pin mask for the piece
                    }
                }
            }

            ulong NE = cache.White_Bishop.NE_move[king];
            diagonal = all_bitboards[8 + mod ^ 1] | all_bitboards[6 + mod ^ 1];

            if ((NE & diagonal) != 0)
            {
                ulong attack = NE & diagonal;
                ulong NE_moves = NE & cache.White_Bishop.bishop_mask[king];

                ulong start_blockers = NE_moves & (all_pieces ^ colour);
                ulong blockers = (all_pieces ^ start_blockers) & cache.White_Bishop.bishop_mask[king];

                ulong key = magic.bishop_magic_values[king] * blockers >> magic.bishop_shift_values[king];
                ulong all_moves = cache.White_Bishop.bishop_look[king, key];

                NE &= all_moves;

                ulong blocker = NE & (all_pieces ^ attack);
                int piece = BitOperations.TrailingZeroCount(blocker);

                if (blocker != 0)
                {
                    blocker &= blocker - 1;
                    if (blocker == 0)
                    {
                        pins[piece] = ~NE;  // Set the pin mask for the piece
                    }
                }
            }
            return pins;  // Return the pin masks for the current board state
        }

    }
}
