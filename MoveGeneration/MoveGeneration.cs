using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualBasic;

namespace ChessEngine
{
    public sealed class MoveGeneration : Engine
    {

        private static ulong iteration = 0;

        private static ulong iteration2 = 0;


        private static Illegal illegal_moves = new Illegal();

        public MoveGeneration()
        {


        }


        // Calculates all legal moves for the current player, including pawn moves, captures, and promotions, and updates the move count
        public Movecheck all_moves(Board board, PieceCall cache, int mod, out int move_count, out ulong pawn, out ulong capture_mask)
        {
            // Create an array to store all the possible moves (max of 128 moves for all pieces)
            Move[] list = new Move[128];

            // Get the opponent's color (opposite side of 'mod')
            ulong colour_inv = board.colour[mod ^ 1];

            // Get the king's position for the current player
            int king = BitOperations.TrailingZeroCount(board.bitboards[10 + mod]);

            // Variable to hold the legal moves
            ulong legal_moves = 0;

            move_count = 0;  // Initialize move count

            int start;

            // Get the illegal moves and pawn-related information
            Check info = illegal_moves.capture_squares(board, mod, cache);

            bool can_passant;  // Flag indicating whether en passant is possible

            // Get the pin masks and determine if en passant is possible
            ulong[] pins = illegal_moves.pin_masks(board.bitboards, king, mod, board.all_pieces, colour_inv, cache, out can_passant);

            // Get pawn-related information from the check data
            pawn = info.pawn_mask;

            // Get capture-related information from the check data
            capture_mask = info.illegal;

            // Store the illegal moves in the cache
            cache.Illegal = info;

            int i = mod;  // Set the current player's index (0 for white, 1 for black)

            // Get the color of the side to move
            ulong sideToMove = board.colour[mod];

            // Loop through each piece on the board for the current player
            for (; sideToMove > 0;)
            {
                // Get the position of the first active piece
                start = BitOperations.TrailingZeroCount(sideToMove);

                // Create a bitmask for the start position
                ulong start_place = 1ul << start;

                // Loop through each piece of the current player
                for (; (start_place & board.bitboards[i]) == 0;)
                {
                    i += 2;  // Move to the next piece (white->black or black->white)
                }

                // Process the current piece based on its type
                switch (i)
                {
                    case 0:  // White Pawn
                        if (start > 7 && start < 16)
                        {
                            // Handle pawn promotion cases for white pawns
                            legal_moves = cache.White_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);
                            for (; legal_moves > 0;)
                            {
                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 8, false);  // Promote to a queen
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 2, false);  // Promote to a knight
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 4, false);  // Promote to a rook
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 6, false);  // Promote to a bishop
                                move_count++;
                                legal_moves &= legal_moves - 1;
                            }
                        }
                        else
                        {
                            // Handle regular white pawn moves
                            legal_moves = cache.White_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);
                            for (; legal_moves > 0;)
                            {
                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular pawn move
                                move_count++;
                                legal_moves &= legal_moves - 1;
                            }
                        }
                        break;

                    case 1:  // Black Pawn
                        if (start > 47 && start < 56)
                        {
                            // Handle pawn promotion cases for black pawns
                            legal_moves = cache.Black_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);
                            for (; legal_moves > 0;)
                            {
                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 9, false);  // Promote to a queen
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 3, false);  // Promote to a knight
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 5, false);  // Promote to a rook
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 7, false);  // Promote to a bishop
                                move_count++;
                                legal_moves &= legal_moves - 1;
                            }
                        }
                        else
                        {
                            // Handle regular black pawn moves
                            legal_moves = cache.Black_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);
                            for (; legal_moves > 0;)
                            {
                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular pawn move
                                move_count++;
                                legal_moves &= legal_moves - 1;
                            }
                        }
                        break;

                    case 2:  // White Rook
                             // Get all legal rook moves for white
                        legal_moves = cache.White_Rook.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular rook move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 3:  // Black Rook
                             // Get all legal rook moves for black
                        legal_moves = cache.Black_Rook.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular rook move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 4:  // White Knight
                             // Get all legal knight moves for white
                        legal_moves = cache.White_Knight.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular knight move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 5:  // Black Knight
                             // Get all legal knight moves for black
                        legal_moves = cache.Black_Knight.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular knight move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 6:  // White Bishop
                             // Get all legal bishop moves for white
                        legal_moves = cache.White_Bishop.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular bishop move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 7:  // Black Bishop
                             // Get all legal bishop moves for black
                        legal_moves = cache.Black_Bishop.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular bishop move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 8:  // White Queen
                             // Get all legal queen moves for white
                        legal_moves = cache.White_Queen.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular queen move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 9:  // Black Queen
                             // Get all legal queen moves for black
                        legal_moves = cache.Black_Queen.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular queen move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 10:  // White King
                              // Get all legal king moves for white
                        legal_moves = cache.White_King.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular king move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;

                    case 11:  // Black King
                              // Get all legal king moves for black
                        legal_moves = cache.Black_King.moves(start, board, cache, info, pins, board.filter);
                        for (; legal_moves > 0;)
                        {
                            int power = BitOperations.TrailingZeroCount(legal_moves);
                            list[move_count] = new Move(i, start, power, 0, 0, false);  // Regular king move
                            move_count++;
                            legal_moves &= legal_moves - 1;
                        }
                        break;
                }

                // Remove the current piece from the side-to-move bitmask
                sideToMove &= sideToMove - 1;

                // Reset to the current player's turn
                i = mod;
            }

            // Return the list of all possible moves along with check information
            Movecheck val = new Movecheck(list, info.check);
            return val;
        }


        public List<Move> all_moves2(Board board, PieceCall cache, int mod)
        {

            List<Move> list = new List<Move>();

            int inv = mod ^ 1;

            ulong[] all_bitboards = board.bitboards;

            ulong colour_inv = board.colour[inv];

            ulong all_pieces = board.all_pieces;

            int king = BitOperations.TrailingZeroCount(all_bitboards[10 + mod]);

            ulong legal_moves = 0;

            int count = 0;

            int start;

            Check info = illegal_moves.capture_squares(board, mod, cache);

            bool can_passant;

            ulong[] pins = illegal_moves.pin_masks(all_bitboards, king, mod, all_pieces, colour_inv, cache, out can_passant);

            int move_count = 0;

            for (int i = mod; i < 12; i += 2)
            {

                ulong bitboard = all_bitboards[i];

                for (; bitboard > 0;)
                {

                    start = BitOperations.TrailingZeroCount(bitboard);

                    switch (i)
                    {

                        case 0:

                            if (start > 7 && start < 16)
                            {
                                legal_moves = cache.White_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);

                                for (; legal_moves > 0;)
                                {

                                    int power = BitOperations.TrailingZeroCount(legal_moves);

                                    list.Add(new Move(i, start, power, 0, 8, false));

                                    list.Add(new Move(i, start, power, 0, 6, false));

                                    list.Add(new Move(i, start, power, 0, 4, false));

                                    list.Add(new Move(i, start, power, 0, 2, false));

                                    legal_moves &= legal_moves - 1;

                                }

                                break;
                            }
                            else
                            {
                                legal_moves = cache.White_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);


                                for (; legal_moves > 0;)
                                {

                                    int power = BitOperations.TrailingZeroCount(legal_moves);

                                    list.Add(new Move(i, start, power, 0, 0, false));

                                    move_count++;

                                    legal_moves &= legal_moves - 1;

                                }

                                break;
                            }

                        case 1:


                            if (start > 47 && start < 56)
                            {
                                legal_moves = cache.Black_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);


                                for (; legal_moves > 0;)
                                {

                                    int power = BitOperations.TrailingZeroCount(legal_moves);

                                    list.Add(new Move(i, start, power, 0, 9, false));

                                    list.Add(new Move(i, start, power, 0, 7, false));

                                    list.Add(new Move(i, start, power, 0, 5, false));

                                    list.Add(new Move(i, start, power, 0, 3, false));

                                    legal_moves &= legal_moves - 1;

                                }

                                break;
                            }
                            else
                            {
                                legal_moves = cache.Black_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);

                                for (; legal_moves > 0;)
                                {

                                    int power = BitOperations.TrailingZeroCount(legal_moves);

                                    list.Add(new Move(i, start, power, 0, 0, false));

                                    legal_moves &= legal_moves - 1;

                                }

                                break;
                            }

                        case 2:
                            legal_moves = cache.White_Rook.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }
                            break;
                        case 3:
                            legal_moves = cache.Black_Rook.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }
                            break;
                        case 4:
                            legal_moves = cache.White_Knight.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        case 5:

                            legal_moves = cache.Black_Knight.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        case 6:

                            legal_moves = cache.White_Bishop.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        case 7:

                            legal_moves = cache.Black_Bishop.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        case 8:
                            legal_moves = cache.White_Queen.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        case 9:
                            legal_moves = cache.Black_Queen.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }
                            break;
                        case 10:
                            legal_moves = cache.White_King.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }
                            break;
                        case 11:

                            legal_moves = cache.Black_King.moves(start, board, cache, info, pins, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list.Add(new Move(i, start, power, 0, 0, false));

                                legal_moves &= legal_moves - 1;

                            }
                            break;
                    }

                    bitboard &= bitboard - 1;
                }

            }

            return list;
        }


        public int[] all_moves3(Board board, PieceCall cache, int mod, out int move_count, out ulong pawn, out int check)
        {
            int[] list = new int[128];

            ulong colour_inv = board.colour[mod ^ 1];

            int king = BitOperations.TrailingZeroCount(board.bitboards[10 + mod]);

            ulong legal_moves = 0;

            move_count = 0;

            int start;

            Check info = illegal_moves.capture_squares(board, mod, cache);

            bool can_passant;

            ulong[] pins = illegal_moves.pin_masks(board.bitboards, king, mod, board.all_pieces, colour_inv, cache, out can_passant);

            pawn = info.pawn_mask;

            check = info.check;
            //capture_mask = info.illegal;

            int i = mod;

            ulong sideToMove = board.colour[mod];

            for (; sideToMove > 0;)
            {
                start = BitOperations.TrailingZeroCount(sideToMove);

                ulong start_place = 1ul << start;

                for (; (start_place & board.bitboards[i]) == 0;)
                {

                    i += 2;
                }

                switch (i)
                {

                    case 0:

                        if (start > 7 && start < 16)
                        {
                            legal_moves = cache.White_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 8 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 2 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 4 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 6 << 23;
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.White_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                                move_count++;

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }

                    case 1:


                        if (start > 47 && start < 56)
                        {
                            legal_moves = cache.Black_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 9 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 3 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 5 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 7 << 23;
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.Black_Pawn.moves(start, board, cache, info, pins, can_passant, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                                move_count++;

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }

                    case 2:

                        legal_moves = cache.White_Rook.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;

                    case 3:
                        legal_moves = cache.Black_Rook.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 4:
                        legal_moves = cache.White_Knight.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 5:

                        legal_moves = cache.Black_Knight.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 6:

                        legal_moves = cache.White_Bishop.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 7:

                        legal_moves = cache.Black_Bishop.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 8:
                        legal_moves = cache.White_Queen.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 9:
                        legal_moves = cache.Black_Queen.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 10:
                        legal_moves = cache.White_King.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 11:

                        legal_moves = cache.Black_King.moves(start, board, cache, info, pins, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                }

                sideToMove &= sideToMove - 1;

                i = mod;

            }



            return list;
        }

        public Movecheck capture_moves(Board board, PieceCall cache, int mod, out int move_count, out ulong pawn, out ulong capture)
        {
            Move[] list = new Move[100];

            ulong colour_inv = board.colour[mod ^ 1];

            int king = BitOperations.TrailingZeroCount(board.bitboards[10 + mod]);

            int enemy_king = BitOperations.TrailingZeroCount(board.bitboards[10 + mod ^ 1]);

            ulong rook_check = cache.White_Rook.mask_moves(enemy_king, board, board.filter);

            ulong bishop_check = cache.White_Bishop.mask_moves(enemy_king, board, board.filter);

            ulong knight_check = cache.White_Knight.mask[enemy_king];

            ulong white_pawn_check = 1ul << enemy_king + 9 | 1ul << enemy_king + 7;

            ulong black_pawn_check = 1ul << enemy_king - 9 | 1ul << enemy_king - 7;

            ulong queen_check = rook_check | bishop_check;

            ulong legal_moves = 0;

            move_count = 0;

            int start;

            Check info = illegal_moves.capture_squares(board, mod, cache);

            bool can_passant;

            ulong[] pins = illegal_moves.pin_masks(board.bitboards, king, mod, board.all_pieces, colour_inv, cache, out can_passant);

            pawn = info.pawn_mask;

            capture = info.illegal;

            int i = mod;

            ulong sideToMove = board.colour[mod];

            for (; sideToMove > 0;)
            {
                start = BitOperations.TrailingZeroCount(sideToMove);

                ulong start_place = 1ul << start;

                for (; (start_place & board.bitboards[i]) == 0;)
                {

                    i += 2;
                }

                switch (i)
                {

                    case 0:

                        if (start > 7 && start < 16)
                        {
                            legal_moves = cache.White_Pawn.capture_moves(start, board, cache, info, pins, can_passant, white_pawn_check, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 8, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 2, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 4, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 6, false);
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.White_Pawn.capture_moves(start, board, cache, info, pins, can_passant, white_pawn_check, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = new Move(i, start, power, 0, 0, false);

                                move_count++;

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }

                    case 1:


                        if (start > 47 && start < 56)
                        {
                            legal_moves = cache.Black_Pawn.capture_moves(start, board, cache, info, pins, can_passant, black_pawn_check, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 9, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 3, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 5, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 7, false);
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.Black_Pawn.capture_moves(start, board, cache, info, pins, can_passant, black_pawn_check, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = new Move(i, start, power, 0, 0, false);

                                move_count++;

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }

                    case 2:
                        legal_moves = cache.White_Rook.capture_moves(start, board, cache, info, pins, rook_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 3:
                        legal_moves = cache.Black_Rook.capture_moves(start, board, cache, info, pins, rook_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 4:
                        legal_moves = cache.White_Knight.capture_moves(start, board, cache, info, pins, knight_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);
                            move_count++;
                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 5:

                        legal_moves = cache.Black_Knight.capture_moves(start, board, cache, info, pins, knight_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 6:

                        legal_moves = cache.White_Bishop.capture_moves(start, board, cache, info, pins, bishop_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 7:

                        legal_moves = cache.Black_Bishop.capture_moves(start, board, cache, info, pins, bishop_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 8:

                        legal_moves = cache.White_Queen.capture_moves(start, board, cache, info, pins, queen_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 9:
                        legal_moves = cache.Black_Queen.capture_moves(start, board, cache, info, pins, queen_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);
                            move_count++;
                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 10:
                        legal_moves = cache.White_King.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 11:

                        legal_moves = cache.Black_King.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                }
                sideToMove &= sideToMove - 1;

                i = mod;

            }
            Movecheck val = new Movecheck(list, info.check);

            return val;
        }

        public Movecheck capture_moves2(Board board, PieceCall cache, int mod, out int move_count, out ulong pawn)
        {
            Move[] list = new Move[100];

            ulong colour_inv = board.colour[mod ^ 1];

            int king = BitOperations.TrailingZeroCount(board.bitboards[10 + mod]);



            ulong legal_moves = 0;

            move_count = 0;

            int start;

            Check info = illegal_moves.capture_squares(board, mod, cache);

            bool can_passant;

            ulong[] pins = illegal_moves.pin_masks(board.bitboards, king, mod, board.all_pieces, colour_inv, cache, out can_passant);

            pawn = info.pawn_mask;

            int i = mod;

            ulong sideToMove = board.colour[mod];

            for (; sideToMove > 0;)
            {
                start = BitOperations.TrailingZeroCount(sideToMove);

                ulong start_place = 1ul << start;

                for (; (start_place & board.bitboards[i]) == 0;)
                {

                    i += 2;
                }

                switch (i)
                {

                    case 0:

                        if (start > 7 && start < 16)
                        {
                            legal_moves = cache.White_Pawn.capture_moves(start, board, cache, info, pins, can_passant, 0, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 8, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 2, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 4, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 6, false);
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.White_Pawn.capture_moves(start, board, cache, info, pins, can_passant, 0, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = new Move(i, start, power, 0, 0, false);

                                move_count++;

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }

                    case 1:


                        if (start > 47 && start < 56)
                        {
                            legal_moves = cache.Black_Pawn.capture_moves(start, board, cache, info, pins, can_passant, 0, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = new Move(i, start, power, 0, 9, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 3, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 5, false);
                                move_count++;
                                list[move_count] = new Move(i, start, power, 0, 7, false);
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.Black_Pawn.capture_moves(start, board, cache, info, pins, can_passant, 0, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = new Move(i, start, power, 0, 0, false);

                                move_count++;

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }

                    case 2:
                        legal_moves = cache.White_Rook.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 3:
                        legal_moves = cache.Black_Rook.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 4:
                        legal_moves = cache.White_Knight.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);
                            move_count++;
                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 5:

                        legal_moves = cache.Black_Knight.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 6:

                        legal_moves = cache.White_Bishop.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 7:

                        legal_moves = cache.Black_Bishop.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 8:

                        legal_moves = cache.White_Queen.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 9:
                        legal_moves = cache.Black_Queen.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);
                            move_count++;
                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 10:
                        legal_moves = cache.White_King.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 11:

                        legal_moves = cache.Black_King.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = new Move(i, start, power, 0, 0, false);

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                }
                sideToMove &= sideToMove - 1;

                i = mod;

            }
            Movecheck val = new Movecheck(list, info.check);

            return val;
        }

        public int[] capture_moves3(Board board, PieceCall cache, int mod, out int move_count, out ulong pawn)
        {
            int[] list = new int[100];

            ulong colour_inv = board.colour[mod ^ 1];

            int king = BitOperations.TrailingZeroCount(board.bitboards[10 + mod]);

            int enemy_king = BitOperations.TrailingZeroCount(board.bitboards[10 + mod ^ 1]);

            ulong rook_check = cache.White_Rook.mask_moves(enemy_king, board, board.filter);

            ulong bishop_check = cache.White_Bishop.mask_moves(enemy_king, board, board.filter);

            ulong knight_check = cache.White_Knight.mask[enemy_king];

            ulong white_pawn_check = 1ul << enemy_king + 9 | 1ul << enemy_king + 7;

            ulong black_pawn_check = 1ul << enemy_king - 9 | 1ul << enemy_king - 7;

            ulong queen_check = rook_check | bishop_check;

            ulong legal_moves = 0;

            move_count = 0;

            int start;

            Check info = illegal_moves.capture_squares(board, mod, cache);

            bool can_passant;

            ulong[] pins = illegal_moves.pin_masks(board.bitboards, king, mod, board.all_pieces, colour_inv, cache, out can_passant);

            pawn = info.pawn_mask;

            int i = mod;

            ulong sideToMove = board.colour[mod];

            for (; sideToMove > 0;)
            {
                start = BitOperations.TrailingZeroCount(sideToMove);

                ulong start_place = 1ul << start;

                for (; (start_place & board.bitboards[i]) == 0;)
                {

                    i += 2;
                }

                switch (i)
                {

                    case 0:

                        if (start > 7 && start < 16)
                        {
                            legal_moves = cache.White_Pawn.capture_moves(start, board, cache, info, pins, can_passant, white_pawn_check, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 8 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 2 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 4 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 6 << 23;
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.White_Pawn.capture_moves(start, board, cache, info, pins, can_passant, white_pawn_check, board.filter);

                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                                move_count++;

                                legal_moves &= legal_moves - 1;


                            }

                            break;
                        }

                    case 1:


                        if (start > 47 && start < 56)
                        {
                            legal_moves = cache.Black_Pawn.capture_moves(start, board, cache, info, pins, can_passant, black_pawn_check, board.filter);

                            for (; legal_moves > 0;)
                            {
                                int power = BitOperations.TrailingZeroCount(legal_moves);
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 9 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 3 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 5 << 23;
                                move_count++;
                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 7 << 23;
                                move_count++;
                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }
                        else
                        {
                            legal_moves = cache.Black_Pawn.capture_moves(start, board, cache, info, pins, can_passant, black_pawn_check, board.filter);


                            for (; legal_moves > 0;)
                            {

                                int power = BitOperations.TrailingZeroCount(legal_moves);

                                list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                                move_count++;

                                legal_moves &= legal_moves - 1;

                            }

                            break;
                        }

                    case 2:
                        legal_moves = cache.White_Rook.capture_moves(start, board, cache, info, pins, rook_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 3:
                        legal_moves = cache.Black_Rook.capture_moves(start, board, cache, info, pins, rook_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 4:
                        legal_moves = cache.White_Knight.capture_moves(start, board, cache, info, pins, knight_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;
                            move_count++;
                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 5:

                        legal_moves = cache.Black_Knight.capture_moves(start, board, cache, info, pins, knight_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 6:

                        legal_moves = cache.White_Bishop.capture_moves(start, board, cache, info, pins, bishop_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 7:

                        legal_moves = cache.Black_Bishop.capture_moves(start, board, cache, info, pins, bishop_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 8:

                        legal_moves = cache.White_Queen.capture_moves(start, board, cache, info, pins, queen_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }

                        break;
                    case 9:
                        legal_moves = cache.Black_Queen.capture_moves(start, board, cache, info, pins, queen_check, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;
                            move_count++;
                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 10:
                        legal_moves = cache.White_King.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                    case 11:

                        legal_moves = cache.Black_King.capture_moves(start, board, cache, info, pins, 0, board.filter);

                        for (; legal_moves > 0;)
                        {

                            int power = BitOperations.TrailingZeroCount(legal_moves);

                            list[move_count] = i | start << 4 | power << 10 | 0 << 16 | 0 << 23;

                            move_count++;

                            legal_moves &= legal_moves - 1;

                        }
                        break;
                }
                sideToMove &= sideToMove - 1;

                i = mod;

            }

            return list;
        }

        // This method tests the generation of moves at different depths in a recursive manner, counting iterations at depth 0.
        public ulong move_generation_test(Board board, PieceCall cache, ulong[] all_bitboards, int mod, int depth, int max)
        {
            // Base case: if depth is 0, increment the iteration counter and return the value
            if (depth == 0)
            {
                iteration++;  // Increment the iteration counter to track the number of moves generated
                return iteration;  // Return the number of iterations
            }

            // Generate all possible moves for the current position
            int[] moves = all_moves3(board, cache, mod, out int move_count, out ulong pawn_mask, out int check);

            // Loop over all generated moves and recursively call the function to generate moves for the next depth
            for (int i = 0; i < move_count; i++)
            {
                // Store the current state of the game before making the move (to restore later)
                Global store = new Global(cache.Global.white, cache.Global.black, cache.Global.Wkingcastle, cache.Global.Wqueencastle, cache.Global.Bkingcastle, cache.Global.Bqueencastle);

                // Update the board with the current move, changing the state of the game
                int update_information = board.update3(moves[i], cache, mod ^ 1);  // Use mod ^ 1 to switch to the opponent's turn

                // Recursively generate moves for the next depth
                move_generation_test(board, cache, board.bitboards, mod ^ 1, depth - 1, max);



                // Restore the board state to its original state after the recursive call
                board.restore3(update_information, moves[i]);

                // Restore the global cache state
                cache.Global = store;
            }

            // Return the total number of iterations
            return iteration;
        }

        public ulong Perft(Board board, PieceCall cache, int mod, int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            Movecheck generatedMoves = all_moves(board, cache, mod, out int move_count, out ulong pawn_mask, out ulong capture_mask);

            ulong totalNodes = 0;

            for (int i = 0; i < move_count; i++)
            {
                Move move = generatedMoves.moves[i]; // change "moves" if your Movecheck field/property has a different name

                Global store = new Global(
                    cache.Global.white,
                    cache.Global.black,
                    cache.Global.Wkingcastle,
                    cache.Global.Wqueencastle,
                    cache.Global.Bkingcastle,
                    cache.Global.Bqueencastle
                );

                int update_information = board.update(move, cache, mod ^ 1);

                totalNodes += Perft(board, cache, mod ^ 1, depth - 1);

                board.restore(update_information, move);
                cache.Global = store;
            }

            return totalNodes;
        }

        public ulong PerftDivide(Board board, PieceCall cache, int mod, int depth)
        {
            if (depth == 0)
            {
                Console.WriteLine("Nodes searched: 1");
                return 1;
            }

            Movecheck generatedMoves = all_moves(board, cache, mod, out int move_count, out ulong pawn_mask, out ulong capture_mask);

            ulong totalNodes = 0;

            for (int i = 0; i < move_count; i++)
            {
                Move move = generatedMoves.moves[i]; // change "moves" if your Movecheck field/property has a different name

                Global store = new Global(
                    cache.Global.white,
                    cache.Global.black,
                    cache.Global.Wkingcastle,
                    cache.Global.Wqueencastle,
                    cache.Global.Bkingcastle,
                    cache.Global.Bqueencastle
                );

                int update_information = board.update(move, cache, mod ^ 1);

                ulong childNodes = Perft(board, cache, mod ^ 1, depth - 1);

                Console.WriteLine($"{move.piece} , {move.start} , {move.end} : {childNodes}");

                totalNodes += childNodes;

                board.restore(update_information, move);
                cache.Global = store;
            }

            Console.WriteLine($"Nodes searched: {totalNodes}");
            return totalNodes;
        }
        // This method runs the iterative testing of move generation, increasing depth and measuring the time taken
        public void iterative_test(Board board, PieceCall cache, int depth, int colour)
        {
            Stopwatch timer = new Stopwatch();  // Initialize a timer to measure performance

            timer.Start();  // Start the timer

            long starting_time = 0;  // Store the starting time
            long last = 0;  // Store the last time to calculate NPS (Nodes per second)

            // Infinite loop to keep running the move generation test
            while (true)
            {
                Console.WriteLine(depth + "  DEPTH  ");  // Print the current depth level

                // Call the move generation test function for the current depth
                move_generation_test(board, cache, board.bitboards, colour, depth, depth);

                // Calculate the time taken in milliseconds since the last check
                ulong g = (ulong)(timer.ElapsedMilliseconds - starting_time);

                // Print out the details of the test for the current depth
                Console.WriteLine("DEPTH :" + depth);
                Console.WriteLine("Positions reached :" + iteration);  // Print the number of iterations (positions generated)
                Console.WriteLine("Time taken :" + timer.ElapsedMilliseconds);  // Print the time taken for this depth
                Console.WriteLine("NPS  :" + (long)iteration / (timer.ElapsedMilliseconds - last + 1) * 100);  // Calculate and print nodes per second

                last = timer.ElapsedMilliseconds;  // Update the last time check
                depth++;  // Increase the depth for the next iteration
                iteration = 0;  // Reset the iteration counter for the next depth level
            }
        }

    }
}
