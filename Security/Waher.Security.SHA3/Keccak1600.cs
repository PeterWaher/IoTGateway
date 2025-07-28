﻿using System;
using System.IO;
using Waher.Runtime.IO;

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
		/// <param name="Suffix">Suffix (in reverse bit order) to append to variable-length messages 
		/// before executing the Sponge function.</param>
		/// <param name="SuffixBits">Number of bits in suffix.</param>
		/// <param name="DigestSize">Size of results of the sponge function.</param>
		public Keccak1600(int Capacity, byte Suffix, byte SuffixBits, int DigestSize)
		{
			this.c = Capacity;
			this.r = 1600 - this.c;
			this.r8 = this.r >> 3;
			this.r8m1 = this.r8 - 1;
			this.dByteSize = DigestSize / 8;
			this.suffix = Suffix;
			this.suffixBits = SuffixBits;

			if ((DigestSize & 7) != 0)
				throw new ArgumentException("Invalid digest size.", nameof(DigestSize));

			if (this.c <= 0 || this.r <= 0 || this.r8m1 < 0 || (Capacity & 7) != 0)
				throw new ArgumentException("Invalid capacity.", nameof(Capacity));

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

			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_0 : this.A_0), 0, Data, 0, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_1 : this.A_1), 0, Data, 8, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_2 : this.A_2), 0, Data, 16, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_3 : this.A_3), 0, Data, 24, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_4 : this.A_4), 0, Data, 32, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_5 : this.A_5), 0, Data, 40, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_6 : this.A_6), 0, Data, 48, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_7 : this.A_7), 0, Data, 56, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_8 : this.A_8), 0, Data, 64, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_9 : this.A_9), 0, Data, 72, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_10 : this.A_10), 0, Data, 80, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_11 : this.A_11), 0, Data, 88, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_12 : this.A_12), 0, Data, 96, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_13 : this.A_13), 0, Data, 104, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_14 : this.A_14), 0, Data, 112, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_15 : this.A_15), 0, Data, 120, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_16 : this.A_16), 0, Data, 128, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_17 : this.A_17), 0, Data, 136, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_18 : this.A_18), 0, Data, 144, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_19 : this.A_19), 0, Data, 152, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_20 : this.A_20), 0, Data, 160, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_21 : this.A_21), 0, Data, 168, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_22 : this.A_22), 0, Data, 176, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_23 : this.A_23), 0, Data, 184, 8);
			Array.Copy(BitConverter.GetBytes(this.inA2 ? this.A2_24 : this.A_24), 0, Data, 192, 8);

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
			{
				EventHandler h = this.NewState;
				if (!(h is null))
					h(this, EventArgs.Empty);
			}

			for (ir = 0; ir < 24; ir++)
			{
				// Rnd function, as defined in section 3.3 of NIST FIPS 202.
				// θ function, as defined in section 3.2.1 of NIST FIPS 202.

				this.C_0 = this.A_0 ^ this.A_5 ^ this.A_10 ^ this.A_15 ^ this.A_20;
				this.C_1 = this.A_1 ^ this.A_6 ^ this.A_11 ^ this.A_16 ^ this.A_21;
				this.C_2 = this.A_2 ^ this.A_7 ^ this.A_12 ^ this.A_17 ^ this.A_22;
				this.C_3 = this.A_3 ^ this.A_8 ^ this.A_13 ^ this.A_18 ^ this.A_23;
				this.C_4 = this.A_4 ^ this.A_9 ^ this.A_14 ^ this.A_19 ^ this.A_24;

				v = this.C_4 ^ ((this.C_1 << 1) | ((this.C_1 >> 63) & 1));
				this.A_0 ^= v;
				this.A_5 ^= v;
				this.A_10 ^= v;
				this.A_15 ^= v;
				this.A_20 ^= v;

				v = this.C_0 ^ ((this.C_2 << 1) | ((this.C_2 >> 63) & 1));
				this.A_1 ^= v;
				this.A_6 ^= v;
				this.A_11 ^= v;
				this.A_16 ^= v;
				this.A_21 ^= v;

				v = this.C_1 ^ ((this.C_3 << 1) | ((this.C_3 >> 63) & 1));
				this.A_2 ^= v;
				this.A_7 ^= v;
				this.A_12 ^= v;
				this.A_17 ^= v;
				this.A_22 ^= v;

				v = this.C_2 ^ ((this.C_4 << 1) | ((this.C_4 >> 63) & 1));
				this.A_3 ^= v;
				this.A_8 ^= v;
				this.A_13 ^= v;
				this.A_18 ^= v;
				this.A_23 ^= v;

				v = this.C_3 ^ ((this.C_0 << 1) | ((this.C_0 >> 63) & 1));
				this.A_4 ^= v;
				this.A_9 ^= v;
				this.A_14 ^= v;
				this.A_19 ^= v;
				this.A_24 ^= v;

				if (this.reportStates)
				{
					EventHandler h = this.NewState;
					if (!(h is null))
						h(this, EventArgs.Empty);
				}

				// ρ function, as defined in section 3.2.2 of NIST FIPS 202.

				this.A_1 = ((v = this.A_1) << 1) | (v >> 63);
				this.A_10 = ((v = this.A_10) << 3) | (v >> 61);
				this.A_7 = ((v = this.A_7) << 6) | (v >> 58);
				this.A_11 = ((v = this.A_11) << 10) | (v >> 54);
				this.A_17 = ((v = this.A_17) << 15) | (v >> 49);
				this.A_18 = ((v = this.A_18) << 21) | (v >> 43);
				this.A_3 = ((v = this.A_3) << 28) | (v >> 36);
				this.A_5 = ((v = this.A_5) << 36) | (v >> 28);
				this.A_16 = ((v = this.A_16) << 45) | (v >> 19);
				this.A_8 = ((v = this.A_8) << 55) | (v >> 9);
				this.A_21 = ((v = this.A_21) << 2) | (v >> 62);
				this.A_24 = ((v = this.A_24) << 14) | (v >> 50);
				this.A_4 = ((v = this.A_4) << 27) | (v >> 37);
				this.A_15 = ((v = this.A_15) << 41) | (v >> 23);
				this.A_23 = ((v = this.A_23) << 56) | (v >> 8);
				this.A_19 = ((v = this.A_19) << 8) | (v >> 56);
				this.A_13 = ((v = this.A_13) << 25) | (v >> 39);
				this.A_12 = ((v = this.A_12) << 43) | (v >> 21);
				this.A_2 = ((v = this.A_2) << 62) | (v >> 2);
				this.A_20 = ((v = this.A_20) << 18) | (v >> 46);
				this.A_14 = ((v = this.A_14) << 39) | (v >> 25);
				this.A_22 = ((v = this.A_22) << 61) | (v >> 3);
				this.A_9 = ((v = this.A_9) << 20) | (v >> 44);
				this.A_6 = ((v = this.A_6) << 44) | (v >> 20);

				if (this.reportStates)
				{
					EventHandler h = this.NewState;
					if (!(h is null))
						h(this, EventArgs.Empty);
				}

				// π function, as defined in section 3.2.3 of NIST FIPS 202.

				this.A2_0 = this.A_0;
				this.A2_5 = this.A_3;
				this.A2_10 = this.A_1;
				this.A2_15 = this.A_4;
				this.A2_20 = this.A_2;

				this.A2_1 = this.A_6;
				this.A2_6 = this.A_9;
				this.A2_11 = this.A_7;
				this.A2_16 = this.A_5;
				this.A2_21 = this.A_8;

				this.A2_2 = this.A_12;
				this.A2_7 = this.A_10;
				this.A2_12 = this.A_13;
				this.A2_17 = this.A_11;
				this.A2_22 = this.A_14;

				this.A2_3 = this.A_18;
				this.A2_8 = this.A_16;
				this.A2_13 = this.A_19;
				this.A2_18 = this.A_17;
				this.A2_23 = this.A_15;

				this.A2_4 = this.A_24;
				this.A2_9 = this.A_22;
				this.A2_14 = this.A_20;
				this.A2_19 = this.A_23;
				this.A2_24 = this.A_21;

				if (this.reportStates)
				{
					this.inA2 = true;

					EventHandler h = this.NewState;
					if (!(h is null))
						h(this, EventArgs.Empty);
				}

				// χ function, as defined in section 3.2.4 of NIST FIPS 202.

				this.A_0 = this.A2_0 ^ (this.A2_2 & ~this.A2_1);
				this.A_5 = this.A2_5 ^ (this.A2_7 & ~this.A2_6);
				this.A_10 = this.A2_10 ^ (this.A2_12 & ~this.A2_11);
				this.A_15 = this.A2_15 ^ (this.A2_17 & ~this.A2_16);
				this.A_20 = this.A2_20 ^ (this.A2_22 & ~this.A2_21);

				this.A_1 = this.A2_1 ^ (this.A2_3 & ~this.A2_2);
				this.A_6 = this.A2_6 ^ (this.A2_8 & ~this.A2_7);
				this.A_11 = this.A2_11 ^ (this.A2_13 & ~this.A2_12);
				this.A_16 = this.A2_16 ^ (this.A2_18 & ~this.A2_17);
				this.A_21 = this.A2_21 ^ (this.A2_23 & ~this.A2_22);

				this.A_2 = this.A2_2 ^ (this.A2_4 & ~this.A2_3);
				this.A_7 = this.A2_7 ^ (this.A2_9 & ~this.A2_8);
				this.A_12 = this.A2_12 ^ (this.A2_14 & ~this.A2_13);
				this.A_17 = this.A2_17 ^ (this.A2_19 & ~this.A2_18);
				this.A_22 = this.A2_22 ^ (this.A2_24 & ~this.A2_23);

				this.A_3 = this.A2_3 ^ (this.A2_0 & ~this.A2_4);
				this.A_8 = this.A2_8 ^ (this.A2_5 & ~this.A2_9);
				this.A_13 = this.A2_13 ^ (this.A2_10 & ~this.A2_14);
				this.A_18 = this.A2_18 ^ (this.A2_15 & ~this.A2_19);
				this.A_23 = this.A2_23 ^ (this.A2_20 & ~this.A2_24);

				this.A_4 = this.A2_4 ^ (this.A2_1 & ~this.A2_0);
				this.A_9 = this.A2_9 ^ (this.A2_6 & ~this.A2_5);
				this.A_14 = this.A2_14 ^ (this.A2_11 & ~this.A2_10);
				this.A_19 = this.A2_19 ^ (this.A2_16 & ~this.A2_15);
				this.A_24 = this.A2_24 ^ (this.A2_21 & ~this.A2_20);

				if (this.reportStates)
				{
					this.inA2 = false;

					EventHandler h = this.NewState;
					if (!(h is null))
						h(this, EventArgs.Empty);
				}

				// ι function, as defined in section 3.2.5 of NIST FIPS 202.

				this.A_0 ^= RC_ir[ir];

				if (this.reportStates)
				{
					EventHandler h = this.NewState;
					if (!(h is null))
						h(this, EventArgs.Empty);
				}
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
			int nm1 = m / this.r;
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
				i = Math.Min(this.r8, this.dByteSize - Pos);
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
			long nm1 = m / this.r;
			byte[] S = new byte[200];
			byte[] r8Buf = new byte[this.r8];
			int i, k;

			N.Position = 0;

			for (i = 0; i < nm1; i++)
			{
				N.ReadAll(r8Buf, 0, this.r8);

				for (k = 0; k < this.r8; k++)
					S[k] ^= r8Buf[k];

				S = this.ComputeFixed(S);
			}

			int Rest = (int)(Len - N.Position);
			if (Len > 0)
			{
				N.ReadAll(r8Buf, 0, Rest);

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
				i = Math.Min(this.r8, this.dByteSize - Pos);
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

		/// <summary>
		/// Initiates a variable length digest computation using the SPONGE function, 
		/// absorbing the input data, as defined in section 4 of NIST FIPS 202.
		/// See NIST FIPS 203 for details on the ML-KEM algorithm and its use of the
		/// Absorbe and Squeeze methods.
		/// </summary>
		/// <param name="N">Input string of variable length.</param>
		/// <returns></returns>
		public Context Absorb(byte[] N)
		{
			this.reportStates = !(this.NewState is null);

			int Len = N.Length;
			int m = Len << 3;
			int nm1 = m / this.r;
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

			return new Context(S, this);
		}

		/// <summary>
		/// Hash digest computation context.
		/// </summary>
		public class Context
		{
			private readonly Keccak1600 hashFunction;
			private byte[] state;
			private int statePosition;

			/// <summary>
			/// Hash digest computation context.
			/// </summary>
			/// <param name="S">Internal state of algorithm.</param>
			/// <param name="H">Hash-function performing computations.</param>
			public Context(byte[] S, Keccak1600 H)
			{
				this.state = S;
				this.hashFunction = H;
				this.statePosition = 0;
			}

			/// <summary>
			/// Calculates another <paramref name="NrBytes"/> number of bytes of the digest.
			/// </summary>
			/// <param name="NrBytes">Number of bytes to compute.</param>
			/// <returns></returns>
			public byte[] Squeeze(int NrBytes)
			{
				byte[] Z = new byte[NrBytes];
				int Pos = 0;
				int i;

				if (this.statePosition >= this.hashFunction.r8)
				{
					this.state = this.hashFunction.ComputeFixed(this.state);
					this.statePosition = 0;
				}

				while (true)
				{
					i = Math.Min(this.hashFunction.r8 - this.statePosition, NrBytes - Pos);
					Array.Copy(this.state, this.statePosition, Z, Pos, i);
					Pos += i;
					this.statePosition += i;

					if (Pos >= NrBytes)
						return Z;

					this.state = this.hashFunction.ComputeFixed(this.state);
					this.statePosition = 0;
				}
			}
		}
	}
}
