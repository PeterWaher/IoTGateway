using System;
using System.IO;

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
        private ulong A_0, A_1, A_2, A_3, A_4, A_5, A_6, A_7, A_8, A_9, A_10, A_11, A_12, A_13, A_14, A_15, A_16, A_17, A_18, A_19, A_20, A_21, A_22, A_23, A_24;
        private ulong A2_0, A2_1, A2_2, A2_3, A2_4, A2_5, A2_6, A2_7, A2_8, A2_9, A2_10, A2_11, A2_12, A2_13, A2_14, A2_15, A2_16, A2_17, A2_18, A2_19, A2_20, A2_21, A2_22, A2_23, A2_24;
        private ulong C_0, C_1, C_2, C_3, C_4;
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

            this.A_0 = BitConverter.ToUInt64(Data, 0);
            this.A_1 = BitConverter.ToUInt64(Data, 8);
            this.A_2 = BitConverter.ToUInt64(Data, 16);
            this.A_3 = BitConverter.ToUInt64(Data, 24);
            this.A_4 = BitConverter.ToUInt64(Data, 32);
            this.A_5 = BitConverter.ToUInt64(Data, 40);
            this.A_6 = BitConverter.ToUInt64(Data, 48);
            this.A_7 = BitConverter.ToUInt64(Data, 56);
            this.A_8 = BitConverter.ToUInt64(Data, 64);
            this.A_9 = BitConverter.ToUInt64(Data, 72);
            this.A_10 = BitConverter.ToUInt64(Data, 80);
            this.A_11 = BitConverter.ToUInt64(Data, 88);
            this.A_12 = BitConverter.ToUInt64(Data, 96);
            this.A_13 = BitConverter.ToUInt64(Data, 104);
            this.A_14 = BitConverter.ToUInt64(Data, 112);
            this.A_15 = BitConverter.ToUInt64(Data, 120);
            this.A_16 = BitConverter.ToUInt64(Data, 128);
            this.A_17 = BitConverter.ToUInt64(Data, 136);
            this.A_18 = BitConverter.ToUInt64(Data, 144);
            this.A_19 = BitConverter.ToUInt64(Data, 152);
            this.A_20 = BitConverter.ToUInt64(Data, 160);
            this.A_21 = BitConverter.ToUInt64(Data, 168);
            this.A_22 = BitConverter.ToUInt64(Data, 176);
            this.A_23 = BitConverter.ToUInt64(Data, 184);
            this.A_24 = BitConverter.ToUInt64(Data, 192);
        }

        /// <summary>
        /// Gets a copy of the internal state.
        /// </summary>
        /// <returns>Binary data</returns>
        public byte[] GetState()
        {
            byte[] Data = new byte[200];    // this.byteSize

            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_0 : A_0), 0, Data, 0, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_1 : A_1), 0, Data, 8, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_2 : A_2), 0, Data, 16, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_3 : A_3), 0, Data, 24, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_4 : A_4), 0, Data, 32, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_5 : A_5), 0, Data, 40, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_6 : A_6), 0, Data, 48, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_7 : A_7), 0, Data, 56, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_8 : A_8), 0, Data, 64, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_9 : A_9), 0, Data, 72, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_10 : A_10), 0, Data, 80, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_11 : A_11), 0, Data, 88, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_12 : A_12), 0, Data, 96, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_13 : A_13), 0, Data, 104, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_14 : A_14), 0, Data, 112, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_15 : A_15), 0, Data, 120, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_16 : A_16), 0, Data, 128, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_17 : A_17), 0, Data, 136, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_18 : A_18), 0, Data, 144, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_19 : A_19), 0, Data, 152, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_20 : A_20), 0, Data, 160, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_21 : A_21), 0, Data, 168, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_22 : A_22), 0, Data, 176, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_23 : A_23), 0, Data, 184, 8);
            Array.Copy(BitConverter.GetBytes(this.inA2 ? A2_24 : A_24), 0, Data, 192, 8);

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
                this.NewState?.Invoke(this, EventArgs.Empty);

            for (ir = 0; ir < 24; ir++)
            {
                // Rnd function, as defined in section 3.3 of NIST FIPS 202.
                // θ function, as defined in section 3.2.1 of NIST FIPS 202.

                C_0 = A_0 ^ A_5 ^ A_10 ^ A_15 ^ A_20;
                C_1 = A_1 ^ A_6 ^ A_11 ^ A_16 ^ A_21;
                C_2 = A_2 ^ A_7 ^ A_12 ^ A_17 ^ A_22;
                C_3 = A_3 ^ A_8 ^ A_13 ^ A_18 ^ A_23;
                C_4 = A_4 ^ A_9 ^ A_14 ^ A_19 ^ A_24;

                v = C_4 ^ ((C_1 << 1) | ((C_1 >> 63) & 1));
                A_0 ^= v;
                A_5 ^= v;
                A_10 ^= v;
                A_15 ^= v;
                A_20 ^= v;

                v = C_0 ^ ((C_2 << 1) | ((C_2 >> 63) & 1));
                A_1 ^= v;
                A_6 ^= v;
                A_11 ^= v;
                A_16 ^= v;
                A_21 ^= v;

                v = C_1 ^ ((C_3 << 1) | ((C_3 >> 63) & 1));
                A_2 ^= v;
                A_7 ^= v;
                A_12 ^= v;
                A_17 ^= v;
                A_22 ^= v;

                v = C_2 ^ ((C_4 << 1) | ((C_4 >> 63) & 1));
                A_3 ^= v;
                A_8 ^= v;
                A_13 ^= v;
                A_18 ^= v;
                A_23 ^= v;

                v = C_3 ^ ((C_0 << 1) | ((C_0 >> 63) & 1));
                A_4 ^= v;
                A_9 ^= v;
                A_14 ^= v;
                A_19 ^= v;
                A_24 ^= v;

                if (this.reportStates)
                    this.NewState?.Invoke(this, EventArgs.Empty);

                // ρ function, as defined in section 3.2.2 of NIST FIPS 202.

                A_1 = ((v = A_1) << 1) | (v >> 63);
                A_10 = ((v = A_10) << 3) | (v >> 61);
                A_7 = ((v = A_7) << 6) | (v >> 58);
                A_11 = ((v = A_11) << 10) | (v >> 54);
                A_17 = ((v = A_17) << 15) | (v >> 49);
                A_18 = ((v = A_18) << 21) | (v >> 43);
                A_3 = ((v = A_3) << 28) | (v >> 36);
                A_5 = ((v = A_5) << 36) | (v >> 28);
                A_16 = ((v = A_16) << 45) | (v >> 19);
                A_8 = ((v = A_8) << 55) | (v >> 9);
                A_21 = ((v = A_21) << 2) | (v >> 62);
                A_24 = ((v = A_24) << 14) | (v >> 50);
                A_4 = ((v = A_4) << 27) | (v >> 37);
                A_15 = ((v = A_15) << 41) | (v >> 23);
                A_23 = ((v = A_23) << 56) | (v >> 8);
                A_19 = ((v = A_19) << 8) | (v >> 56);
                A_13 = ((v = A_13) << 25) | (v >> 39);
                A_12 = ((v = A_12) << 43) | (v >> 21);
                A_2 = ((v = A_2) << 62) | (v >> 2);
                A_20 = ((v = A_20) << 18) | (v >> 46);
                A_14 = ((v = A_14) << 39) | (v >> 25);
                A_22 = ((v = A_22) << 61) | (v >> 3);
                A_9 = ((v = A_9) << 20) | (v >> 44);
                A_6 = ((v = A_6) << 44) | (v >> 20);

                if (this.reportStates)
                    this.NewState?.Invoke(this, EventArgs.Empty);

                // π function, as defined in section 3.2.3 of NIST FIPS 202.

                A2_0 = A_0;
                A2_5 = A_3;
                A2_10 = A_1;
                A2_15 = A_4;
                A2_20 = A_2;

                A2_1 = A_6;
                A2_6 = A_9;
                A2_11 = A_7;
                A2_16 = A_5;
                A2_21 = A_8;

                A2_2 = A_12;
                A2_7 = A_10;
                A2_12 = A_13;
                A2_17 = A_11;
                A2_22 = A_14;

                A2_3 = A_18;
                A2_8 = A_16;
                A2_13 = A_19;
                A2_18 = A_17;
                A2_23 = A_15;

                A2_4 = A_24;
                A2_9 = A_22;
                A2_14 = A_20;
                A2_19 = A_23;
                A2_24 = A_21;

                if (this.reportStates)
                {
                    this.inA2 = true;
                    this.NewState?.Invoke(this, EventArgs.Empty);
                }

                // χ function, as defined in section 3.2.4 of NIST FIPS 202.

                A_0 = A2_0 ^ (A2_2 & ~A2_1);
                A_5 = A2_5 ^ (A2_7 & ~A2_6);
                A_10 = A2_10 ^ (A2_12 & ~A2_11);
                A_15 = A2_15 ^ (A2_17 & ~A2_16);
                A_20 = A2_20 ^ (A2_22 & ~A2_21);

                A_1 = A2_1 ^ (A2_3 & ~A2_2);
                A_6 = A2_6 ^ (A2_8 & ~A2_7);
                A_11 = A2_11 ^ (A2_13 & ~A2_12);
                A_16 = A2_16 ^ (A2_18 & ~A2_17);
                A_21 = A2_21 ^ (A2_23 & ~A2_22);

                A_2 = A2_2 ^ (A2_4 & ~A2_3);
                A_7 = A2_7 ^ (A2_9 & ~A2_8);
                A_12 = A2_12 ^ (A2_14 & ~A2_13);
                A_17 = A2_17 ^ (A2_19 & ~A2_18);
                A_22 = A2_22 ^ (A2_24 & ~A2_23);

                A_3 = A2_3 ^ (A2_0 & ~A2_4);
                A_8 = A2_8 ^ (A2_5 & ~A2_9);
                A_13 = A2_13 ^ (A2_10 & ~A2_14);
                A_18 = A2_18 ^ (A2_15 & ~A2_19);
                A_23 = A2_23 ^ (A2_20 & ~A2_24);

                A_4 = A2_4 ^ (A2_1 & ~A2_0);
                A_9 = A2_9 ^ (A2_6 & ~A2_5);
                A_14 = A2_14 ^ (A2_11 & ~A2_10);
                A_19 = A2_19 ^ (A2_16 & ~A2_15);
                A_24 = A2_24 ^ (A2_21 & ~A2_20);

                if (this.reportStates)
                {
                    this.inA2 = false;
                    this.NewState?.Invoke(this, EventArgs.Empty);
                }

                // ι function, as defined in section 3.2.5 of NIST FIPS 202.

                A_0 ^= RC_ir[ir];

                if (this.reportStates)
                    this.NewState?.Invoke(this, EventArgs.Empty);
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
        /// Computes the SPONGE function, as defined in section 4 of NIST FIPS 202.
        /// </summary>
        /// <param name="N">Input string of variable length.</param>
        /// <returns>Output string of fixed length.</returns>
        public byte[] ComputeVariable(Stream N)
        {
            this.reportStates = !(this.NewState is null);

            long Len = N.Length;
            long m = Len << 3;
            long nm1 = m / r;
            byte[] S = new byte[200];
            byte[] r8Buf = new byte[this.r8];
            int i, k;

            N.Position = 0;

            for (i = 0; i < nm1; i++)
            {
                if (N.Read(r8Buf, 0, this.r8) != this.r8)
                    throw new IOException("Unable to read from stream.");

                for (k = 0; k < this.r8; k++)
                    S[k] ^= r8Buf[k];

                S = this.ComputeFixed(S);
            }

            int Rest = (int)(Len - N.Position);
            if (Len > 0)
            {
                if (N.Read(r8Buf, 0, Rest) != Rest)
                    throw new IOException("Unable to read from stream.");

                for (k = 0; k < Rest; k++)
                    S[k] ^= r8Buf[k];
            }
            else
                k = 0;

            S[k] ^= this.suffix;
            S[this.r8m1] ^= 0x80;   // Last bit of pad10*1
            S = this.ComputeFixed(S);

            byte[] Z = new byte[this.dByteSize];

            int Pos = 0;
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
