using System;
using System.Runtime.CompilerServices;

namespace ChessEngine
{
    public enum NodeType
    {
        Exact,
        LowerBound,
        UpperBound
    }

    // Structure for a Transposition Table Entry
    public struct TranspositionEntry
    {
        public ulong Key;          // Zobrist hash key
        public double Eval;        // Evaluation score
        public int Depth;          // Depth at which the evaluation was done
        public NodeType Type;      // Type of node (Exact, LowerBound, UpperBound)
        public Move BestMove;      // Best move from this position
    }

    // Transposition Table Class
    public class TranspositionTable
    {
        private TranspositionEntry[] table;
        private int size;

        public TranspositionTable(int size)
        {
            this.size = size;
            table = new TranspositionEntry[size];
        }

        // Simple modulo-based hashing for the table index
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(ulong key)
        {
            return (int)(key % (ulong)size);
        }

        // Store an entry in the TT
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(ulong key, double eval, int depth, NodeType type, Move bestMove)
        {
            int index = GetIndex(key);
            TranspositionEntry entry = new TranspositionEntry
            {
                Key = key,
                Eval = eval,
                Depth = depth,
                Type = type,
                BestMove = bestMove
            };
            table[index] = entry;
        }

        // Try to retrieve an entry from the TT
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEntry(ulong key, int depth, out TranspositionEntry entry)
        {
            int index = GetIndex(key);
            entry = table[index];
            if (entry.Key == key && entry.Depth >= depth)
            {
                return true;
            }
            return false;
        }
    }
}
// Enum to represent the type of node stored in TT

