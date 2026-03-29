using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public struct Global
    {
        public int white;
        public int black;
        public bool Wkingcastle;
        public bool Wqueencastle;
        public bool Bkingcastle;
        public bool Bqueencastle;

        public Global(int white, int black, bool Wkingcastle, bool Wqueencastle, bool Bkingcastle, bool Bqueencastle)
        {
            this.white = white;
            this.black = black;
            this.Wkingcastle = Wkingcastle;
            this.Wqueencastle = Wqueencastle;
            this.Bkingcastle = Bkingcastle;
            this.Bqueencastle = Bqueencastle;
        }
    }
}
