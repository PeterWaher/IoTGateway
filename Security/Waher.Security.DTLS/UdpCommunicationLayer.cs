﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking;

namespace Waher.Security.DTLS
{
    /// <summary>
    /// Class acting as an interface for DTLS to communicate UDP.
    /// </summary>
    public class UdpCommunicationLayer : CommunicationLayer, IDisposable, ICommunicationLayer
	{
		private readonly LinkedList<KeyValuePair<byte[], IPEndPoint>> outputQueue = new LinkedList<KeyValuePair<byte[], IPEndPoint>>();
		private UdpClient client;
		private bool disposed = false;
		private bool isWriting = false;

		/// <summary>
		/// Class acting as an interface for DTLS to communicate UDP.
		/// </summary>
		/// <param name="UdpClient">UDP client.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public UdpCommunicationLayer(UdpClient UdpClient, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.client = UdpClient;
			this.BeginReceive();
		}

		/// <summary>
		/// UDP Client.
		/// </summary>
		public UdpClient Client => this.client;

		private async void BeginReceive()   // Starts parallel task
		{
			try
			{
				while (!this.disposed)
				{
					UdpReceiveResult Data = await this.client.ReceiveAsync();
					if (this.disposed)
						return;

					await this.ReceiveBinary(Data.Buffer);

					try
					{
						DataReceivedEventHandler h = this.PacketReceived;

						if (!(h is null))
							await h(Data.Buffer, Data.RemoteEndPoint);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// Session closed.
			}
			catch (Exception ex)
			{
				if (!this.disposed)
					await this.Exception(ex);
			}
		}

		private async Task BeginTransmit(byte[] Packet, IPEndPoint RemoteEndpoint)
		{
			if (this.disposed)
				return;

			lock (this.outputQueue)
			{
				if (this.isWriting)
				{
					this.outputQueue.AddLast(new KeyValuePair<byte[], IPEndPoint>(Packet, RemoteEndpoint));
					return;
				}
				else
					this.isWriting = true;
			}

			try
			{
				while (!(Packet is null))
				{
					await this.client.SendAsync(Packet, Packet.Length, RemoteEndpoint);
					if (this.disposed)
						return;

					lock (this.outputQueue)
					{
						if (this.outputQueue.First is null)
						{
							this.isWriting = false;
							Packet = null;
						}
						else
						{
							Packet = this.outputQueue.First.Value.Key;
							RemoteEndpoint = this.outputQueue.First.Value.Value;
							this.outputQueue.RemoveFirst();
						}
					}
				}
			}
			catch (Exception ex)
			{
				await this.Exception(ex);
			}
		}

		/// <summary>
		/// Event is reaised whenever data has been received from the UDP layer.
		/// </summary>
		public event DataReceivedEventHandler PacketReceived;

		/// <summary>
		/// Method called by the DTLS layer when a datagram is to be sent.
		/// </summary>
		/// <param name="Packet">Datagram</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public Task SendPacket(byte[] Packet, object RemoteEndpoint)
		{
			IPEndPoint EP = (IPEndPoint)RemoteEndpoint;
			return this.BeginTransmit(Packet, EP);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;

			if (!(this.client is null))
			{
				this.client.Client.Shutdown(SocketShutdown.Both);
				this.client.Dispose();
				this.client = null;
			}
		}
	}
}
