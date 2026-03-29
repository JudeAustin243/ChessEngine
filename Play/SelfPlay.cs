using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





//using ChessDotNet;
using Microsoft.VisualBasic;
//using ChessEngineGUI;

namespace ChessEngine
{
    public class SelfPlay
    {
        Search White;
        Search Black;
        Board Board;
        PieceCall Cache;
        public SelfPlay(Search White, Search Black, Board Board, PieceCall Cache)
        {
            this.White = White;
            this.Black = Black;
            this.Board = Board;
            this.Cache = Cache;
        }
        public void play()
        {
            List<ulong[]> game_store = new List<ulong[]>();

            bool loop = true;

            int move = 0;

            Game game = new Game(0);

            Checkmate checkmate = new Checkmate();

            int WHITE = 0;
            int BLACK = 0;
            int DRAW = 0;
            int three = 0;
            int skip = 0;
            List<ulong> last_four = new List<ulong>();
            GameState gamestate = new GameState();

            string[] positions = { "r1b2rk1/1p3ppp/p1n1p3/q2pP3/3NnP2/P1P1B3/2P1B1PP/R3QR1K", "r1bq1rk1/1pp1bpp1/p2p1nnp/3Pp3/2B1P3/2NQBN1P/PPP2PP1/2KR3R", "4r1k1/p1p3pp/6p1/4p3/3q2P1/2rPR2P/P4P2/R3Q1K1", "2kr2nr/ppp3pp/3bb1q1/5p2/3n4/2NB4/PPPB1PPP/R2QNR1K", "r1b1r1k1/ppppqppp/2n5/8/4P3/2P1Q3/PP3PPP/R1B1KB1R", "r6r/pQpkq2p/3p2p1/4n3/3pn3/8/PPP2PPP/RN3RK1", "2kr1bnr/ppp2ppp/8/8/3n4/5B2/PPPB1PPP/RN1K3R", "r3r1k1/1b3pb1/p2p1qpp/1pp1p3/2P1P1PP/1P1P1N2/1P3P2/R2QRNK1", "r2q1rk1/ppp2ppp/2p1b3/2b5/4P3/5Q2/PPP2PPP/RNB2RK1", "7k/1p4p1/pP4np/P3p3/1Br5/7P/3n1PP1/1R4K1" };
            Piece p = new Piece();
            bool b = true;
            int count = 0;
            int v2 = 0;
            int v1 = 0;
            while (b)
            {
                if (count % 2 == 0)
                {
                    Random random = new Random();
                    int choice = random.Next(positions.Length - 1);
                    Board = new Board("startpos", "");
                    int size = 8;
                    int repeat = 0;
                    White = new Search(0);
                    //Search3 s = new Search3(0);
                    TranspositionTable transposition = new TranspositionTable(0);
                    Search Black = new Search(1);
                    Cache = p.get_cache();

                    last_four = [0, 0, 0, 0];
                    Board.set_repeat(last_four.ToArray());
                    //Black.iterative_deepening();
                    //GUI gui = new GUI();
                    //gui.Display(Board.bitboards);
                    //Board.update(new Move(0, 52, 36, 0, 0), Cache);
                    loop = true;
                    move = 0;
                    int opening = 0;
                    int capture = 12;
                    while (loop)

                    {
                        //Console.WriteLine();

                        Move best_move = new Move(-1, -1, -1, -1, -1, false);
                        MoveGeneration generate = new MoveGeneration();
                        Move[] all_moves = generate.all_moves(Board, Cache, move % 2, out int move_count, out ulong pawn, out ulong capture_mask).moves;

                        if (move % 2 == 0)
                        {
                            bool[] states = gamestate.get_gamestate(Board);
                            Board.set_states(states);
                            Board.set_gamestate();
                            best_move = White.iterative_deepening(Board, Cache, 100000, 100, out string eval);


                            capture = Board.update(best_move, Cache, 1);
                            game_store.Add(Board.bitboards);
                            Cache.Opponent_Move = best_move;
                            //White.set_board(Board);
                        }

                        if (move % 2 == 1)

                        {
                            bool[] states = gamestate.get_gamestate(Board);
                            Board.set_states(states);
                            Board.set_gamestate();
                            best_move = Black.iterative_deepening(Board, Cache, 100000, 100, out string eval);


                            capture = Board.update(best_move, Cache, 0);
                            game_store.Add(Board.bitboards);
                            Cache.Opponent_Move = best_move;


                            //Black.set_board(Board);
                            //loop = false;
                            //Console.WriteLine($"Piece :{best_move.piece}, Start :{best_move.start}, End :{best_move.end}");

                        }




                        if (Compare(best_move, all_moves, move_count))
                        {

                            //Board.update(best_move, Cache, move^1);
                            //analysis.store(Board.bitboards);
                            //legal = true;
                        }
                        else
                        {
                            Console.WriteLine("Illegal Move");
                            b = false;
                            best_move.view();
                            Board.print(8, false);
                            break;
                            //legal = false;
                        }
                        move++;
                        if (Board.fifty_move_rule_test(best_move, capture))
                        {
                            Board.increment();
                        }
                        else
                        {
                            Board.reset();
                        }
                        if (Board.fifty_move_rule == 50)
                        {
                            Console.WriteLine("DRAW");
                            DRAW++;
                            loop = false;
                            Board.print(size, true);
                        }
                        if (Board.insufficient_material())
                        {
                            Console.WriteLine("DRAW");
                            DRAW++;
                            loop = false;
                            Board.print(size, true);
                        }



                        //gui.Display(Board.bitboards);
                        //game.view_board(Board);
                        Board.print(size, true);
                        Console.WriteLine($"V2 wins: {v2}, V1 Wins: {v1}, Draws: {DRAW}");
                        //Console.WriteLine($"Piece :{best_move.piece}, Start :{best_move.start}, End :{best_move.end}");
                        MoveGeneration all = new MoveGeneration();
                        int mate = Board.check_mate(all, Board, Cache, move % 2);
                        if (mate == 2)
                        {
                            if (move % 2 == 0)
                            {
                                Console.WriteLine("BLACK WINS");
                                BLACK++;
                                v2++;
                                loop = false;
                                Board.print(size, true);
                            }
                            else
                            {
                                Console.WriteLine("WHITE WINS");
                                WHITE++;
                                v1++;
                                loop = false;
                                Board.print(size, true);
                            }
                        }
                        else if (mate == 1)
                        {
                            Console.WriteLine("STALEMATE");
                            DRAW++;
                            loop = false;
                            Board.print(size, true);

                        }
                        if (last_four.Count == 2)
                        {
                            if (last_four[0] == Board.all_pieces)
                            {
                                three += 1;
                                Board.set_repeat(last_four.ToArray());
                            }
                            else
                            {
                                three = 0;
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
                        // break;
                    }

                    //      Console.WriteLine(ITER+1);

                }
                if (count % 2 == 1)
                {
                    Random random = new Random();
                    int choice = random.Next(positions.Length - 1);
                    Board = new Board("startpos", "");
                    int size = 8;

                    Search White = new Search(0);
                    //Search3 s = new Search3(0);
                    TranspositionTable transposition = new TranspositionTable(0);
                    Search Black = new Search(1);
                    Cache = p.get_cache();

                    last_four = [0, 0, 0, 0];
                    Board.set_repeat(last_four.ToArray());
                    //Black.iterative_deepening();
                    //GUI gui = new GUI();
                    //gui.Display(Board.bitboards);
                    //Board.update(new Move(0, 52, 36, 0, 0), Cache);
                    loop = true;
                    move = 0;
                    int opening = 0;
                    int capture = 12;
                    while (loop)

                    {
                        //Console.WriteLine();

                        Move best_move = new Move(-1, -1, -1, -1, -1, false);
                        MoveGeneration generate = new MoveGeneration();
                        Move[] all_moves = generate.all_moves(Board, Cache, move % 2, out int move_count, out ulong pawn, out ulong capture_mask).moves;

                        if (move % 2 == 0)
                        {
                            bool[] states = gamestate.get_gamestate(Board);
                            Board.set_states(states);
                            Board.set_gamestate();
                            best_move = White.iterative_deepening(Board, Cache, 100000, 100, out string eval);


                            capture = Board.update(best_move, Cache, 1);
                            game_store.Add(Board.bitboards);
                            Cache.Opponent_Move = best_move;
                            //Console.WriteLine(Cache.Opponent_Move.piece);
                            //White.set_board(Board);
                        }

                        if (move % 2 == 1)

                        {
                            bool[] states = gamestate.get_gamestate(Board);
                            Board.set_states(states);
                            Board.set_gamestate();
                            best_move = Black.iterative_deepening(Board, Cache, 100000, 100, out string eval);


                            capture = Board.update(best_move, Cache, 0);
                            game_store.Add(Board.bitboards);
                            Cache.Opponent_Move = best_move;


                            //Black.set_board(Board);
                            //loop = false;
                            //Console.WriteLine($"Piece :{best_move.piece}, Start :{best_move.start}, End :{best_move.end}");

                        }




                        if (Compare(best_move, all_moves, move_count))
                        {

                            //Board.update(best_move, Cache, move^1);
                            //analysis.store(Board.bitboards);
                            //legal = true;
                        }
                        else
                        {
                            Console.WriteLine("Illegal Move");
                            b = false;
                            best_move.view();
                            Board.print(8, false);
                            break;
                            //legal = false;
                        }
                        move++;
                        if (Board.fifty_move_rule_test(best_move, capture))
                        {
                            Board.increment();
                        }
                        else
                        {
                            Board.reset();
                        }
                        if (Board.fifty_move_rule == 50)
                        {
                            Console.WriteLine("DRAW");
                            DRAW++;
                            loop = false;
                            Board.print(size, true);
                        }
                        if (Board.insufficient_material())
                        {
                            Console.WriteLine("DRAW");
                            DRAW++;
                            loop = false;
                            Board.print(size, true);
                        }



                        //gui.Display(Board.bitboards);
                        //game.view_board(Board);
                        Board.print(size, true);
                        Console.WriteLine($"V2 wins: {v2}, V1 Wins: {v1}, Draws: {DRAW}");
                        //Console.WriteLine($"Piece :{best_move.piece}, Start :{best_move.start}, End :{best_move.end}");
                        MoveGeneration all = new MoveGeneration();
                        int mate = Board.check_mate(all, Board, Cache, move % 2);
                        if (mate == 2)
                        {
                            if (move % 2 == 0)
                            {
                                Console.WriteLine("BLACK WINS");
                                BLACK++;
                                v1++;
                                loop = false;
                                Board.print(size, true);
                            }
                            else
                            {
                                Console.WriteLine("WHITE WINS");
                                WHITE++;
                                v2++;
                                loop = false;
                                Board.print(size, true);
                            }
                        }
                        else if (mate == 1)
                        {
                            Console.WriteLine("STALEMATE");
                            DRAW++;
                            loop = false;
                            Board.print(size, true);

                        }
                        if (last_four.Count == 2)
                        {
                            if (last_four[0] == Board.all_pieces)
                            {
                                three += 1;
                                Board.set_repeat(last_four.ToArray());
                            }
                            else
                            {
                                three = 0;
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
                        // break;
                    }


                }
                count++;
            }
            //stat.binomial_test(43, 38, 19, 0.05);

        }

        public bool Compare(Move move, Move[] moves, int move_count)
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

    }

}
