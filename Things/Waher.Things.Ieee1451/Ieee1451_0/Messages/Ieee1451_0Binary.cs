using System;
using System.IO;
using System.Text;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Binary object
	/// </summary>
	public class Ieee1451_0Binary
	{
		private readonly int len;
		private int pos = 0;

		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="Body">Binary Body</param>
		public Ieee1451_0Binary(byte[] Body)
		{
			this.Body = Body;
			this.len = this.Body.Length;
		}
		
		/// <summary>
		/// Message Body
		/// </summary>
		public byte[] Body { get; }

		/// <summary>
		/// Current position.
		/// </summary>
		public int Position => this.pos;

		/// <summary>
		/// If the end of the data has been readed.
		/// </summary>
		public bool EOF => this.pos >= this.len;

		private static IOException UnexpectedEndOfData()
		{
			throw new IOException("Unexpected end of data.");
		}

		/// <summary>
		/// Gets the next <see cref="Byte"/>
		/// </summary>
		/// <returns>Next Value</returns>
		public byte NextUInt8()
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			return this.Body[this.pos++];
		}

		/// <summary>
		/// Gets the next <see cref="SByte"/>
		/// </summary>
		/// <returns>Next Value</returns>
		public sbyte NextInt8()
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			return (sbyte)this.Body[this.pos++];
		}

		/// <summary>
		/// Gets the next <see cref="UInt16"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public ushort NextUInt16()
		{
			if (this.pos + 1 >= this.len)
				throw UnexpectedEndOfData();

			ushort Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int16"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public short NextInt16()
		{
			if (this.pos + 1 >= this.len)
				throw UnexpectedEndOfData();

			ushort Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			return (short)Result;
		}

		/// <summary>
		/// Gets the next <see cref="UInt32"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public uint NextUInt32()
		{
			if (this.pos + 3 >= this.len)
				throw UnexpectedEndOfData();

			uint Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int32"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public int NextInt32()
		{
			if (this.pos + 3 >= this.len)
				throw UnexpectedEndOfData();

			int Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			return Result;
		}

		/// <summary>
		/// Gets the next UInt48.
		/// </summary>
		/// <returns>Next Value</returns>
		public ulong NextUInt48()
		{
			if (this.pos + 5 >= this.len)
				throw UnexpectedEndOfData();

			ulong Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int64"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public long NextInt64()
		{
			if (this.pos + 7 >= this.len)
				throw UnexpectedEndOfData();

			long Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Single"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public float NextSingle()
		{
			if (this.pos + 3 >= this.len)
				throw UnexpectedEndOfData();

			if (this.Body[this.pos] == 0x7f &&
				this.Body[this.pos + 1] == 0xff &&
				this.Body[this.pos + 2] == 0xff &&
				this.Body[this.pos + 3] == 0xff)
			{
				return float.NaN;
			}

			float Result = BitConverter.ToSingle(this.Body, this.pos);
			this.pos += 4;

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Double"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public double NextDouble()
		{
			if (this.pos + 7 >= this.len)
				throw UnexpectedEndOfData();

			double Result = BitConverter.ToDouble(this.Body, this.pos);
			this.pos += 8;

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="String"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public string NextString()
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			int i = this.pos;

			while (i < this.len && this.Body[i] != 0)
				i++;

			if (i >= this.len)
				throw UnexpectedEndOfData();

			string Result = Encoding.UTF8.GetString(this.Body, this.pos, i - this.pos);
			this.pos = i + 1;

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Boolean"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public bool NextBoolean()
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			return this.Body[this.pos++] != 0;
		}

		/// <summary>
		/// Gets the next <see cref="Ieee1451_0Time"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public Ieee1451_0Time NextTime()
		{
			return new Ieee1451_0Time()
			{
				Seconds = this.NextUInt48(),
				NanoSeconds = this.NextUInt32()
			};
		}

		/// <summary>
		/// Gets the next time duration, expressed in seconds.
		/// </summary>
		/// <returns>Time duration.</returns>
		public double NextDurationSeconds()
		{
			long TimeDuration = this.NextInt64();
			return TimeDuration * Math.Pow(2, -16) * 1e-9;
		}

		/// <summary>
		/// Gets the next physical units structure.
		/// </summary>
		/// <returns>Physical Units.</returns>
		public Ieee1451_0PhysicalUnits NextPhysicalUnits()
		{
			return new Ieee1451_0PhysicalUnits()
			{
				Interpretation = (Ieee1451_0PhysicalUnitsInterpretation)this.NextUInt8(),
				Radians = this.NextUInt8(),
				Steradians = this.NextUInt8(),
				Meters = this.NextUInt8(),
				Kilograms = this.NextUInt8(),
				Seconds = this.NextUInt8(),
				Amperes = this.NextUInt8(),
				Kelvins = this.NextUInt8(),
				Moles = this.NextUInt8(),
				Candelas = this.NextUInt8(),
			};
		}

		/// <summary>
		/// Gets the next <see cref="Byte"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public byte[] NextUInt8Array()
		{
			return this.NextUInt8Array(this.Body.Length - this.pos);
		}

		/// <summary>
		/// Gets the next <see cref="Byte"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public byte[] NextUInt8Array(int NrItems)
		{
			byte[] Result=new byte[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextUInt8();

			return Result;
		}

		/// <summary>
		/// Gets the next string array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public string[] NextStringArray(int NrItems)
		{
			string[] Result = new string[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextString();

			return Result;
		}

		/// <summary>
		/// Gets the next Boolean array
		/// </summary>
		/// <returns>Array.</returns>
		public bool[] NextBooleanArray()
		{
			return this.NextBooleanArray(this.Body.Length - this.pos);
		}

		/// <summary>
		/// Gets the next Boolean array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public bool[] NextBooleanArray(int NrItems)
		{
			bool[] Result = new bool[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextBoolean();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="SByte"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public sbyte[] NextInt8Array()
		{
			return this.NextInt8Array(this.Body.Length - this.pos);
		}

		/// <summary>
		/// Gets the next <see cref="SByte"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public sbyte[] NextInt8Array(int NrItems)
		{
			sbyte[] Result = new sbyte[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextInt8();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int16"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public short[] NextInt16Array()
		{
			return this.NextInt16Array((this.Body.Length - this.pos) / 2);
		}

		/// <summary>
		/// Gets the next <see cref="Int16"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public short[] NextInt16Array(int NrItems)
		{
			short[] Result = new short[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextInt16();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int32"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public int[] NextInt32Array()
		{
			return this.NextInt32Array((this.Body.Length - this.pos) / 4);
		}

		/// <summary>
		/// Gets the next <see cref="Int32"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public int[] NextInt32Array(int NrItems)
		{
			int[] Result = new int[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextInt32();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="UInt16"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public ushort[] NextUInt16Array()
		{
			return this.NextUInt16Array((this.Body.Length - this.pos) / 2);
		}

		/// <summary>
		/// Gets the next <see cref="UInt16"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public ushort[] NextUInt16Array(int NrItems)
		{
			ushort[] Result = new ushort[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextUInt16();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="UInt32"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public uint[] NextUInt32Array()
		{
			return this.NextUInt32Array((this.Body.Length - this.pos) / 4);
		}

		/// <summary>
		/// Gets the next <see cref="UInt32"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public uint[] NextUInt32Array(int NrItems)
		{
			uint[] Result = new uint[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextUInt32();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Single"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public float[] NextSingleArray()
		{
			return this.NextSingleArray((this.Body.Length - this.pos) / 4);
		}

		/// <summary>
		/// Gets the next <see cref="Single"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public float[] NextSingleArray(int NrItems)
		{
			float[] Result = new float[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextSingle();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Double"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public double[] NextDoubleArray()
		{
			return this.NextDoubleArray((this.Body.Length - this.pos) / 8);
		}

		/// <summary>
		/// Gets the next <see cref="Double"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public double[] NextDoubleArray(int NrItems)
		{
			double[] Result = new double[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextDouble();

			return Result;
		}

		/// <summary>
		/// Gets the next Time Duration array
		/// </summary>
		/// <returns>Array.</returns>
		public double[] NextDurationSecondsArray()
		{
			return this.NextDurationSecondsArray((this.Body.Length - this.pos) / 8);
		}

		/// <summary>
		/// Gets the next Time Duration array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public double[] NextDurationSecondsArray(int NrItems)
		{
			double[] Result = new double[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextDurationSeconds();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Ieee1451_0Time"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public Ieee1451_0Time[] NextTimeArray()
		{
			return this.NextTimeArray((this.Body.Length - this.pos) / 10);
		}

		/// <summary>
		/// Gets the next <see cref="Ieee1451_0Time"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public Ieee1451_0Time[] NextTimeArray(int NrItems)
		{
			Ieee1451_0Time[] Result = new Ieee1451_0Time[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextTime();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Guid"/>.
		/// </summary>
		/// <returns>GUID</returns>
		public Guid NextGuid()
		{
			return new Guid(this.NextUInt8Array(16));
		}

	}
}
