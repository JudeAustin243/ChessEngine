using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;



namespace ChessEngine
{
    public class Piece
    {

        ulong move;

        public int colour;

        public int start;

        public static ulong[] board_parts = { 255, 18374686479671623680, 18446462598732840960, 65535, 72340172838076673, 9259542123273814144, 217020518514230019, 13889313184910721216 };

        public static ulong[] array_column = { 72340172838076673, 144680345676153346, 289360691352306692, 578721382704613384, 1157442765409226768, 2314885530818453536, 4629771061636907072, 9259542123273814144 };

        public static ulong[] SE_NW = new ulong[15] { 128, 32832, 8405024, 2151686160, 550831656968, 141012904183812, 36099303471055874, 9241421688590303745, 4620710844295151872, 2310355422147575808, 1155177711073755136, 577588855528488960, 288794425616760832, 144396663052566528, 72057594037927936 };

        public static ulong[] SW_NE = new ulong[15] { 1, 258, 66052, 16909320, 4328785936, 1108169199648, 283691315109952, 72624976668147840, 145249953336295424, 290499906672525312, 580999813328273408, 1161999622361579520, 2323998145211531264, 4647714815446351872, 9223372036854775808 };

        public ulong[] left_move = new ulong[64];

        public ulong[] right_move = new ulong[64];

        public ulong[] up_move = new ulong[64];

        public ulong[] down_move = new ulong[64];

        public ulong[] rook_mask = new ulong[64];

        public ulong[] bishop_mask = new ulong[64];

        public ulong[,] rook_look = new ulong[64, 4096];

        public ulong[,] bishop_look = new ulong[64, 512];


        public (ulong, double)[] queen_pawn_look = new (ulong, double)[4096];

        public (ulong, double)[] king_pawn_look = new (ulong, double)[4096];



        public ulong queen_mask = 0x000F0F0F0F0F0F00;

        public ulong king_mask = 0x00F0F0F0F0F0F000;



        public ulong[] NW_move = new ulong[64];

        public ulong[] SW_move = new ulong[64];

        public ulong[] NE_move = new ulong[64];

        public ulong[] SE_move = new ulong[64];

        public const ulong Wking_side = 6917529027641081856;

        public const ulong Wqueen_side = 1008806316530991104;

        public const ulong Bqueen_side = 14;

        public const ulong Bking_side = 96;

        public string[] pieces = { "P", "p", "R", "r", "N", "n", "B", "b", "Q", "q", "K", "k" };

        //public static Board board = new Board("startpos", );

        public string bit64(ulong n)
        {

            string binaryString = Convert.ToString((long)n, 2);

            binaryString = binaryString.PadLeft(64, '0');

            return binaryString;
        }

        public void show(ulong n)

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
            Console.WriteLine();

        }

        public virtual void place(int new_start)
        {
            start = new_start;
        }


        public virtual ulong moves(int startIndex, Board board, PieceCall cache, Check info, ulong[] pins, ulong filter)
        {
            return 0ul;
        }
        public virtual ulong capture_moves(int startIndex, Board board, PieceCall cache, Check info, ulong[] pins, ulong check, ulong filter)
        {
            return 0ul;
        }

        public Dictionary<int, ulong> column(Dictionary<int, ulong> column_dict, ulong[] array, int c)
        {

            int count = c;

            foreach (ulong i in array)

            {

                column_dict.Add(count, i);

                count++;

            }

            return column_dict;

        }
        //Instantiate all classes before the main program is run so contructor methods do not need to be called everytime they need to be used
        public PieceCall get_cache()
        {
            Pawn White_Pawn = new Pawn(0);
            Pawn Black_Pawn = new Pawn(1);
            Rook White_Rook = new Rook(0);
            Rook Black_Rook = new Rook(1);
            Knight White_Knight = new Knight(0);
            Knight Black_Knight = new Knight(1);
            Bishop White_Bishop = new Bishop(0);
            Bishop Black_Bishop = new Bishop(1);
            Queen White_Queen = new Queen(0);
            Queen Black_Queen = new Queen(1);
            King White_King = new King(0);
            King Black_King = new King(1);
            Castling Castle = new Castling();
            EnPassant Passant = new EnPassant();
            Global Global = new Global(-1, -1, true, true, true, true);
            Check Illegal = new Check();
            Move Opponent_Move = new Move(-1, -1, -1, -1, -1, false);
            PieceCall cache = new PieceCall(White_Pawn, Black_Pawn, White_Rook, Black_Rook, White_Knight, Black_Knight, White_Bishop, Black_Bishop, White_Queen, Black_Queen, White_King, Black_King, Castle, Passant, Global, Illegal, Opponent_Move);
            return cache;
        }
    }
}
