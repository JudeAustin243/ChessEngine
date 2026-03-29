using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.VisualBasic;



namespace ChessEngine
{
    public class Engine
    {
        public void play()
        {
            // Declaration of variables needed
            Board board = new Board("", "");  // Instantiate board with empty initial parameters

            string FEN;            // Stores the FEN string representing the board position
            int ELO;               // Represents the engine's ELO rating
            string? GAMESTATE;     // Holds the game state: "N", "early", "middle", or "end"
            string BOARDSIZE;      // User input for board size (e.g., "N" for 8x8, "7x7", etc.)
            bool RANDOM;           // (Potentially for future use) Flag for random position selection
            int colour = 1;        // Numeric representation of player's colour (default 1 for black)
            int size;              // Numeric board size deduced from user input
            int time;              // Engine's thinking time in milliseconds

            // ------------------------------------------------------------------
            // FEN String Input and Validation
            // ------------------------------------------------------------------

            // Asks player to input FEN string; instructs to type 'N' for starting position
            Console.WriteLine("Set FEN? Type 'N' for starting position");
            FEN = Console.ReadLine();

            // Method to check for valid FEN strings using check_fen method
            bool valid_fen = check_fen(FEN);

            // Iteratively continues to ask until a valid FEN string is entered
            while (!valid_fen)
            {
                Console.WriteLine("Enter a valid FEN");
                FEN = Console.ReadLine();
                valid_fen = check_fen(FEN);  // Re-validate the new input
            }

            // ------------------------------------------------------------------
            // Random Position Option
            // ------------------------------------------------------------------

            // If user typed "N", prompt to optionally set a random position
            if (FEN == "N")
            {
                Console.WriteLine("Set random position?- Press 'Y' for yes and anything else for no");
                string answer = Console.ReadLine();

                if (answer == "Y")
                {
                    // Use board class method to retrieve a random position FEN
                    FEN = board.get_random_position();
                }
                else
                {
                    // Default to starting position FEN if random is not selected
                    FEN = "startpos";
                }
            }

            // ------------------------------------------------------------------
            // ELO Rating Input and Validation
            // ------------------------------------------------------------------

            // Asks user to input a valid ELO for the engine between 100 and 2000
            Console.WriteLine("Set ELO for engine between 100 and 2000");
            string input = Console.ReadLine();

            // Continue prompting until the entered ELO is an integer within the allowed range
            while (!int.TryParse(input, out ELO) || ELO < 100 || ELO > 2000)
            {
                Console.WriteLine("Enter a valid ELO");
                input = Console.ReadLine();
            }

            // ------------------------------------------------------------------
            // Game State Input and Validation
            // ------------------------------------------------------------------

            // Ask the user to set the game state (e.g., early, middle, endgame)
            Console.WriteLine("Set Gamestate?- Type 'N' for no, 'early' for early game, 'middle' for middle game and 'end' for endgame");
            GAMESTATE = Console.ReadLine();

            // Allowed game state values
            string[] valids = { "N", "early", "middle", "end" };

            // Loop until the user inputs one of the valid game state options
            while (!valids.Contains(GAMESTATE))
            {
                Console.WriteLine("Enter a valid gamestate");
                GAMESTATE = Console.ReadLine();
            }

            // ------------------------------------------------------------------
            // Board Size Input, Mapping and Validation
            // ------------------------------------------------------------------

            // Prompt user to set the board size; 'N' represents standard 8x8 board
            Console.WriteLine("Set board size?- Type 'N' for 8x8, '7x7' for 7x7, '6x6' for 6x6 and '5x5' for 5x5");
            BOARDSIZE = Console.ReadLine();

            // Arrays defining valid board size inputs, corresponding ELO adjustments, and starting FEN strings
            string[] valids2 = { "N", "7x7", "6x6", "5x5" };
            int[] ELOs = { int.MaxValue, 1001, 101, 101 };
            string[] size_FEN = { FEN, board.startpos_7, board.startpos_6, board.startpos_5 };

            // Validate user input for board size until it matches one of the allowed options
            while (!valids2.Contains(BOARDSIZE))
            {
                Console.WriteLine("Enter a valid board size");
                BOARDSIZE = Console.ReadLine();
            }

            // Determine board size, update FEN and ELO based on the chosen board size using the index in the valid options array
            size = 8 - Array.IndexOf(valids2, BOARDSIZE);
            FEN = size_FEN[Array.IndexOf(valids2, BOARDSIZE)];
            ELO = ELOs[Array.IndexOf(valids2, BOARDSIZE)];

            // ------------------------------------------------------------------
            // Player Colour Selection
            // ------------------------------------------------------------------

            // Ask the user whether they want to play as white or black
            Console.WriteLine("Play as black or white? 'W' for white and anything else for black");

            string? colour_choice = Console.ReadLine();

            // If the user chooses white, set colour to 0; otherwise, remain black (default value of 1)
            if (colour_choice == "W")
            {
                colour = 0;
            }

            // ------------------------------------------------------------------
            // Engine Thinking Time Configuration
            // ------------------------------------------------------------------

            // Prompt user to set the engine's thinking time in seconds
            Console.WriteLine("Set Engine's thinking time in seconds");
            time = Convert.ToInt16(Console.ReadLine());

            // Convert the time from seconds to milliseconds for further use in the engine
            time *= 1000;


            List<ulong[]> game_store = new List<ulong[]>();

            Dictionary<ulong, int> repetitionCount = new Dictionary<ulong, int>();

            Checkmate checkmate = new Checkmate();

            int WHITE = 0;
            int BLACK = 0;
            int DRAW = 0;
            int three = 0;

            List<ulong> last_four = new List<ulong>();
            GameState gamestate = new GameState();
            Piece Piece = new Piece();
            PieceCall Cache = Piece.get_cache();
            Game game = new Game(0);
            Analysis analysis = new Analysis(colour, FEN, BOARDSIZE);

            for (int ITER = 0; ITER < 2000; ITER++)
            {
                Board Board = new Board(FEN, BOARDSIZE);
                Board.print(size, true);
                analysis.store(Board.bitboards);
                Search Engine = new Search(colour ^ 1);
                bool loop = true;
                int move = 0;
                bool legal = false;
                int shift = 8 * (8 - size);
                int repeat = 0;
                while (loop)
                {

                    if (move % 2 == (colour ^ 1))
                    {
                        bool[] states = gamestate.get_gamestate(Board);
                        Board.set_states(states);
                        Board.set_gamestate();
                        Move best_move = Engine.iterative_deepening(Board, Cache, ELO, time, out string eval);
                        Console.WriteLine($"Piece :{best_move.piece}, Start :{best_move.start}, End :{best_move.end}");

                        Board.update(best_move, Cache, colour);
                        analysis.store(Board.bitboards);
                        game_store.Add(Board.bitboards);
                        Engine = new Search(colour ^ 1);
                        legal = true;
                    }

                    if (move % 2 == colour)

                    {
                        Console.WriteLine("Piece");

                        int piece = Array.IndexOf(Piece.pieces, Console.ReadLine());

                        Console.WriteLine("Start");

                        int start = notation_to_index(Console.ReadLine()) - shift;

                        Console.WriteLine("End");

                        int end = notation_to_index(Console.ReadLine()) - shift;

                        int promote = 0;

                        if (colour == 1)
                        {
                            if (piece == 1 && start > 47 && start < 56)
                            {
                                Console.WriteLine("Promote");
                                promote = Array.IndexOf(Piece.pieces, Console.ReadLine());
                            }


                        }
                        if (colour == 0)
                        {
                            if (piece == 0 && start > 7 && start < 16)
                            {
                                Console.WriteLine("Promote");

                                promote = Array.IndexOf(Piece.pieces, Console.ReadLine());
                            }
                            else
                            {
                                promote = 0;
                            }

                        }

                        Move new_move = new Move(piece, start, end, 0, promote, false);

                        MoveGeneration generate = new MoveGeneration();

                        Move[] all_moves = generate.all_moves(Board, Cache, colour, out int move_count, out ulong pawn, out ulong capture_mask).moves;

                        if (compare(new_move, all_moves, move_count))
                        {

                            Board.update(new_move, Cache, colour ^ 1);
                            analysis.store(Board.bitboards);
                            legal = true;
                        }
                        else
                        {
                            Console.WriteLine("Illegal Move");
                            legal = false;
                        }

                    }
                    if (legal)
                    {
                        move++;
                        Console.WriteLine(size);
                        Board.print(size, true);

                        MoveGeneration all = new MoveGeneration();
                        if (checkmate.mate(all, Board, Cache, move % 2) == 2)
                        {
                            if (move % 2 == 0)
                            {
                                Console.WriteLine("BLACK WINS");
                                BLACK++;
                                loop = false;

                            }
                            else
                            {
                                Console.WriteLine("WHITE WINS");
                                WHITE++;
                                loop = false;

                            }
                        }
                        else if (checkmate.mate(all, Board, Cache, move % 2) == 1)
                        {
                            Console.WriteLine("STALEMATE");
                            DRAW++;
                            loop = false;


                        }
                        //THREE FOLD LOGIC- INCORRECT
                        if (last_four.Count == 2)
                        {
                            if (last_four[0] == Board.all_pieces)
                            {
                                three += 1;
                                Board.set_repeat(last_four.ToArray());
                                repeat += 2;
                            }
                            else
                            {
                                three = 0;
                                repeat = 0;
                            }
                            if (three == 3)
                            {
                                Console.WriteLine("DRAW");
                                DRAW += 1;
                                Board.print(size, true);
                                loop = false;
                            }
                            Board.threefold = three;
                            last_four.Clear();

                        }

                        last_four.Add(Board.all_pieces);
                    }


                    //break;
                }
                //break;

                Console.WriteLine($"V2 wins: {BLACK}, V1 Wins: {WHITE}, Draws: {DRAW}");
                Console.WriteLine("Press 'A' to start analysis and anything else to begin a new game");
                string answer = Console.ReadLine();
                if (answer == "A")
                {
                    analysis.start(size);
                }
            }
        }


        // Checks if the FEN string contains only valid characters from the allowed set
        private bool check_fen(string? fen)
        {
            string valid = "prnbkPRNBKqQ/12345678";

            // Loop through each character in FEN and check if it is valid
            foreach (char character in fen)
            {
                if (!valid.Contains(character))
                {
                    return false; // Return false if any invalid character is found
                }
            }

            return true; // Return true if all characters are valid
        }

        public bool compare(Move move, Move[] moves, int move_count)
        {
            for (int i = 0; i < move_count; i++)
            {
                if (move.Encode() == moves[i].Encode())
                {
                    return true;
                }
            }
            return false;
        }
        public int notation_to_index(string notation)
        {
            if (notation.Length < 3 && notation.Length > 1)
            {
                char file = notation[0];
                int rank = int.Parse(notation[1].ToString());
                return (8 - rank) * 8 + (file - 'a');
            }
            return -1;

        }



    }
}
