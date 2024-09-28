using System;
using System.IO;
using System.Text;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Message
	/// </summary>
	public abstract class Ieee14510Message
	{
		private readonly int len;
		private int pos = 0;

		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="NetworkServiceId">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public Ieee14510Message(NetworkServiceType NetworkServiceType, byte NetworkServiceId,
			MessageType MessageType, byte[] Body, byte[] Tail)
		{
			this.NetworkServiceType = NetworkServiceType;
			this.NetworkServiceId = NetworkServiceId;
			this.MessageType = MessageType;
			this.Body = Body;
			this.Tail = Tail;

			this.len = this.Body.Length;
		}

		/// <summary>
		/// Network Service Type
		/// </summary>
		public NetworkServiceType NetworkServiceType { get; }

		/// <summary>
		/// Network Service ID
		/// </summary>
		public byte NetworkServiceId { get; }

		/// <summary>
		/// Name of <see cref="NetworkServiceId"/>
		/// </summary>
		public abstract string NetworkServiceIdName { get; }

		/// <summary>
		/// Message Type
		/// </summary>
		public MessageType MessageType { get; }

		/// <summary>
		/// Message Body
		/// </summary>
		public byte[] Body { get; }

		/// <summary>
		/// Bytes that are received after the body.
		/// </summary>
		public byte[] Tail { get; }

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
		public bool NextBooean()
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
	}
}
