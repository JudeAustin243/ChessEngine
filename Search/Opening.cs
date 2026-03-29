using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;



namespace ChessEngine
{
    // This class handles opening book functionality for the chess engine.
    public class Openings
    {
        // Method to find the best moves from an opening book based on the current board hash.
        public List<Move> find_best_moves(ulong current_hash, Board board, PieceCall cache, int mod)
        {
            // List to store the best moves found in the opening book.
            List<Move> best_moves = new List<Move>();

            // Convert the current board hash to a string to use as a key.
            string current_hash_str = current_hash.ToString();

            // Define the file path for the opening book.
            string file_path = "C:\\Users\\judea\\source\\repos\\NEA\\NEA_opening_book.txt";

            // Check if the file exists; if not, log an error and return an empty list.
            if (!File.Exists(file_path))
            {
                Console.WriteLine($"Error: The file '{file_path}' was not found.");
                return best_moves;
            }

            // Instantiate a ChessMoveParser using the current board and cache.
            ChessMoveParser parse = new ChessMoveParser(board, cache);

            try
            {
                // Open the file for reading.
                using (StreamReader sr = new StreamReader(file_path))
                {
                    string line;
                    int line_number = 0; // Keep track of the current line number
                    int count = 0;       // Count how many moves have been added

                    // Read each line from the file.
                    while ((line = sr.ReadLine()) != null)
                    {
                        line_number++;
                        line = line.Trim();  // Remove any leading/trailing whitespace

                        // Skip empty lines.
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        // Each line is expected to have two parts separated by a tab:
                        // the hash key and the move in algebraic notation.
                        string[] parts = line.Split('\t');

                        // If the line doesn't have exactly 2 parts, log a warning.
                        if (parts.Length != 2)
                        {
                            Console.WriteLine($"Warning: Line {line_number} is malformed: '{line}'");
                            continue;
                        }

                        // Extract the hash key and the move from the line.
                        string hash_key = parts[0];
                        string move = parts[1];

                        // Ignore castling moves ("O-O" or "O-O-O") in this context.
                        if (move != "O-O" && move != "O-O-O")
                        {
                            // If the hash key from the file matches the current board hash...
                            if (hash_key == current_hash_str)
                            {
                                // Parse the move from its string notation and add it to the best moves list.
                                best_moves.Add(parse.parse_move(move, mod));
                                count++;
                            }
                        }

                        // Stop after collecting 10 moves.
                        if (count == 10)
                        {
                            break;
                        }
                    }
                }
            }
            catch (IOException e)
            {
                // Log any I/O errors that occur while reading the file.
                Console.WriteLine($"IOError while reading the file: {e.Message}");
            }

            // Return the list of best moves found.
            return best_moves;
        }
    }

    // This class processes chess CSV/PGN files to extract moves from games.
    class ChessCSVProcessor
    {
        // Public method to get moves from a chess PGN file.
        public List<List<string>> get_moves()
        {
            // Define the path to the PGN file.
            string csv_file_path = "C:\\Users\\judea\\source\\repos\\NEA\\twic1569.pgn";
            return extract_moves_from_csv(csv_file_path);
        }

        // Extracts moves from the given file path by processing its content.
        static List<List<string>> extract_moves_from_csv(string filePath)
        {
            // List to store moves for each game found.
            List<List<string>> games_moves = new List<List<string>>();

            try
            {
                // Read the entire file content.
                string new_line = File.ReadAllText(filePath);

                // Split the content into games using "[Event " as a delimiter.
                string[] lines = new_line.Split("[Event ");

                // Process each game individually.
                foreach (var line in lines)
                {
                    // Extract moves for a single game.
                    List<string> moves = extract_moves_from_game(line);

                    // Add the moves list to the games list if moves were found.
                    if (moves.Count > 0)
                    {
                        games_moves.Add(moves);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during processing.
                Console.WriteLine("Error processing CSV: " + ex.Message);
            }

            return games_moves;
        }

        // Extracts a list of moves from a single game data string.
        static List<string> extract_moves_from_game(string gameData)
        {
            List<string> moves = new List<string>();

            try
            {
                // Define a regex pattern to match common move notations, including castling.
                string moves_pattern = @"\b(?:O-O(?:-O)?|[a-h][1-8]|[QRNBK]?[a-h]?[1-8]?[x-]?[a-h][1-8](=[QRNB])?[+#]?)\b";

                // Use Regex to find all matches within the game data.
                MatchCollection matches = Regex.Matches(gameData, moves_pattern);

                foreach (Match match in matches)
                {
                    // Remove any capture markers ('x') from the move notation.
                    string news = match.Value.Replace("x", "");
                    moves.Add(news);
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during move extraction.
                Console.WriteLine("Error extracting moves: " + ex.Message);
            }

            return moves;
        }

        // Main method for processing chess games and saving an opening book.
        static void Man(string[] args)
        {
            // Create an instance of the processor and get all moves from the PGN file.
            ChessCSVProcessor games = new ChessCSVProcessor();
            List<List<string>> moves = games.get_moves();

            // List to hold the opening book entries as (hash key, move label) pairs.
            var opening_book = new List<(ulong Key, string Label)>();
            int count = 0;
            int count2 = 0;

            // Instantiate a Piece and retrieve its cache.
            Piece piece = new Piece();
            PieceCall cache = piece.get_cache();

            // Process each game's move list.
            foreach (List<string> list_move in moves)
            {
                // Initialize a new board for each game starting from the default starting position.
                Board board = new Board("startpos", "N");
                ChessMoveParser parse = new ChessMoveParser(board, cache);
                int mod = 0;

                // Get the board key for the starting position.
                ulong key = board.get_key(mod, cache);
                // The first element in the list is considered the label.
                string label = list_move[0];

                // Add the starting position to the opening book.
                opening_book.Add((key, label));
                count++;

                // Iterate over the moves of the game (except the last element which is used as the label for the next position).
                for (int i = 0; i < list_move.Count() - 1; i++)
                {
                    // Parse the move from the string notation.
                    Move move = parse.parse_move(list_move[i], mod);
                    // If an invalid move is encountered or more than 20 moves, break out of the loop.
                    if (move.piece == -1 || i > 20)
                    {
                        break;
                    }

                    // Update the board with the move.
                    board.update(move, cache, mod ^ 1);
                    mod ^= 1;
                    // Get the updated board key.
                    key = board.get_key(mod, cache);

                    // Use the next move notation as the label for the new position.
                    label = list_move[i + 1];
                    opening_book.Add((key, label));
                    count2++;
                }
            }

            // Save the opening book to the user's desktop.
            string desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string file_path = Path.Combine(desktop_path, "NEA_opening_book.txt");
            save_opening_book(opening_book, file_path);

            Console.WriteLine($"Opening book saved to {file_path}");
        }

        // Saves the opening book entries to a file.
        static void save_opening_book(List<(ulong Key, string Label)> opening_book, string file_path)
        {
            // Append each entry as a new line with the key and label separated by a tab.
            using (StreamWriter writer = new StreamWriter(file_path, append: true))
            {
                foreach (var entry in opening_book)
                {
                    writer.WriteLine($"{entry.Key}\t{entry.Label}");
                }
            }
        }
    }

    // This class is responsible for parsing chess moves from their notation.
    public class ChessMoveParser
    {
        public Piece piece = new Piece();
        public Board board;      // The current board state
        PieceCall cache;        // Cache used for move generation or other calculations

        // Constructor initializing the parser with a board and a cache.
        public ChessMoveParser(Board Board, PieceCall Cache)
        {
            cache = Cache;
            board = Board;
        }

        // Converts algebraic notation (e.g., "e4") to a board index (0-63).
        public static int notation_to_index(string notation)
        {
            char file = notation[0];                           // File letter (a-h)
            int rank = int.Parse(notation[1].ToString());        // Rank number (1-8)
            return (8 - rank) * 8 + (file - 'a');                // Convert to index (0 = a8, 63 = h1)
        }

        // Parses a chess move from string notation based on the current board state.
        public Move parse_move(string move, int mod)
        {
            bool pawn = false;

            // Handle castling for white and black separately.
            if (move == "O-O" && mod == 0)
            {
                return new Move(10, 60, 62, -1, 0, false);
            }
            if (move == "O-O-O" && mod == 0)
            {
                return new Move(10, 60, 58, -1, 0, false);
            }
            if (move == "O-O" && mod == 1)
            {
                return new Move(11, 4, 6, -1, 0, false);
            }
            if (move == "O-O-O" && mod == 1)
            {
                return new Move(11, 4, 2, -1, 0, false);
            }

            // Generate all legal moves from the current position.
            MoveGeneration generate = new MoveGeneration();
            var legal_moves = generate.all_moves2(board, cache, mod);

            int? specific_piece = null;
            string end_notation = move;

            // If the move starts with an uppercase letter, it specifies a piece type.
            if (char.IsUpper(move[0]))
            {
                specific_piece = piece_type_from_char(move[0], mod);
                // Remove the piece letter from the move notation.
                end_notation = move[1..];
            }
            else
            {
                // If no uppercase letter, then it's a pawn move.
                pawn = true;
            }

            int? promotion_piece = null;
            // If the move includes a promotion indicator ("="), return an invalid move.
            if (move.Contains("="))
            {
                return new Move(-1, -1, -1, -1, -1, false);
            }

            // Determine the destination square index based on the last two characters of the move.
            int end_index = notation_to_index(end_notation[^2..]);

            // Filter legal moves to those that match the destination square.
            var matching_moves = legal_moves.Where(m => m.end == end_index).ToList();

            // If a specific piece was indicated, further filter the moves.
            if (specific_piece.HasValue)
            {
                matching_moves = matching_moves.Where(m => m.piece == specific_piece.Value).ToList();
            }

            // If no matching moves found, return an invalid move.
            if (matching_moves.Count == 0)
            {
                return new Move(-1, -1, -1, -1, -1, false);
            }

            Move chosenMove = new Move(-1, -1, -1, -1, -1, false);

            // If there is only one matching move, choose it.
            if (matching_moves.Count == 1)
            {
                chosenMove = matching_moves.First();
            }
            else
            {
                // If multiple moves match, use disambiguation to select the correct move.
                chosenMove = disambiguate_move(move, matching_moves, pawn);
            }

            return chosenMove;
        }

        // Converts a promotion piece (e.g., "Q", "R", "B", "N") to an integer identifier.
        public static int promotion_to_piece(string promotion)
        {
            return promotion.ToUpper() switch
            {
                "Q" => 8,
                "R" => 6,
                "B" => 4,
                "N" => 2,
                _ => throw new ArgumentException("Invalid promotion piece")
            };
        }

        // Determines the piece type based on a character (e.g., 'K', 'Q', 'R', etc.)
        public static int piece_type_from_char(char pieceChar, int mod)
        {
            return pieceChar switch
            {
                'P' => 0 + mod,  // Pawn
                'N' => 4 + mod,  // Knight
                'B' => 6 + mod,  // Bishop
                'R' => 2 + mod,  // Rook
                'Q' => 8 + mod,  // Queen
                'K' => 10 + mod, // King
                _ => throw new ArgumentException("Invalid piece character")
            };
        }

        // Disambiguates a move when multiple moves end on the same square.
        public static Move disambiguate_move(string move, List<Move> matching_moves, bool pawn)
        {
            // For non-pawn moves with additional disambiguation info
            if (move.Length > 3 && pawn == false)
            {
                char disambiguation = move[^3];
                if (char.IsDigit(disambiguation))
                {
                    // If the disambiguation character is a digit, it represents the rank.
                    int rank_index = 8 - int.Parse(disambiguation.ToString());
                    return matching_moves.First(m => m.start / 8 == rank_index);
                }
                else
                {
                    // Otherwise, it represents the file.
                    int file_index = disambiguation - 'a';
                    return matching_moves.First(m => m.start % 8 == file_index);
                }
            }
            // For pawn moves or moves with minimal disambiguation info.
            if (move.Length >= 3)
            {
                char disambiguation = move[^3];
                if (char.IsDigit(disambiguation))
                {
                    int rank_index = 8 - int.Parse(disambiguation.ToString());
                    return matching_moves.First(m => m.start / 8 == rank_index);
                }
                else
                {
                    int file_index = disambiguation - 'a';
                    return matching_moves.First(m => m.start % 8 == file_index);
                }
            }

            // If no disambiguation is needed, return the first matching move.
            return matching_moves.First();
        }
    }
}
