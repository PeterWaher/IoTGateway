﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Maintains a peer connection
	/// </summary>
	public class PeerConnection : IDisposableAsync
	{
		private byte[] packetBuffer = null;
		private readonly PeerToPeerNetwork network;
		private IPEndPoint remoteEndpoint;
		private BinaryTcpClient tcpConnection;
		private EventHandlerAsync resynchCallback;
		private object stateObject = null;
		private int readState = 0;
		private int packetSize = 0;
		private ushort outgoingPacketNumber = 0;
		private int offset = 0;
		private int packetPos = 0;
		private bool closed = false;
		private bool disposed = false;
		private readonly bool encapsulatePackets;

		internal PeerConnection(BinaryTcpClient TcpConnection, PeerToPeerNetwork Network, IPEndPoint RemoteEndpoint,
			bool EncapsulatePackets)
		{
			this.network = Network;
			this.remoteEndpoint = RemoteEndpoint;
			this.tcpConnection = TcpConnection;
			this.encapsulatePackets = EncapsulatePackets;

			this.tcpConnection.OnDisconnected += this.TcpConnection_OnDisconnected;
			this.tcpConnection.OnError += this.TcpConnection_OnError;
			this.tcpConnection.OnReceived += this.TcpConnection_OnReceived;
			this.tcpConnection.OnSent += this.TcpConnection_OnSent;
		}

		private async Task TcpConnection_OnSent(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			this.lastTcpPacket = DateTime.Now;

			BinaryDataWrittenEventHandler h = this.OnSent;
			if (!(h is null))
			{
				try
				{
					await h(this, ConstantBuffer, Buffer, Offset, Count);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		private async Task<bool> TcpConnection_OnReceived(object Sender, bool ConstantBuffer, byte[] Buffer, int Offset, int Count)
		{
			bool Continue = true;

			this.lastTcpPacket = DateTime.Now;
			this.resynchCallback = null;

			if (this.encapsulatePackets)
			{
				int NrLeft;
				byte b;
				
				while (Count-- > 0 && Continue && !this.disposed)
				{
					switch (this.readState)
					{
						case 0:
							b = Buffer[Offset++];
							this.packetSize |= (b & 127) << this.offset;
							this.offset += 7;
							if ((b & 128) == 0)
							{
								this.packetBuffer = new byte[this.packetSize];
								this.packetPos = 0;
								this.readState = 1;
							}
							break;

						case 1:
							NrLeft = Math.Min(Count, this.packetSize - this.packetPos);
							Array.Copy(Buffer, Offset, this.packetBuffer, this.packetPos, NrLeft);
							Offset += NrLeft;
							this.packetPos += NrLeft;

							if (this.packetPos >= this.packetSize)
							{
								Continue = await this.OnPacketReceived();

								this.readState = 0;
								this.packetSize = 0;
								this.offset = 0;
								this.packetBuffer = null;
							}
							break;

						default:
							Count = 0;
							break;
					}
				}
			}
			else
			{
				this.packetSize = Count;
				this.packetBuffer = new byte[Count];
				Array.Copy(Buffer, Offset, this.packetBuffer, 0, Count);
				Continue = await this.OnPacketReceived();
			}

			return Continue;
		}

		private async Task<bool> OnPacketReceived()
		{
			BinaryDataReadEventHandler h = this.OnReceived;
			if (!(h is null))
			{
				try
				{
					return await h(this, false, this.packetBuffer, 0, this.packetSize);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		
			return true;
		}

		private Task TcpConnection_OnError(object _, Exception _2)
		{
			return this.Closed();
		}

		private Task TcpConnection_OnDisconnected(object Sender, EventArgs e)
		{
			return this.Closed();
		}

		/// <summary>
		/// Starts receiving on the connection.
		/// </summary>
		public void Start()
		{
			this.Start(null);
		}

		/// <summary>
		/// Starts receiving on the connection.
		/// </summary>
		///	<param name="ResynchCallback">Resynchronization callback to call, if connection is discarded. Can be null if no 
		///	resynchronization method is used.</param>
		public void Start(EventHandlerAsync ResynchCallback)
		{
			this.readState = 0;
			this.packetSize = 0;
			this.offset = 0;
			this.resynchCallback = ResynchCallback;
			this.tcpConnection?.Continue();
		}

		/// <summary>
		/// Underlying TCP connection
		/// </summary>
		public BinaryTcpClient Tcp => this.tcpConnection;

		/// <summary>
		/// Peer-to-peer network.
		/// </summary>
		public PeerToPeerNetwork Network => this.network;

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndpoint
		{
			get => this.remoteEndpoint;
			internal set => this.remoteEndpoint = value;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync()")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public async Task DisposeAsync()
		{
			this.disposed = true;

			this.idleTimer?.Dispose();
			this.idleTimer = null;

			if (!(this.tcpConnection is null))
			{
				await this.tcpConnection.DisposeAsync();
				this.tcpConnection = null;
			}

			await this.Closed();
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task SendTcp(byte[] Packet)
		{
			return this.SendTcp(false, Packet);
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Packet to send.</param>
		public Task SendTcp(bool ConstantBuffer, byte[] Packet)
		{
			return this.SendTcp(ConstantBuffer, Packet, null, null);
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="Callback">Optional method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		[Obsolete("Use an overload with a ConstantBuffer argument. This increases performance, as the buffer will not be unnecessarily cloned if queued.")]
		public Task SendTcp(byte[] Packet, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			return this.SendTcp(false, Packet, Callback, State);
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="Callback">Optional method to call when packet has been sent.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendTcp(bool ConstantBuffer, byte[] Packet, EventHandlerAsync<DeliveryEventArgs> Callback, object State)
		{
			if (this.disposed)
				return Task.CompletedTask;

			byte[] EncodedPacket = this.EncodePacket(Packet, false, out bool ConstantBuffer2);
			return this.tcpConnection.SendAsync(ConstantBuffer || ConstantBuffer2, EncodedPacket, Callback, State);
		}

		private byte[] EncodePacket(byte[] Packet, bool IncludePacketNumber, out bool ConstantBuffer)
		{
			if (!this.encapsulatePackets)
			{
				ConstantBuffer = false;
				return Packet;
			}

			ushort PacketNr;
			int i = Packet.Length;
			int j = 0;
			int c = 1;
			byte b;

			i >>= 7;
			while (i > 0)
			{
				c++;
				i >>= 7;
			}

			if (IncludePacketNumber)
				c += 2;

			i = Packet.Length;

			byte[] Packet2 = new byte[c + i];
			Array.Copy(Packet, 0, Packet2, c, i);
			ConstantBuffer = true;

			do
			{
				b = (byte)(i & 127);
				i >>= 7;
				if (i > 0)
					b |= 128;

				Packet2[j++] = b;
			}
			while (i > 0);

			if (IncludePacketNumber)
			{
				PacketNr = ++this.outgoingPacketNumber;

				Packet2[j++] = (byte)PacketNr;
				Packet2[j++] = (byte)(PacketNr >> 8);
			}

			return Packet2;
		}

		/// <summary>
		/// Sends a packet to a peer using UDP. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed. Since UDP datagrams can be lost, the
		/// method allows you to include previous packets in the datagram, so that the receiving end
		/// can recover seamlessly from occational lost datagrams.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="IncludeNrPreviousPackets">Number of previous packets to include in the datagram. Note that the network limits
		/// total size of datagram packets.</param>
		public Task SendUdp(byte[] Packet, int IncludeNrPreviousPackets)
		{
			byte[] EncodedPacket = this.EncodePacket(Packet, true, out bool _);

			lock (this.historicPackets)
			{
				byte[] ToSend;
				int j, i = 0;
				int c = EncodedPacket.Length;

				if (IncludeNrPreviousPackets == 0)
					ToSend = EncodedPacket;
				else
				{
					foreach (byte[] Packet2 in this.historicPackets)
					{
						c += Packet2.Length;
						i++;
						if (i >= IncludeNrPreviousPackets)
							break;
					}

					ToSend = new byte[c];
					j = EncodedPacket.Length;
					Array.Copy(EncodedPacket, 0, ToSend, 0, j);

					i = 0;
					foreach (byte[] Packet2 in this.historicPackets)
					{
						Array.Copy(Packet2, 0, ToSend, j, Packet2.Length);
						j += Packet2.Length;
						i++;
						if (i >= IncludeNrPreviousPackets)
							break;
					}
				}

				this.historicPackets.AddFirst(EncodedPacket);

				if (this.nrHistoricPackets >= IncludeNrPreviousPackets)
					this.historicPackets.RemoveLast();  // Doesn't reduce the size to INcludeNrPreviousPackets, but keeps list at the largest requested number, to date.
				else
					this.nrHistoricPackets++;

				return this.network.SendUdp(this.remoteEndpoint, ToSend);
			}
		}

		private int nrHistoricPackets = 0;
		private readonly LinkedList<byte[]> historicPackets = new LinkedList<byte[]>();

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryDataWrittenEventHandler OnSent = null;

		/// <summary>
		/// If reading has been paused.
		/// </summary>
		public bool Paused => this.tcpConnection.Paused;

		/// <summary>
		/// Continues a paused connection.
		/// </summary>
		public void Continue()
		{
			this.tcpConnection.Continue();
		}

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		public event BinaryDataReadEventHandler OnReceived = null;

		private async Task Closed()
		{
			if (!this.closed)
			{
				this.closed = true;

				if (!(this.resynchCallback is null))
				{
					await this.resynchCallback.Raise(this, EventArgs.Empty, false);
					await this.DisposeAsync();
				}
				else
					await this.RaiseOnClosed();
			}
		}

		private Task RaiseOnClosed()
		{
			return this.OnClosed.Raise(this, EventArgs.Empty);
		}

		/// <summary>
		/// Event raised when a connection has been closed for some reason.
		/// </summary>
		public event EventHandlerAsync OnClosed = null;

		/// <summary>
		/// State object that applications can use to attach information to a connection.
		/// </summary>
		public object StateObject
		{
			get => this.stateObject;
			set => this.stateObject = value;
		}

		internal async Task UdpDatagramReceived(object _, UdpDatagramEventArgs e)
		{
			if (this.encapsulatePackets)
			{
				LinkedList<KeyValuePair<ushort, byte[]>> LostPackets = null;
				byte[] FirstPacket = null;
				ushort FirstPacketNr = 0;
				ushort PacketNr;
				byte[] Packet;
				byte[] Data = e.Data;
				int Len = Data.Length;
				int Pos = 0;
				int PacketLen;
				int Offset;
				byte b;

				lock (this.udpReceiveLock)
				{
					while (Pos < Len)
					{
						b = Data[Pos++];
						PacketLen = (b & 127);
						Offset = 7;
						while (Pos < Len && (b & 128) != 0)
						{
							b = Data[Pos++];
							PacketLen |= (b & 127) << Offset;
							Offset += 7;
						}

						if (Pos + 2 > Len)
							break;

						PacketNr = Data[Pos++];
						PacketNr |= (ushort)(Data[Pos++] << 8);

						if (Pos + PacketLen > Len)
							break;

						Packet = new byte[PacketLen];
						Array.Copy(Data, Pos, Packet, 0, PacketLen);
						Pos += PacketLen;

						if ((short)(PacketNr - this.lastReceivedPacket) > 0)
						{
							if (FirstPacket is null)
							{
								FirstPacket = Packet;
								FirstPacketNr = PacketNr;
							}
							else
							{
								LostPackets ??= new LinkedList<KeyValuePair<ushort, byte[]>>();
								LostPackets.AddFirst(new KeyValuePair<ushort, byte[]>(PacketNr, Packet));   // Reverse order
							}
						}
					}

					if (!(FirstPacket is null))
						this.lastReceivedPacket = FirstPacketNr;
				}

				BinaryDataReadEventHandler h = this.OnReceived;
				if (!(h is null))
				{
					if (!(LostPackets is null))
					{
						foreach (KeyValuePair<ushort, byte[]> P in LostPackets)
						{
							try
							{
								await h(this, true, P.Value, 0, P.Value.Length);
							}
							catch (Exception ex)
							{
								Log.Exception(ex);
							}
						}
					}

					if (!(FirstPacket is null))
					{
						try
						{
							await h(this, true, FirstPacket, 0, FirstPacket.Length);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				}
			}
			else
			{
				byte[] Data = e.Data;
				int Len = Data.Length;
				byte[] Packet = new byte[Len];

				Array.Copy(Data, 0, Packet, 0, Len);

				BinaryDataReadEventHandler h = this.OnReceived;
				if (!(h is null))
				{
					try
					{
						await h(this, true, Packet, 0, Packet.Length);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
		}

		private ushort lastReceivedPacket = 0;
		private readonly object udpReceiveLock = new object();

		internal void StartIdleTimer()
		{
			this.idleTimer = new Timer(this.IdleTimerCallback, null, 5000, 5000);
		}

		private async void IdleTimerCallback(object P)
		{
			try
			{
				if ((DateTime.Now - this.lastTcpPacket).TotalSeconds > 10)
				{
					try
					{
						await this.SendTcp(true, Array.Empty<byte>());
					}
					catch (Exception)
					{
						try
						{
							await this.Closed();
							await this.DisposeAsync();
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private Timer idleTimer = null;
		private DateTime lastTcpPacket = DateTime.Now;

	}
}
