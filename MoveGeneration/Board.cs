using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using ChessEngine;




namespace ChessEngine
{
    public sealed class Board
    {
        public string FEN { get; set; }

        public string startpos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        public string startpos_7 = "rnbkqnr1/ppppppp1/8/8/8/PPPPPPP1/RNBKQNR1/8";

        public string startpos_6 = "rbkqnr2/pppppp2/8/8/PPPPPP2/RBKQNR2/8/8";

        public string startpos_5 = "rbknr3/ppppp3/8/PPPPP3/RBKNR3/8/8/8";

        string[] positions = { "r1b2rk1/1p3ppp/p1n1p3/q2pP3/3NnP2/P1P1B3/2P1B1PP/R3QR1K", "r1bq1rk1/1pp1bpp1/p2p1nnp/3Pp3/2B1P3/2NQBN1P/PPP2PP1/2KR3R", "4r1k1/p1p3pp/6p1/4p3/3q2P1/2rPR2P/P4P2/R3Q1K1", "2kr2nr/ppp3pp/3bb1q1/5p2/3n4/2NB4/PPPB1PPP/R2QNR1K", "r1b1r1k1/ppppqppp/2n5/8/4P3/2P1Q3/PP3PPP/R1B1KB1R", "r6r/pQpkq2p/3p2p1/4n3/3pn3/8/PPP2PPP/RN3RK1", "2kr1bnr/ppp2ppp/8/8/3n4/5B2/PPPB1PPP/RN1K3R", "r3r1k1/1b3pb1/p2p1qpp/1pp1p3/2P1P1PP/1P1P1N2/1P3P2/R2QRNK1", "r2q1rk1/ppp2ppp/2p1b3/2b5/4P3/5Q2/PPP2PPP/RNB2RK1", "7k/1p4p1/pP4np/P3p3/1Br5/7P/3n1PP1/1R4K1" };

        public ulong[] bitboards = new ulong[13];

        public ulong all_pieces;

        public ulong[] colour = new ulong[2];

        public Castling castle = new Castling();

        public EnPassant passant = new EnPassant();

        public bool[] states = new bool[2];

        public double[] end_game_weights = new double[2];

        public ulong[] repeat = new ulong[3];

        public int threefold = 0;

        private ulong[] castling_keys = new ulong[4];

        private ulong[] enpassant_keys = new ulong[8];

        public Random random;

        private ulong side;

        public ulong[,] piece_keys = new ulong[12, 64];

        private Random rng = new Random(1234567);

        public bool[] is_castle = { false, false };

        public bool white_in_check = false;

        public bool black_in_check = false;

        public int fifty_move_rule = 0;

        public Illegal illegal;

        public int start_shift = 4;

        public int end_shift = 10;

        public int capture_shift = 16;

        public int promotion_shift = 23;

        public int piece_mask = 0b1111;

        public int start_mask = 0b111111 << 4;

        public int end_mask = 0b111111 << 10;

        public int capture_mask = 0b1111111 << 16;

        public int promotion_mask = 0b1111 << 23;

        public ulong seven_grid = 18410856566090662016;

        public ulong six_grid = 18446674532832952512;

        public ulong five_grid = 18446743940043432160;

        public Magic magic = new Magic("", false, 0ul);

        public ulong filter;

        public Board(string FEN, string filter)
        {
            this.FEN = FEN;

            this.filter = set_filter(filter);

            illegal = new Illegal();

            set_board();

            random_zobrist();

        }

        public ulong set_filter(string Filter)
        {

            switch (Filter)
            {
                case "7x7":
                    return seven_grid;
                case "6x6":
                    return six_grid;
                case "5x5":
                    return five_grid;

            }
            return 0;
        }

        public void set_repeat(ulong[] repeats)
        {
            repeat = repeats;
        }

        public void set_states(bool[] state)
        {
            states = state;
        }
        public void set_gamestate()
        {

            double material_count = 0;

            int[] material = new int[10] { 10, 10, 4000, 4000, 300, 300, 300, 300, 25000, 25000 };

            for (int i = 1; i < 10; i += 2)
            {

                ulong bitboard = bitboards[i];

                for (; bitboard > 0;)
                {
                    material_count += material[i];

                    bitboard &= bitboard - 1;
                }

            }

            end_game_weights[0] = 1 - material_count / 34280;
            //Console.WriteLine(end_game_weights[0]);
            material_count = 0;

            for (int i = 0; i < 10; i += 2)
            {

                ulong bitboard = bitboards[i];

                for (; bitboard > 0;)
                {
                    material_count += material[i];

                    bitboard &= bitboard - 1;
                }

            }

            end_game_weights[1] = 1 - material_count / 34280;
        }
        public string bit64(ulong bitboard)
        {

            string binaryString = Convert.ToString((long)bitboard, 2);

            binaryString = binaryString.PadLeft(64, '0');

            return binaryString;
        }

        public void view(ulong n)

        {
            string binaryString = Convert.ToString((long)n, 2);

            binaryString = binaryString.PadLeft(64, '0');

            char[] temp = binaryString.ToCharArray();

            Array.Reverse(temp);

            string temp2 = string.Join("", temp.ToArray());

            for (int i = 0; i < 64; i += 8)

            {
                Console.WriteLine(temp2.Substring(i, 8));
            }

        }

        private void set_board()
        {
            // Check and set the FEN string based on predefined starting positions
            if (FEN == "startpos")
            {
                FEN = startpos;  // Set to standard starting position
            }
            if (FEN == "startpos7")
            {
                FEN = startpos_7;  // Set to 7x7 starting position
            }
            if (FEN == "startpos6")
            {
                FEN = startpos_6;  // Set to 6x6 starting position
            }
            if (FEN == "startpos5")
            {
                FEN = startpos_5;  // Set to 5x5 starting position
            }

            // String of valid chess pieces and separators in FEN
            string non_num = "prnbkPRNBKqQ/";

            // Dictionary to map each chess piece to a unique index
            Dictionary<char, int> FEN_dict = new Dictionary<char, int>
    {
        { 'P', 0 }, { 'p', 1 }, { 'R', 2 }, { 'r', 3 }, { 'N', 4 }, { 'n', 5 },
        { 'B', 6 }, { 'b', 7 }, { 'Q', 8 }, { 'q', 9 }, { 'K', 10 }, { 'k', 11 }
    };

            int count = 0;  // Counter to keep track of the board positions

            // Loop through each character in the FEN string
            foreach (char i in FEN)
            {
                // When a '/' is encountered (indicating the end of a rank)
                if (i == '/')
                {
                    // Adjust the count to ensure the row is complete (8 squares per row)
                    if (count % 8 != 0)
                    {
                        count = 8 + count & -8;  // Align to the next multiple of 8
                    }
                }
                // If the character is a number (indicating empty squares)
                else if (non_num.Contains(i) == false)
                {
                    int num = int.Parse(i.ToString());  // Convert the number to an integer
                    count += num;  // Increment the count by the number of empty squares
                }
                else
                {
                    // Update the bitboard for the corresponding piece based on its index in the FEN_dict
                    bitboards[FEN_dict[i]] |= 1ul << count;
                    count++;  // Move to the next position on the board
                }
            }

            // Update the bitboards for the two colors (white and black)
            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];  // White pieces
            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];  // Black pieces

            // Combine both color bitboards to get the total position of all pieces on the board
            all_pieces = colour[0] | colour[1];
        }

        private static Dictionary<int, string> board_dicts(Dictionary<int, string> column, string[] array)

        {

            int count = 0;

            foreach (string i in array)

            {

                column.Add(count, i);

                count++;

            }

            return column;

        }

        private void promotion(int piece, int end, bool update, int promote)

        {




            if (update)
            {

                bitboards[piece] ^= 1ul << end;

                bitboards[promote] |= 1ul << end;
            }
            else
            {
                bitboards[promote] ^= 1ul << end;

                bitboards[piece] ^= 1ul << end;

            }




        }

        private void promotion2(int piece, int end, bool update, int promote, ref ulong currentKey)

        {




            if (update)
            {

                bitboards[piece] ^= 1ul << end;

                bitboards[promote] |= 1ul << end;
            }
            else
            {
                bitboards[promote] ^= 1ul << end;

                bitboards[piece] ^= 1ul << end;

            }
            currentKey ^= piece_keys[piece, end];
            currentKey ^= piece_keys[promote, end];




        }

        // Updates the board state based on a move, handling captures, castling, en passant, and promotions
        public int update(Move move, PieceCall cache, int mod)
        {
            int capture_piece = 12;  // Default value for no capture (12 represents an invalid piece)

            ulong end_place = 1UL << move.end;  // The new position of the moved piece, represented as a bitmask

            // Update the bitboard by removing the piece from the start position and adding it to the end position
            bitboards[move.piece] ^= 1UL << move.start | end_place;

            // Check if a capture occurred at the end position
            ulong cap = end_place & all_pieces;  // Intersection between end place and all pieces on the board

            //Don't waste time looking for captures if no cap. Cap is quickly determined so time is saves
            if (cap != 0)
            {
                for (int i = mod; i < 10; i += 2)  // Only loop over enemy pieces to save time
                {
                    ulong board = bitboards[i];


                    if ((board & end_place) != 0)
                    {
                        bitboards[i] ^= cap;
                        capture_piece = i;
                        break;  // Exit the loop once the capture is found
                    }
                }
            }

            // Handle castling if the move involves a king or rook
            if (9 < move.piece || 1 < move.piece && move.piece < 4)
            {
                castle.set_castling(move, cache);  // Update castling rights based on the move
            }

            // Perform castling if the move involves castling (piece > 9 indicates a rook or king)
            if (move.piece > 9)
            {
                castle.do_castling(move, bitboards, true, is_castle);  // Execute the castling move
            }

            // Handle en passant for pawn captures
            passant.two_squares(move, cache);

            // Handle en passant capture and promotion if the move involves a pawn
            if (move.piece < 2)
            {
                passant.do_passant(move, bitboards, true, capture_piece);  // Perform en passant if applicable

                // Promote the pawn if the move results in a promotion
                if (move.ispromotion > 0)
                {
                    promotion(move.piece, move.end, true, move.ispromotion);  // Handle promotion logic
                }
            }

            // Update the bitboards for both colors after the move
            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];  // White pieces
            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];  // Black pieces

            // Update the bitboard for all pieces on the board
            all_pieces = colour[0] | colour[1];

            // Return the index of the captured piece (or 12 if no capture)
            return capture_piece;
        }

        public int update2(Move move, PieceCall cache, int mod, ref ulong currentKey)
        {
            int capture_piece = 12;
            ulong end_place = 1UL << move.end;

            // ---- Incremental Zobrist key updates ----
            // Remove the moving piece from its starting square.
            currentKey ^= piece_keys[move.piece, move.start];
            // Add the moving piece to its destination square.
            currentKey ^= piece_keys[move.piece, move.end];
            // -------------------------------------------

            // Update bitboards for the moving piece.
            bitboards[move.piece] ^= 1UL << move.start | end_place;

            ulong cap = end_place & all_pieces;
            if (cap != 0)
            {
                for (int i = mod; i < 10; i += 2)
                {
                    ulong boardTemp = bitboards[i];
                    if ((boardTemp & end_place) != 0)
                    {
                        bitboards[i] ^= cap;
                        capture_piece = i;
                        // Remove captured piece's key from the destination square.
                        currentKey ^= piece_keys[i, move.end];
                        break;
                    }
                }
            }

            if (9 < move.piece || 1 < move.piece && move.piece < 4)
            {
                castle.set_castling(move, cache);
                // If castling rights change, update currentKey accordingly here.
            }

            if (move.piece > 9)
            {
                castle.do_castling2(move, bitboards, true, is_castle, ref currentKey, piece_keys);

                // For a castling move, update the key for the rook's movement as well.
                // For example:
                // currentKey ^= Zobrist.PieceKeys[rookPiece, rookStart] ^ Zobrist.PieceKeys[rookPiece, rookEnd];
            }

            passant.two_squares(move, cache);
            // If en passant rights change, update currentKey with your en passant keys here.

            if (move.piece < 2)
            {
                passant.do_passant2(move, bitboards, true, capture_piece, ref currentKey, piece_keys);
                if (move.ispromotion > 0)
                {
                    // Promotion: remove the pawn key at the destination...
                    //currentKey ^= piece_keys[move.piece, move.end];
                    // ...and add the key for the promoted piece.
                    //currentKey ^= piece_keys[move.ispromotion, move.end];
                    promotion2(move.piece, move.end, true, move.ispromotion, ref currentKey);
                }
            }

            // Recompute aggregate bitboards.
            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];
            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];
            all_pieces = colour[0] | colour[1];

            return capture_piece;
        }

        public int restore2(int update_information, Move move, ref ulong currentKey)
        {
            if (move.piece > 9)
            {
                castle.do_castling2(move, bitboards, false, is_castle, ref currentKey, piece_keys);
                // Update the key for the rook's movement if needed.
            }

            ulong end_place = 1UL << move.end;

            // ---- Incremental Zobrist key undo for moving piece ----
            // Remove the moving piece from the destination square.
            currentKey ^= piece_keys[move.piece, move.end];
            // Add the moving piece back to its starting square.
            currentKey ^= piece_keys[move.piece, move.start];
            // ---------------------------------------------------------

            bitboards[move.piece] ^= end_place | 1UL << move.start;

            // Restore the captured piece, if any.
            bitboards[update_information] |= end_place;
            if (update_information != 12)
            {
                // Restore captured piece's key.
                currentKey ^= piece_keys[update_information, move.end];
            }

            if (move.piece < 2)
            {
                passant.do_passant2(move, bitboards, true, update_information, ref currentKey, piece_keys);
                if (move.ispromotion > 0)
                {
                    // Undo promotion: remove the promoted piece's key and re-add the pawn's key.

                    promotion2(move.piece, move.end, false, move.ispromotion, ref currentKey);
                }
            }

            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];
            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];
            all_pieces = colour[0] | colour[1];

            return update_information;
        }


        public int update3(int move, PieceCall cache, int mod)
        {
            int piece = move & piece_mask;

            int start = (move & start_mask) >> start_shift;
            // Console.WriteLine(start);
            int end = (move & end_mask) >> end_shift;
            int promo = (move & promotion_mask) >> promotion_shift;
            int capture_piece = 12;

            ulong end_place = 1UL << end;

            bitboards[piece] ^= 1UL << start | end_place;

            ulong cap = end_place & all_pieces;

            if (cap != 0)
            {
                for (int i = mod; i < 10; i += 2)

                {
                    ulong board = bitboards[i];

                    if ((board & end_place) != 0)

                    {

                        bitboards[i] ^= cap;

                        capture_piece = i;

                    }

                }
            }



            if (9 < piece || 1 < piece && piece < 4)
            {
                castle.set_castling2(piece, start, end, cache);

            }

            if (piece > 9)
            {
                castle.do_castling2(piece, start, end, bitboards, true, is_castle);
            }

            passant.two_squares2(piece, start, end, cache);

            if (piece < 2)
            {
                passant.do_passant2(piece, start, end, bitboards, true, capture_piece);
                if (promo > 0)
                {
                    promotion(piece, end, true, promo);
                }
            }

            //Check info = illegal.capture_squares(bitboards, 1-mod, cache);

            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];
            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];
            all_pieces = colour[0] | colour[1];

            return capture_piece;
        }

        // Restores the board state after an undo move, reversing the changes made by the previous update
        public int restore(int update_information, Move move)
        {
            // If the move involves a castling (king or rook), reverse the castling action
            if (move.piece > 9)
            {
                castle.do_castling(move, bitboards, false, is_castle);  // Undo the castling move
            }

            ulong end_place = 1UL << move.end;  // The bitmask for the end position of the move

            // Reverse the move by toggling the start and end positions for the piece
            bitboards[move.piece] ^= end_place | 1UL << move.start;

            // Restore the captured piece (if any) to its previous position
            bitboards[update_information] |= end_place;

            // If the move was made by a pawn, handle en passant and promotion
            if (move.piece < 2)
            {
                passant.do_passant(move, bitboards, true, update_information);  // Reverse en passant if it occurred

                // Reverse the promotion if the pawn was promoted during the move
                if (move.ispromotion > 0)
                {
                    promotion(move.piece, move.end, false, move.ispromotion);  // Undo promotion
                }
            }

            // Update the bitboards for both colors after restoring the move
            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];  // White pieces
            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];  // Black pieces

            // Update the bitboard for all pieces on the board
            all_pieces = colour[0] | colour[1];

            // Return the updated index of the captured piece (or the original piece)
            return update_information;
        }



        public int restore3(int update_information, int move)
        {
            int piece = move & piece_mask;
            int start = (move & start_mask) >> start_shift;
            int end = (move & end_mask) >> end_shift;
            int promo = (move & promotion_mask) >> promotion_shift;
            if (piece > 9)
            {
                castle.do_castling2(piece, start, end, bitboards, false, is_castle);
            }

            ulong end_place = 1UL << end;

            bitboards[piece] ^= end_place | 1UL << start;

            bitboards[update_information] |= end_place;

            if (piece < 2)
            {
                passant.do_passant2(piece, start, end, bitboards, true, update_information);

                if (promo > 0)
                {
                    promotion(piece, end, false, promo);
                }
            }

            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];

            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];

            all_pieces = colour[0] | colour[1];

            return update_information;
        }



        public bool in_check(int mod, PieceCall cache)

        {

            ulong king = bitboards[mod + 10];

            ulong move = 0;

            for (int i = mod ^ 1; i < 12; i += 2)

            {
                ulong board = bitboards[i];

                int name = i;

                if (i > 1)
                {
                    name -= mod ^ 1;
                }


                while (board > 0)
                {

                    int num = BitOperations.TrailingZeroCount(board);

                    switch (name)
                    {
                        case 0:
                            move = cache.White_Pawn.mask[num];

                            //pawn |= move;

                            break;
                        case 1:
                            move = cache.Black_Pawn.mask[num];

                            //pawn |= move;

                            break;
                        case 2:
                            ulong blockers = cache.White_Rook.rook_mask[num] & (all_pieces ^ king);
                            ulong key = blockers * magic.rook_magic_values[num] >> magic.rook_shift_values[num];

                            move = cache.White_Rook.rook_look[num, key];

                            break;
                        case 4:
                            move = cache.White_Knight.mask[num];

                            break;
                        case 6:
                            blockers = cache.White_Bishop.bishop_mask[num] & (all_pieces ^ king);
                            key = blockers * magic.bishop_magic_values[num] >> magic.bishop_shift_values[num];

                            move = cache.Black_Bishop.bishop_look[num, key];

                            break;

                        case 8:
                            blockers = cache.White_Rook.rook_mask[num] & (all_pieces ^ king);
                            key = blockers * magic.rook_magic_values[num] >> magic.rook_shift_values[num];

                            move = cache.White_Rook.rook_look[num, key];

                            blockers = cache.White_Bishop.bishop_mask[num] & (all_pieces ^ king);
                            key = blockers * magic.bishop_magic_values[num] >> magic.bishop_shift_values[num];

                            move |= cache.White_Bishop.bishop_look[num, key];

                            break;
                        case 10:
                            move = cache.White_King.mask[num];

                            break;
                    }

                    if ((king & move) != 0)
                    {
                        return true;
                    }
                    board &= board - 1;
                }
            }
            return false;
        }

        private void random_zobrist()
        {
            // Loop through all 12 pieces and 64 squares to generate random Zobrist keys
            for (int piece = 0; piece < 12; piece++)
            {
                for (int square = 0; square < 64; square++)
                {
                    // Generate a random key for the current piece and square combination
                    piece_keys[piece, square] = RandomULong();
                }
            }

            // Generate a random key for the side-to-move (white or black)
            side = RandomULong();

        }

        // Generates a random unsigned long (64-bit number) using a random byte array
        public ulong RandomULong()
        {
            byte[] buffer = new byte[8];  // Create a byte array of size 8 (64 bits)
            rng.NextBytes(buffer);  // Fill the byte array with random bytes
            return BitConverter.ToUInt64(buffer, 0);  // Convert the byte array into a 64-bit unsigned long (ulong)
        }

        // Generates a hash key for the current board state, using the Zobrist hashing method
        public ulong get_key(int mod, PieceCall cache)
        {
            ulong key = 0UL;  // Initialize the hash key to 0

            // Loop through all 12 pieces
            for (int i = 0; i < 12; i++)
            {
                ulong bitboard = bitboards[i];  // Get the bitboard for the current piece type

                // Loop through all the squares occupied by the current piece
                for (; bitboard > 0;)
                {
                    int place = BitOperations.TrailingZeroCount(bitboard);  // Get the index of the lowest set bit (the occupied square)

                    key ^= piece_keys[i, place];  // XOR the Zobrist key for the current piece and square with the hash key

                    bitboard &= bitboard - 1;  // Remove the lowest set bit (move to the next occupied square)
                }
            }

            // If the side to move is black, XOR the side-to-move key with the board hash
            if (mod == 1)
            {
                key ^= side;
            }

            return key;  // Return the final board hash key
        }


        public ulong GetKey(int sideToMove, PieceCall cache)
        {
            ulong key = 0UL;


            for (int i = 0; i < 12; i++)
            {
                ulong bitboard = bitboards[i];


                for (; bitboard != 0;)
                {
                    int square = BitOperations.TrailingZeroCount(bitboard);
                    key ^= piece_keys[i, square];
                    bitboard &= bitboard - 1;
                }
            }

            if (sideToMove == 0)
            {
                key ^= side;
            }


            //key ^= cache.Global.Wqueencastle ? castling_keys[0] : 0;
            //key ^= cache.Global.Wkingcastle ? castling_keys[1] : 0;
            //key ^= cache.Global.Bqueencastle ? castling_keys[2] : 0;
            //key ^= cache.Global.Bkingcastle ? castling_keys[3] : 0;
            //key ^= cache.Global.white == -1 ? 0 : enpassant_keys[cache.Global.white % 8];
            //key ^= cache.Global.black == -1 ? 0 : enpassant_keys[cache.Global.black % 8];

            return key;
        }

        // Method to print the chess board based on size and skip parameters
        public void print(int size, bool skip)
        {
            // Create a dictionary to map board indices to piece symbols
            Dictionary<int, string> board_dict = new Dictionary<int, string>();

            // Array of piece symbols (uppercase for white, lowercase for black)
            string[] pieces = new string[12] { "P", "p", "R", "r", "N", "n", "B", "b", "Q", "q", "K", "k" };

            // Arrays representing board squares for different board sizes (8x8, 7x7, etc.)
            string[] board_squares = new string[64];
            string[] board_squares7 = new string[49];
            string[] board_squares6 = new string[36];
            string[] board_squares5 = new string[25];

            // Initialize all squares to an empty marker "."
            Array.Fill(board_squares, ".");

            int count = 0;

            // Loop through each piece type represented by the 12 bitboards
            for (int i = 0; i < 12; i++)
            {
                // Get the bitboard corresponding to the current piece type
                ulong bitboard = bitboards[i];

                // Convert bitboard to a string representation (assumed to return a 64-char string)
                string board_string = bit64(bitboard);

                // Process each character in the board string
                foreach (char piece in board_string)
                {
                    // If the bit is set (represented by '1'), place the piece symbol at the current square
                    if (piece == '1')
                    {
                        board_squares[count] = pieces[i];
                    }
                    // If the bit is not set and the square is still empty, ensure it remains marked as empty
                    if (piece == '0' && board_squares[count] == ".")
                    {
                        board_squares[count] = ".";
                    }
                    count++;
                }
                // Reset counter for the next bitboard
                count = 0;
            }

            count = 0;
            // Populate the dictionary with board square indices and their corresponding symbols
            board_dicts(board_dict, board_squares);

            // Create a 2D array to represent an 8x8 board
            string[,] board = new string[8, 8];

            // Map the one-dimensional board dictionary to the 2D board array
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i, j] = board_dict[63 - count];
                    count++;
                }
                // If skip is true, print additional blank lines between rows for spacing
                if (skip)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            // Set output encoding to support Unicode chess symbols
            Console.OutputEncoding = Encoding.UTF8;
            // Call the overloaded print method to display the board graphically
            print(board, size);
            // Reset the console color to default
            Console.ResetColor();
        }

        // Overloaded method to print the board based on its size
        public void print(string[,] board, int size)
        {
            // Local function to convert piece symbols to their corresponding Unicode chess characters
            Func<string, string> piece_to_unicode = piece =>
            {
                switch (piece)
                {
                    case "P": return "♙";
                    case "N": return "♘";
                    case "B": return "♗";
                    case "R": return "♖";
                    case "Q": return "♕";
                    case "K": return "♔";
                    case "p": return "♟";
                    case "n": return "♞";
                    case "b": return "♝";
                    case "r": return "♜";
                    case "q": return "♛";
                    case "k": return "♚";
                    default: return " ";
                }
            };

            // Check board size and print corresponding board layout
            if (size == 8)
            {
                // Print column labels for 8x8 board
                Console.WriteLine("    a   b   c   d   e   f   g   h");
                Console.WriteLine("  ┌───┬───┬───┬───┬───┬───┬───┬───┐");

                // Iterate through each rank of the board
                for (int rank = 0; rank < 8; rank++)
                {
                    int displayed_rank = 8 - rank;
                    Console.Write(displayed_rank + " │");

                    // Iterate through each file in the rank
                    for (int file = 0; file < 8; file++)
                    {
                        string piece = board[rank, file];
                        string symbol = piece_to_unicode(piece);

                        // Set console color based on piece color (assumes uppercase = white, lowercase = blue)
                        if (char.IsUpper(piece.Length > 0 ? piece[0] : ' '))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }

                        // Print the chess piece symbol within cell borders
                        Console.Write(" " + symbol + " ");
                        Console.ResetColor();
                        Console.Write("│");
                    }
                    // Display material count on a specific rank (here, rank 4)
                    if (rank == 4)
                    {
                        int material = material_count();
                        Console.Write($" 4        Material: {material}");
                        Console.WriteLine(" ");
                    }
                    else
                    {
                        Console.WriteLine(" " + displayed_rank);
                    }

                    // Print row separators
                    if (rank < 7)
                    {
                        Console.WriteLine("  ├───┼───┼───┼───┼───┼───┼───┼───┤");
                    }
                    else
                    {
                        Console.WriteLine("  └───┴───┴───┴───┴───┴───┴───┴───┘");
                    }
                }
                // Print bottom column labels
                Console.WriteLine("    a   b   c   d   e   f   g   h");
            }
            else if (size == 7)
            {
                // Print column labels for 7x7 board
                Console.WriteLine("    a   b   c   d   e   f   g");
                Console.WriteLine("  ┌───┬───┬───┬───┬───┬───┬───┐");

                // Iterate through each rank for 7x7 board
                for (int rank = 0; rank < 7; rank++)
                {
                    int displayed_rank = 7 - rank;
                    Console.Write(displayed_rank + " │");

                    // Print each file in the current rank
                    for (int file = 0; file < 7; file++)
                    {
                        string piece = board[rank, file];
                        string symbol = piece_to_unicode(piece);

                        // Set color based on piece case
                        if (char.IsUpper(piece.Length > 0 ? piece[0] : ' '))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }

                        Console.Write(" " + symbol + " ");
                        Console.ResetColor();
                        Console.Write("│");
                    }
                    // Display material count on a specific rank (here, rank 3)
                    if (rank == 3)
                    {
                        int material = material_count();
                        Console.Write($" 3        Material: {material}");
                        Console.WriteLine(" ");
                    }
                    else
                    {
                        Console.WriteLine(" " + displayed_rank);
                    }

                    // Print row separator or border at the bottom
                    if (rank < 6)
                    {
                        Console.WriteLine("  ├───┼───┼───┼───┼───┼───┼───┤");
                    }
                    else
                    {
                        Console.WriteLine("  └───┴───┴───┴───┴───┴───┴───┘");
                    }
                }
                // Print bottom column labels for 7x7 board
                Console.WriteLine("    a   b   c   d   e   f   g");
            }
            else if (size == 6)
            {
                // Print column labels for 6x6 board
                Console.WriteLine("    a   b   c   d   e   f");
                Console.WriteLine("  ┌───┬───┬───┬───┬───┬───┐");

                // Loop through each rank for 6x6 board
                for (int rank = 0; rank < 6; rank++)
                {
                    int displayed_rank = 6 - rank;
                    Console.Write(displayed_rank + " │");

                    // Loop through each file in the current rank
                    for (int file = 0; file < 6; file++)
                    {
                        string piece = board[rank, file];
                        string symbol = piece_to_unicode(piece);

                        // Choose console color based on piece's case
                        if (char.IsUpper(piece.Length > 0 ? piece[0] : ' '))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }

                        Console.Write(" " + symbol + " ");
                        Console.ResetColor();
                        Console.Write("│");
                    }
                    // Display material count on a specific rank (here, rank 2)
                    if (rank == 2)
                    {
                        int material = material_count();
                        Console.Write($" 2        Material: {material}");
                        Console.WriteLine(" ");
                    }
                    else
                    {
                        Console.WriteLine(" " + displayed_rank);
                    }

                    // Print the row separators or final border
                    if (rank < 5)
                    {
                        Console.WriteLine("  ├───┼───┼───┼───┼───┼───┤");
                    }
                    else
                    {
                        Console.WriteLine("  └───┴───┴───┴───┴───┴───┘");
                    }
                }
                // Print bottom column labels for 6x6 board
                Console.WriteLine("    a   b   c   d   e   f");
            }
            else if (size == 5)
            {
                // Print column labels for 5x5 board
                Console.WriteLine("    a   b   c   d   e");
                Console.WriteLine("  ┌───┬───┬───┬───┬───┐");

                // Iterate through each rank for 5x5 board
                for (int rank = 0; rank < 5; rank++)
                {
                    int displayed_rank = 5 - rank;
                    Console.Write(displayed_rank + " │");

                    // Iterate through each file in the rank
                    for (int file = 0; file < 5; file++)
                    {
                        string piece = board[rank, file];
                        string symbol = piece_to_unicode(piece);

                        // Set color based on piece's case
                        if (char.IsUpper(piece.Length > 0 ? piece[0] : ' '))
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                        }

                        Console.Write(" " + symbol + " ");
                        Console.ResetColor();
                        Console.Write("│");
                    }
                    // Display material count on a specific rank (here, rank 1)
                    if (rank == 1)
                    {
                        int material = material_count();
                        Console.Write($" 1        Material: {material}");
                        Console.WriteLine(" ");
                    }
                    else
                    {
                        Console.WriteLine(" " + displayed_rank);
                    }

                    // Print row separators or the final board border
                    if (rank < 4)
                    {
                        Console.WriteLine("  ├───┼───┼───┼───┼───┤");
                    }
                    else
                    {
                        Console.WriteLine("  └───┴───┴───┴───┴───┘");
                    }
                }
                // Print bottom column labels for 5x5 board
                Console.WriteLine("    a   b   c   d   e");
            }
        }


        private int material_count()
        {
            int[] piece_values = new int[12] { 1, -1, 5, -5, 3, -3, 3, -3, 9, -9, 0, 0 };

            int eval = 0;

            for (int i = 0; i < 12; i++)
            {

                ulong bitboard = bitboards[i];


                for (; bitboard > 0;)
                {

                    eval += piece_values[i];

                    bitboard &= bitboard - 1;
                }

            }
            return eval;
        }

        public void increment()
        {
            fifty_move_rule += 1;
        }

        public void reset()
        {
            fifty_move_rule = 0;
        }

        public bool fifty_move_rule_test(Move move, int capture)
        {
            return move.piece > 1 && capture == 12;
        }

        public bool insufficient_material()
        {

            bool no_heavy = (bitboards[0] | bitboards[1] | bitboards[2] | bitboards[3] | bitboards[8] | bitboards[9]) == 0 ? true : false;

            ulong white_knightsbishops = bitboards[4] | bitboards[6];

            ulong black_knightsbishops = bitboards[5] | bitboards[7];
            bool no_white = white_knightsbishops == 0 ? true : false;
            bool no_black = black_knightsbishops == 0 ? true : false;
            if (white_knightsbishops != 0)
            {
                no_white = (white_knightsbishops &= white_knightsbishops - 1) == 0 ? true : false;
            }

            if (black_knightsbishops != 0)
            {
                no_black = (black_knightsbishops &= black_knightsbishops - 1) == 0 ? true : false;
            }


            return no_heavy && no_black && no_white;
        }

        public int check_mate(MoveGeneration move, Board board, PieceCall cache, int mod)
        {
            Movecheck all = move.all_moves(board, cache, mod, out int move_count, out ulong pawn_mask, out ulong capture_mask);
            if (move_count == 0)
            {
                if (all.check > 0)
                {
                    return 2;
                }
                return 1;
            }
            return 0;
        }

        // Returns a random position from the predefined set of positions
        public string get_random_position()
        {
            Random random = new Random();  // Initialize a random number generator
            int index = random.Next(0, positions.Length - 1);  // Generate a random index within the array bounds
            return positions[index];  // Return the position at the randomly selected index
        }


        public void set_bitboards(ulong[] bitboard)
        {
            for (int i = 0; i < 13; i++)
            {
                bitboards[i] = bitboard[i];
            }

            colour[0] = bitboards[0] | bitboards[2] | bitboards[4] | bitboards[6] | bitboards[8] | bitboards[10];

            colour[1] = bitboards[1] | bitboards[3] | bitboards[5] | bitboards[7] | bitboards[9] | bitboards[11];

            all_pieces = colour[0] | colour[1];
        }

        public Board Clone()
        {
            return new Board(FEN, "");
        }



    }


}
