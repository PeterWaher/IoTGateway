using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.Modbus.Exceptions;
using Waher.Networking.Sniffers;
using Waher.Runtime.Threading;
using Waher.Security;

namespace Waher.Networking.Modbus
{
    /// <summary>
    /// Modbus over TCP client
    /// 
    /// References:
    /// https://waher.se/Downloads/modbus_tcp_specification.pdf
    /// https://modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf
    /// </summary>
    public class ModbusTcpClient : CommunicationLayer, IDisposable
	{
		/// <summary>
		/// Default Modbus port (502)
		/// </summary>
		public const int DefaultPort = 502;

		private readonly Dictionary<ushort, Transaction> transactions = new Dictionary<ushort, Transaction>();
		private readonly BinaryTcpClient tcpClient;
		private readonly MultiReadSingleWriteObject synchObject;
		private ushort transactionId = 0;
		private int timeoutMs = 10000;
		private bool connected = false;
		private int state = 0;


		/// <summary>
		/// Modbus over TCP client
		/// </summary>
		/// <param name="Sniffers">Sniffers</param>
		private ModbusTcpClient(params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.synchObject = new MultiReadSingleWriteObject(this);
			this.tcpClient = new BinaryTcpClient(false);
		}

		/// <summary>
		/// Connects to an Modbus TCP/IP Gateway
		/// </summary>
		/// <param name="Host">Host name or IP Address of gateway.</param>
		/// <param name="Port">Port number to connect to.</param>
		/// <param name="Sniffers">Sniffers</param>
		/// <returns>Connection objcet</returns>
		/// <exception cref="Exception">If conncetion could not be established.</exception>
		public static async Task<ModbusTcpClient> Connect(string Host, int Port, params ISniffer[] Sniffers)
		{
#if !WINDOWS_UWP
			return await Connect(Host, Port, false, Sniffers);
		}

		/// <summary>
		/// Connects to an Modbus TCP/IP Gateway
		/// </summary>
		/// <param name="Host">Host name or IP Address of gateway.</param>
		/// <param name="Port">Port number to connect to.</param>
		/// <param name="Tls">If connction is protected by TLS.</param>
		/// <param name="Sniffers">Sniffers</param>
		/// <returns>Connection objcet</returns>
		/// <exception cref="Exception">If conncetion could not be established.</exception>
		public static async Task<ModbusTcpClient> Connect(string Host, int Port, bool Tls, params ISniffer[] Sniffers)
		{
#endif
			ModbusTcpClient Result = new ModbusTcpClient(Sniffers);

			Result.tcpClient.OnDisconnected += Result.TcpClient_OnDisconnected;
			Result.tcpClient.OnError += Result.TcpClient_OnError;
			Result.tcpClient.OnInformation += Result.TcpClient_OnInformation;
			Result.tcpClient.OnWarning += Result.TcpClient_OnWarning;
			Result.tcpClient.OnReceived += Result.TcpClient_OnReceived;

			if (!await Result.tcpClient.ConnectAsync(Host, Port))
				throw new IOException("Unable to connect to " + Host + ":" + Port);

#if !WINDOWS_UWP
			if (Tls)
			{
				try
				{
					await Result.tcpClient.UpgradeToTlsAsClient(Crypto.SecureTls);
				}
				catch (Exception ex)
				{
					Result.Dispose();
					ExceptionDispatchInfo.Capture(ex).Throw();
				}
			}
#endif

			Result.connected = true;

			return Result;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.tcpClient?.DisposeWhenDone();
			this.synchObject?.Dispose();

			Transaction[] ToClose;

			lock (this.transactions)
			{
				if (this.transactions.Count == 0)
					ToClose = null;
				else
				{
					ToClose = new Transaction[this.transactions.Count];
					this.transactions.Values.CopyTo(ToClose, 0);
					this.transactions.Clear();
				}
			}

			if (!(ToClose is null))
			{
				foreach (Transaction T in ToClose)
					T.Handle.TrySetException(new IOException("Connection closed by owner."));
			}
		}

		/// <summary>
		/// If the client is currently connected to the Modbus gateway.
		/// </summary>
		public bool Connected => this.connected;

		/// <summary>
		/// Timeout, in milliseconds.
		/// </summary>
		public int TimeoutMs
		{
			get => this.timeoutMs;
			set
			{
				if (value <= 0)
					throw new ArgumentException("Timeout must be positive.", nameof(this.TimeoutMs));

				this.timeoutMs = value;
			}
		}

		private Task TcpClient_OnDisconnected(object Sender, EventArgs e)
		{
			this.connected = false;
			return Task.CompletedTask;
		}

		private Task TcpClient_OnError(object Sender, Exception Exception)
		{
			this.Error(Exception.Message);
			return Task.CompletedTask;
		}

		private string TcpClient_OnWarning(string Text)
		{
			this.Warning(Text);
			return Text;
		}

		private string TcpClient_OnInformation(string Text)
		{
			this.Information(Text);
			return Text;
		}

		private Task<bool> TcpClient_OnReceived(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			if (this.HasSniffers)
				this.ReceiveBinary(ConstantBuffer, Buffer, Offset, Count);

			int i;
			byte b;

			for (i = 0; i < Count; i++)
			{
				b = Buffer[Offset++];

				switch (this.state)
				{
					case 0: // Transaction ID (Hi)
						this.rxTransactionId = b;
						this.state++;
						break;

					case 1: // Transaction ID (Lo)
						this.rxTransactionId <<= 8;
						this.rxTransactionId |= b;
						this.state++;
						break;

					case 2: // Protocol (Hi)
						this.rxProtocol = b;
						this.state++;
						break;

					case 3: // Protocol (Lo)
						this.rxProtocol <<= 8;
						this.rxProtocol |= b;
						this.state++;
						break;

					case 4: // Length (Hi)
						this.rxLen = b;
						this.state++;
						break;

					case 5: // Length (Lo)
						this.rxLen <<= 8;
						this.rxLen |= b;
						this.state++;
						break;

					case 6: // Unit Address
						this.rxUnitAddress = b;
						this.state++;
						this.rxLen--;
						if (this.rxLen == 0)
							this.ProcessIncomingPacket();
						break;

					case 7: // Expecting function code
						this.rxFunctionCode = b;
						this.rxLen--;
						this.state++;
						if (this.rxLen == 0)
							this.ProcessIncomingPacket();
						break;

					case 8: // Data
						this.rx.Add(b);
						this.rxLen--;
						if (this.rxLen == 0)
							this.ProcessIncomingPacket();
						break;
				}
			}

			return Task.FromResult(true);
		}

		private void ProcessIncomingPacket()
		{
			ModbusResponse Response = new ModbusResponse()
			{
				TransactionId = this.rxTransactionId,
				Protocol = this.rxProtocol,
				UnitAddress = this.rxUnitAddress,
				FunctionCode = this.rxFunctionCode,
				Data = this.rx.ToArray()
			};

			this.state = 0;
			this.rx.Clear();

			Transaction Request;

			lock (this.transactions)
			{
				if (!this.transactions.TryGetValue(Response.TransactionId, out Request))
					return;

				this.transactions.Remove(Response.TransactionId);
			}

			Request.Handle.TrySetResult(Response);
		}

		private ushort rxTransactionId = 0;
		private ushort rxProtocol = 0;
		private ushort rxLen = 0;
		private byte rxUnitAddress = 0;
		private byte rxFunctionCode = 0;
		private Transaction lastTransaction = null;
		private readonly List<byte> rx = new List<byte>();

		private Transaction PrepareRequest(byte UnitAddress, byte FunctionCode, params byte[] Data)
		{
			Transaction Result;

			lock (this.transactions)
			{
				Result = new Transaction()
				{
					TransactionId = this.transactionId
				};

				this.transactions[this.transactionId++] = Result;
			}

			Result.Request.WriteByte((byte)(Result.TransactionId >> 8));
			Result.Request.WriteByte((byte)Result.TransactionId);
			Result.Request.WriteByte(0);    // Protocol Identitfier (Hi)
			Result.Request.WriteByte(0);    // Protocol Identitfier (Lo)

			int Len = Data?.Length ?? 0;
			ushort Length = (ushort)(Len + 2);

			Result.Request.WriteByte((byte)(Length >> 8));
			Result.Request.WriteByte((byte)Length);
			Result.Request.WriteByte(UnitAddress);
			Result.Request.WriteByte(FunctionCode);

			if (Len > 0)
				Result.Request.Write(Data, 0, Len);

			this.lastTransaction = Result;

			return Result;
		}

		private class Transaction
		{
			public ushort TransactionId;
			public TaskCompletionSource<ModbusResponse> Handle = new TaskCompletionSource<ModbusResponse>();
			public MemoryStream Request = new MemoryStream();
			public MemoryStream Response = new MemoryStream();
		}

		private class ModbusResponse
		{
			public ushort TransactionId;
			public ushort Protocol;
			public byte UnitAddress;
			public byte FunctionCode;
			public byte[] Data;
		}

		private async Task<ModbusResponse> Request(byte UnitAddress, byte FunctionCode, params byte[] Data)
		{
			Transaction Req = this.PrepareRequest(UnitAddress, FunctionCode, Data);

			try
			{
				byte[] Bin = Req.Request.ToArray();

				await this.tcpClient.SendAsync(true, Bin);
				this.TransmitBinary(true, Bin);

				Task<ModbusResponse> Result = Req.Handle.Task;
				Task Timeout = Task.Delay(this.timeoutMs);

				Task T = await Task.WhenAny(Result, Timeout);

				if (Result.IsCompleted)
					return Result.Result;
				else
					throw new TimeoutException("No response returned in time.");
			}
			finally
			{
				lock (this.transactions)
				{
					this.transactions.Remove(Req.TransactionId);
				}

				if (this.lastTransaction == Req)
					this.lastTransaction = null;
			}
		}

		private Exception GetException(ModbusResponse Response)
		{
			if ((Response.FunctionCode & 0x80) != 0 && Response.Data.Length > 0)
			{
				switch (Response.Data[0])
				{
					case 0x01: return new IllegalFunctionException(Response.FunctionCode, Response.Data);
					case 0x02: return new IllegalDataAddressException(Response.FunctionCode, Response.Data);
					case 0x03: return new IllegalDataValueException(Response.FunctionCode, Response.Data);
					case 0x04: return new IllegalResponseLengthException(Response.FunctionCode, Response.Data);
					case 0x05: return new AcknowledgeException(Response.FunctionCode, Response.Data);
					case 0x06: return new SlaveDeviceBusyException(Response.FunctionCode, Response.Data);
					case 0x07: return new NegativeAcknowledgeException(Response.FunctionCode, Response.Data);
					case 0x08: return new MemoryParityException(Response.FunctionCode, Response.Data);
					case 0x0a: return new GatewayPathUnavailableException(Response.FunctionCode, Response.Data);
					case 0x0b: return new GatewayTargetDeviceFailedToRespondException(Response.FunctionCode, Response.Data);
				}
			}

			StringBuilder sb = new StringBuilder();

			sb.Append("Modbus Exception: ");
			sb.Append(Response.FunctionCode.ToString("X2"));

			foreach (byte b in Response.Data)
			{
				sb.Append(' ');
				sb.Append(b.ToString("X2"));
			}

			return new IOException(sb.ToString());
		}

		/// <summary>
		/// Reads multiple registers from a Modbus unit.
		/// </summary>
		/// <param name="UnitAddress">Unit Address</param>
		/// <param name="ReferenceNumber">Reference Number</param>
		/// <param name="NrWords">Number of words.</param>
		/// <returns>Words read.</returns>
		public async Task<ushort[]> ReadMultipleRegisters(byte UnitAddress, ushort ReferenceNumber, ushort NrWords)
		{
			if (NrWords < 1 || NrWords > 125)
				throw new ArgumentOutOfRangeException(nameof(NrWords), "1 <= NrWords <= 125");

			ModbusResponse Response = await this.Request(UnitAddress, 0x03,
				(byte)(ReferenceNumber >> 8), (byte)ReferenceNumber,
				(byte)(NrWords >> 8), (byte)NrWords);

			if (Response.FunctionCode != 0x03)
				throw this.GetException(Response);

			int i = 0;
			int c = Response.Data.Length;

			if (c == 0)
				throw UnexpectedEndOfResponse();

			byte ByteCount = Response.Data[i++];

			if (i + ByteCount > c)
				throw UnexpectedEndOfResponse();

			int j = 0;
			int d = ByteCount / 2;
			ushort[] Words = new ushort[d];
			ushort w;

			while (j < d)
			{
				w = Response.Data[i++];
				w <<= 8;
				w |= Response.Data[i++];

				Words[j++] = w;
			}

			return Words;
		}

		private static Exception UnexpectedEndOfResponse()
		{
			return new IOException("Unexpected end of response.");
		}

		/// <summary>
		/// Writes multiple registers to a Modbus unit.
		/// </summary>
		/// <param name="UnitAddress">Unit Address</param>
		/// <param name="ReferenceNumber">Reference Number</param>
		/// <param name="Words">Words to write.</param>
		public async Task WriteMultipleRegisters(byte UnitAddress, ushort ReferenceNumber, params ushort[] Words)
		{
			int WordCount = Words.Length;
			if (WordCount <= 0 || WordCount > 100)
				throw new ArgumentOutOfRangeException(nameof(Words), "Can only write 1-100 words at a time.");

			MemoryStream Data = new MemoryStream();
			int i;

			Data.WriteByte((byte)(ReferenceNumber >> 8));
			Data.WriteByte((byte)ReferenceNumber);
			Data.WriteByte((byte)(WordCount >> 8));
			Data.WriteByte((byte)WordCount);
			Data.WriteByte((byte)(WordCount << 1));

			for (i = 0; i < WordCount; i++)
			{
				Data.WriteByte((byte)(Words[i] >> 8));
				Data.WriteByte((byte)Words[i]);
			}

			ModbusResponse Response = await this.Request(UnitAddress, 0x10, Data.ToArray());

			if (Response.FunctionCode != 0x10)
				throw this.GetException(Response);
		}

		/// <summary>
		/// Reads coils from a Modbus unit.
		/// </summary>
		/// <param name="UnitAddress">Unit Address</param>
		/// <param name="ReferenceNumber">Reference Number</param>
		/// <param name="NrBits">Number of bits.</param>
		/// <returns>Coils read.</returns>
		public async Task<BitArray> ReadCoils(byte UnitAddress, ushort ReferenceNumber, ushort NrBits)
		{
			if (NrBits < 1 || NrBits > 2000)
				throw new ArgumentOutOfRangeException(nameof(NrBits), "1 <= NrBits <= 2000");

			ModbusResponse Response = await this.Request(UnitAddress, 0x01,
				(byte)(ReferenceNumber >> 8), (byte)ReferenceNumber,
				(byte)(NrBits >> 8), (byte)NrBits);

			if (Response.FunctionCode != 0x01)
				throw this.GetException(Response);

			int i = 0;
			int c = Response.Data.Length;

			if (c == 0)
				throw UnexpectedEndOfResponse();

			byte ByteCount = Response.Data[i++];

			if (i + ByteCount > c)
				throw UnexpectedEndOfResponse();

			byte[] Bytes = new byte[ByteCount];

			Buffer.BlockCopy(Response.Data, i, Bytes, 0, ByteCount);

			return new BitArray(Bytes);
		}

		/// <summary>
		/// Reads input discretes from a Modbus unit.
		/// </summary>
		/// <param name="UnitAddress">Unit Address</param>
		/// <param name="ReferenceNumber">Reference Number</param>
		/// <param name="NrBits">Number of bits.</param>
		/// <returns>Input discretes read.</returns>
		public async Task<BitArray> ReadInputDiscretes(byte UnitAddress, ushort ReferenceNumber, ushort NrBits)
		{
			if (NrBits < 1 || NrBits > 2000)
				throw new ArgumentOutOfRangeException(nameof(NrBits), "1 <= NrBits <= 2000");

			ModbusResponse Response = await this.Request(UnitAddress, 0x02,
				(byte)(ReferenceNumber >> 8), (byte)ReferenceNumber,
				(byte)(NrBits >> 8), (byte)NrBits);

			if (Response.FunctionCode != 0x02)
				throw this.GetException(Response);

			int i = 0;
			int c = Response.Data.Length;

			if (c == 0)
				throw UnexpectedEndOfResponse();

			byte ByteCount = Response.Data[i++];

			if (i + ByteCount > c)
				throw UnexpectedEndOfResponse();

			byte[] Bytes = new byte[ByteCount];

			Buffer.BlockCopy(Response.Data, i, Bytes, 0, ByteCount);

			return new BitArray(Bytes);
		}

		/// <summary>
		/// Reads input registers from a Modbus unit.
		/// </summary>
		/// <param name="UnitAddress">Unit Address</param>
		/// <param name="ReferenceNumber">Reference Number</param>
		/// <param name="NrWords">Number of words.</param>
		/// <returns>Words read.</returns>
		public async Task<ushort[]> ReadInputRegisters(byte UnitAddress, ushort ReferenceNumber, ushort NrWords)
		{
			if (NrWords < 1 || NrWords > 125)
				throw new ArgumentOutOfRangeException(nameof(NrWords), "1 <= NrWords <= 125");

			ModbusResponse Response = await this.Request(UnitAddress, 0x04,
				(byte)(ReferenceNumber >> 8), (byte)ReferenceNumber,
				(byte)(NrWords >> 8), (byte)NrWords);

			if (Response.FunctionCode != 0x04)
				throw this.GetException(Response);

			int i = 0;
			int c = Response.Data.Length;

			if (c == 0)
				throw UnexpectedEndOfResponse();

			byte ByteCount = Response.Data[i++];

			if (i + ByteCount > c)
				throw UnexpectedEndOfResponse();

			int j = 0;
			int d = ByteCount / 2;
			ushort[] Words = new ushort[d];
			ushort w;

			while (j < d)
			{
				w = Response.Data[i++];
				w <<= 8;
				w |= Response.Data[i++];

				Words[j++] = w;
			}

			return Words;
		}

		/// <summary>
		/// Write to a single coil
		/// </summary>
		/// <param name="UnitAddress">Unit Address</param>
		/// <param name="ReferenceNumber">Reference Number</param>
		/// <param name="Value">Value to write.</param>
		/// <returns>Coil output.</returns>
		public async Task<bool> WriteCoil(byte UnitAddress, ushort ReferenceNumber, bool Value)
		{
			ModbusResponse Response = await this.Request(UnitAddress, 0x05,
				(byte)(ReferenceNumber >> 8), (byte)ReferenceNumber,
				(byte)(Value ? 0xff : 0x00), 0x00);

			if (Response.FunctionCode != 0x05)
				throw this.GetException(Response);

			int c = Response.Data.Length;

			if (c < 3)
				throw UnexpectedEndOfResponse();

			return Response.Data[2] != 0;
		}

		/// <summary>
		/// Write to a single register
		/// </summary>
		/// <param name="UnitAddress">Unit Address</param>
		/// <param name="ReferenceNumber">Reference Number</param>
		/// <param name="Value">Value to write.</param>
		/// <returns>Register value.</returns>
		public async Task<ushort> WriteRegister(byte UnitAddress, ushort ReferenceNumber, ushort Value)
		{
			ModbusResponse Response = await this.Request(UnitAddress, 0x06,
				(byte)(ReferenceNumber >> 8), (byte)ReferenceNumber,
				(byte)(Value >> 8), (byte)Value);

			if (Response.FunctionCode != 0x06)
				throw this.GetException(Response);

			int c = Response.Data.Length;

			if (c < 4)
				throw UnexpectedEndOfResponse();

			Value = Response.Data[2];
			Value <<= 8;
			Value |= Response.Data[3];

			return Value;
		}

		/// <summary>
		/// Enters unique access to the TCP client. Must be followed by exactly one <see cref="Leave"/> call.
		/// </summary>
		public Task Enter()
		{
			return this.synchObject.BeginWrite();
		}

		/// <summary>
		/// Leaves unique access to the TCP client. Must be called exactly one for each call to <see cref="Enter"/>.
		/// </summary>
		public Task Leave()
		{
			return this.synchObject.EndWrite();
		}

	}
}
