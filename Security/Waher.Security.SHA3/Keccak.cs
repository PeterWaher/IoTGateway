using System;

namespace Waher.Security.SHA3
{
    /// <summary>
    /// Acceptable bit sizes for the SHA-3 and KECCAK algorithms.
    /// </summary>
    public enum BitSize
    {
        /// <summary>
        /// 25 bits
        /// </summary>
        BitSize25 = 25,

        /// <summary>
        /// 50 bits
        /// </summary>
        BitSize50 = 50,

        /// <summary>
        /// 100 bits
        /// </summary>
        BitSize100 = 100,

        /// <summary>
        /// 200 bits
        /// </summary>
        BitSize200 = 200,

        /// <summary>
        /// 400 bits
        /// </summary>
        BitSize400 = 400,

        /// <summary>
        /// 800 bits
        /// </summary>
        BitSize800 = 800,

        /// <summary>
        /// 1600 bits
        /// </summary>
        BitSize1600 = 1600,
    }

    /// <summary>
    /// Implementation of the KECCAK-p permutations, as defined in section 3
    /// in the NIST FIPS 202: 
    /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
    /// </summary>
    public abstract class Keccak
    {
        private static readonly bool[] rcs = GetRcs();
        private static readonly ulong[] RCs = new ulong[] { 0x01, 0x02, 0x08, 0x80, 0x8000, 0x80000000, 0x8000000000000000 };
        private ulong[,] A = new ulong[5, 5];
        private ulong[,] A2 = new ulong[5, 5];
        private readonly ulong[] C = new ulong[5];
        private readonly int b;
        private readonly int w;
        private readonly int l;
        private readonly int r;
        private readonly int c;
        private readonly int r8;
        private readonly int r8m1;
        private readonly int dByteSize;
        private readonly int nr;
        private readonly int byteSize;
        private readonly byte suffix;
        private readonly byte suffixBits;
        private readonly ulong wMask;
        private bool reportStates = false;

        /// <summary>
        /// Implementation of the KECCAK-f permutations, as defined in section 3.4
        /// in the NIST FIPS 202: 
        /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
        /// </summary>
        /// <param name="BitSize">Bit Size.</param>
        /// <param name="Capacity">Capacity of sponge function.</param>
        /// <param name="Suffix">Suffix to append to variable-length messages before executing the Sponge function.</param>
        /// <param name="DigestSize">Size of results of the sponge function.</param>
        public Keccak(BitSize BitSize, int Capacity, byte Suffix, int DigestSize)
            : this(BitSize, 0, Capacity, Suffix, DigestSize)
        {
            this.nr = 12 + 2 * this.l;
        }

        /// <summary>
        /// Implementation of the KECCAK-p permutations, as defined in section 3.3
        /// in the NIST FIPS 202: 
        /// https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf
        /// </summary>
        /// <param name="BitSize">Bit Size.</param>
        /// <param name="Iterations">Number of iterations</param>
        /// <param name="Capacity">Capacity of sponge function.</param>
        /// <param name="Suffix">Suffix to append to variable-length messages before executing the Sponge function.</param>
        /// <param name="DigestSize">Size of results of the sponge function.</param>
        public Keccak(BitSize BitSize, int Iterations, int Capacity, byte Suffix, int DigestSize)
        {
            this.b = (int)BitSize;
            this.w = this.b / 25;
            this.byteSize = this.b / 8;
            this.nr = Iterations;
            this.c = Capacity;
            this.r = this.b - this.c;
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

            switch (this.b)
            {
                case 25:
                    this.l = 0;
                    this.wMask = 0x01;
                    break;

                case 50:
                    this.l = 1;
                    this.wMask = 0x03;
                    break;

                case 100:
                    this.l = 2;
                    this.wMask = 0x0f;
                    break;

                case 200:
                    this.l = 3;
                    this.wMask = 0xff;
                    break;

                case 400:
                    this.l = 4;
                    this.wMask = 0xffff;
                    break;

                case 800:
                    this.l = 5;
                    this.wMask = 0xffffffff;
                    break;

                case 1600:
                    this.l = 6;
                    this.wMask = 0xffffffffffffffff;
                    break;

                default:
                    throw new ArgumentException("Invalid bit size.", nameof(BitSize));
            }
        }

        /// <summary>
        /// Initializes the internal state.
        /// </summary>
        /// <param name="Data">Binary data</param>
        public void InitState(byte[] Data)
        {
            if (Data.Length != this.byteSize)
                throw new ArgumentException("Expected array of " + this.byteSize.ToString() + " bytes.", nameof(Data));

            int i = 0;
            int x, y;
            int Offset = 0;

            for (y = 0; y < 5; y++)
            {
                for (x = 0; x < 5; x++)
                {
                    switch (this.l)
                    {
                        case 0:
                            this.A[x, y] = (byte)((Data[i] >> Offset) & 0x01);
                            Offset++;
                            if (Offset >= 8)
                            {
                                i++;
                                Offset = 0;
                            }
                            break;

                        case 1:
                            this.A[x, y] = (byte)((Data[i] >> Offset) & 0x03);
                            Offset += 2;
                            if (Offset >= 8)
                            {
                                i++;
                                Offset = 0;
                            }
                            break;

                        case 2:
                            this.A[x, y] = (byte)((Data[i] >> Offset) & 0x0f);
                            Offset += 4;
                            if (Offset >= 8)
                            {
                                i++;
                                Offset = 0;
                            }
                            break;

                        case 3:
                            this.A[x, y] = Data[i++];
                            break;

                        case 4:
                            this.A[x, y] = BitConverter.ToUInt16(Data, i);
                            i += 2;
                            break;

                        case 5:
                            this.A[x, y] = BitConverter.ToUInt32(Data, i);
                            i += 4;
                            break;

                        case 6:
                            this.A[x, y] = BitConverter.ToUInt64(Data, i);
                            i += 8;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a copy of the internal state.
        /// </summary>
        /// <returns>Binary data</returns>
        public byte[] GetState()
        {
            byte[] Data = new byte[this.byteSize];

            int i = 0;
            int x, y;
            int Offset = 0;

            for (y = 0; y < 5; y++)
            {
                for (x = 0; x < 5; x++)
                {
                    switch (this.l)
                    {
                        case 0:
                            Data[i] |= (byte)((this.A[x, y] & 0x01) << Offset);
                            Offset++;
                            if (Offset >= 8)
                            {
                                i++;
                                Offset = 0;
                            }
                            break;

                        case 1:
                            Data[i] |= (byte)((this.A[x, y] & 0x03) << Offset);
                            Offset += 2;
                            if (Offset >= 8)
                            {
                                i++;
                                Offset = 0;
                            }
                            break;

                        case 2:
                            Data[i] |= (byte)((this.A[x, y] & 0x0f) << Offset);
                            Offset += 4;
                            if (Offset >= 8)
                            {
                                i++;
                                Offset = 0;
                            }
                            break;

                        case 3:
                            Data[i++] = (byte)this.A[x, y];
                            break;

                        case 4:
                            Array.Copy(BitConverter.GetBytes((ushort)A[x, y]), 0, Data, i, 2);
                            i += 2;
                            break;

                        case 5:
                            Array.Copy(BitConverter.GetBytes((uint)A[x, y]), 0, Data, i, 4);
                            i += 4;
                            break;

                        case 6:
                            Array.Copy(BitConverter.GetBytes(A[x, y]), 0, Data, i, 8);
                            i += 8;
                            break;
                    }
                }
            }

            return Data;
        }

        /// <summary>
        /// rc(t) function, as defined in section 3.2.5 of NIST FIPS 202.
        /// </summary>
        /// <param name="t">Integer input</param>
        /// <returns>Boolean output</returns>
        private static bool rc(int t)
        {
            int i = t % 255;
            if (i == 0)
                return true;
            else if (i < 0)
                i += 255;

            byte R = 1;// 0x80;

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
                Result[t] = rc(t);

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

            D = C[4] ^ ((C[1] << 1) | ((C[1] >> (this.w - 1)) & 1));
            A[0, 0] ^= D;
            A[0, 1] ^= D;
            A[0, 2] ^= D;
            A[0, 3] ^= D;
            A[0, 4] ^= D;

            D = C[0] ^ ((C[2] << 1) | ((C[2] >> (this.w - 1)) & 1));
            A[1, 0] ^= D;
            A[1, 1] ^= D;
            A[1, 2] ^= D;
            A[1, 3] ^= D;
            A[1, 4] ^= D;

            D = C[1] ^ ((C[3] << 1) | ((C[3] >> (this.w - 1)) & 1));
            A[2, 0] ^= D;
            A[2, 1] ^= D;
            A[2, 2] ^= D;
            A[2, 3] ^= D;
            A[2, 4] ^= D;

            D = C[2] ^ ((C[4] << 1) | ((C[4] >> (this.w - 1)) & 1));
            A[3, 0] ^= D;
            A[3, 1] ^= D;
            A[3, 2] ^= D;
            A[3, 3] ^= D;
            A[3, 4] ^= D;

            D = C[3] ^ ((C[0] << 1) | ((C[0] >> (this.w - 1)) & 1));
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
                i = ((t + 1) * (t + 2) / 2) % this.w;
                j = this.w - i;
                v = A[x, y];
                A[x, y] = (v << i) | ((v >> j) & this.wMask);

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

            for (j = 0; j <= l; j++)
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
            int to = 12 + 2 * l;
            int from = to - nr;

            this.InitState(S);

            if (this.reportStates)
                this.NewState?.Invoke(this, new EventArgs());

            for (ir = from; ir < to; ir++)
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
            byte[] S = new byte[this.byteSize];
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
