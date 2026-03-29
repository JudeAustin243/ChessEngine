using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ChessEngine
{
    // The Analysis class is responsible for storing game positions and providing move analysis based on stored positions.
    class Analysis
    {
        // Stores a list of game positions (each represented as an array of 13 ulong bitboards)
        List<ulong[]> Game_store = new List<ulong[]>();

        // The current board instance
        Board board;

        // Piece instance used to obtain piece-related cache
        Piece piece = new Piece();

        // Cache for piece move data and masks
        PieceCall cache;

        // The player's colour (e.g., 0 for white, 1 for black)
        int colour;

        // Constructor: initializes the analysis with a player's colour, a FEN string, and board size.
        public Analysis(int colour, string FEN, string size)
        {
            this.colour = colour;
            // Retrieve cache from the Piece object
            cache = piece.get_cache();
            // Initialize the board using the FEN string and board size
            board = new Board(FEN, size);
        }

        // Stores a given position (bitboard array) into the game store.
        public void store(ulong[] position)
        {
            ulong[] bitboards = new ulong[13];
            // Copy each of the 13 bitboards from the provided position
            for (int i = 0; i < 13; i++)
            {
                bitboards[i] = position[i];
            }
            // Add the copied position to the game store
            Game_store.Add(bitboards);
        }

        // Copies bitboard values from bitboard1 into bitboard2 for 13 entries.
        private ulong[] set(ulong[] bitboard2, ulong[] bitboard1)
        {
            for (int i = 0; i < 13; i++)
            {
                bitboard2[i] = bitboard1[i];
            }
            return bitboard2;
        }

        // Starts the analysis process, iterating through stored game positions and optionally analyzing moves.
        public void start(int size)
        {
            // Set the board to the first stored position
            board.set_bitboards(Game_store[0]);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------Analysis--------");
            // Print the board without highlighting moves (false parameter)
            board.print(size, false);

            // Copy the first position into a temporary variable and remove it from the store
            ulong[] temp = new ulong[13];
            temp = set(temp, Game_store[0]);
            Game_store.RemoveAt(0);

            int turn = 0;
            // Continue as long as there are positions in the game store.
            while (Game_store.Count() > 0)
            {
                Console.WriteLine("Press anything to go to next move");
                Console.ReadLine();

                // Set the board to the next stored position
                board.set_bitboards(Game_store[0]);
                // Print the board with move highlighting (true parameter)
                board.print(size, true);

                // If it's the player's turn (comparing turn modulo 2 to player's colour)
                if (turn % 2 == colour)
                {
                    Console.WriteLine("Press 'A' to analyse this move");
                    string next = Console.ReadLine();
                    // If the user chooses to analyse the move:
                    if (next == "A")
                    {
                        // Restore the board to the previous position from temp for analysis
                        board.set_bitboards(temp);
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Analysing...");
                        // Analyse the move and obtain the best move and its evaluation
                        Move best_move = analyse(out string eval);

                        // Print the board with move highlighting after analysis
                        board.print(size, true);
                        Console.WriteLine("----Best Move----");
                        best_move.map();  // Display the best move in algebraic notation
                        Console.WriteLine("----Evaluation----");
                        Console.WriteLine(eval);  // Output the evaluation of the best move
                    }
                }
                else
                {
                    // If it's not the player's turn, update the temporary stored position
                    temp = set(temp, Game_store[0]);
                }
                // Remove the current position from the game store after processing
                Game_store.RemoveAt(0);

                turn++;
            }
        }

        // Analyses the current board position to determine the best move.
        // Returns the best move and outputs an evaluation string.
        private Move analyse(out string eval)
        {
            // Print the current board without move highlighting (size 8, false)
            board.print(8, false);

            // Initialize a search engine for the given player's colour
            Search engine = new Search(colour);

            // Perform iterative deepening search to get the best move, with given ELO parameter (3000) and duration (5000 ms)
            Move best_move = engine.iterative_deepening(board, cache, 3000, 5000, out eval);

            // Update the board with the best move (switching turn using colour XOR 1)
            board.update(best_move, cache, colour ^ 1);

            return best_move;
        }
    }
}
