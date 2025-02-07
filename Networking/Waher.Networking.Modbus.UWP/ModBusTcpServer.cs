using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking.Modbus
{
    /// <summary>
    /// Modbus over TCP server
    /// 
    /// References:
    /// https://waher.se/Downloads/modbus_tcp_specification.pdf
    /// https://modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf
    /// </summary>
    public class ModBusTcpServer : IDisposable, ICommunicationLayer
	{
		private readonly Dictionary<Guid, ModBusParser> parsers = new Dictionary<Guid, ModBusParser>();
		private BinaryTcpServer server;

		/// <summary>
		/// Modbus over TCP server
		/// </summary>
		/// <param name="Port">Port number to open</param>
		/// <param name="Sniffers">Optional set of sniffers.</param>
		private ModBusTcpServer(int Port, params ISniffer[] Sniffers)
		{
			this.server = new BinaryTcpServer(false, Port, TimeSpan.FromSeconds(30), false, Sniffers);
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Modbus over TCP server
		/// </summary>
		/// <param name="Port">Port number to open</param>
		/// <param name="ServerCertificate">Server certificate, for encryption.</param>
		/// <param name="Sniffers">Optional set of sniffers.</param>
		private ModBusTcpServer(int Port, X509Certificate ServerCertificate, params ISniffer[] Sniffers)
		{
			this.server = new BinaryTcpServer(false, Port, TimeSpan.FromSeconds(30), ServerCertificate, false, Sniffers);
		}
#endif
		/// <summary>
		/// Creates and opens a ModBus server.
		/// </summary>
		/// <param name="Port">Port to listen on.</param>
		/// <param name="Sniffers">Optional set of sniffers.</param>
		/// <returns>Created ModBus server object.</returns>
		public static async Task<ModBusTcpServer> CreateAsync(int Port, params ISniffer[] Sniffers)
		{
			ModBusTcpServer Result = new ModBusTcpServer(Port, Sniffers);
			await Result.Init();
			return Result;
		}

#if !WINDOWS_UWP
		/// <summary>
		/// Creates and opens a ModBus server.
		/// </summary>
		/// <param name="Port">Port to listen on.</param>
		/// <param name="ServerCertificate">Server certificate, for encryption.</param>
		/// <param name="Sniffers">Optional set of sniffers.</param>
		/// <returns>Created ModBus server object.</returns>
		public static async Task<ModBusTcpServer> CreateAsync(int Port, X509Certificate ServerCertificate, params ISniffer[] Sniffers)
		{
			ModBusTcpServer Result = new ModBusTcpServer(Port, ServerCertificate, Sniffers);
			await Result.Init();
			return Result;
		}
#endif
		private async Task Init()
		{
			this.server.OnClientConnected += this.Server_OnClientConnected;
			this.server.OnClientDisconnected += this.Server_OnClientDisconnected;
			this.server.OnDataReceived += this.Server_OnDataReceived;

			await this.server.Open();
		}

		#region IDisposable

		/// <summary>
		/// Closes and disposes of the server.
		/// </summary>
		public void Dispose()
		{
			this.server?.Dispose();
			this.server = null;
		}

		#endregion

		#region ICommunicationLayer

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => this.server?.DecoupledEvents ?? false;

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer to add.</param>
		public void Add(ISniffer Sniffer)
		{
			this.server?.Add(Sniffer);
		}

		/// <summary>
		/// Adds a range of sniffers to the node.
		/// </summary>
		/// <param name="Sniffers">Sniffers to add.</param>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			this.server?.AddRange(Sniffers);
		}

		/// <summary>
		/// Removes a sniffer, if registered.
		/// </summary>
		/// <param name="Sniffer">Sniffer to remove.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public bool Remove(ISniffer Sniffer)
		{
			return this.server?.Remove(Sniffer) ?? false;
		}

		/// <summary>
		/// Gets an enumerator of registered sniffers.
		/// </summary>
		/// <returns>Enumerable set of sniffers.</returns>
		public IEnumerator<ISniffer> GetEnumerator()
		{
			return this.server?.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of registered sniffers.
		/// </summary>
		/// <returns>Enumerable set of sniffers.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.server?.GetEnumerator();
		}

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers => this.server?.Sniffers;

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers => this.server?.HasSniffers ?? false;

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count) => this.server?.ReceiveBinary(Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data) => this.server?.ReceiveBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.server?.ReceiveBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count) => this.server?.TransmitBinary(Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data) => this.server?.TransmitBinary(ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.server?.TransmitBinary(ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text) => this.server?.ReceiveText(Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text) => this.server?.TransmitText(Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment) => this.server?.Information(Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning) => this.server?.Warning(Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error) => this.server?.Error(Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception) => this.server?.Exception(Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception) => this.server?.Exception(Exception);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count) => this.server?.ReceiveBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.server?.ReceiveBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.server?.ReceiveBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count) => this.server?.TransmitBinary(Timestamp, Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data) => this.server?.TransmitBinary(Timestamp, ConstantBuffer, Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count) => this.server?.TransmitBinary(Timestamp, ConstantBuffer, Data, Offset, Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(DateTime Timestamp, string Text) => this.server?.ReceiveText(Timestamp, Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(DateTime Timestamp, string Text) => this.server?.TransmitText(Timestamp, Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(DateTime Timestamp, string Comment) => this.server?.Information(Timestamp, Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(DateTime Timestamp, string Warning) => this.server?.Warning(Timestamp, Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(DateTime Timestamp, string Error) => this.server?.Error(Timestamp, Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, string Exception) => this.server?.Exception(Timestamp, Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, Exception Exception) => this.server?.Exception(Timestamp, Exception);

		#endregion

		#region Binary communication

		private Task Server_OnClientConnected(object Sender, ServerConnectionEventArgs e)
		{
			lock (this.parsers)
			{
				this.parsers[e.Id] = new ModBusParser(this);
			}

			return Task.CompletedTask;
		}

		private Task Server_OnClientDisconnected(object Sender, ServerConnectionEventArgs e)
		{
			lock (this.parsers)
			{
				this.parsers.Remove(e.Id);
			}

			return Task.CompletedTask;
		}

		private async Task Server_OnDataReceived(object Sender, ServerConnectionDataEventArgs e)
		{
			ModBusParser Parser;

			lock (this.parsers)
			{
				if (!this.parsers.TryGetValue(e.Id, out Parser))
					return;
			}

			if (!await Parser.DataReceived(e))
				e.CloseConnection();
		}

		#endregion

		#region events

		internal async Task RaiseReadCoils(ReadBitsEventArgs e)
		{
			if (this.HasSniffers)
			{
				this.Information("ReadCoils(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrBits.ToString() + ")");
			}

			await this.OnReadCoils.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read coils.
		/// </summary>
		public event EventHandlerAsync<ReadBitsEventArgs> OnReadCoils;

		internal async Task RaiseReadInputDiscretes(ReadBitsEventArgs e)
		{
			if (this.HasSniffers)
			{
				this.Information("ReadInputDiscretes(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrBits.ToString() + ")");
			}

			await this.OnReadInputDiscretes.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read input descrete registers.
		/// </summary>
		public event EventHandlerAsync<ReadBitsEventArgs> OnReadInputDiscretes;

		internal async Task RaiseReadMultipleRegisters(ReadWordsEventArgs e)
		{
			if (this.HasSniffers)
			{
				this.Information("ReadMultipleRegisters(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrWords.ToString() + ")");
			}

			await this.OnReadMultipleRegisters.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read multiple registers.
		/// </summary>
		public event EventHandlerAsync<ReadWordsEventArgs> OnReadMultipleRegisters;

		internal async Task RaiseReadInputRegisters(ReadWordsEventArgs e)
		{
			if (this.HasSniffers)
			{
				this.Information("ReadInputRegisters(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrWords.ToString() + ")");
			}

			await this.OnReadInputRegisters.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read input registers.
		/// </summary>
		public event EventHandlerAsync<ReadWordsEventArgs> OnReadInputRegisters;

		internal async Task RaiseWriteCoil(WriteBitEventArgs e)
		{
			if (this.HasSniffers)
			{
				this.Information("WriteCoil(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.Value.ToString() + ")");
			}

			await this.OnWriteCoil.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to write a coil output value.
		/// </summary>
		public event EventHandlerAsync<WriteBitEventArgs> OnWriteCoil;

		internal async Task RaiseWriteRegister(WriteWordEventArgs e)
		{
			if (this.HasSniffers)
			{
				this.Information("WriteRegister(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.Value.ToString() + ")");
			}

			await this.OnWriteRegister.Raise(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to write a register output value.
		/// </summary>
		public event EventHandlerAsync<WriteWordEventArgs> OnWriteRegister;

		#endregion

	}
}
