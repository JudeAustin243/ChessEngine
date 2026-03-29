using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;






//using ChessDotNet;
using Microsoft.VisualBasic;

//using ChessEngineGUI;

namespace ChessEngine.Play
{
    public class Play
    {
        Board Board;
        Piece p;
        PieceCall Cache;
        Search White;
        Search Black;
        public Play()
        {
            Board = new Board("startpos", "");
            p = new Piece();
            Cache = p.get_cache();
            White = new Search(0);
            Black = new Search(1);
        }
        public void play()
        {
            List<ulong[]> game_store = new List<ulong[]>();

            Checkmate checkmate = new Checkmate();

            int WHITE = 0;
            int BLACK = 0;
            int DRAW = 0;
            int three = 0;
            int skip = 0;
            List<ulong> last_four = new List<ulong>();
            GameState gamestate = new GameState();
            string[] pieces = { "P", "p", "R", "r", "N", "n", "B", "b", "Q", "q", "K", "k" };

            Game game = new Game(0);
            for (int ITER = 0; ITER < 2000; ITER++)
            {
                Board Board = new Board("r2qk2r/1ppbbppp/p1np1n2/1B2p1B1/4P3/P1NP1N2/1PP2PPP/R2Q1RK1", "");
                int size = 8;
                int shift = 8 * (8 - size);
                Board.print(size, true);
                White = new Search(0);
                Black = new Search(1);
                bool loop = true;
                int move = 0;
                int opening = 0;
                bool legal = false;
                while (loop)
                {
                    //Console.WriteLine();

                    if (move % 2 == 0)
                    {
                        //bool[] states = gamestate.get_gamestate(Board);
                        //Board.set_states(states);
                        //Board.set_gamestate();
                        ////Move best_move = White.iterative_deepening(Board, Cache, ref opening, out string eval);
                        //Console.WriteLine($"Piece :{best_move.piece}, Start :{best_move.start}, End :{best_move.end}");
                        //Board.update(best_move, Cache, 1);
                        //game_store.Add(Board.bitboards);

                        //White = new Search2(0);
                        //legal = true;
                    }

                    if (move % 2 == 1)

                    {

                        Console.WriteLine("Piece");


                        int piece = Array.IndexOf(pieces, Console.ReadLine());

                        Console.WriteLine("Start");
                        int start = NotationToIndex(Console.ReadLine()) - shift;

                        Console.WriteLine("End");
                        int end = NotationToIndex(Console.ReadLine()) - shift;
                        int promote;
                        if (piece == 1 && start > 47 && start < 56)
                        {
                            Console.WriteLine("Promote");
                            promote = NotationToIndex(Console.ReadLine());
                        }
                        else
                        {
                            promote = 0;
                        }

                        Move newmove = new Move(piece, start, end, 0, promote, false);
                        MoveGeneration generate = new MoveGeneration();
                        Move[] all_moves = generate.all_moves(Board, Cache, 1, out int movecount, out ulong pawn, out ulong capture_mask).moves;
                        if (Compare(newmove, all_moves, movecount))
                        {
                            Board.update(newmove, Cache, 0);
                            legal = true;
                        }
                        else
                        {
                            Console.WriteLine("Illegal Move");
                            legal = false;
                        }

                        //Console.WriteLine($"Piece :{best_move.piece}, Start :{best_move.start}, End :{best_move.end}");

                    }
                    if (legal)
                    {
                        move++;
                        //Console.WriteLine();
                        //gui.Display(Board.bitboards);
                        //game.view_board(Board);
                        Board.print(size, true);
                        MoveGeneration all = new MoveGeneration();
                        if (checkmate.mate(all, Board, Cache, move % 2) == 2)
                        {
                            if (move % 2 == 0)
                            {
                                Console.WriteLine("BLACK WINS");
                                BLACK++;
                                loop = false;
                                game.view_board(Board);
                            }
                            else
                            {
                                Console.WriteLine("WHITE WINS");
                                WHITE++;
                                loop = false;
                                game.view_board(Board);
                            }
                        }
                        else if (checkmate.mate(all, Board, Cache, move % 2) == 1)
                        {
                            Console.WriteLine("STALEMATE");
                            DRAW++;
                            loop = false;
                            game.view_board(Board);

                        }
                        if (last_four.Count == 4)
                        {
                            if (last_four[0] == Board.all_pieces)
                            {
                                three += 1;

                            }
                            else
                            {
                                three = 0;
                            }
                            if (three == 3)
                            {
                                Console.WriteLine("DRAW");
                                DRAW += 1;
                                game.view_board(Board);
                                loop = false;
                            }

                            last_four.Clear();

                        }
                        last_four.Add(Board.all_pieces);
                    }


                    //break;
                }
                //break;
                Console.WriteLine($"V2 wins: {BLACK}, V1 Wins: {WHITE}, Draws: {DRAW}");
            }


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
        public int NotationToIndex(string notation)
        {
            char file = notation[0];
            int rank = int.Parse(notation[1].ToString());
            return (8 - rank) * 8 + (file - 'a');
        }



    }
}
