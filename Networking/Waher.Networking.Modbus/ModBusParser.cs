using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Networking.Modbus
{
	/// <summary>
	/// Processes incoming ModBus data from a client.
	/// </summary>
	public class ModBusParser
	{
		private readonly ModBusTcpServer server;
		private byte[] data;
		private byte state;
		private ushort transactionId;
		private ushort protocolId;
		private ushort length;
		private ushort bytesLeft;
		private ushort dataPos;
		private byte unitAddress;
		private byte functionCode;

		/// <summary>
		/// Processes incoming ModBus data from a client.
		/// </summary>
		/// <param name="Server">Server reference.</param>
		public ModBusParser(ModBusTcpServer Server)
		{
			this.server = Server;
			this.state = 0;
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		/// <returns>If data was received successfully.</returns>
		public async Task<bool> DataReceived(ServerConnectionDataEventArgs e)
		{
			byte[] Buffer = e.Buffer;
			int Count = e.Count;
			int Offset = e.Offset;
			byte b;

			while (Count > 0)
			{
				b = Buffer[Offset++];

				switch (this.state)
				{
					case 0:                         // Transaction ID (MSB)
						this.transactionId = b;
						this.state++;
						break;

					case 1:                         // Transaction ID (LSB)
						this.transactionId <<= 8;
						this.transactionId |= b;
						this.state++;
						break;

					case 2:                         // Protocol ID (MSB)
						this.protocolId = b;
						this.state++;
						break;

					case 3:                         // Protocol ID (LSB)
						this.protocolId <<= 8;
						this.protocolId |= b;
						this.state++;
						break;

					case 4:                         // Length (MSB)
						this.length = b;
						this.state++;
						break;

					case 5:                         // Length (LSB)
						this.length <<= 8;
						this.length |= b;
						this.bytesLeft = (ushort)(this.length - 2);
						this.dataPos = 0;
						this.data = new byte[this.bytesLeft];
						this.state++;
						break;

					case 6:                         // Unit Address
						this.unitAddress = b;
						this.state++;
						break;

					case 7:                         // Function Code
						this.functionCode = b;
						this.state++;

						if (this.bytesLeft == 0)
						{
							this.state = 0;

							if (!await this.FrameReceived(e))
								return false;
						}
						break;

					case 8:                         // Data
						this.data[this.dataPos++] = b;
						this.bytesLeft--;

						if (this.bytesLeft == 0)
						{
							this.state = 0;

							if (!await this.FrameReceived(e))
								return false;
						}
						break;
				}
			}

			return true;
		}

		private async Task<bool> FrameReceived(ServerConnectionDataEventArgs e)
		{
			try
			{
				if (this.protocolId != 0)
				{
					e.Server.Error("Invalid protocol ID received.");
					return false;
				}

				switch (this.functionCode)
				{
					case 0x01:      // Read Coils
						if (this.data.Length != 4)
						{
							e.Server.Error("Expected four bytes of data.");
							return false;
						}

						ushort ReferenceNr = this.data[0];
						ReferenceNr <<= 8;
						ReferenceNr |= this.data[1];

						ushort NrBits = this.data[2];
						NrBits <<= 8;
						NrBits |= this.data[3];

						BitArray Bits = new BitArray(NrBits);

						ReadBitsEventArgs BitsEventArgs = new ReadBitsEventArgs(
							this.unitAddress, ReferenceNr, NrBits, Bits);

						await this.server.RaiseReadCoils(BitsEventArgs);

						byte[] Bytes = ToArray(Bits);
						int c = Bytes.Length;
						byte[] Data = new byte[c + 1];
						Data[0] = (byte)c;
						Array.Copy(Bytes, 0, Data, 1, c);

						return await this.SendResponse(e, false, Data);

					case 0x02:      // Read Input Discretes
						if (this.data.Length != 4)
						{
							e.Server.Error("Expected four bytes of data.");
							return false;
						}

						ReferenceNr = this.data[0];
						ReferenceNr <<= 8;
						ReferenceNr |= this.data[1];

						NrBits = this.data[2];
						NrBits <<= 8;
						NrBits |= this.data[3];

						Bits = new BitArray(NrBits);

						BitsEventArgs = new ReadBitsEventArgs(this.unitAddress,
							ReferenceNr, NrBits, Bits);

						await this.server.RaiseReadInputDiscretes(BitsEventArgs);

						Bytes = ToArray(Bits);
						c = Bytes.Length;
						Data = new byte[c + 1];
						Data[0] = (byte)c;
						Array.Copy(Bytes, 0, Data, 1, c);

						return await this.SendResponse(e, false, Data);

					case 0x03:      // Read Multiple Registers
					case 0x04:      // Read Input Registers
					case 0x05:      // Write Coil
					case 0x06:      // Write Register
					case 0x10:      // Write Multiple Registers
					default:
						e.Server.Error("Unsupported function code received.");
						return await this.SendResponse(e, true, 0x01);
				}
			}
			catch (Exception ex)
			{
				// TODO: Return error message.

				e.Server.Error(ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Converts a bit-array to a byte array.
		/// </summary>
		/// <param name="Bits"></param>
		/// <returns></returns>
		public static byte[] ToArray(BitArray Bits)
		{
			int c = Bits.Length;
			int d = (c + 7) / 8;
			byte[] Result = new byte[d];
			int i, j;
			byte b, o;

			for (i = j = b = 0, o = 1; i < c; i++)
			{
				if (Bits[i])
					b |= o;

				o <<= 1;
				if (o == 0)
				{
					Result[j++] = b;
					o = 1;
					b = 0;
				}
			}

			if (j < d)
				Result[j] = b;

			return Result;
		}

		private Task<bool> SendResponse(ServerConnectionDataEventArgs e, bool Error, params byte[] Data)
		{
			MemoryStream Frame = new MemoryStream();

			Frame.WriteByte((byte)(this.transactionId >> 8));
			Frame.WriteByte((byte)this.transactionId);
			Frame.WriteByte(0);    // Protocol Identitfier (Hi)
			Frame.WriteByte(0);    // Protocol Identitfier (Lo)

			int Len = Data?.Length ?? 0;
			ushort Length = (ushort)(Len + 2);

			Frame.WriteByte((byte)(Length >> 8));
			Frame.WriteByte((byte)Length);
			Frame.WriteByte(this.unitAddress);
			Frame.WriteByte(Error ? (byte)(this.functionCode | 0x80) : this.functionCode);

			if (Len > 0)
				Frame.Write(Data, 0, Len);

			return e.SendAsync(Frame.ToArray());
		}

	}
}
