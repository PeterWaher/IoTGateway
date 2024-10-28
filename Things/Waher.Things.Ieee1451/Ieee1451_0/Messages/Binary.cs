using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Binary object
	/// </summary>
	public class Binary
	{
		private static readonly Dictionary<ClassTypePair, IFieldType> fieldTypes = new Dictionary<ClassTypePair, IFieldType>();

		private readonly int len;
		private int pos = 0;

		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="Body">Binary Body</param>
		public Binary(byte[] Body)
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
		public int Position
		{
			get => this.pos;
			internal set => this.pos = value;
		}

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
		/// Gets the next 24-bit unsigned integer.
		/// </summary>
		/// <returns>Next Value</returns>
		public uint NextUInt24()
		{
			if (this.pos + 2 >= this.len)
				throw UnexpectedEndOfData();

			uint Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			return Result;
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

			byte[] Temp = new byte[4];
			Array.Copy(this.Body, this.pos, Temp, 0, 4);
			Array.Reverse(Temp);

			float Result = BitConverter.ToSingle(Temp, this.pos);
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

			byte[] Temp = new byte[8];
			Array.Copy(this.Body, this.pos, Temp, 0, 8);
			Array.Reverse(Temp);

			double Result = BitConverter.ToDouble(Temp, this.pos);
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
		/// Gets the next <see cref="Time"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public Time NextTime()
		{
			return new Time(
				this.NextUInt48(), 
				this.NextUInt32());
		}

		/// <summary>
		/// Gets the next time duration, expressed in seconds.
		/// </summary>
		/// <returns>Time duration.</returns>
		public double NextTimeDurationSeconds()
		{
			long TimeDuration = this.NextInt64();   // ns * 2^16
			double Result = TimeDuration;           // ns * 2^16
			Result /= 65536;                        // ns
			Result *= 1e-9;                         // s
			return Result;
		}

		/// <summary>
		/// Gets the next physical units structure.
		/// </summary>
		/// <returns>Physical Units.</returns>
		public PhysicalUnits NextPhysicalUnits()
		{
			byte[] Bin = this.NextUInt8Array(10);

			return new PhysicalUnits()
			{
				Binary = Bin,
				Interpretation = (Ieee1451_0PhysicalUnitsInterpretation)Bin[0],
				Radians = Bin[1],
				Steradians = Bin[2],
				Meters = Bin[3],
				Kilograms = Bin[4],
				Seconds = Bin[5],
				Amperes = Bin[6],
				Kelvins = Bin[7],
				Moles = Bin[8],
				Candelas = Bin[9]
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
			if (this.pos + NrItems > this.len)
				throw UnexpectedEndOfData();

			byte[] Result = new byte[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.Body[this.pos++];

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
				Result[i] = this.NextTimeDurationSeconds();

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Time"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public Time[] NextTimeArray()
		{
			return this.NextTimeArray((this.Body.Length - this.pos) / 10);
		}

		/// <summary>
		/// Gets the next <see cref="Time"/> array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public Time[] NextTimeArray(int NrItems)
		{
			Time[] Result = new Time[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextTime();

			return Result;
		}

		/// <summary>
		/// Gets the next UUID.
		/// </summary>
		/// <returns>UUID</returns>
		public byte[] NextUuid()
		{
			return this.NextUInt8Array(16);
		}

		/// <summary>
		/// Gets the next UUID array
		/// </summary>
		/// <returns>Array.</returns>
		public byte[][] NextUuidArray()
		{
			return this.NextUuidArray((this.Body.Length - this.pos) / 10);
		}

		/// <summary>
		/// Gets the next UUID array
		/// </summary>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public byte[][] NextUuidArray(int NrItems)
		{
			byte[][] Result = new byte[NrItems][];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextUuid();

			return Result;
		}

		/// <summary>
		/// Parses a set of TEDS records.
		/// </summary>
		/// <returns>Array of records.</returns>
		/// <exception cref="IOException">If records are not correctly encoded.</exception>
		public TedsRecord[] ParseTedsRecords(ParsingState State)
		{
			List<TedsRecord> Records = new List<TedsRecord>();
			byte TupleLength = 1;

			while (!this.EOF)
			{
				byte Type = this.NextUInt8();
				int Length;

				switch (TupleLength)
				{
					case 0:
						Length = 0;
						break;

					case 1:
						Length = this.NextUInt8();
						break;

					case 2:
						Length = this.NextUInt16();
						break;

					case 3:
						Length = (int)this.NextUInt24();
						break;

					case 4:
						uint i = this.NextUInt32();
						if (i > int.MaxValue)
							throw new IOException("Invalid length: " + i.ToString());

						Length = (int)i;
						break;

					default:
						throw new IOException("Invalid tuple length: " + TupleLength.ToString());
				}

				byte[] RawValue = this.NextUInt8Array(Length);
				ClassTypePair RecordTypeId = new ClassTypePair(State.Class, Type);
				IFieldType FieldType;

				lock (fieldTypes)
				{
					if (!fieldTypes.TryGetValue(RecordTypeId, out FieldType))
						FieldType = null;
				}

				if (FieldType is null)
				{
					FieldType = Types.FindBest<IFieldType, ClassTypePair>(RecordTypeId);

					lock (fieldTypes)
					{
						fieldTypes[RecordTypeId] = FieldType;
					}
				}

				TedsRecord Record = FieldType.Parse(RecordTypeId, new Binary(RawValue), State);
				if (Record is TedsId TedsId)
				{
					State.Class = TedsId.Class;
					TupleLength = TedsId.TupleLength;
				}

				Records.Add(Record);
			}

			return Records.ToArray();
		}
	}
}
