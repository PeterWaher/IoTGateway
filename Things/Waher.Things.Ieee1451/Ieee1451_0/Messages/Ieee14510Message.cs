using System;
using System.Collections.Generic;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Message
	/// </summary>
	public abstract class Ieee14510Message : Ieee14510Binary
	{
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
			: base(Body)
		{
			this.NetworkServiceType = NetworkServiceType;
			this.NetworkServiceId = NetworkServiceId;
			this.MessageType = MessageType;
			this.Tail = Tail;
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
		/// Bytes that are received after the body.
		/// </summary>
		public byte[] Tail { get; }

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(out Ieee14510Teds Teds)
		{
			return this.TryParseTeds(true, out Teds);
		}

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="CheckChecksum">If checksum should be checked.</param>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(bool CheckChecksum, out Ieee14510Teds Teds)
		{
			Teds = null;

			try
			{
				int Start = this.Position;
				uint Len = this.NextUInt32();
				if (Len < 2)
					return false;

				Len -= 2;
				if (Len > int.MaxValue)
					return false;

				byte[] Data = this.NextUInt8Array((int)Len);
				ushort CheckSum = 0;

				while (Start < this.Position)
					CheckSum += this.Body[Start++];

				CheckSum ^= 0xffff;

				ushort CheckSum2 = this.NextUInt16();
				if (CheckChecksum && CheckSum != CheckSum2)
					return false;

				Ieee14510Binary TedsBlock = new Ieee14510Binary(Data);
				List<TedsRecord> Records = new List<TedsRecord>();

				while (!TedsBlock.EOF)
				{
					byte Type = TedsBlock.NextUInt8();
					byte Length = TedsBlock.NextUInt8();    // Number of bytes may vary.
					byte[] Value = TedsBlock.NextUInt8Array(Length);

					Records.Add(new TedsRecord(Type, Value));
				}

				Teds = new Ieee14510Teds(Records.ToArray());

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
