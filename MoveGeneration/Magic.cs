using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ChessEngine
{
    public readonly struct Magic
    {
        public readonly string type;
        public readonly ulong mask;
        public readonly bool shift;
        public readonly ulong legal_moves;
        //Precomputed magic numbers 
        public readonly ulong[] rook_magic_values = new ulong[64]{9835862101112012800,
1297054835767575040,
936769789326524480,
2449971907904274464,
5620501458014306320,
1657340094695605248,
8791051216340516868,
2449959863850500129,
1091841602185363460,
3572762296004378752,
6178657283583508736,
2543971055335243800,
15023867636614692992,
1198520622907523076,
3185171042673240100,
531143341263126784,
12826586540180242560,
14034875603006554368,
558728928689266688,
9467219526807601312,
4614054366660395024,
3894056774042669120,
6073143680363989040,
7462770247217742849,
6918202515020857344,
2688368099568124194,
6656409868045516832,
23187605015957504,
1630584870798428160,
4143073067455159296,
4435278946940162056,
1601357777262805124,
5567926905077760128,
1241024839939006976,
1309175303410552832,
1705459947828948992,
1516120988037686273,
1354176217829344256,
2976395900697448466,
1191262867140117121,
2158139026501763074,
3544897520037658658,
1517961019698577426,
7001692038548750348,
3483535415850631176,
589408687273148432,
8833837217324204048,
14138020139337646084,
5287901621224014336,
4334753720211931648,
4503108168005452032,
1734168087437648128,
5352533656032772352,
81629015362519552,
473116048368370688,
1028302871910744576,
2019319497105768451,
6437931376782804482,
3839938304921174146,
5305346147172352006,
7162708129095811139,
10553341336776344065,
1779318982679462420,
3631119577691259010 };
        //Precomputed shift values
        public readonly int[] rook_shift_values = new int[64] {52,
53,
53,
53,
53,
53,
53,
52,
53,
54,
54,
54,
54,
54,
54,
53,
53,
54,
54,
54,
54,
54,
54,
53,
53,
54,
54,
54,
54,
54,
54,
53,
53,
54,
54,
54,
54,
54,
54,
53,
53,
54,
54,
54,
54,
54,
54,
53,
53,
54,
54,
54,
54,
54,
54,
53,
52,
53,
53,
53,
53,
53,
53,
52};
        public readonly ulong[] bishop_magic_values = new ulong[64]
        {
            3611944694261022752,
3747577812161267712,
7228290648946836000,
11612558323879575553,
5244521211187823616,
4043964324070293552,
874052439995056642,
33797926598754304,
5483229926135041024,
324065689226610177,
4706846606826733848,
1199123069180985347,
1572337981402906720,
202893440902561794,
4841385585431691264,
1020405619834233088,
3792072225615512064,
5860320243371344906,
2122331460526669896,
4648278057711435842,
4777198484498481152,
7543249558735552784,
2531801496461771776,
1477475519296569856,
2055928702220370176,
12623906443751719936,
3551097108600324641,
8504485497702744448,
4641241995536252928,
559940590305845256,
168654094825980928,
4991414733776488452,
7002006864468201472,
5379595300496212995,
10258639114433333248,
1386831612751905024,
13890234027229839616,
1485629904946331712,
7561557296173056512,
2387476611950513152,
4376118204463976576,
1748597478347582469,
7653305382408161288,
117016435854678017,
1395156359296648192,
1086040999840449024,
6916469562821551104,
484223357542793344,
15830513357671170052,
642928588341903617,
7554043190908551184,
4985520526086111232,
2214408929547780098,
5299681445740873472,
8403834346345791504,
1864263825438212096,
8603358821598380040,
2309513243478459392,
6103840485708206080,
8418140676658824209,
4003223998184953344,
4654270242766389761,
5220816424878211136,
7177639797775532160
        };
        public readonly int[] bishop_shift_values = new int[64]
        {
            58,
59,
59,
59,
59,
59,
59,
58,
59,
59,
59,
59,
59,
59,
59,
59,
59,
59,
57,
57,
57,
57,
59,
59,
59,
59,
57,
55,
55,
57,
59,
59,
59,
59,
57,
55,
55,
57,
59,
59,
59,
59,
57,
57,
57,
57,
59,
59,
59,
59,
59,
59,
59,
59,
59,
59,
58,
59,
59,
59,
59,
59,
59,
58,
        };

        // Magic class constructor initializing type, shift, and mask properties
        public Magic(string type, bool shift, ulong mask)
        {
            this.type = type;  // Assigning the type of the magic
            this.shift = shift;  // Assigning the shift flag
            this.mask = mask;  // Assigning the mask value
        }

        // Method to calculate all blockers based on the given mask
        public ulong[] all_blockers()
        {
            List<int> pad_left = new List<int>();  // List to store the positions of the 1s in the binary representation of mask
            int count = 0;  // Counter to track the bit position

            // Convert the mask to a binary string representation
            string binaryString = Convert.ToString((long)mask, 2);

            // Pad the binary string to ensure it's 64 bits long
            binaryString = binaryString.PadLeft(64, '0');

            // Loop through each bit in the binary string
            foreach (char i in binaryString)
            {
                if (i == '1')  // If the bit is 1, store its position (from left to right)
                {
                    pad_left.Add(63 - count);
                }
                count++;  // Increment bit position
            }

            // Calculate the number of possible blocker combinations (2^n, where n is the number of 1 bits in mask)
            int num_blockers = 1 << pad_left.Count();

            // Reverse the list of positions to generate blockers in the proper order
            pad_left.Reverse();

            ulong[] blocker_bitboards = new ulong[num_blockers];  // Array to hold the generated blocker bitboards

            // Generate all possible blocker combinations
            for (int i = 0; i < num_blockers; i++)
            {
                for (int j = 0; j < pad_left.Count(); j++)
                {
                    int one = i >> j & 1;  // Extract the j-th bit from the current combination
                    blocker_bitboards[i] |= (ulong)one << pad_left[j];  // Set the corresponding bit in the bitboard
                }
            }

            return blocker_bitboards;  // Return the generated blocker bitboards
        }

        // Method to check if a given magic number is valid for the provided blockers
        public bool is_magic_number(ulong[] blockers, ulong magic)
        {
            ulong num = (ulong)blockers.Count();  // Get the number of blockers
            ulong[] indices = new ulong[num];  // Array to store indices for each blocker
            int shift_value = 64 - (int)Math.Log(num, 2);  // Shift value for masking (based on board size)

            // If shift is true, output the shift value for debugging
            if (shift)
            {
                Console.WriteLine("Shift: " + shift_value);
            }
            else
            {

                for (ulong occupancy = 0; occupancy < num; occupancy++)
                {
                    // Calculate the index using the magic number and the current blocker
                    ulong index = blockers[occupancy] * magic >> shift_value;

                    // If the index has already been seen and it's not the current occupancy, the magic number is invalid
                    if (indices[index] != 0 && indices[index] != occupancy)
                    {
                        return false;
                    }

                    // Store the current occupancy in the index array
                    indices[index] = occupancy;
                }
            }

            return true;  // If no conflicts were found, the magic number is valid
        }

        // Method to generate a random magic number by combining multiple random 64-bit integers
        private static ulong random_magic()
        {
            Random rand1 = new Random();  // First random generator
            Random rand2 = new Random();  // Second random generator
            Random rand3 = new Random();  // Third random generator
            Random rand4 = new Random();  // Fourth random generator

            // Generate 4 random 64-bit numbers and combine them to create a single magic number
            ulong magic1 = (ulong)rand1.Next();
            ulong magic2 = (ulong)rand2.Next() << 16;
            ulong magic3 = (ulong)rand3.Next() << 32;
            ulong magic4 = (ulong)rand4.Next() << 48;

            ulong magic = magic1 | magic2 | magic3 | magic4;  // Combine the 4 parts into one 64-bit magic number

            return magic;  // Return the generated magic number
        }

        // Method to generate a combined random magic number from three different random magic numbers
        public ulong random_magic2()
        {
            return random_magic() & random_magic() & random_magic();  // Combine the results of 3 random magic numbers using a bitwise AND
        }

        // Method to calculate a valid magic number for the given blockers
        public void CalculateMagicNumber(ulong[] blockers)
        {
            ulong magic;

            while (true)
            {
                // Try generating a valid magic number by combining two random magic numbers
                magic = random_magic2() & random_magic2();

                // Check if the generated magic number is valid for the current blockers
                if (is_magic_number(blockers, magic))
                {
                    break;  // If valid, exit the loop
                }
            }

            Console.WriteLine("Magic : " + magic);  // Output the valid magic number
        }

    }
}