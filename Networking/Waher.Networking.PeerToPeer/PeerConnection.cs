using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Waher.Events;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Maintains a peer connection
	/// </summary>
	public class PeerConnection : IDisposable
	{
		private byte[] packetBuffer = null;
		private readonly PeerToPeerNetwork network;
		private IPEndPoint remoteEndpoint;
		private BinaryTcpClient tcpConnection;
		private EventHandler resynchCallback;
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

			this.tcpConnection.OnDisconnected += TcpConnection_OnDisconnected;
			this.tcpConnection.OnError += TcpConnection_OnError;
			this.tcpConnection.OnReceived += TcpConnection_OnReceived;
			this.tcpConnection.OnSent += TcpConnection_OnSent;
		}

		private async Task TcpConnection_OnSent(object Sender, byte[] Buffer, int Offset, int Count)
		{
			this.lastTcpPacket = DateTime.Now;

			BinaryDataWrittenEventHandler h = this.OnSent;
			if (!(h is null))
			{
				try
				{
					await h(this, Buffer, Offset, Count);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private async Task<bool> TcpConnection_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
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
					return await h(this, this.packetBuffer, 0, this.packetSize);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		
			return true;
		}

		private Task TcpConnection_OnError(object _, Exception _2)
		{
			this.Closed();
			return Task.CompletedTask;
		}

		private void TcpConnection_OnDisconnected(object sender, EventArgs e)
		{
			this.Closed();
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
		public void Start(EventHandler ResynchCallback)
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
		public BinaryTcpClient Tcp
		{
			get { return this.tcpConnection; }
		}

		/// <summary>
		/// Peer-to-peer network.
		/// </summary>
		public PeerToPeerNetwork Network
		{
			get { return this.network; }
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint RemoteEndpoint
		{
			get { return this.remoteEndpoint; }
			internal set { this.remoteEndpoint = value; }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;

			this.idleTimer?.Dispose();
			this.idleTimer = null;

			this.tcpConnection?.Dispose();
			this.tcpConnection = null;

			this.Closed();
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		public Task SendTcp(byte[] Packet)
		{
			return this.SendTcp(Packet, null);
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="Callback">Optional method to call when packet has been sent.</param>
		public Task SendTcp(byte[] Packet, EventHandler Callback)
		{
			if (this.disposed)
				return Task.CompletedTask;

			byte[] EncodedPacket = this.EncodePacket(Packet, false);
			return this.tcpConnection.SendAsync(EncodedPacket, Callback);
		}

		private byte[] EncodePacket(byte[] Packet, bool IncludePacketNumber)
		{
			if (!this.encapsulatePackets)
				return Packet;

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
		public void SendUdp(byte[] Packet, int IncludeNrPreviousPackets)
		{
			byte[] EncodedPacket = this.EncodePacket(Packet, true);

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

				this.network.SendUdp(this.remoteEndpoint, ToSend);
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

		private void Closed()
		{
			if (!this.closed)
			{
				this.closed = true;

				if (this.resynchCallback != null)
				{
					try
					{
						this.resynchCallback(this, new EventArgs());

						this.closed = true;
						this.Dispose();
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						this.RaiseOnClosed();
					}
				}
				else
					this.RaiseOnClosed();
			}
		}

		private void RaiseOnClosed()
		{
			EventHandler h = this.OnClosed;
			if (h != null)
			{
				try
				{
					h(this, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a connection has been closed for some reason.
		/// </summary>
		public event EventHandler OnClosed = null;

		/// <summary>
		/// State object that applications can use to attach information to a connection.
		/// </summary>
		public object StateObject
		{
			get { return this.stateObject; }
			set { this.stateObject = value; }
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
								if (LostPackets is null)
									LostPackets = new LinkedList<KeyValuePair<ushort, byte[]>>();

								LostPackets.AddFirst(new KeyValuePair<ushort, byte[]>(PacketNr, Packet));   // Reverse order
							}
						}
					}

					if (FirstPacket != null)
						this.lastReceivedPacket = FirstPacketNr;
				}

				BinaryDataReadEventHandler h = this.OnReceived;
				if (h != null)
				{
					if (LostPackets != null)
					{
						foreach (KeyValuePair<ushort, byte[]> P in LostPackets)
						{
							try
							{
								await h(this, P.Value, 0, P.Value.Length);
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}
					}

					if (FirstPacket != null)
					{
						try
						{
							await h(this, FirstPacket, 0, FirstPacket.Length);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
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
				if (h != null)
				{
					try
					{
						await h(this, Packet, 0, Packet.Length);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
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

		private void IdleTimerCallback(object P)
		{
			if ((DateTime.Now - this.lastTcpPacket).TotalSeconds > 10)
			{
				try
				{
					this.SendTcp(new byte[0]);
				}
				catch (Exception)
				{
					try
					{
						this.Closed();
						this.Dispose();
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		private Timer idleTimer = null;
		private DateTime lastTcpPacket = DateTime.Now;

	}
}
