using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Units;
using Waher.Security;
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

		private readonly StringBuilder snifferOutput;
		private readonly int len;
		private readonly bool multiRow;
		private readonly bool hasSniffer;
		private readonly Binary containerPacket;
		private int pos = 0;
		private bool firstOutput = true;

		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="Body">Binary Body</param>
		/// <param name="Sniffable">Sniffable interface on which the message was received.</param>
		/// <param name="MultiRow">If sniffer output should be on multiple rows (true) or a single row (false).</param>
		public Binary(byte[] Body, ISniffable Sniffable, bool MultiRow)
		{
			this.Body = Body;
			this.len = this.Body.Length;

			this.Sniffable = Sniffable;
			this.hasSniffer = Sniffable?.HasSniffers ?? false;
			this.snifferOutput = this.hasSniffer ? new StringBuilder() : null;
			this.multiRow = MultiRow;
			this.containerPacket = null;
		}

		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="Body">Binary Body</param>
		/// <param name="ContainerPacket">Packet containing the current binary block</param>
		public Binary(byte[] Body, Binary ContainerPacket)
		{
			this.Body = Body;
			this.len = this.Body.Length;

			this.Sniffable = ContainerPacket.Sniffable;
			this.hasSniffer = ContainerPacket.hasSniffer;
			this.snifferOutput = ContainerPacket.snifferOutput;
			this.multiRow = ContainerPacket.multiRow;
			this.firstOutput = ContainerPacket.firstOutput;
			this.containerPacket = ContainerPacket;
		}

		/// <summary>
		/// Message Body
		/// </summary>
		public byte[] Body { get; }

		/// <summary>
		/// Sniffable interface on which the message was received.
		/// </summary>
		public ISniffable Sniffable { get; }

		/// <summary>
		/// If sniffers are available.
		/// </summary>
		public bool HasSniffers => this.Sniffable?.HasSniffers ?? false;

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
		/// Logs accumulated sniffer output to associated sniffable interface.
		/// </summary>
		public Task LogInformationToSniffer()
		{
			if (this.hasSniffer)
			{
				string s = this.snifferOutput.ToString();

				if (!string.IsNullOrEmpty(s))
				{
					this.snifferOutput.Clear();
					this.firstOutput = true;
				
					return this.Sniffable.Information(s);
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Adds a named value to the sniffer output.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Value">Value</param>
		public void SniffValue(string Name, string Value)
		{
			if (this.hasSniffer)
			{
				if (this.containerPacket is null)
				{
					if (this.firstOutput)
						this.firstOutput = false;
					else if (this.multiRow)
						this.snifferOutput.AppendLine();
					else
						this.snifferOutput.Append(", ");

					this.snifferOutput.Append(Name);
					this.snifferOutput.Append('=');
					this.snifferOutput.Append(Value);
				}
				else
					this.containerPacket.SniffValue(Name, Value);
			}
		}

		/// <summary>
		/// Gets the next <see cref="byte"/>
		/// </summary>
		/// <returns>Next Value</returns>
		public byte NextUInt8(string Name)
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			byte Result = this.Body[this.pos++];

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="byte"/>
		/// </summary>
		/// <returns>Next Value</returns>
		public T NextUInt8<T>(string Name)
			where T : Enum
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			byte b = this.Body[this.pos++];
			T Result = (T)Enum.ToObject(typeof(T), b);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="sbyte"/>
		/// </summary>
		/// <returns>Next Value</returns>
		public sbyte NextInt8(string Name)
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			sbyte Result = (sbyte)this.Body[this.pos++];

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="UInt16"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public ushort NextUInt16(string Name)
		{
			if (this.pos + 1 >= this.len)
				throw UnexpectedEndOfData();

			ushort Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int16"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public short NextInt16(string Name)
		{
			if (this.pos + 1 >= this.len)
				throw UnexpectedEndOfData();

			ushort Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, ((short)Result).ToString());

			return (short)Result;
		}

		/// <summary>
		/// Gets the next 24-bit unsigned integer.
		/// </summary>
		/// <returns>Next Value</returns>
		public uint NextUInt24(string Name)
		{
			if (this.pos + 2 >= this.len)
				throw UnexpectedEndOfData();

			uint Result = this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];
			Result <<= 8;
			Result |= this.Body[this.pos++];

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="UInt32"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public uint NextUInt32(string Name)
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

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int32"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public int NextInt32(string Name)
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

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next UInt48.
		/// </summary>
		/// <returns>Next Value</returns>
		public ulong NextUInt48(string Name)
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

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int64"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public long NextInt64(string Name)
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

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Single"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public float NextSingle(string Name)
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

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Double"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public double NextDouble(string Name)
		{
			if (this.pos + 7 >= this.len)
				throw UnexpectedEndOfData();

			byte[] Temp = new byte[8];
			Array.Copy(this.Body, this.pos, Temp, 0, 8);
			Array.Reverse(Temp);

			double Result = BitConverter.ToDouble(Temp, this.pos);
			this.pos += 8;

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="String"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public string NextString(string Name)
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

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Boolean"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public bool NextBoolean(string Name)
		{
			if (this.pos >= this.len)
				throw UnexpectedEndOfData();

			bool Result = this.Body[this.pos++] != 0;

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Time"/>.
		/// </summary>
		/// <returns>Next Value</returns>
		public Time NextTime(string Name)
		{
			Time Result = new Time(
				this.NextUInt48(null),
				this.NextUInt32(null));

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToDateTime().ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next time duration, expressed in seconds.
		/// </summary>
		/// <returns>Time duration.</returns>
		public double NextTimeDurationSeconds(string Name)
		{
			long TimeDuration = this.NextInt64(null);   // ns * 2^16
			double Result = TimeDuration;				// ns * 2^16
			Result /= 65536;							// ns
			Result *= 1e-9;                             // s

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Result.ToString());

			return Result;
		}

		/// <summary>
		/// Gets the next physical units structure.
		/// </summary>
		/// <returns>Physical Units.</returns>
		public PhysicalUnits NextPhysicalUnits(string Name)
		{
			byte[] Bin = this.NextUInt8Array(null, 10);

			PhysicalUnits Result = new PhysicalUnits()
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

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
			{
				Unit Unit = Result.TryCreateUnit();
				if (Unit is null)
					this.SniffValue(Name, Hashes.BinaryToString(Bin, true));
				else
					this.SniffValue(Name, Unit.ToString());
			}

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Byte"/> array
		/// </summary>
		/// <returns>Array.</returns>
		public byte[] NextUInt8Array(string Name)
		{
			return this.NextUInt8Array(Name, this.Body.Length - this.pos);
		}

		/// <summary>
		/// Gets the next <see cref="Byte"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public byte[] NextUInt8Array(string Name, int NrItems)
		{
			if (this.pos + NrItems > this.len)
				throw UnexpectedEndOfData();

			byte[] Result = new byte[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.Body[this.pos++];

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Hashes.BinaryToString(Result, true));

			return Result;
		}

		/// <summary>
		/// Gets the next string array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public string[] NextStringArray(string Name, int NrItems)
		{
			string[] Result = new string[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextString(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next Boolean array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public bool[] NextBooleanArray(string Name)
		{
			return this.NextBooleanArray(Name, this.Body.Length - this.pos);
		}

		/// <summary>
		/// Gets the next Boolean array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public bool[] NextBooleanArray(string Name, int NrItems)
		{
			bool[] Result = new bool[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextBoolean(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="SByte"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public sbyte[] NextInt8Array(string Name)
		{
			return this.NextInt8Array(Name, this.Body.Length - this.pos);
		}

		/// <summary>
		/// Gets the next <see cref="SByte"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public sbyte[] NextInt8Array(string Name, int NrItems)
		{
			sbyte[] Result = new sbyte[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextInt8(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int16"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public short[] NextInt16Array(string Name)
		{
			return this.NextInt16Array(Name, (this.Body.Length - this.pos) / 2);
		}

		/// <summary>
		/// Gets the next <see cref="Int16"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public short[] NextInt16Array(string Name, int NrItems)
		{
			short[] Result = new short[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextInt16(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Int32"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public int[] NextInt32Array(string Name)
		{
			return this.NextInt32Array(Name, (this.Body.Length - this.pos) / 4);
		}

		/// <summary>
		/// Gets the next <see cref="Int32"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public int[] NextInt32Array(string Name, int NrItems)
		{
			int[] Result = new int[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextInt32(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="UInt16"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public ushort[] NextUInt16Array(string Name)
		{
			return this.NextUInt16Array(Name, (this.Body.Length - this.pos) / 2);
		}

		/// <summary>
		/// Gets the next <see cref="UInt16"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public ushort[] NextUInt16Array(string Name, int NrItems)
		{
			ushort[] Result = new ushort[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextUInt16(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="UInt32"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public uint[] NextUInt32Array(string Name)
		{
			return this.NextUInt32Array(Name, (this.Body.Length - this.pos) / 4);
		}

		/// <summary>
		/// Gets the next <see cref="UInt32"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public uint[] NextUInt32Array(string Name, int NrItems)
		{
			uint[] Result = new uint[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextUInt32(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Single"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public float[] NextSingleArray(string Name)
		{
			return this.NextSingleArray(Name, (this.Body.Length - this.pos) / 4);
		}

		/// <summary>
		/// Gets the next <see cref="Single"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public float[] NextSingleArray(string Name, int NrItems)
		{
			float[] Result = new float[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextSingle(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Double"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public double[] NextDoubleArray(string Name)
		{
			return this.NextDoubleArray(Name, (this.Body.Length - this.pos) / 8);
		}

		/// <summary>
		/// Gets the next <see cref="Double"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public double[] NextDoubleArray(string Name, int NrItems)
		{
			double[] Result = new double[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextDouble(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next Time Duration array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public double[] NextDurationSecondsArray(string Name)
		{
			return this.NextDurationSecondsArray(Name, (this.Body.Length - this.pos) / 8);
		}

		/// <summary>
		/// Gets the next Time Duration array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public double[] NextDurationSecondsArray(string Name, int NrItems)
		{
			double[] Result = new double[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextTimeDurationSeconds(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next <see cref="Time"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public Time[] NextTimeArray(string Name)
		{
			return this.NextTimeArray(Name, (this.Body.Length - this.pos) / 10);
		}

		/// <summary>
		/// Gets the next <see cref="Time"/> array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public Time[] NextTimeArray(string Name, int NrItems)
		{
			Time[] Result = new Time[NrItems];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextTime(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

			return Result;
		}

		/// <summary>
		/// Gets the next UUID.
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>UUID</returns>
		public byte[] NextUuid(string Name)
		{
			return this.NextUInt8Array(Name, 16);
		}

		/// <summary>
		/// Gets the next UUID array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <returns>Array.</returns>
		public byte[][] NextUuidArray(string Name)
		{
			return this.NextUuidArray(Name, (this.Body.Length - this.pos) / 10);
		}

		/// <summary>
		/// Gets the next UUID array
		/// </summary>
		/// <param name="Name">Name to display in sniffers.</param>
		/// <param name="NrItems">Number of items in array.</param>
		/// <returns>Array.</returns>
		public byte[][] NextUuidArray(string Name, int NrItems)
		{
			byte[][] Result = new byte[NrItems][];
			int i;

			for (i = 0; i < NrItems; i++)
				Result[i] = this.NextUuid(null);

			if (this.hasSniffer && !string.IsNullOrEmpty(Name))
				this.SniffValue(Name, Expression.ToString(Expression.Encapsulate(Result)));

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
				byte Type = this.NextUInt8(null);
				int Length;

				switch (TupleLength)
				{
					case 0:
						Length = 0;
						break;

					case 1:
						Length = this.NextUInt8(null);
						break;

					case 2:
						Length = this.NextUInt16(null);
						break;

					case 3:
						Length = (int)this.NextUInt24(null);
						break;

					case 4:
						uint i = this.NextUInt32(null);
						if (i > int.MaxValue)
							throw new IOException("Invalid length: " + i.ToString());

						Length = (int)i;
						break;

					default:
						throw new IOException("Invalid tuple length: " + TupleLength.ToString());
				}

				byte[] RawValue = this.NextUInt8Array(null, Length);
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

				Binary RecordData = new Binary(RawValue, this.Sniffable, false);
				TedsRecord Record = FieldType.Parse(RecordTypeId, RecordData, State);
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
