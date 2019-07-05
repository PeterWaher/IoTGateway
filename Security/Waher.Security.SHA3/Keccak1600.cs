using System;

namespace Waher.Security.SHA3
{
    /// <summary>
    /// Implementation of the KECCAK-p permutations, with a bitsize of 1600 bits, 
    /// as defined in section 3 in the NIST FIPS 202: 
    /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
    /// </summary>
    public abstract class Keccak1600
    {
        private static readonly bool[] rcs = GetRcs();
        private static readonly ulong[] RCs = new ulong[] { 0x01, 0x02, 0x08, 0x80, 0x8000, 0x80000000, 0x8000000000000000 };
        private static readonly ulong[] RC_ir = GetRcIr();
        private readonly ulong[,] A = new ulong[5, 5];
        private readonly ulong[,] A2 = new ulong[5, 5];
        private readonly ulong[] C = new ulong[5];
        private readonly int r;
        private readonly int c;
        private readonly int r8;
        private readonly int r8m1;
        private readonly int dByteSize;
        private readonly byte suffix;
        private readonly byte suffixBits;
        private bool reportStates = false;
        private bool inA2 = false;

        /// <summary>
        /// Implementation of the KECCAK-p permutations, with a bitsize of 1600 bits, 
        /// as defined in section 3 in the NIST FIPS 202: 
        /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
        /// </summary>
        /// <param name="Capacity">Capacity of sponge function.</param>
        /// <param name="Suffix">Suffix to append to variable-length messages before executing the Sponge function.</param>
        /// <param name="DigestSize">Size of results of the sponge function.</param>
        public Keccak1600(int Capacity, byte Suffix, int DigestSize)
        {
            this.c = Capacity;
            this.r = 1600 - this.c;
            this.r8 = this.r >> 3;
            this.r8m1 = this.r8 - 1;
            this.dByteSize = DigestSize / 8;
            this.suffix = Suffix;
            this.suffixBits = 0;

            if ((DigestSize & 7) != 0)
                throw new ArgumentException("Invalid digest size.", nameof(DigestSize));

            if (this.c <= 0 || this.r <= 0 || this.r8m1 < 0 || (Capacity & 7) != 0)
                throw new ArgumentException("Invalid capacity.", nameof(Capacity));

            while (Suffix > 0)
            {
                Suffix >>= 1;
                this.suffixBits++;
            }

            if (this.suffixBits > 6)
                throw new ArgumentException("Invalid suffix.", nameof(Suffix));

            this.suffix |= (byte)(1 << this.suffixBits);    // First bit of pad10*1
        }

        /// <summary>
        /// Initializes the internal state.
        /// </summary>
        /// <param name="Data">Binary data</param>
        public void InitState(byte[] Data)
        {
            if (Data.Length != 200)
                throw new ArgumentException("Expected array of 200 bytes.", nameof(Data));

            int i = 0;
            int x, y;

            for (y = 0; y < 5; y++)
            {
                for (x = 0; x < 5; x++)
                {
                    this.A[x, y] = BitConverter.ToUInt64(Data, i);
                    i += 8;
                }
            }
        }

        /// <summary>
        /// Gets a copy of the internal state.
        /// </summary>
        /// <returns>Binary data</returns>
        public byte[] GetState()
        {
            byte[] Data = new byte[200];    // this.byteSize

            int i = 0;
            int x, y;

            for (y = 0; y < 5; y++)
            {
                for (x = 0; x < 5; x++)
                {
                    Array.Copy(BitConverter.GetBytes(this.inA2 ? A2[x, y] : A[x, y]), 0, Data, i, 8);
                    i += 8;
                }
            }

            return Data;
        }

        /// <summary>
        /// rc(t) function, as defined in section 3.2.5 of NIST FIPS 202.
        /// </summary>
        /// <param name="t">Integer input</param>
        /// <returns>Boolean output</returns>
        private static bool Rc(int t)
        {
            int i = t % 255;
            if (i == 0)
                return true;
            else if (i < 0)
                i += 255;

            byte R = 1;

            while (i-- > 0)
            {
                if ((R & 0x80) != 0)
                {
                    R <<= 1;
                    R ^= 0b01110001;
                }
                else
                    R <<= 1;
            }

            return (R & 1) != 0;
        }

        private static bool[] GetRcs()
        {
            bool[] Result = new bool[255];
            int t;

            for (t = 0; t < 255; t++)
                Result[t] = Rc(t);

            return Result;
        }

        private static ulong[] GetRcIr()
        {
            ulong[] Result = new ulong[24];
            int ir, i, j;

            for (ir = 0; ir < 24; ir++)
            {
                ulong RC = 0;

                for (j = 0; j <= 6; j++)
                {
                    i = (j + 7 * ir) % 255;
                    if (i < 0)
                        i += 255;

                    if (rcs[i])
                        RC |= RCs[j];
                }

                Result[ir] = RC;
            }

            return Result;
        }

        /// <summary>
        /// Computes the KECCAK-p[b, nr=24] algorithm, as defined in section 3.3 of NIST FIPS 202.
        /// </summary>
        /// <param name="S">Input string of fixed length</param>
        /// <returns>Output string of fixed length</returns>
        public byte[] ComputeFixed(byte[] S)
        {
            ulong v;
            int ir;

            this.InitState(S);

            if (this.reportStates)
                this.NewState?.Invoke(this, new EventArgs());

            for (ir = 0; ir < 24; ir++)
            {
                // Rnd function, as defined in section 3.3 of NIST FIPS 202.
                // θ function, as defined in section 3.2.1 of NIST FIPS 202.

                C[0] = A[0, 0] ^ A[0, 1] ^ A[0, 2] ^ A[0, 3] ^ A[0, 4];
                C[1] = A[1, 0] ^ A[1, 1] ^ A[1, 2] ^ A[1, 3] ^ A[1, 4];
                C[2] = A[2, 0] ^ A[2, 1] ^ A[2, 2] ^ A[2, 3] ^ A[2, 4];
                C[3] = A[3, 0] ^ A[3, 1] ^ A[3, 2] ^ A[3, 3] ^ A[3, 4];
                C[4] = A[4, 0] ^ A[4, 1] ^ A[4, 2] ^ A[4, 3] ^ A[4, 4];

                v = C[4] ^ ((C[1] << 1) | ((C[1] >> 63) & 1));
                A[0, 0] ^= v;
                A[0, 1] ^= v;
                A[0, 2] ^= v;
                A[0, 3] ^= v;
                A[0, 4] ^= v;

                v = C[0] ^ ((C[2] << 1) | ((C[2] >> 63) & 1));
                A[1, 0] ^= v;
                A[1, 1] ^= v;
                A[1, 2] ^= v;
                A[1, 3] ^= v;
                A[1, 4] ^= v;

                v = C[1] ^ ((C[3] << 1) | ((C[3] >> 63) & 1));
                A[2, 0] ^= v;
                A[2, 1] ^= v;
                A[2, 2] ^= v;
                A[2, 3] ^= v;
                A[2, 4] ^= v;

                v = C[2] ^ ((C[4] << 1) | ((C[4] >> 63) & 1));
                A[3, 0] ^= v;
                A[3, 1] ^= v;
                A[3, 2] ^= v;
                A[3, 3] ^= v;
                A[3, 4] ^= v;

                v = C[3] ^ ((C[0] << 1) | ((C[0] >> 63) & 1));
                A[4, 0] ^= v;
                A[4, 1] ^= v;
                A[4, 2] ^= v;
                A[4, 3] ^= v;
                A[4, 4] ^= v;

                if (this.reportStates)
                    this.NewState?.Invoke(this, new EventArgs());

                // ρ function, as defined in section 3.2.2 of NIST FIPS 202.

                A[1, 0] = ((v = A[1, 0]) << 1) | (v >> 63);
                A[0, 2] = ((v = A[0, 2]) << 3) | (v >> 61);
                A[2, 1] = ((v = A[2, 1]) << 6) | (v >> 58);
                A[1, 2] = ((v = A[1, 2]) << 10) | (v >> 54);
                A[2, 3] = ((v = A[2, 3]) << 15) | (v >> 49);
                A[3, 3] = ((v = A[3, 3]) << 21) | (v >> 43);
                A[3, 0] = ((v = A[3, 0]) << 28) | (v >> 36);
                A[0, 1] = ((v = A[0, 1]) << 36) | (v >> 28);
                A[1, 3] = ((v = A[1, 3]) << 45) | (v >> 19);
                A[3, 1] = ((v = A[3, 1]) << 55) | (v >> 9);
                A[1, 4] = ((v = A[1, 4]) << 2) | (v >> 62);
                A[4, 4] = ((v = A[4, 4]) << 14) | (v >> 50);
                A[4, 0] = ((v = A[4, 0]) << 27) | (v >> 37);
                A[0, 3] = ((v = A[0, 3]) << 41) | (v >> 23);
                A[3, 4] = ((v = A[3, 4]) << 56) | (v >> 8);
                A[4, 3] = ((v = A[4, 3]) << 8) | (v >> 56);
                A[3, 2] = ((v = A[3, 2]) << 25) | (v >> 39);
                A[2, 2] = ((v = A[2, 2]) << 43) | (v >> 21);
                A[2, 0] = ((v = A[2, 0]) << 62) | (v >> 2);
                A[0, 4] = ((v = A[0, 4]) << 18) | (v >> 46);
                A[4, 2] = ((v = A[4, 2]) << 39) | (v >> 25);
                A[2, 4] = ((v = A[2, 4]) << 61) | (v >> 3);
                A[4, 1] = ((v = A[4, 1]) << 20) | (v >> 44);
                A[1, 1] = ((v = A[1, 1]) << 44) | (v >> 20);

                if (this.reportStates)
                    this.NewState?.Invoke(this, new EventArgs());

                // π function, as defined in section 3.2.3 of NIST FIPS 202.

                A2[0, 0] = A[0, 0];
                A2[0, 1] = A[3, 0];
                A2[0, 2] = A[1, 0];
                A2[0, 3] = A[4, 0];
                A2[0, 4] = A[2, 0];

                A2[1, 0] = A[1, 1];
                A2[1, 1] = A[4, 1];
                A2[1, 2] = A[2, 1];
                A2[1, 3] = A[0, 1];
                A2[1, 4] = A[3, 1];

                A2[2, 0] = A[2, 2];
                A2[2, 1] = A[0, 2];
                A2[2, 2] = A[3, 2];
                A2[2, 3] = A[1, 2];
                A2[2, 4] = A[4, 2];

                A2[3, 0] = A[3, 3];
                A2[3, 1] = A[1, 3];
                A2[3, 2] = A[4, 3];
                A2[3, 3] = A[2, 3];
                A2[3, 4] = A[0, 3];

                A2[4, 0] = A[4, 4];
                A2[4, 1] = A[2, 4];
                A2[4, 2] = A[0, 4];
                A2[4, 3] = A[3, 4];
                A2[4, 4] = A[1, 4];

                if (this.reportStates)
                {
                    this.inA2 = true;
                    this.NewState?.Invoke(this, new EventArgs());
                }

                // χ function, as defined in section 3.2.4 of NIST FIPS 202.

                A[0, 0] = A2[0, 0] ^ ((~A2[1, 0]) & A2[2, 0]);
                A[0, 1] = A2[0, 1] ^ ((~A2[1, 1]) & A2[2, 1]);
                A[0, 2] = A2[0, 2] ^ ((~A2[1, 2]) & A2[2, 2]);
                A[0, 3] = A2[0, 3] ^ ((~A2[1, 3]) & A2[2, 3]);
                A[0, 4] = A2[0, 4] ^ ((~A2[1, 4]) & A2[2, 4]);

                A[1, 0] = A2[1, 0] ^ ((~A2[2, 0]) & A2[3, 0]);
                A[1, 1] = A2[1, 1] ^ ((~A2[2, 1]) & A2[3, 1]);
                A[1, 2] = A2[1, 2] ^ ((~A2[2, 2]) & A2[3, 2]);
                A[1, 3] = A2[1, 3] ^ ((~A2[2, 3]) & A2[3, 3]);
                A[1, 4] = A2[1, 4] ^ ((~A2[2, 4]) & A2[3, 4]);

                A[2, 0] = A2[2, 0] ^ ((~A2[3, 0]) & A2[4, 0]);
                A[2, 1] = A2[2, 1] ^ ((~A2[3, 1]) & A2[4, 1]);
                A[2, 2] = A2[2, 2] ^ ((~A2[3, 2]) & A2[4, 2]);
                A[2, 3] = A2[2, 3] ^ ((~A2[3, 3]) & A2[4, 3]);
                A[2, 4] = A2[2, 4] ^ ((~A2[3, 4]) & A2[4, 4]);

                A[3, 0] = A2[3, 0] ^ ((~A2[4, 0]) & A2[0, 0]);
                A[3, 1] = A2[3, 1] ^ ((~A2[4, 1]) & A2[0, 1]);
                A[3, 2] = A2[3, 2] ^ ((~A2[4, 2]) & A2[0, 2]);
                A[3, 3] = A2[3, 3] ^ ((~A2[4, 3]) & A2[0, 3]);
                A[3, 4] = A2[3, 4] ^ ((~A2[4, 4]) & A2[0, 4]);

                A[4, 0] = A2[4, 0] ^ ((~A2[0, 0]) & A2[1, 0]);
                A[4, 1] = A2[4, 1] ^ ((~A2[0, 1]) & A2[1, 1]);
                A[4, 2] = A2[4, 2] ^ ((~A2[0, 2]) & A2[1, 2]);
                A[4, 3] = A2[4, 3] ^ ((~A2[0, 3]) & A2[1, 3]);
                A[4, 4] = A2[4, 4] ^ ((~A2[0, 4]) & A2[1, 4]);

                if (this.reportStates)
                {
                    this.inA2 = false;
                    this.NewState?.Invoke(this, new EventArgs());
                }

                // ι function, as defined in section 3.2.5 of NIST FIPS 202.

                A[0, 0] ^= RC_ir[ir];

                if (this.reportStates)
                    this.NewState?.Invoke(this, new EventArgs());
            }

            return this.GetState();
        }

        /// <summary>
        /// Computes the SPONGE function, as defined in section 4 of NIST FIPS 202.
        /// </summary>
        /// <param name="N">Input string of variable length.</param>
        /// <returns>Output string of fixed length.</returns>
        public byte[] ComputeVariable(byte[] N)
        {
            this.reportStates = !(this.NewState is null);

            int Len = N.Length;
            int m = Len << 3;
            int nm1 = m / r;
            byte[] S = new byte[200];
            int Pos = 0;
            int i, k;

            for (i = 0; i < nm1; i++)
            {
                for (k = 0; k < this.r8; k++)
                    S[k] ^= N[Pos++];

                S = this.ComputeFixed(S);
            }

            k = 0;
            while (Pos < Len)
                S[k++] ^= N[Pos++];

            S[k] ^= this.suffix;
            S[this.r8m1] ^= 0x80;   // Last bit of pad10*1
            S = this.ComputeFixed(S);

            byte[] Z = new byte[this.dByteSize];

            Pos = 0;
            while (true)
            {
                i = Math.Min(r8, this.dByteSize - Pos);
                Array.Copy(S, 0, Z, Pos, i);
                Pos += i;

                if (Pos >= this.dByteSize)
                    return Z;

                S = this.ComputeFixed(S);
            }
        }

        /// <summary>
        /// Event raised when the internal state has changed. You can use this event in unit tests
        /// to validate the evaluation of the function.
        /// </summary>
        public event EventHandler NewState = null;
    }
}
