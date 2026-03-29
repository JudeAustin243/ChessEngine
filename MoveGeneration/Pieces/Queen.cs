using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ChessEngine
{
    public sealed class Queen : Piece
    {


        public readonly Magic magic = new Magic("", false, 0ul);

        Rook rook;
        Bishop bishop;
        public Queen(int colour)
        {
            this.colour = colour;

            rook = new Rook(0);
            bishop = new Bishop(0);


            //mask = pawn_mask();
        }


        // Returns the legal moves for a piece that can move as both a rook and a bishop, excluding moves that would place the player in check
        public override ulong moves(int s, Board board, PieceCall cache, Check info, ulong[] pins, ulong filter)
        {
            // Calculate the blockers for the rook and bishop on the given square
            ulong blockers = rook.rook_mask[s] & board.all_pieces;
            ulong key = blockers * magic.rook_magic_values[s] >> magic.rook_shift_values[s];

            blockers = bishop.bishop_mask[s] & board.all_pieces;
            ulong key2 = blockers * magic.bishop_magic_values[s] >> magic.bishop_shift_values[s];

            // Return the combined legal moves for both rook and bishop, excluding the player's own color, pinned pieces, check, and the filter
            return (rook.rook_look[s, key] | bishop.bishop_look[s, key2]) & ~board.colour[colour] & ~pins[s] & info.mask & ~filter;
        }

        // Returns the possible moves for a piece that can move as both a rook and a bishop, applying a filter
        public ulong mask_moves(int s, Board board, ulong filter)
        {
            // Calculate the blockers for the rook and bishop on the given square
            ulong blockers = rook.rook_mask[s] & board.all_pieces;
            ulong key = blockers * magic.rook_magic_values[s] >> magic.rook_shift_values[s];

            blockers = bishop.bishop_mask[s] & board.all_pieces;
            ulong key2 = blockers * magic.bishop_magic_values[s] >> magic.bishop_shift_values[s];

            // Get all possible moves for both rook and bishop
            ulong all_moves = rook.rook_look[s, key] | bishop.bishop_look[s, key2];

            // Define boundaries for the row (rounddown and roundup)
            ulong rounddown = 1ul << (s & -8);
            ulong roundup = 1ul << (s & -8) + 8;
            ulong start = 1ul << s;

            // Filter out moves that would place the piece on the same rank as the start square
            ulong filtered = all_moves ^ (start - rounddown | roundup - start) & all_moves;

            // Return the filtered moves, excluding positions occupied by pieces and applying the filter
            return filtered & ~board.all_pieces & ~filter;
        }

        // Returns the legal capture moves for a piece that can move as both a rook and a bishop, excluding moves that would place the player in check
        public ulong capture_moves(int s, Board board, PieceCall cache, Check info, ulong[] pins, ulong check, ulong filter)
        {
            // Calculate the blockers for the rook and bishop on the given square
            ulong blockers = rook.rook_mask[s] & board.all_pieces;
            ulong key = blockers * magic.rook_magic_values[s] >> magic.rook_shift_values[s];

            blockers = bishop.bishop_mask[s] & board.all_pieces;
            ulong key2 = blockers * magic.bishop_magic_values[s] >> magic.bishop_shift_values[s];

            // Get the legal moves for both rook and bishop, excluding moves that result in check, pins, and the player's own color
            ulong legal = (rook.rook_look[s, key] | bishop.bishop_look[s, key2]) & ~board.colour[colour] & ~pins[s] & info.mask;

            // Return the legal capture moves, including those that would result in check
            return legal & board.all_pieces & ~filter | legal & check;
        }

    }

}
