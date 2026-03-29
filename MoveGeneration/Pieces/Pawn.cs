using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace ChessEngine
{
    public sealed class Pawn : Piece
    {

        public readonly ulong[] mask = new ulong[64];

        public Magic magic = new Magic();

        public ulong qpawn_magic_values = 6781365799782203396;

        public ulong kpawn_magic_values = 76562362182729744;

        public Pawn(int colour)
        {
            this.colour = colour;

            mask = pawn_mask();

            pawn_lookup();

        }

        public ulong[] pawn_mask()
        {

            ulong[] mask = new ulong[64];

            int direction = 1;

            int diagonal = 0;

            int boardpart1 = 4;

            int boardpart2 = 5;

            if (colour == 1)
            {
                direction = -1;

                diagonal = 2;
            }

            for (int s = 0; s < 64; s++)
            {

                ulong start = 1ul << s;

                ulong legal_capture = 0;

                if ((start & board_parts[boardpart1]) == 0)

                {
                    legal_capture |= 1ul << s - (9 - diagonal) * direction;
                }

                if ((start & board_parts[boardpart2]) == 0)

                {
                    legal_capture |= 1ul << s - (7 + diagonal) * direction;
                }

                mask[s] = legal_capture;
            }


            return mask;
        }

        public ulong moves(int s, Board board, PieceCall cache, Check info, ulong[] pins, bool can_passant, ulong filter)
        {

            ulong legal_moves = 0;


            ulong start = 1UL << s;

            if (colour == 0)
            {

                legal_moves |= 1UL << s - 8 & ~board.all_pieces;

                if (s > 47 && s < 56)
                {
                    if (((start >> 16 | start >> 8) & board.all_pieces) == 0)

                    {

                        legal_moves |= 1UL << s - 16;

                    }
                }


                //show(board.bitboards[1]);show(passant.can_enpassant(s, legal_moves, colour));
                //show(board.bitboards[1]);
                legal_moves |= mask[s] & board.colour[1];
                //show(pins[37]);
                //show(board.all_pieces);
                if (can_passant)
                {
                    legal_moves |= cache.Passant.can_enpassant(s, legal_moves, colour, cache.Global, board);
                }



                //show(passant.can_enpassant(s, legal_moves, colour));
                //show(check_mask);
                //Console.WriteLine();
                //show(1ul<<cache.Global.black);
                if (info.mask == 1ul << cache.Global.black)
                {
                    info.mask |= 1ul << cache.Global.black - 8;
                }

                return legal_moves & ~pins[s] & info.mask & ~filter;
            }

            legal_moves |= 1UL << s + 8 & ~board.all_pieces & ~filter;

            if (s > 7 && s < 16)
            {

                if (((start << 16 | start << 8) & board.all_pieces) == 0)

                {

                    legal_moves |= 1UL << s + 16;

                }
            }

            legal_moves |= mask[s] & board.colour[0];
            if (can_passant)
            {
                legal_moves |= cache.Passant.can_enpassant(s, legal_moves, colour, cache.Global, board);
            }


            //show(legal_moves);
            //Console.WriteLine("aceiufabnliaubfewiu");
            //show(pins[37]);
            if (info.mask == 1ul << cache.Global.white)
            {
                info.mask |= 1ul << cache.Global.white + 8;
            }
            return legal_moves & ~pins[s] & info.mask & ~filter;
        }

        public ulong capture_moves(int s, Board board, PieceCall cache, Check info, ulong[] pins, bool can_passant, ulong check, ulong filter)
        {

            ulong legal_moves = 0;


            ulong start = 1UL << s;

            if (colour == 0)
            {

                //legal_moves |= (1UL << (s - 8)) & ~board.all_pieces;

                //if (s > 47 && s < 56)
                //{
                //    if (((start >> 16 | start >> 8) & board.all_pieces) == 0)

                //    {

                //        legal_moves |= 1UL << (s - 16);

                //    }
                //}


                //show(board.bitboards[1]);show(passant.can_enpassant(s, legal_moves, colour));
                //show(board.bitboards[1]);
                legal_moves |= mask[s] & board.colour[1];

                if (can_passant)
                {
                    legal_moves |= cache.Passant.can_enpassant(s, legal_moves, colour, cache.Global, board);
                }

                if (info.mask == 1ul << cache.Global.black)
                {
                    info.mask |= 1ul << cache.Global.black - 8;
                }


                ulong legals = legal_moves & ~pins[s] & info.mask;

                return legals & board.all_pieces | legals & check & ~filter;

            }

            //legal_moves |= (1UL << (s + 8)) & ~board.all_pieces;

            //if (s > 7 && s < 16)
            //{

            //    if (((start << 16 | start << 8) & board.all_pieces) == 0)

            //    {

            //        legal_moves |= 1UL << (s + 16);

            //    }
            //}

            legal_moves |= mask[s] & board.colour[0];
            if (can_passant)
            {
                legal_moves |= cache.Passant.can_enpassant(s, legal_moves, colour, cache.Global, board);
            }

            if (info.mask == 1ul << cache.Global.white)
            {
                info.mask |= 1ul << cache.Global.white + 8;
            }

            ulong legal = legal_moves & ~pins[s] & info.mask;

            return legal & board.all_pieces | legal & check & ~filter;
        }

        public ulong[] pawn_combinations(int startFile, int endFile)
        {
            int numFiles = endFile - startFile + 1;

            ulong[][] filePossibilities = new ulong[numFiles][];
            for (int f = 0; f < numFiles; f++)
            {
                int file = startFile + f;
                filePossibilities[f] = new ulong[7];

                filePossibilities[f][0] = 0UL;

                for (int r = 1; r <= 6; r++)
                {
                    int square = r * 8 + file;
                    filePossibilities[f][r] = 1UL << square;
                }
            }

            int totalCombinations = (int)Math.Pow(7, numFiles);
            List<ulong> combinations = new List<ulong>(totalCombinations);


            for (int i = 0; i < totalCombinations; i++)
            {
                ulong occupancy = 0UL;
                int temp = i;

                for (int f = 0; f < numFiles; f++)
                {
                    int poss = temp % 7;
                    temp /= 7;
                    occupancy |= filePossibilities[f][poss];
                }
                combinations.Add(occupancy);
            }

            for (int i = 0; i < 1695; i++)
            {
                combinations.Add(0);
            }



            return combinations.ToArray();
        }





        private const ulong A_FILE = 0x0101010101010101UL;
        private const ulong B_FILE = 0x0202020202020202UL;
        private const ulong C_FILE = 0x0404040404040404UL;
        private const ulong D_FILE = 0x0808080808080808UL;
        private const ulong E_FILE = 0x1010101010101010UL;
        private const ulong F_FILE = 0x2020202020202020UL;
        private const ulong G_FILE = 0x4040404040404040UL;
        private const ulong H_FILE = 0x8080808080808080UL;

        private const ulong RANK3_6 = 0x00FFFFFFFF0000UL;

        public static double connect_score(ulong bitboard)
        {
            double score = 0;
            ulong temp = bitboard;

            while (temp != 0)
            {
                ulong pawn = 1UL << BitOperations.TrailingZeroCount(temp);

                if ((pawn & H_FILE) == 0)
                {

                    ulong mask = pawn << 1 | pawn << 9 | pawn >> 7;
                    if ((mask & bitboard) != 0)
                        score += 300;
                }
                temp &= temp - 1;
            }
            return score;
        }

        public static double backward_score(ulong bitboard)
        {
            double score = 0;
            ulong temp = bitboard;

            while (temp != 0)
            {
                ulong pawn = 1UL << BitOperations.TrailingZeroCount(temp);


                if ((pawn & (H_FILE | A_FILE)) == 0)
                {
                    ulong mask = pawn << 1 | pawn << 7 | pawn >> 7 | pawn >> 1;
                    if ((mask & bitboard) != 0)
                        score += 300;
                }
                else if ((pawn & H_FILE) != 0)
                {
                    ulong mask = pawn << 7 | pawn >> 1;
                    if ((mask & bitboard) != 0)
                        score += 300;
                }
                else if ((pawn & A_FILE) != 0)
                {
                    ulong mask = pawn << 9 | pawn << 1;
                    if ((mask & bitboard) != 0)
                        score += 300;
                }

                temp &= temp - 1;
            }
            return score;
        }

        public static double chain_score(ulong bitboard)
        {
            double score = 0;
            ulong temp = bitboard;

            while (temp != 0)
            {
                ulong pawn = 1UL << BitOperations.TrailingZeroCount(temp);

                ulong mask = pawn >> 7 | pawn >> 9 | pawn << 7 | pawn << 9;
                if ((mask & bitboard) != 0)
                    score += 300;

                temp &= temp - 1;
            }
            return score;
        }


        public static double doubled_score(ulong bitboard)
        {
            double score = 0;
            for (int file = 0; file < 8; file++)
            {
                ulong fileMask = 0x0101010101010101UL << file;
                int count = BitOperations.PopCount(bitboard & fileMask);
                if (count > 1)
                {

                    score -= (count - 1) * 50;
                }
            }
            return score;
        }


        public static double isolated_score(ulong bitboard)
        {
            double score = 0;
            ulong temp = bitboard;
            while (temp != 0)
            {
                ulong pawn = 1UL << BitOperations.TrailingZeroCount(temp);
                bool isolated = false;
                if ((pawn & A_FILE) != 0)
                {

                    isolated = (pawn << 1 & bitboard) == 0;
                }
                else if ((pawn & H_FILE) != 0)
                {

                    isolated = (pawn >> 1 & bitboard) == 0;
                }
                else
                {

                    isolated = ((pawn << 1 | pawn >> 1) & bitboard) == 0;
                }
                if (isolated)
                    score -= 50;
                temp &= temp - 1;
            }
            return score;
        }


        public static double island_score(ulong bitboard)
        {
            int islands = 0;
            bool inIsland = false;

            for (int file = 0; file < 8; file++)
            {
                ulong fileMask = 0x0101010101010101UL << file;
                if ((bitboard & fileMask) != 0)
                {
                    if (!inIsland)
                    {
                        islands++;
                        inIsland = true;
                    }
                }
                else
                {
                    inIsland = false;
                }
            }

            return -20 * Math.Max(islands - 1, 0);
        }


        public static double central_score(ulong bitboard)
        {

            ulong centralFiles = D_FILE | E_FILE;
            ulong centralPawns = bitboard & centralFiles & RANK3_6;
            int count = BitOperations.PopCount(centralPawns);
            return count * 50;
        }



        public static double num_score(ulong bitboard)
        {

            int count = BitOperations.PopCount(bitboard);
            return count * 100;
        }

        public double Evaluate(ulong friendly)
        {
            double score = 0;
            score += num_score(friendly);
            score += connect_score(friendly);
            score += backward_score(friendly);
            score += chain_score(friendly);
            score += doubled_score(friendly);
            score += isolated_score(friendly);
            score += island_score(friendly);
            score += central_score(friendly);
            return score;
        }


        public void pawn_lookup()
        {
            ulong[] queen = pawn_combinations(0, 3);

            ulong[] king = pawn_combinations(4, 7);

            for (int ki = 0; ki < king.Length; ki++)
            {
                int kingIndex = (int)(king[ki] * kpawn_magic_values >> 52);

                ulong fullPawnConfig = king[ki];

                double eval = Evaluate(fullPawnConfig);

                king_pawn_look[kingIndex] = (fullPawnConfig, eval);
            }

            for (int qi = 0; qi < queen.Length; qi++)
            {
                int queenIndex = (int)(queen[qi] * qpawn_magic_values >> 52);

                ulong fullPawnConfig = queen[qi];

                double eval = Evaluate(fullPawnConfig);

                queen_pawn_look[queenIndex] = (fullPawnConfig, eval);
            }
        }
    }
}
