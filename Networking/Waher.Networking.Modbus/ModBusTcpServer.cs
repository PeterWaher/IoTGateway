using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
	public class ModBusTcpServer : IDisposable, ISniffable
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
			this.server = new BinaryTcpServer(Port, TimeSpan.FromSeconds(30), Sniffers);
		}

		/// <summary>
		/// Modbus over TCP server
		/// </summary>
		/// <param name="Port">Port number to open</param>
		/// <param name="ServerCertificate">Server certificate, for encryption.</param>
		/// <param name="Sniffers">Optional set of sniffers.</param>
		private ModBusTcpServer(int Port, X509Certificate ServerCertificate, params ISniffer[] Sniffers)
		{
			this.server = new BinaryTcpServer(Port, TimeSpan.FromSeconds(30), ServerCertificate, Sniffers);
		}

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

		#region ISniffable

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
				await this.Information("ReadCoils(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrBits.ToString() + ")");
			}

			EventHandlerAsync<ReadBitsEventArgs> h = this.OnReadCoils;

			if (!(h is null))
				await h(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read coils.
		/// </summary>
		public event EventHandlerAsync<ReadBitsEventArgs> OnReadCoils;

		internal async Task RaiseReadInputDiscretes(ReadBitsEventArgs e)
		{
			if (this.HasSniffers)
			{
				await this.Information("ReadInputDiscretes(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrBits.ToString() + ")");
			}

			EventHandlerAsync<ReadBitsEventArgs> h = this.OnReadInputDiscretes;

			if (!(h is null))
				await h(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read input descrete registers.
		/// </summary>
		public event EventHandlerAsync<ReadBitsEventArgs> OnReadInputDiscretes;

		internal async Task RaiseReadMultipleRegisters(ReadWordsEventArgs e)
		{
			if (this.HasSniffers)
			{
				await this.Information("ReadMultipleRegisters(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrWords.ToString() + ")");
			}

			EventHandlerAsync<ReadWordsEventArgs> h = this.OnReadMultipleRegisters;

			if (!(h is null))
				await h(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read multiple registers.
		/// </summary>
		public event EventHandlerAsync<ReadWordsEventArgs> OnReadMultipleRegisters;

		internal async Task RaiseReadInputRegisters(ReadWordsEventArgs e)
		{
			if (this.HasSniffers)
			{
				await this.Information("ReadInputRegisters(" + e.UnitAddress.ToString() + "," +
					e.ReferenceNr.ToString() + "," + e.NrWords.ToString() + ")");
			}

			EventHandlerAsync<ReadWordsEventArgs> h = this.OnReadInputRegisters;

			if (!(h is null))
				await h(this, e);
		}

		/// <summary>
		/// Event raised when a client wants to read input registers.
		/// </summary>
		public event EventHandlerAsync<ReadWordsEventArgs> OnReadInputRegisters;

		#endregion

	}
}
