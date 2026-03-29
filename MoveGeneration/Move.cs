using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChessEngine
{
    // Struct representing a move in the chess game, containing details such as piece type, start and end positions, and promotion information
    public struct Move
    {
        public int piece;           // Piece type (e.g., pawn, rook, knight, etc.)
        public int start;           // Starting position (index) of the move
        public int end;             // Ending position (index) of the move
        public int captureval;      // The value of the captured piece (if any)
        public int ispromotion;     // Indicates if the move is a promotion (e.g., for pawns)
        public bool iscapture;      // Whether the move is a capture

        // Null move, used as a default placeholder (invalid move)
        public static readonly Move Null = new Move(0, 0, 0, 0, 0, false);

        // Returns true if the current move is a Null move
        public bool IsNull => Equals(Null);

        // Constructor to initialize a move with all relevant information
        public Move(int piece, int start, int end, int captureval, int ispromotion, bool iscapture)
        {
            this.piece = piece;
            this.start = start;
            this.end = end;
            this.captureval = captureval;
            this.ispromotion = ispromotion;
            this.iscapture = iscapture;
        }

        // Encodes the move into a single integer using bitwise operations
        public int Encode()
        {
            int encodedMove = 0;

            // Encode the end position (6 bits)
            encodedMove |= end & 0b111111;

            // Encode the start position (6 bits), shifted by 6 positions
            encodedMove |= (start & 0b111111) << 6;

            // Encode the piece type (4 bits), shifted by 12 positions
            encodedMove |= (piece & 0b1111) << 12;

            // Encode the promotion flag (3 bits), shifted by 16 positions
            encodedMove |= (ispromotion & 0b111) << 16;

            return encodedMove;
        }

        // Decodes an encoded move back into a Move struct
        public static Move Decode(int encodedMove)
        {
            // Extract the end position (6 bits)
            int endIndex = encodedMove & 0b111111;

            // Extract the start position (6 bits), shifted by 6 positions
            int startIndex = encodedMove >> 6 & 0b111111;

            // Extract the piece type (4 bits), shifted by 12 positions
            int piece = encodedMove >> 12 & 0b1111;

            // Extract the promotion flag (3 bits), shifted by 16 positions
            int promotion = encodedMove >> 16 & 0b111;

            // Return the decoded move
            return new Move(piece, startIndex, endIndex, 0, promotion, false);
        }

        // Prints the details of the move to the console
        public void view()
        {
            Console.WriteLine($"Piece :{piece}, Start:{start}, End:{end}");
        }

        // Maps the move from start to end in algebraic notation (e.g., e2 to e4) and prints it
        public void map()
        {
            string[] names = { "Pawn", "Pawn", "Rook", "Rook", "Knight", "Knight", "Bishop", "Bishop", "Queen", "Queen", "King", "King" };

            // Calculate row and column for start position
            int row = start / 8;
            int col = start % 8;
            char file = (char)('a' + col);
            int rank = 8 - row;

            // Calculate row and column for end position
            int row2 = end / 8;
            int col2 = end % 8;
            char file2 = (char)('a' + col2);
            int rank2 = 8 - row2;

            // Print the piece and its move in algebraic notation
            Console.WriteLine($"Piece: {names[piece]} Start: {file}{rank} End: {file2}{rank2}");
        }
    }



}

