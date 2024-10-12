using System;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 TEDS Access Message
	/// </summary>
	public class TedsAccessMessage : Message
	{
		/// <summary>
		/// IEEE 1451.0 TEDS Access Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="TedsAccessService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public TedsAccessMessage(NetworkServiceType NetworkServiceType, TedsAccessService TedsAccessService,
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)TedsAccessService, MessageType, Body, Tail)
		{
			this.TedsAccessService = TedsAccessService;
		}

		/// <summary>
		/// TEDS Access Service
		/// </summary>
		public TedsAccessService TedsAccessService { get; }

		/// <summary>
		/// Name of <see cref="Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.TedsAccessService.ToString();

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="ErrorCode">Error code, if available.</param>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(out ushort ErrorCode, out Teds Teds)
		{
			return this.TryParseTeds(true, out ErrorCode, out Teds);
		}

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="CheckChecksum">If checksum should be checked.</param>
		/// <param name="ErrorCode">Error code, if available.</param>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(bool CheckChecksum, out ushort ErrorCode, out Teds Teds)
		{
			Teds = null;

			try
			{
				if (this.MessageType == MessageType.Reply)
					ErrorCode = this.NextUInt16();
				else
					ErrorCode = 0;

				ChannelAddress ChannelInfo = this.NextChannelId();
				uint TedsOffset = this.NextUInt32();
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

				Binary TedsBlock = new Binary(Data);
				ParsingState State = new ParsingState();

				Teds = new Teds(ChannelInfo, TedsBlock.ParseTedsRecords(State));

				return true;
			}
			catch (Exception)
			{
				ErrorCode = 0xffff;
				return false;
			}
		}
	}
}
