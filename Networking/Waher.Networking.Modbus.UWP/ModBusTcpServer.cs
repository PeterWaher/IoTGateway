using System;
using System.Collections;
using System.Collections.Generic;
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
		private BinaryTcpServer server;

		/// <summary>
		/// Modbus over TCP server
		/// </summary>
		/// <param name="Port">Port number to open</param>
		/// <param name="Tls">If connections are required to be encypted.</param>
		/// <param name="Sniffers">Optional set of sniffers.</param>
		public ModBusTcpServer(int Port, bool Tls, params ISniffer[] Sniffers)
		{
			this.server = new BinaryTcpServer(Port, TimeSpan.FromSeconds(30), Sniffers);
		}

		/// <summary>
		/// Closes and disposes of the server.
		/// </summary>
		public void Dispose()
		{
			this.server.Dispose();
			this.server = null;
		}

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

	}
}
