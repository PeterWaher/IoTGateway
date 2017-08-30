using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Security.DTLS;

namespace Waher.Networking.LWM2M.ContentFormats
{
	/// <summary>
	/// Class used to deserialize data from the TLV (Type-Length-Value) binary format.
	/// </summary>
	public class TlvReader
	{
		private MemoryStream ms;

		/// <summary>
		/// Class used to deserialize data from the TLV (Type-Length-Value) binary format.
		/// </summary>
		public TlvReader(byte[] Data)
		{
			this.ms = new MemoryStream(Data);
		}

		/// <summary>
		/// If the reader has reached the end of the file.
		/// </summary>
		public bool EOF
		{
			get { return this.ms.Position >= this.ms.Length; }
		}

		/// <summary>
		/// Reads a TLV record from the stream.
		/// </summary>
		/// <returns></returns>
		public TlvRecord ReadRecord()
		{
			byte b = (byte)this.ms.ReadByte();
			IdentifierType Type = (IdentifierType)(b & 0b11000000);
			bool Id16 = (b & 0b00100000) != 0;
			byte LenMode = (byte)((b >> 3) & 3);
			int Len;
			ushort Id;

			if (Id16)
			{
				Id = (byte)this.ms.ReadByte();
				Id <<= 8;
				Id |= (byte)this.ms.ReadByte();
			}
			else
				Id = (byte)this.ms.ReadByte();

			if (LenMode == 0)
				Len = b & 7;
			else
			{
				Len = this.ms.ReadByte();

				if (LenMode >= 2)
				{
					Len <<= 8;
					Len |= this.ms.ReadByte();

					if (LenMode == 3)
					{
						Len <<= 8;
						Len |= this.ms.ReadByte();
					}
				}
			}

			byte[] Raw = new byte[Len];

			this.ms.Read(Raw, 0, Len);

			return new TlvRecord(Type, Id, Raw);
		}

	}
}
