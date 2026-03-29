using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using ChessEngine.Play;

namespace ChessEngine
{
    public sealed class Evaluation : Engine
    {
        Piece piece;
        PieceCall cache;

        private static double[][] placement = new double[12][];
        private static double[][] placement_2 = new double[12][];

        private static readonly ulong[] king_zone_masks = new ulong[64];
        private static readonly ulong[] passed_pawn_mask_white = new ulong[64];
        private static readonly ulong[] passed_pawn_mask_black = new ulong[64];
        private static readonly int[] square_row = new int[64];
        private static readonly ulong[] shield_mask = new ulong[64];
        private static readonly ulong[] file_masks = new ulong[8];
        private static readonly int[] piece_values = new int[12] { 100, -100, 500, -500, 320, -320, 330, -330, 900, -900, 0, 0 };

        static Evaluation()
        {
            for (int sq = 0; sq < 64; sq++)
            {
                int row = sq / 8;
                int col = sq % 8;

                int r_start = Math.Max(0, row - 1);
                int r_end = Math.Min(7, row + 1);
                int c_start = Math.Max(0, col - 1);
                int c_end = Math.Min(7, col + 1);
                ulong mask = 0UL;
                for (int r = r_start; r <= r_end; r++)
                {
                    int start_square = r * 8 + c_start;
                    int count = c_end - c_start + 1;
                    mask |= (1UL << count) - 1 << start_square;
                }
                king_zone_masks[sq] = mask;


            }
            for (int sq = 0; sq < 64; sq++)
            {
                int row = sq / 8;
                int col = sq % 8;
                ulong mask_white = 0UL;
                for (int r = row + 1; r < 8; r++)
                {
                    mask_white |= 1UL << r * 8 + col;
                    if (col > 0)
                        mask_white |= 1UL << r * 8 + col - 1;
                    if (col < 7)
                        mask_white |= 1UL << r * 8 + col + 1;
                }
                passed_pawn_mask_black[sq] = mask_white;

                ulong mask_black = 0UL;
                for (int r = row - 1; r >= 0; r--)
                {
                    mask_black |= 1UL << r * 8 + col;
                    if (col > 0)
                        mask_black |= 1UL << r * 8 + col - 1;
                    if (col < 7)
                        mask_black |= 1UL << r * 8 + col + 1;
                }
                passed_pawn_mask_white[sq] = mask_black;
            }
            for (int sq = 0; sq < 64; sq++)
            {
                square_row[sq] = sq / 8;
            }
            for (int i = 0; i < 64; i++)
            {
                ulong a_file = 0x0101010101010101UL;
                ulong h_file = a_file << 7;
                ulong sq_mask = 1UL << i;
                ulong neighborhood = sq_mask << 8 | sq_mask >> 8 |
                                     sq_mask << 1 | sq_mask >> 1 |
                                     sq_mask << 7 | sq_mask >> 7 |
                                     sq_mask << 9 | sq_mask >> 9;
                neighborhood &= ~a_file | ~h_file;
                shield_mask[i] = neighborhood;
            }
            for (int file = 0; file < 8; file++)
            {
                file_masks[file] = 0x0101010101010101UL << file;
            }
        }

        public Evaluation()
        {
            // White Pawn (index 0)

            placement[0] = new double[64] {
                 0,   0,   0,   0,   0,   0,   0,   0,
                50,  50,  50,  50,  50,  50,  50,  50,
                10,  10,  20,  30,  30,  20,  10,  10,
                 5,   5,  10,  25,  25,  10,   5,   5,
                 0,   0,   0,  20,  20,   0,   0,   0,
                 5,  -5, -10,   0,   0, -10,  -5,   5,
                 5,  10,  10, -20, -20,  10,  10,   5,
                 0,   0,   0,   0,   0,   0,   0,   0
            };
            // Black Pawn (index 1) reversed from white pawn table
            placement[1] = reverse_placement(placement[0]);

            // White Bishop (index 4)
            placement[4] = new double[64] {
               -50, -40, -30, -30, -30, -30, -40, -50,
               -40, -20,   0,   0,   0,   0, -20, -40,
               -30,   0,  10,  15,  15,  10,   0, -30,
               -30,   5,  15,  20,  20,  15,   5, -30,
               -30,   0,  15,  20,  20,  15,   0, -30,
               -30,   5,  10,  15,  15,  10,   5, -30,
               -40, -20,   0,   5,   5,   0, -20, -40,
               -50, -40, -30, -30, -30, -30, -40, -50
            };
            // Black Bishop (index 5) reversed from white bishop table
            placement[5] = reverse_placement(placement[4]);

            // White Rook (index 6)
            placement[6] = new double[64] {
               -20, -10, -10, -10, -10, -10, -10, -20,
               -10,   5,   0,   0,   0,   0,   5, -10,
               -10,  10,  10,  10,  10,  10,  10, -10,
               -10,   0,  10,  10,  10,  10,   0, -10,
               -10,   5,   5,  10,  10,   5,   5, -10,
               -10,   0,   5,  10,  10,   5,   0, -10,
               -10,   0,   0,   0,   0,   0,   0, -10,
               -20, -10, -10, -10, -10, -10, -10, -20
            };
            // Black Rook (index 7) reversed from white rook table
            placement[7] = reverse_placement(placement[6]);

            // White Knight (index 2)
            placement[2] = new double[64] {
                 0,  0,  0,  0,  0,  0,  0,  0,
                 5, 10, 10, 10, 10, 10, 10,  5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                -5,  0,  0,  0,  0,  0,  0, -5,
                 0,  0,  0,  5,  5,  0,  0,  0
            };
            // Black Knight (index 3) reversed from white knight table
            placement[3] = reverse_placement(placement[2]);

            // White Queen (index 8)
            placement[8] = new double[64] {
               -20, -10, -10,  -5,  -5, -10, -10, -20,
               -10,   0,   0,   0,   0,   0,   0, -10,
               -10,   0,   5,   5,   5,   5,   0, -10,
                -5,   0,   5,   5,   5,   5,   0,  -5,
                 0,   0,   5,   5,   5,   5,   0, -10,
               -10,   5,   5,   5,   5,   5,   0, -10,
               -10,   0,   5,   0,   0,   0,   0, -10,
               -20, -10, -10,  -5,  -5, -10, -10, -20
            };
            // Black Queen (index 9) reversed from white queen table
            placement[9] = reverse_placement(placement[8]);

            // White King (index 10)
            placement[10] = new double[64] {
                 20,  30,  10,   0,   0,  10,  30,  20,
                 20,  20,   0,   0,   0,   0,  20,  20,
                -10, -20, -20, -20, -20, -20, -20, -10,
                -20, -30, -30, -40, -40, -30, -30, -20,
                -30, -40, -40, -50, -50, -40, -40, -30,
                -30, -40, -40, -50, -50, -40, -40, -30,
                -30, -40, -40, -50, -50, -40, -40, -30,
                -30, -40, -40, -50, -50, -40, -40, -30
            };
            // Black King (index 11) reversed from white king table
            placement[11] = reverse_placement(placement[10]);

            // Endgame piece-square tables
            placement_2[0] = new double[64] {
                 0, 0, 0, 0, 0, 0, 0, 0,
                50, 50, 50, 50, 50, 50, 50, 50,
                20, 20, 30, 40, 40, 30, 20, 20,
                10, 10, 20, 30, 30, 20, 10, 10,
                 5,  5, 10, 20, 20, 10,  5,  5,
                 0,  0,  0, 10, 10,  0,  0,  0,
                 0,  0,  0,  0,  0,  0,  0,  0,
                 0,  0,  0,  0,  0,  0,  0,  0
            };
            placement_2[1] = reverse_placement(placement_2[0]);
            placement_2[2] = (double[])placement[2].Clone();
            placement_2[3] = reverse_placement(placement_2[2]);
            placement_2[4] = (double[])placement[4].Clone();
            placement_2[5] = reverse_placement(placement_2[4]);
            placement_2[6] = (double[])placement[6].Clone();
            placement_2[7] = reverse_placement(placement_2[6]);
            placement_2[8] = (double[])placement[8].Clone();
            placement_2[9] = reverse_placement(placement_2[8]);
            placement_2[10] = new double[64] {
               -50, -30, -30, -30, -30, -30, -30, -50,
               -30, -30,   0,   0,   0,   0, -30, -30,
               -30, -10,  20,  30,  30,  20, -10, -30,
               -30, -10,  30,  40,  40,  30, -10, -30,
               -30, -10,  30,  40,  40,  30, -10, -30,
               -30, -10,  20,  30,  30,  20, -10, -30,
               -30, -20, -10,   0,   0, -10, -20, -30,
               -50, -40, -30, -20, -20, -30, -40, -50
            };
            placement_2[11] = reverse_placement(placement_2[10]);

            piece = new Piece();
            cache = piece.get_cache();

        }

        private static double[] reverse_placement(double[] arr)
        {
            double[] reversed = new double[64];
            for (int r = 0; r < 8; r++)
            {
                for (int f = 0; f < 8; f++)
                {
                    reversed[r * 8 + f] = arr[(7 - r) * 8 + f];
                }
            }
            return reversed;
        }

        public double evaluation(Board board)
        {
            // Initial evaluation score set to 0
            double eval = 0;

            // Iterate over all 12 piece types (6 for white and 6 for black)
            for (int i = 0; i < 12; i++)
            {
                // Determine the sign for white (+1) or black (-1)
                int sign = i % 2 == 0 ? 1 : -1;
                int colour = i % 2; // 0 for white, 1 for black
                ulong bitboard = board.bitboards[i]; // Get the bitboard for the current piece type

                // Loop through all the pieces for the current type (using the bitboard)
                while (bitboard != 0)
                {
                    // Find the index of the least significant bit (LSB), i.e., the position of the piece on the board
                    int sq = BitOperations.TrailingZeroCount(bitboard);

                    // Clear the LSB bit (removes the found piece from the bitboard)
                    bitboard &= bitboard - 1;

                    // Get the piece's placement value for the current position in middle game (mg) and end game (eg)
                    double mg = placement[i][sq];
                    double eg = placement_2[i][sq];

                    // Calculate the piece value + weighted placement values based on the phase of the game
                    eval += piece_values[i] + (mg + (eg - mg) * board.end_game_weights[colour]) * sign;
                }
            }

            // Active squares for evaluation (the bitboard is hardcoded here)
            ulong active = 139081753165824UL;

            // Evaluate the activity of white and black pieces based on the active squares
            int white_activity = BitOperations.PopCount((board.bitboards[4] | board.bitboards[6]) & active) * 50;
            int black_activity = -BitOperations.PopCount((board.bitboards[5] | board.bitboards[7]) & active) * 50;
            eval += white_activity + black_activity;

            // Evaluate the protection of pawns for both white and black
            int white_protection = pawn_shield(board.bitboards[10], board.bitboards[0]) * 50;
            int black_protection = -pawn_shield(board.bitboards[11], board.bitboards[1]) * 50;
            eval += white_protection + black_protection;

            // Evaluate any passed pawns (pawns that can potentially be promoted)
            eval += evaluate_passed_pawns(board);

            // If the game is in the endgame phase (either white or black has high end_game_weights), evaluate the kings' safety
            double w_end = board.end_game_weights[0];
            double b_end = board.end_game_weights[1];
            if (w_end > 0.75 || b_end > 0.75)
            {
                // Find the positions of the white and black kings
                int white_king_sq = BitOperations.TrailingZeroCount(board.bitboards[10]);
                int row_1 = white_king_sq / 8;
                int col_1 = white_king_sq % 8;
                int black_king_sq = BitOperations.TrailingZeroCount(board.bitboards[11]);
                int row_2 = black_king_sq / 8;
                int col_2 = black_king_sq % 8;

                // Calculate king safety (distance from the edge of the board)
                int white_king_safety = Math.Min(Math.Min(row_1, 7 - row_1), Math.Min(col_1, 7 - col_1));
                int black_king_safety = Math.Min(Math.Min(row_2, 7 - row_2), Math.Min(col_2, 7 - col_2));

                // Calculate distance between the white and black kings
                double distance = calculate_king_distance(row_1, row_2, col_1, col_2);

                // If the evaluation is positive (white is winning), give a bonus for king distance and safety
                if (eval > 5)
                {
                    eval += (7 - distance) * 80; // Bonus for distance between kings
                    eval += (7 - black_king_safety) * 80; // Bonus for black king safety
                }
                // If the evaluation is negative (black is winning), penalize based on king distance and safety
                else if (eval < -5)
                {
                    eval -= (7 - distance) * 80; // Penalty for distance between kings
                    eval -= (7 - white_king_safety) * 80; // Penalty for white king safety
                }
                return eval; // Return early if the game is in the endgame phase
            }

            // Evaluate king safety in the middlegame for both sides
            int white_king_penalty = evaluate_king_safety(board, 0); // For white (side 0)
            int black_king_penalty = evaluate_king_safety(board, 1); // For black (side 1)
            eval -= white_king_penalty; // Subtract the white king penalty
            eval += black_king_penalty; // Add the black king penalty

            return eval; // Final evaluation score
        }


        private static double calculate_king_distance(int row_1, int row_2, int col_1, int col_2)
        {
            int dx = Math.Abs(row_1 - row_2);
            int dy = Math.Abs(col_1 - col_2);
            return Math.Max(dx, dy);
        }

        private static double doubled_pawns(ulong pawns)
        {
            ulong a_file = 0x0101010101010101UL;
            int score = 0;
            for (int i = 0; i < 8; i++)
            {
                ulong file = a_file << i;
                ulong pawn_mask = pawns & file;
                if ((pawn_mask & pawn_mask - 1) != 0)
                    score -= 30;
            }
            return score;
        }

        private static int pawn_shield(ulong king_position, ulong pawns)
        {
            int king = BitOperations.TrailingZeroCount(king_position);
            ulong shield = shield_mask[king] & pawns;
            int num_protectors = BitOperations.PopCount(shield);
            return num_protectors;
        }

        private static int evaluate_king_safety(Board board, int mod)
        {
            int king_index = 10 + mod;
            int king_square = BitOperations.TrailingZeroCount(board.bitboards[king_index]);
            bool castled = board.is_castle[mod];
            ulong zone = king_zone_masks[king_square];
            ulong friendly_pawns = board.bitboards[0 + mod];
            int pawn_shield_count = BitOperations.PopCount(zone & friendly_pawns);
            int penalty = pawn_shield_count < 2 ? (2 - pawn_shield_count) * 6 : 0;
            int inv = mod ^ 1;
            ulong enemy_pieces = board.bitboards[0 + inv] | board.bitboards[2 + inv] | board.bitboards[4 + inv] |
                                  board.bitboards[6 + inv] | board.bitboards[8 + inv];
            int enemy_count = BitOperations.PopCount(zone & enemy_pieces);
            penalty += enemy_count * 8;
            if (!castled)
                penalty += 80;
            int row = king_square / 8;
            int col = king_square % 8;
            int dist_from_edge = Math.Min(Math.Min(row, 7 - row), Math.Min(col, 7 - col));
            if (dist_from_edge > 2)
                penalty += (dist_from_edge - 2) * 6;
            return penalty;
        }

        private static int evaluate_passed_pawns(Board board)
        {
            int bonus = 0;
            ulong white_pawns = board.bitboards[0];
            while (white_pawns != 0)
            {
                int sq = BitOperations.TrailingZeroCount(white_pawns);
                white_pawns &= white_pawns - 1;
                if ((passed_pawn_mask_white[sq] & board.bitboards[1]) == 0)
                {
                    int row = sq / 8;
                    bonus += (7 - row) * 10;
                }
            }
            ulong black_pawns = board.bitboards[1];
            while (black_pawns != 0)
            {
                int sq = BitOperations.TrailingZeroCount(black_pawns);
                black_pawns &= black_pawns - 1;
                if ((passed_pawn_mask_black[sq] & board.bitboards[0]) == 0)
                {
                    int row = sq / 8;
                    bonus -= row * 10;
                }
            }
            return bonus;
        }

        private static int evaluate_pawn_advancement(Board board)
        {
            int bonus = 0;
            const int pawn_push_bonus = 50;
            ulong white_pawns = board.bitboards[0];
            while (white_pawns != 0)
            {
                int sq = BitOperations.TrailingZeroCount(white_pawns);
                white_pawns &= white_pawns - 1;
                bonus += (7 - square_row[sq]) * pawn_push_bonus;
            }
            ulong black_pawns = board.bitboards[1];
            while (black_pawns != 0)
            {
                int sq = BitOperations.TrailingZeroCount(black_pawns);
                black_pawns &= black_pawns - 1;
                bonus -= square_row[sq] * pawn_push_bonus;
            }
            return bonus;
        }

        private static int count_white_pieces(Board board)
        {
            return BitOperations.PopCount(board.bitboards[0])
                 + BitOperations.PopCount(board.bitboards[2])
                 + BitOperations.PopCount(board.bitboards[4])
                 + BitOperations.PopCount(board.bitboards[6])
                 + BitOperations.PopCount(board.bitboards[8]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int count_black_pieces(Board board)
        {
            return BitOperations.PopCount(board.bitboards[1])
                 + BitOperations.PopCount(board.bitboards[3])
                 + BitOperations.PopCount(board.bitboards[5])
                 + BitOperations.PopCount(board.bitboards[7])
                 + BitOperations.PopCount(board.bitboards[9]);
        }

        private static double apply_trade_bonus(Board board, double eval)
        {
            const int baseline = 10;
            const int multiplier = 3;
            if (eval < 0)
            {
                int white_count = count_white_pieces(board);
                int extra_pieces = Math.Max(0, white_count - baseline);
                eval += extra_pieces * multiplier;
            }
            else if (eval > 0)
            {
                int black_count = count_black_pieces(board);
                int extra_pieces = Math.Max(0, black_count - baseline);
                eval -= extra_pieces * multiplier;
            }
            return eval;
        }

        private static int evaluate_rook_open_files(Board board)
        {
            int bonus = 0;
            int open_file_bonus = 15;
            int semi_open_file_bonus = 10;

            ulong white_rook_bb = board.bitboards[2];
            while (white_rook_bb != 0)
            {
                int sq = BitOperations.TrailingZeroCount(white_rook_bb);
                white_rook_bb &= white_rook_bb - 1;
                int file = sq % 8;
                ulong file_mask = file_masks[file];
                bool has_white_pawn = (board.bitboards[0] & file_mask) != 0;
                bool has_black_pawn = (board.bitboards[1] & file_mask) != 0;
                if (!has_white_pawn && !has_black_pawn)
                    bonus += open_file_bonus;
                else if (!has_white_pawn && has_black_pawn)
                    bonus += semi_open_file_bonus;
            }

            ulong black_rook_bb = board.bitboards[3];
            while (black_rook_bb != 0)
            {
                int sq = BitOperations.TrailingZeroCount(black_rook_bb);
                black_rook_bb &= black_rook_bb - 1;
                int file = sq % 8;
                ulong file_mask = file_masks[file];
                bool has_black_pawn = (board.bitboards[1] & file_mask) != 0;
                bool has_white_pawn = (board.bitboards[0] & file_mask) != 0;
                if (!has_black_pawn && !has_white_pawn)
                    bonus -= open_file_bonus;
                else if (!has_black_pawn && has_white_pawn)
                    bonus -= semi_open_file_bonus;
            }
            return bonus;
        }

        private static int evaluate_central_control(Board board)
        {
            ulong central_mask = 0x00003C3C3C3C0000UL;
            int white_central = BitOperations.PopCount(board.bitboards[0] & central_mask)
                             + BitOperations.PopCount(board.bitboards[2] & central_mask)
                             + BitOperations.PopCount(board.bitboards[4] & central_mask)
                             + BitOperations.PopCount(board.bitboards[6] & central_mask)
                             + BitOperations.PopCount(board.bitboards[8] & central_mask);
            int black_central = BitOperations.PopCount(board.bitboards[1] & central_mask)
                             + BitOperations.PopCount(board.bitboards[3] & central_mask)
                             + BitOperations.PopCount(board.bitboards[5] & central_mask)
                             + BitOperations.PopCount(board.bitboards[7] & central_mask)
                             + BitOperations.PopCount(board.bitboards[9] & central_mask);
            const int central_weight = 2;
            return (white_central - black_central) * central_weight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int evaluate_space_advantage(Board board)
        {
            int white_space = 0, black_space = 0;
            ulong white_pawns = board.bitboards[0];
            while (white_pawns != 0)
            {
                int sq = BitOperations.TrailingZeroCount(white_pawns);
                white_pawns &= white_pawns - 1;
                int row = sq / 8;
                if (row >= 4)
                    white_space++;
            }
            ulong black_pawns = board.bitboards[1];
            while (black_pawns != 0)
            {
                int sq = BitOperations.TrailingZeroCount(black_pawns);
                black_pawns &= black_pawns - 1;
                int row = sq / 8;
                if (row <= 3)
                    black_space++;
            }
            const int space_weight = 5;
            return (white_space - black_space) * space_weight;
        }

        private int antiPerpetualBonus(Board board, int mod)
        {
            // Use mod==0 for white (king is stored in bitboards[10]) and mod==1 for black (bitboards[11]).
            int kingSquare = BitOperations.TrailingZeroCount(board.bitboards[10 + mod]);


            // If the king is in check (using the board's in_check method), return 0 bonus.
            if (board.in_check(mod, cache))
                return 0;

            // Use the precomputed king zone to determine the free squares around the king.
            ulong zone = king_zone_masks[kingSquare];
            // Free squares: those not occupied by any piece.
            ulong freeSquares = zone & ~board.all_pieces;
            int mobility = BitOperations.PopCount(freeSquares);

            // Return a bonus; adjust the multiplier (here 10) as needed.
            return mobility * 10;
        }
    }
}
