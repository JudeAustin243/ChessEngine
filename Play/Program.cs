using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;




namespace ChessEngine.Play
{
    class Program
    {

        static void Main(string[] args)
        {
            Magic magic = new Magic();


            Board board = new Board("startpos", "");


            Piece pieces = new Piece();



            PieceCall cache = pieces.get_cache();


            Console.WriteLine("PLAY ENGINE: E| RUN TEST MATCHES: M| RUN PERFT: P| RUN TEST SEARCH: S| RUN NODES: N | DEBUG: D");

            string? answer = Console.ReadLine();



            if (answer == "E")
            {

                Engine engine = new Engine();

                engine.play();

            }

            if (answer == "M")
            {
                Search white = new Search(0);

                Search black = new Search(1);

                SelfPlay engine = new SelfPlay(white, black, board, cache);

                engine.play();
            }
            if (answer == "P")
            {
                MoveGeneration perft = new MoveGeneration();
                perft.iterative_test(board, cache, 0, 0);
            }
            if (answer == "S")
            {
                string answer2 = Console.ReadLine();

                if (answer2 == "0")
                {
                    board.set_repeat([]);
                    Search white2 = new Search(1);
                    white2.iterative_test(board, cache);
                }
                else
                {
                    board.set_repeat([]);
                    Search black2 = new Search(1);
                    black2.iterative_test(board, cache);
                }
            }
            if (answer == "D")
            {
                int colour = 0;
                MoveGeneration perft;
                Console.WriteLine("Depth");
                int depth = Convert.ToInt32(Console.ReadLine());
                perft = new MoveGeneration();
                perft.PerftDivide(board, cache, colour, depth);
                while (true)
                {
                    Console.WriteLine("Piece");
                    int piece = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("Start");
                    int start = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("end");
                    int end = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("promotion");
                    int promotion = Convert.ToInt32(Console.ReadLine());
                    Move move = new Move(piece, start, end, 0, promotion, false);
                    colour ^= 1;
                    depth -= 1;
                    board.update(move, cache, colour ^ 1);
                    perft = new MoveGeneration();
                    perft.PerftDivide(board, cache, colour, depth);

                }
            }
            if (answer == "N")
            {



                string[] positions = { "7B/1b5p/R2rk3/2PR1r1p/1bP3p1/7p/4P2K/8", "2R5/1P4k1/Qr6/1p2P2p/R3K1p1/1P1p1N1N/4n3/8", "1N3K2/3P1P2/5p2/3Rp2n/5p2/p2BBN1k/3r3b/8", "8/B7/2b5/5P2/6k1/K7/P7/8", "8/5N2/8/2KP4/4k3/7P/6p1/8", "8/8/3P4/P7/1k2P3/5P2/2K5/8" };
                int node2;
                int node1;
                int score2 = 0;
                int score1 = 0;
                for (int i = 0; i < 6; i++)
                {
                    string fen = positions[i];
                    board = new Board(fen, "");
                    //board.print(8,true);
                    board.set_repeat([]);
                    Search V2 = new Search(0);
                    node2 = V2.node_test(board, cache, out double time);
                    Console.WriteLine("V2: Iterations: " + node2 + " Time: " + time);
                    Search V1 = new Search(0);
                    node1 = V1.node_test(board, cache, out double time2);
                    Console.WriteLine("V1: Iterations: " + node1 + " Time: " + time2);
                    if (node2 > node1)
                    {
                        score1 += 1;
                    }
                    if (node2 < node1)
                    {
                        score2 += 1;
                    }
                    Console.WriteLine();

                }

                Console.WriteLine("V2: " + score2);
                Console.WriteLine("V1: " + score1);



            }

        }
    }
}
