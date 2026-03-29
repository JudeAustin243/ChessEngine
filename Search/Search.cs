using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

using ChessEngine.Play;
using Microsoft.VisualBasic;

namespace ChessEngine
{
    public sealed class Search : Engine
    {
        private readonly int colour;  // The player's colour (0 for white, 1 for black)
        private readonly int R = 3;  // Constant value for pruning
        private int iteration = 0;  // Counter for iterations
        private MoveGeneration move_generate;  // Handles move generation
        private Evaluation evaluation;  // Evaluates positions
        private Stopwatch timer;  // Used to track the time spent on searches
        Random random;  // Random number generator for randomness in move selection

        // Transposition Table entry types and table
        private enum TTFlag { Exact, LowerBound, UpperBound }  // Flags for TT table entries
        private class TTEntry
        {
            public ulong Key;  // Position hash key
            public int Depth;  // Depth of the search for this entry
            public double Value;  // Evaluated value for this position
            public TTFlag Flag;  // Flag indicating the type of evaluation (Exact, LowerBound, UpperBound)
            public bool ismate;  // Whether this position is a checkmate
            public Move BestMove;  // Best move for this position
        }
        private const int TTSize = 1 << 22;  // Size of the Transposition Table

        private TTEntry[] TTTable = new TTEntry[TTSize];  // Transposition Table

        // Killer moves storage (two per ply)
        private const int MAX_DEPTH = 128;  // Maximum search depth
        private Move[,] killer_moves = new Move[MAX_DEPTH, 2];  // Stores killer moves for each depth

        // History heuristic table: indexed by [side, fromSquare, toSquare]
        private int[,,] history_table = new int[2, 64, 64];  // Used for move ordering

        public Search(int colour)
        {
            this.colour = colour;
            move_generate = new MoveGeneration();  // Initialize move generation
            evaluation = new Evaluation();  // Initialize evaluation function
            random = new Random();  // Initialize random number generator
            timer = new Stopwatch();  // Initialize stopwatch
        }

        private MoveComparer comparer = new MoveComparer();
        private class MoveComparer : IComparer<Move>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(Move a, Move b)
            {
                return b.captureval.CompareTo(a.captureval);  // Compare moves based on capture value
            }
        }

        //Aggressive Inlining for small methods with high call countsS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int is_capture(Board board, PieceCall cache, Move move, int inv, ulong pawn_mask, ulong capture)
        {
            int[] value = { 1, 1, 5, 5, 3, 3, 4, 4, 9, 9, 0, 0 };  // Piece values for different types
            ulong end = 1ul << move.end;  // End square of the move
            int score = 0;
            if ((end & board.all_pieces) != 0)  // If the move captures a piece
            {
                ulong attacker = cache.Illegal.illegal;

                int i = inv;
                for (; (end & board.bitboards[i]) == 0;)  // Check for the victim piece
                    i += 2;

                int victim_value = value[i];  // Value of the captured piece
                int attacker_value = value[move.piece];  // Value of the attacking piece
                int net_gain = victim_value - attacker_value;
                if ((end & attacker) == 0)  // If the attacker is not in check
                {
                    if (victim_value > attacker_value)
                    {
                        score = 100000000 + net_gain;  // Large gain if capturing a more valuable piece
                    }
                    else
                    {
                        score = 1000000 + net_gain;  // Smaller gain for capturing a weaker piece
                    }
                }
                else
                {
                    if (victim_value > attacker_value)
                    {
                        score = 100000 + net_gain;  // Smaller gain if the attacker is in check
                    }
                    else
                    {
                        score = 100 + net_gain;  // Even smaller gain for weaker capture
                    }
                }
            }
            return score;  // Return the capture score
        }

        // Method to order moves based on capture value and other heuristics
        private Move[] move_ordering(
     Board board,
     PieceCall cache,
     Move[] moves,
     Move best_move,
     int max_depth,
     int depth,
     int move_count,
     int inv,
     ulong pawn_mask,
     ulong capture
 )
        {
            Move[] ordered = new Move[move_count];

            // Assign capture values and bonuses to each move
            for (int i = 0; i < move_count; i++)
            {
                moves[i].captureval = is_capture(board, cache, moves[i], inv, pawn_mask, capture);

                // Add killer move and history bonuses for quiet moves
                if (moves[i].captureval == 0)
                {
                    if (moves[i].start == killer_moves[depth, 0].start && moves[i].end == killer_moves[depth, 0].end)
                    {
                        moves[i].captureval += 90000;  // Killer move bonus
                    }
                    else if (moves[i].start == killer_moves[depth, 1].start && moves[i].end == killer_moves[depth, 1].end)
                    {
                        moves[i].captureval += 80000;  // Killer move bonus
                    }

                    // Add history table bonus
                    moves[i].captureval += history_table[inv ^ 1, moves[i].start, moves[i].end];
                    moves[i].iscapture = true;
                }

                // Add bonus for best move in the final depth
                if (depth == max_depth && max_depth > 1)
                {
                    if (moves[i].start == best_move.start && moves[i].end == best_move.end)
                    {
                        moves[i].captureval += 100000000;
                    }
                }

                ordered[i] = moves[i];
            }

            // Use merge sort instead of Array.Sort
            Array.Sort(ordered, 0, move_count, comparer);

            return ordered;
        }

        private static void merge_sort(Move[] array, int left, int right, IComparer<Move> comparer)
        {
            if (left < right)
            {
                int mid = left + (right - left) / 2;
                merge_sort(array, left, mid, comparer);
                merge_sort(array, mid + 1, right, comparer);
                merge(array, left, mid, right, comparer);
            }
        }

        private static void merge(Move[] array, int left, int mid, int right, IComparer<Move> comparer)
        {
            int n1 = mid - left + 1;
            int n2 = right - mid;

            Move[] left_array = new Move[n1];
            Move[] right_array = new Move[n2];

            // Copy data to temporary arrays
            for (int i = 0; i < n1; i++)
            {
                left_array[i] = array[left + i];
            }
            for (int j = 0; j < n2; j++)
            {
                right_array[j] = array[mid + 1 + j];
            }

            int iIndex = 0, jIndex = 0, k = left;

            // Merge the temporary arrays back into the original array
            while (iIndex < n1 && jIndex < n2)
            {
                if (comparer.Compare(left_array[iIndex], right_array[jIndex]) <= 0)
                {
                    array[k++] = left_array[iIndex++];
                }
                else
                {
                    array[k++] = right_array[jIndex++];
                }
            }

            // Copy any remaining elements of leftArray, if any
            while (iIndex < n1)
            {
                array[k++] = left_array[iIndex++];
            }

            // Copy any remaining elements of rightArray, if any
            while (jIndex < n2)
            {
                array[k++] = right_array[jIndex++];
            }
        }


        // Decay history table after each search to reduce old history impact
        private void decay_history_table()
        {
            for (int side = 0; side < 2; side++)
            {
                for (int from = 0; from < 64; from++)
                {
                    for (int to = 0; to < 64; to++)
                    {
                        history_table[side, from, to] /= 2;  // Decay the history values
                    }
                }
            }
        }

        // Minimax algorithm with alpha-beta pruning
        private Eval minimax(
            Board board,
            PieceCall cache,
            Move root_move,
            int depth,
            int mod,
            double alpha,
            double beta,
            Move best_move,
            int max_depth,
            double duration
        )
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool IsTimeExceeded() => timer.ElapsedMilliseconds > duration;  // Check if time limit is exceeded

            if (IsTimeExceeded())  // If time exceeded, return current evaluation
            {
                return new Eval(best_move, 0, false);
            }

            // Draw detection (repetition of position)
            if (board.repeat.Contains(board.all_pieces) && depth < max_depth)
            {
                return new Eval(root_move, 0, false);
            }

            if (depth == 0)  // If depth is zero, perform quiescence search
            {
                double evals = quiescence_search(board, cache, alpha, beta, mod, duration, best_move, max_depth);
                Eval eval = new Eval(root_move, evals, false);
                iteration++;
                return eval;
            }

            ulong tt_key = board.get_key(mod, cache);  // Get the hash key for the position
            int tt_index = (int)(tt_key & (ulong)TTTable.Length - 1);  // Get the index in the TT table
            TTEntry tt_entry = TTTable[tt_index];
            double original_alpha = alpha;

            // Check if the current position is in the transposition table
            if (tt_entry != null && tt_entry.Key == tt_key && tt_entry.Depth >= depth && root_move.piece != -1)
            {
                if (tt_entry.Flag == TTFlag.Exact)
                    return new Eval(root_move, tt_entry.Value, tt_entry.ismate);  // Exact match in TT
                if (tt_entry.Flag == TTFlag.LowerBound && tt_entry.Value >= beta)
                    return new Eval(root_move, tt_entry.Value, tt_entry.ismate);  // Lower bound pruning
                if (tt_entry.Flag == TTFlag.UpperBound && tt_entry.Value <= alpha)
                    return new Eval(root_move, tt_entry.Value, tt_entry.ismate);  // Upper bound pruning

                // Update alpha or beta if needed
                if (tt_entry.Flag == TTFlag.LowerBound)
                    alpha = Math.Max(alpha, tt_entry.Value);
                else if (tt_entry.Flag == TTFlag.UpperBound)
                    beta = Math.Min(beta, tt_entry.Value);
            }

            // Generate all possible moves
            Movecheck premoves = move_generate.all_moves(board, cache, mod, out int move_count, out ulong pawn_mask, out ulong capture_mask);

            if (move_count == 0)  // If no moves, check for checkmate or stalemate
            {
                if (premoves.check > 0)
                {
                    int sign = 1 - 2 * (mod % 2);
                    double mate_val = -100000000 * sign * depth;  // Mate evaluation
                    Eval mate_eval = new Eval(root_move, mate_val, true);
                    iteration++;
                    return mate_eval;
                }
                return new Eval(root_move, 0, false);  // Stalemate or draw
            }

            bool is_check_node = premoves.check > 0;  // Check if the position is a check

            // Razoring: prune shallow positions that are unlikely to affect the outcome
            const int RAZOR_DEPTH = 2;
            const double RAZOR_MARGIN = 400;
            if (!is_check_node && depth <= RAZOR_DEPTH)
            {
                double static_eval = evaluation.evaluation(board);
                if (static_eval + RAZOR_MARGIN <= alpha)
                {
                    double q_val = quiescence_search(board, cache, alpha, beta, mod, duration, best_move, max_depth);
                    return new Eval(root_move, q_val, false);
                }
            }

            // Null move pruning: prune branches where the opponent is unlikely to benefit
            const int R = 2;
            if (!is_check_node && depth > R + 1)
            {
                int null_mod = 1 - mod;
                Eval null_eval = minimax(board, cache, root_move, depth - 1 - R, null_mod, alpha, beta, best_move, max_depth, duration);
                if (mod == 0 && null_eval.eval >= beta || mod == 1 && null_eval.eval <= alpha)
                {
                    return null_eval;
                }
            }

            // Futility pruning: avoid exploring moves that will not significantly change the position
            const int FUTILITY_DEPTH = 2;
            if (depth <= FUTILITY_DEPTH)
            {
                double static_eval = evaluation.evaluation(board);

                double max_possible_gain = 900;
                if (mod == 0)
                {
                    if (static_eval + max_possible_gain < alpha)
                    {
                        return new Eval(root_move, static_eval, false);
                    }
                }
                else
                {
                    if (static_eval - max_possible_gain > beta)
                    {
                        return new Eval(root_move, static_eval, false);
                    }
                }
            }

            int next_mod = 1 - mod;

            // Order moves based on various heuristics
            Move[] ordered_moves = move_ordering(
                board,
                cache,
                premoves.moves,
                best_move,
                max_depth,
                depth,
                move_count,
                mod ^ 1,  // Flip side for history lookup
                pawn_mask,
                capture_mask
            );

            Eval final_eval = mod == 0
                ? new Eval(new Move(-1, -1, -1, -1, -1, false), double.NegativeInfinity, false)  // Worst case for white
                : new Eval(new Move(-1, -1, -1, -1, -1, false), double.PositiveInfinity, false);  // Worst case for black

            Move best_so_far_move = best_move;

            const int LMR_DEPTH = 2;  // Late move reduction depth
            const int LMR_MOVE_THRESHOLD = 3;  // Threshold for late move reduction
            int REDUCE = 1;

            // Explore ordered moves and apply pruning techniques
            for (int i = 0; i < ordered_moves.Length; i++)
            {
                int extended_depth = depth;
                if (premoves.check > 0 && depth < max_depth && !final_eval.ismate)
                {
                    extended_depth += 1;
                }
                extended_depth = Math.Min(extended_depth, max_depth);

                if (depth == max_depth)
                {
                    root_move = ordered_moves[i];
                }

                Global store = cache.Global;
                int update_information = board.update(ordered_moves[i], cache, next_mod);  // Make the move

                bool is_check = board.in_check(next_mod, cache);  // Check if the move results in check
                bool is_capture = ordered_moves[i].captureval > 0;  // Check if it's a capture move

                bool can_reduce =
                    extended_depth >= LMR_DEPTH &&
                    i >= LMR_MOVE_THRESHOLD &&
                    !is_check_node &&
                    !is_capture &&
                    !is_check &&
                    cache.Illegal.check == 0;

                Eval eval;

                // Late move reduction if applicable
                if (can_reduce)
                {
                    if (depth > 2)
                    {
                        if (i > 6)
                        {
                            REDUCE = 2;  // Increase reduction depth
                        }
                    }

                    int reduced_depth = extended_depth - REDUCE;
                    Eval reduced_eval = minimax(board, cache, root_move, reduced_depth - 1, next_mod, alpha, beta, best_so_far_move, max_depth, duration);
                    bool should_full_search =
                        mod == 0 && reduced_eval.eval > alpha ||
                        mod == 1 && reduced_eval.eval < beta;
                    if (should_full_search)
                    {
                        eval = minimax(board, cache, root_move, extended_depth - 1, next_mod, alpha, beta, best_so_far_move, max_depth, duration);
                    }
                    else
                    {
                        eval = reduced_eval;
                    }
                }
                else
                {
                    eval = minimax(board, cache, root_move, extended_depth - 1, next_mod, alpha, beta, best_so_far_move, max_depth, duration);
                }

                board.restore(update_information, ordered_moves[i]);  // Restore the board after the move
                cache.Global = store;  // Restore cache state

                // Alpha-beta update
                if (mod == 0)
                {
                    if (eval.eval > final_eval.eval)
                    {
                        final_eval = eval;
                        if (depth == max_depth)
                        {
                            best_so_far_move = eval.move;
                        }
                    }
                    alpha = Math.Max(alpha, eval.eval);
                }
                else
                {
                    if (eval.eval < final_eval.eval)
                    {
                        final_eval = eval;
                        if (depth == max_depth)
                        {
                            best_so_far_move = eval.move;
                        }
                    }
                    beta = Math.Min(beta, eval.eval);
                }

                if (IsTimeExceeded())
                {
                    return new Eval(best_so_far_move, final_eval.eval, final_eval.ismate);  // Return if time limit is exceeded
                }

                if (beta <= alpha)
                {
                    if (!is_capture)
                    {
                        killer_moves[depth, 1] = killer_moves[depth, 0];  // Update killer moves
                        killer_moves[depth, 0] = ordered_moves[i];
                    }
                    if (ordered_moves[i].iscapture == true)
                    {
                        history_table[mod, ordered_moves[i].start, ordered_moves[i].end] += depth * depth;  // Update history table for captures
                    }
                    break;
                }
            }

            if (IsTimeExceeded())
            {
                return new Eval(best_so_far_move, final_eval.eval, final_eval.ismate);  // Return if time limit is exceeded
            }

            if (best_so_far_move.piece != -1 && final_eval.move.piece != -1)
            {
                TTFlag store_flag;
                if (final_eval.eval <= original_alpha)
                    store_flag = TTFlag.UpperBound;  // Store in TT with UpperBound flag
                else if (final_eval.eval >= beta)
                    store_flag = TTFlag.LowerBound;  // Store in TT with LowerBound flag
                else
                    store_flag = TTFlag.Exact;  // Store in TT with Exact flag

                TTEntry newEntry = new TTEntry()
                {
                    Key = tt_key,
                    Depth = depth,
                    Value = final_eval.eval,
                    Flag = store_flag,
                    ismate = final_eval.ismate,
                    BestMove = final_eval.move,
                };

                if (TTTable[tt_index] != null)
                {
                    if (TTTable[tt_index].Depth <= depth)
                    {
                        TTTable[tt_index] = newEntry;  // Update TT entry if the new depth is greater
                    }
                }
                else
                {
                    TTTable[tt_index] = newEntry;  // Insert new TT entry
                }
            }

            return final_eval;  // Return the final evaluation
        }

        // Quiescence search for volatile positions like captures or checks
        private double quiescence_search(Board board, PieceCall cache, double alpha, double beta, int mod, double duration, Move best_move, int max_depth)
        {
            double stand_pat = evaluation.evaluation(board);  // Static evaluation of the position
            if (mod == 0)
            {
                if (stand_pat >= beta)
                    return beta;  // Prune if the evaluation is worse than beta
                if (stand_pat > alpha)
                    alpha = stand_pat;  // Update alpha if the evaluation is better
            }
            else
            {
                if (stand_pat <= alpha)
                    return alpha;  // Prune if the evaluation is worse than alpha
                if (stand_pat < beta)
                    beta = stand_pat;  // Update beta if the evaluation is better
            }

            // Generate capture moves (moves that involve a capture)
            Movecheck pseudo_moves = move_generate.capture_moves(board, cache, mod, out int move_count, out ulong pawn_mask, out ulong capture);
            if (move_count == 0)
            {
                return stand_pat;  // Return the static evaluation if no capture moves are available
            }

            // Order the capture moves
            Move[] moves = move_ordering(board, cache, pseudo_moves.moves, best_move, max_depth, 0, move_count, mod ^ 1, pawn_mask, capture);
            int num = move_count;

            if (mod == 0)
            {
                double eval = stand_pat;
                // Loop through the ordered capture moves and evaluate them
                for (int i = 0; i < num; i++)
                {
                    Move currentMove = moves[i];
                    Global store = cache.Global;
                    int update_information = board.update(currentMove, cache, mod ^ 1);  // Apply the move
                    double score = quiescence_search(board, cache, alpha, beta, mod ^ 1, duration, best_move, max_depth);  // Recurse for further captures
                    board.restore(update_information, currentMove);  // Restore the board state
                    cache.Global = store;

                    if (score > eval)
                    {
                        eval = score;  // Update evaluation if the new score is better
                    }

                    if (eval > alpha)
                    {
                        alpha = eval;  // Update alpha if the evaluation is better than the previous alpha
                    }

                    if (alpha >= beta)  // Prune if the evaluation exceeds beta
                        return beta;
                }
                return eval;  // Return the best evaluation found
            }
            else
            {
                double eval = stand_pat;
                // Loop through the ordered capture moves and evaluate them
                for (int i = 0; i < num; i++)
                {
                    Move currentMove = moves[i];
                    Global store = cache.Global;
                    int update_information = board.update(currentMove, cache, mod ^ 1);  // Apply the move
                    double score = quiescence_search(board, cache, alpha, beta, mod ^ 1, duration, best_move, max_depth);  // Recurse for further captures
                    board.restore(update_information, currentMove);  // Restore the board state
                    cache.Global = store;

                    if (score < eval)
                    {
                        eval = score;  // Update evaluation if the new score is worse
                    }

                    if (eval < beta)
                    {
                        beta = eval;  // Update beta if the evaluation is worse than the previous beta
                    }

                    if (alpha >= beta)  // Prune if the evaluation exceeds alpha
                        return alpha;
                }
                return eval;  // Return the worst evaluation found
            }
        }

        // Iterative deepening search method
        public Move iterative_deepening(Board board, PieceCall cache, int ELO, int duration, out string evaluation)
        {
            // Decay history table at the start of the search
            decay_history_table();

            int max_depth = ELO_handler(ELO);  // Set the maximum search depth based on ELO rating
            double eval = 0;
            evaluation = "0";

            // Check if the ELO rating is high enough for an opening book move
            if (ELO > 800)
            {
                Openings book = new Openings();
                List<Move> moves = book.find_best_moves(board.get_key(colour, cache), board, cache, colour);  // Get best opening moves
                int count = moves.Count();
                if (count > 0)
                {
                    Random random = new Random();
                    int choice = random.Next(count);  // Choose a random move from the list
                    if (moves[choice].piece != -1)
                    {
                        return moves[choice];  // Return the best opening move
                    }
                }
            }

            // Initialize the best move and start timing
            Move best_move = new Move(-1, -1, -1, -1, -1, false);
            timer.Restart();
            double guess = 0;
            double window = 100;
            int depth = 1;
            timer.Start();

            // Perform iterative deepening until time is exceeded or max depth is reached
            while (timer.ElapsedMilliseconds < duration && depth < 127)
            {
                double alpha = guess - window;
                double beta = guess + window;
                Eval result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Perform minimax search
                eval = result.eval;
                if (eval <= alpha)
                {
                    alpha = double.NegativeInfinity;
                    result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Retry with new alpha
                }
                else if (eval >= beta)
                {
                    beta = double.PositiveInfinity;
                    result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Retry with new beta
                }
                best_move = result.move.piece != -1 ? result.move : best_move;  // Update best move
                guess = result.eval;
                iteration = 0;
                depth++;  // Increase search depth for the next iteration
                if (depth > max_depth)
                {
                    break;
                }
            }

            eval /= 10000;  // Normalize the evaluation value
            if (Math.Abs(eval) < 1000)
            {
                evaluation = Convert.ToString(eval);  // Return evaluation as a string
            }
            else if (eval > 1000)
            {
                evaluation = "Mate for white";  // Checkmate for white
            }
            else if (eval < -1000)
            {
                evaluation = "Mate for black";  // Checkmate for black
            }
            Console.WriteLine("V1: " + depth);  // Print final search depth
            timer.Stop();
            return best_move;  // Return the best move found
        }

        // Test method for iterative deepening
        public void iterative_test(Board board, PieceCall cache)
        {
            double duration = 1000000000;  // Set an extremely long time limit for testing
            Move best_move = new Move(-1, -1, -1, -1, -1, false);
            timer.Restart();
            timer.Start();
            double guess = 0;
            double window = 100;
            int depth = 1;
            double last = 0;

            // Perform iterative deepening with testing duration
            while (timer.ElapsedMilliseconds < duration)
            {
                double alpha = guess - window;
                double beta = guess + window;
                Eval result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Perform minimax search
                double val = result.eval;
                if (val <= alpha)
                {
                    alpha = double.NegativeInfinity;
                    result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Retry with new alpha
                }
                else if (val >= beta)
                {
                    beta = double.PositiveInfinity;
                    result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Retry with new beta
                }
                best_move = result.move;  // Update the best move found
                guess = result.eval;
                Console.WriteLine("DEPTH :" + depth);  // Print the current search depth
                Console.WriteLine("Positions reached :" + iteration);  // Print the number of positions explored
                Console.WriteLine("Time taken :" + timer.ElapsedMilliseconds);  // Print time taken so far
                Console.WriteLine("NPS  :" + iteration / (timer.ElapsedMilliseconds - last + 1) * 100);  // Print nodes per second
                last = timer.ElapsedMilliseconds;
                Console.WriteLine("Best Move : Piece, " + best_move.piece + " , Start, " + best_move.start + " , End, " + best_move.end);  // Print the best move found
                iteration = 0;
                depth++;  // Increase search depth for the next iteration
            }
            timer.Stop();  // Stop the timer
        }

        // Method to handle ELO rating and set search depth
        public int ELO_handler(int ELO)
        {
            if (ELO <= 500)
            {
                return 1;  // Set max depth to 1 for low ELO players
            }
            else if (ELO > 500 && ELO <= 1000)
            {
                return 3;  // Set max depth to 3 for medium ELO players
            }
            else if (ELO > 1000 && ELO <= 1500)
            {
                return 7;  // Set max depth to 7 for higher ELO players
            }
            return 2000;  // Set max depth to 2000 for very high ELO players
        }

        // Method to display the binary representation of a 64-bit integer
        public void show(ulong n)
        {
            string binaryString = Convert.ToString((long)n, 2);  // Convert to binary string
            binaryString = binaryString.PadLeft(64, '0');  // Pad to 64 bits
            char[] temp = binaryString.ToCharArray();
            Array.Reverse(temp);  // Reverse the bits for display
            string temp2 = string.Join("", temp.ToArray());
            for (int i = 0; i < 64; i += 8)
            {
                Console.WriteLine(temp2.Substring(i, 8));  // Print 8-bit chunks
            }
            Console.WriteLine();
        }

        // Method to test node processing time and iteration count
        public int node_test(Board board, PieceCall cache, out double time)
        {
            double duration = 1000000000;  // Set an extremely long time limit for testing
            Move best_move = new Move(-1, -1, -1, -1, -1, false);
            timer.Restart();
            timer.Start();
            double guess = 0;
            double window = 100;
            int depth = 1;
            double last = 0;

            // Perform iterative deepening for testing
            while (depth < 11)
            {
                double alpha = guess - window;
                double beta = guess + window;
                Eval result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Perform minimax search
                double val = result.eval;
                if (val <= alpha)
                {
                    alpha = double.NegativeInfinity;
                    result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Retry with new alpha
                }
                else if (val >= beta)
                {
                    beta = double.PositiveInfinity;
                    result = minimax(board, cache, new Move(-1, -1, -1, -1, -1, false), depth, colour, alpha, beta, best_move, depth, duration);  // Retry with new beta
                }
                best_move = result.move;  // Update the best move found
                guess = result.eval;
                last = timer.ElapsedMilliseconds;

                depth++;  // Increase search depth
            }
            timer.Stop();  // Stop the timer

            time = timer.ElapsedMilliseconds;  // Output the time taken
            return iteration;  // Return the number of iterations
        }
    }
}

