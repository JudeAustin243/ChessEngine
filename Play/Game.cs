using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace ChessEngine
{

    public class Game
    {
        int colour;

        Board board = new Board("startpos", "N");



        public Game(int Acolour)
        {
            colour = Acolour;
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

        private static Dictionary<string, int> square_name()

        {

            List<string> names = new List<string>();

            string[] alpha = { "a", "b", "c", "d", "e", "f", "g", "h" };

            string[] num = { "1", "2", "3", "4", "5", "6", "7", "8" };

            int[] index = { 56, 48, 40, 32, 24, 16, 8, 0, 57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18, 10, 2, 59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36, 28, 20, 12, 4, 61, 53, 45, 37, 29, 21, 13, 5, 62, 54, 46, 38, 30, 22, 14, 6, 63, 55, 47, 39, 31, 23, 15, 7 };

            foreach (string a in alpha)

            {

                foreach (string n in num)

                {

                    names.Add(a + n);

                }

            }

            Dictionary<string, int> map = new Dictionary<string, int>();

            for (int i = 0; i < 64; i++)

            {

                map.Add(names[i], index[i]);

            }



            return map;



        }

        private static Dictionary<char, int> dict(Dictionary<char, int> x)

        {

            string fen_dict_add = "PpRrNnBbQqKk";

            int count = 0;

            foreach (char i in fen_dict_add)

            {

                x.Add(i, count);

                count++;

            }

            return x;

        }



        public void view_board(Board b)
        {
            Dictionary<int, string> board_dict = new Dictionary<int, string>();

            string[] pieces = new string[12] { "P", "p", "R", "r", "N", "n", "B", "b", "Q", "q", "K", "k" };

            string[] board_squares = new string[64];

            Array.Fill(board_squares, "0");

            int count = 0;



            //Console.WriteLine(board_string);

            for (int i = 0; i < 12; i++)

            {
                ulong bitboard = b.bitboards[i];

                string board_string = b.bit64(bitboard);

                foreach (char piece in board_string)

                {

                    if (piece == '1')

                    {

                        board_squares[count] = pieces[i];
                        //Console.WriteLine(board_squares[count]);



                    }

                    if (piece == '0' && board_squares[count] == "0")

                    {

                        board_squares[count] = "-";

                    }

                    count++;

                }

                count = 0;

            }

            count = 0;

            board_dicts(board_dict, board_squares);


            for (int i = 0; i < 8; i++)

            {

                for (int j = 0; j < 8; j++)

                {

                    Console.Write("  " + board_dict[63 - count] + "  ");

                    //Console.WriteLine(board_dict[63-count]);
                    //Console.WriteLine(count);
                    count++;

                }

                Console.WriteLine();

                Console.WriteLine();

            }
        }


    }
}
