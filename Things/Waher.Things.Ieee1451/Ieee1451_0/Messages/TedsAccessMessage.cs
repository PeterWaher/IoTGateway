using System;
using System.Collections.Generic;
using System.IO;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 TEDS Access Message
	/// </summary>
	public class TedsAccessMessage : Ieee1451_0Message
	{
		private static readonly Dictionary<uint, IFieldType> fieldTypes = new Dictionary<uint, IFieldType>();

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
		/// Name of <see cref="Ieee1451_0Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.TedsAccessService.ToString();

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(out Ieee1451_0Teds Teds)
		{
			return this.TryParseTeds(true, out Teds);
		}

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="CheckChecksum">If checksum should be checked.</param>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(bool CheckChecksum, out Ieee1451_0Teds Teds)
		{
			Teds = null;

			try
			{
				Ieee1451_0ChannelId ChannelInfo = this.NextChannelId();
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

				Ieee1451_0Binary TedsBlock = new Ieee1451_0Binary(Data);
				List<TedsRecord> Records = new List<TedsRecord>();
				byte TupleLength = 1;

				while (!TedsBlock.EOF)
				{
					byte Type = TedsBlock.NextUInt8();
					int Length;

					switch (TupleLength)
					{
						case 0:
							Length = 0;
							break;

						case 1:
							Length = TedsBlock.NextUInt8();
							break;

						case 2:
							Length = TedsBlock.NextUInt16();
							break;

						case 3:
							Length = (int)TedsBlock.NextUInt24();
							break;

						case 4:
							uint i = TedsBlock.NextUInt32();
							if (i > int.MaxValue)
								throw new IOException("Invalid length: " + i.ToString());

							Length = (int)i;
							break;

						default:
							throw new IOException("Invalid tuple length: " + TupleLength.ToString());
					}

					byte[] RawValue = TedsBlock.NextUInt8Array(Length);
					IFieldType FieldType;

					lock (fieldTypes)
					{
						if (!fieldTypes.TryGetValue(Type, out FieldType))
							FieldType = null;
					}

					if (FieldType is null)
					{
						FieldType = Types.FindBest<IFieldType, byte>(Type);
						if (FieldType is null)
							FieldType = new TedsRecord();

						lock (fieldTypes)
						{
							fieldTypes[Type] = FieldType;
						}
					}

					TedsRecord Record = FieldType.Parse(Type, new Ieee1451_0Binary(RawValue));
					if (Record is TedsId TedsId)
						TupleLength = TedsId.TupleLength;

					Records.Add(Record);
				}

				Teds = new Ieee1451_0Teds(Records.ToArray());

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
