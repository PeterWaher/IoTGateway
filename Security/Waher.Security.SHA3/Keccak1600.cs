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
        private ulong[,] A = new ulong[5, 5];
        private ulong[,] A2 = new ulong[5, 5];
        private readonly ulong[] C = new ulong[5];
        private readonly int r;
        private readonly int c;
        private readonly int r8;
        private readonly int r8m1;
        private readonly int dByteSize;
        private readonly int nr;
        private readonly byte suffix;
        private readonly byte suffixBits;
        private bool reportStates = false;

        /// <summary>
        /// Implementation of the KECCAK-p permutations, with a bitsize of 1600 bits, 
        /// as defined in section 3 in the NIST FIPS 202: 
        /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
        /// </summary>
        /// <param name="Capacity">Capacity of sponge function.</param>
        /// <param name="Suffix">Suffix to append to variable-length messages before executing the Sponge function.</param>
        /// <param name="DigestSize">Size of results of the sponge function.</param>
        public Keccak1600(int Capacity, byte Suffix, int DigestSize)
            : this(24, Capacity, Suffix, DigestSize)
        {
        }

        /// <summary>
        /// Implementation of the KECCAK-p permutations, with a bitsize of 1600 bits, 
        /// as defined in section 3 in the NIST FIPS 202: 
        /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
        /// </summary>
        /// <param name="Iterations">Number of iterations</param>
        /// <param name="Capacity">Capacity of sponge function.</param>
        /// <param name="Suffix">Suffix to append to variable-length messages before executing the Sponge function.</param>
        /// <param name="DigestSize">Size of results of the sponge function.</param>
        public Keccak1600(int Iterations, int Capacity, byte Suffix, int DigestSize)
        {
            this.nr = Iterations;
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
                    Array.Copy(BitConverter.GetBytes(A[x, y]), 0, Data, i, 8);
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

        /// <summary>
        /// Rnd function, as defined in section 3.3 of NIST FIPS 202.
        /// </summary>
        public void Rnd(int ir)
        {
            // θ function, as defined in section 3.2.1 of NIST FIPS 202.

            int x;
            ulong D;

            C[0] = A[0, 0] ^ A[0, 1] ^ A[0, 2] ^ A[0, 3] ^ A[0, 4];
            C[1] = A[1, 0] ^ A[1, 1] ^ A[1, 2] ^ A[1, 3] ^ A[1, 4];
            C[2] = A[2, 0] ^ A[2, 1] ^ A[2, 2] ^ A[2, 3] ^ A[2, 4];
            C[3] = A[3, 0] ^ A[3, 1] ^ A[3, 2] ^ A[3, 3] ^ A[3, 4];
            C[4] = A[4, 0] ^ A[4, 1] ^ A[4, 2] ^ A[4, 3] ^ A[4, 4];

            D = C[4] ^ ((C[1] << 1) | ((C[1] >> 63) & 1));
            A[0, 0] ^= D;
            A[0, 1] ^= D;
            A[0, 2] ^= D;
            A[0, 3] ^= D;
            A[0, 4] ^= D;

            D = C[0] ^ ((C[2] << 1) | ((C[2] >> 63) & 1));
            A[1, 0] ^= D;
            A[1, 1] ^= D;
            A[1, 2] ^= D;
            A[1, 3] ^= D;
            A[1, 4] ^= D;

            D = C[1] ^ ((C[3] << 1) | ((C[3] >> 63) & 1));
            A[2, 0] ^= D;
            A[2, 1] ^= D;
            A[2, 2] ^= D;
            A[2, 3] ^= D;
            A[2, 4] ^= D;

            D = C[2] ^ ((C[4] << 1) | ((C[4] >> 63) & 1));
            A[3, 0] ^= D;
            A[3, 1] ^= D;
            A[3, 2] ^= D;
            A[3, 3] ^= D;
            A[3, 4] ^= D;

            D = C[3] ^ ((C[0] << 1) | ((C[0] >> 63) & 1));
            A[4, 0] ^= D;
            A[4, 1] ^= D;
            A[4, 2] ^= D;
            A[4, 3] ^= D;
            A[4, 4] ^= D;

            if (this.reportStates)
                this.NewState?.Invoke(this, new EventArgs());

            // ρ function, as defined in section 3.2.2 of NIST FIPS 202.

            x = 1;
            int y = 0;
            int t;
            int i, j;
            ulong v;

            for (t = 0; t <= 23; t++)
            {
                i = ((t + 1) * (t + 2) / 2) & 63;
                j = 64 - i;
                v = A[x, y];
                A[x, y] = (v << i) | (v >> j);

                i = (2 * x + 3 * y) % 5;
                x = y;
                y = i;
            }

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

            ulong[,] Temp = A;
            A = A2;
            A2 = Temp;

            if (this.reportStates)
                this.NewState?.Invoke(this, new EventArgs());

            // χ function, as defined in section 3.2.4 of NIST FIPS 202.

            A2[0, 0] = A[0, 0] ^ ((~A[1, 0]) & A[2, 0]);
            A2[0, 1] = A[0, 1] ^ ((~A[1, 1]) & A[2, 1]);
            A2[0, 2] = A[0, 2] ^ ((~A[1, 2]) & A[2, 2]);
            A2[0, 3] = A[0, 3] ^ ((~A[1, 3]) & A[2, 3]);
            A2[0, 4] = A[0, 4] ^ ((~A[1, 4]) & A[2, 4]);

            A2[1, 0] = A[1, 0] ^ ((~A[2, 0]) & A[3, 0]);
            A2[1, 1] = A[1, 1] ^ ((~A[2, 1]) & A[3, 1]);
            A2[1, 2] = A[1, 2] ^ ((~A[2, 2]) & A[3, 2]);
            A2[1, 3] = A[1, 3] ^ ((~A[2, 3]) & A[3, 3]);
            A2[1, 4] = A[1, 4] ^ ((~A[2, 4]) & A[3, 4]);

            A2[2, 0] = A[2, 0] ^ ((~A[3, 0]) & A[4, 0]);
            A2[2, 1] = A[2, 1] ^ ((~A[3, 1]) & A[4, 1]);
            A2[2, 2] = A[2, 2] ^ ((~A[3, 2]) & A[4, 2]);
            A2[2, 3] = A[2, 3] ^ ((~A[3, 3]) & A[4, 3]);
            A2[2, 4] = A[2, 4] ^ ((~A[3, 4]) & A[4, 4]);

            A2[3, 0] = A[3, 0] ^ ((~A[4, 0]) & A[0, 0]);
            A2[3, 1] = A[3, 1] ^ ((~A[4, 1]) & A[0, 1]);
            A2[3, 2] = A[3, 2] ^ ((~A[4, 2]) & A[0, 2]);
            A2[3, 3] = A[3, 3] ^ ((~A[4, 3]) & A[0, 3]);
            A2[3, 4] = A[3, 4] ^ ((~A[4, 4]) & A[0, 4]);

            A2[4, 0] = A[4, 0] ^ ((~A[0, 0]) & A[1, 0]);
            A2[4, 1] = A[4, 1] ^ ((~A[0, 1]) & A[1, 1]);
            A2[4, 2] = A[4, 2] ^ ((~A[0, 2]) & A[1, 2]);
            A2[4, 3] = A[4, 3] ^ ((~A[0, 3]) & A[1, 3]);
            A2[4, 4] = A[4, 4] ^ ((~A[0, 4]) & A[1, 4]);

            Temp = A;
            A = A2;
            A2 = Temp;

            if (this.reportStates)
                this.NewState?.Invoke(this, new EventArgs());

            // ι function, as defined in section 3.2.5 of NIST FIPS 202.

            ulong RC = 0;

            for (j = 0; j <= 6; j++)    // l
            {
                i = (j + 7 * ir) % 255;
                if (i < 0)
                    i += 255;

                if (rcs[i])
                    RC |= RCs[j];
            }

            A[0, 0] ^= RC;

            if (this.reportStates)
                this.NewState?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Computes the KECCAK-p[b, nr] algorithm, as defined in section 3.3 of NIST FIPS 202.
        /// </summary>
        /// <param name="S">Input string of fixed length</param>
        /// <returns>Output string of fixed length</returns>
        public byte[] ComputeFixed(byte[] S)
        {
            int ir;

            this.InitState(S);

            if (this.reportStates)
                this.NewState?.Invoke(this, new EventArgs());

            for (ir = 24 - nr; ir < 24; ir++)
                this.Rnd(ir);

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
