using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Networking.Sniffers;

namespace Waher.Networking.Modbus
{
	/// <summary>
	/// Modbus over TCP client
	/// </summary>
	public class ModbusTcpClient : Sniffable, IDisposable
	{
		/// <summary>
		/// Default Modbus port (502)
		/// </summary>
		public const int DefaultPort = 502;

		private readonly BinaryTcpClient tcpClient;
		private bool connected = false;

		/// <summary>
		/// Modbus over TCP client
		/// </summary>
		/// <param name="Sniffers">Sniffers</param>
		private ModbusTcpClient(params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.tcpClient = new BinaryTcpClient();
		}

		/// <summary>
		/// Connects to an Modbus TCP/IP Gateway
		/// </summary>
		/// <param name="Host">Host name or IP Address of gateway.</param>
		/// <param name="Port">Port number to connect to.</param>
		/// <returns>Connection objcet</returns>
		/// <exception cref="Exception">If conncetion could not be established.</exception>
		public static async Task<ModbusTcpClient> Connect(string Host, int Port)
		{
			ModbusTcpClient Result = new ModbusTcpClient();

			Result.tcpClient.OnDisconnected += Result.TcpClient_OnDisconnected;
			Result.tcpClient.OnError += Result.TcpClient_OnError;
			Result.tcpClient.OnInformation += Result.TcpClient_OnInformation;
			Result.tcpClient.OnWarning += Result.TcpClient_OnWarning;
			Result.tcpClient.OnReceived += Result.TcpClient_OnReceived;

			if (!await Result.tcpClient.ConnectAsync(Host, Port))
				throw new IOException("Unable to connect to " + Host + ":" + Port);

			Result.connected = true;

			return Result;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.tcpClient?.DisposeWhenDone();
		}

		/// <summary>
		/// If the client is currently connected to the Modbus gateway.
		/// </summary>
		public bool Connected => this.connected;

		private void TcpClient_OnDisconnected(object sender, EventArgs e)
		{
			this.connected = false;
		}

		private Task TcpClient_OnError(object Sender, Exception Exception)
		{
			this.Error(Exception.Message);
			return Task.CompletedTask;
		}

		private void TcpClient_OnWarning(ref string Text)
		{
			this.Warning(Text);
		}

		private void TcpClient_OnInformation(ref string Text)
		{
			this.Information(Text);
		}

		private Task<bool> TcpClient_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			// TODO
			return Task.FromResult<bool>(true);
		}

	}
}
