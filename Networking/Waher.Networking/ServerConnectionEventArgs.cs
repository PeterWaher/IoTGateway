﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking
{
	/// <summary>
	/// Event arguments for connection events.
	/// </summary>
	public class ServerConnectionEventArgs : EventArgs
	{
		private readonly ServerTcpConnection connection;

		/// <summary>
		/// Event arguments for connection events.
		/// </summary>
		/// <param name="Connection">Server connection</param>
		public ServerConnectionEventArgs(ServerTcpConnection Connection)
		{
			this.connection = Connection;
		}

		/// <summary>
		/// Connection ID
		/// </summary>
		public Guid Id => this.connection.Id;

		/// <summary>
		/// Client connection
		/// </summary>
		public BinaryTcpClient Client => this.connection.Client;

		/// <summary>
		/// Server reference.
		/// </summary>
		public BinaryTcpServer Server => this.connection.Server;

		/// <summary>
		/// Sends data back to the client.
		/// </summary>
		/// <param name="Data">Data to send.</param>
		/// <returns>If data was sent.</returns>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task<bool> SendAsync(params byte[] Data)
		{
			return this.SendAsync(false, Data);
		}

		/// <summary>
		/// Sends data back to the client.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Data to send.</param>
		/// <returns>If data was sent.</returns>
		public Task<bool> SendAsync(bool ConstantBuffer, params byte[] Data)
		{
			return this.connection.SendAsync(ConstantBuffer, Data);
		}

		/// <summary>
		/// Closes the underlying connection.
		/// </summary>
		public void CloseConnection()
		{
			this.connection.Dispose();
		}
	}
}