using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Waher.Networking.PeerToPeer
{
	/// <summary>
	/// Maintains a peer connection
	/// </summary>
	public class PeerConnection : IDisposable
	{
		private const int BufferSize = 65536;

		private byte[] incomingBuffer = new byte[BufferSize];
		private byte[] packetBuffer = null;
		private LinkedList<QueuedItem> outgoingPackets = new LinkedList<QueuedItem>();
		private PeerToPeerNetwork network;
		private IPEndPoint remoteEndpoint;
		private TcpClient tcpConnection;
		private NetworkStream stream;
		private object stateObject = null;
		private int readState = 0;
		private int packetSize = 0;
		private ushort outgoingPacketNumber = 0;
		private int offset = 0;
		private int packetPos = 0;
		private bool writing = false;
		private bool closed = false;
		private bool encapsulatePackets;

		internal PeerConnection(TcpClient TcpConnection, PeerToPeerNetwork Network, IPEndPoint RemoteEndpoint,
			bool EncapsulatePackets)
		{
			this.network = Network;
			this.remoteEndpoint = RemoteEndpoint;
			this.tcpConnection = TcpConnection;
			this.stream = this.tcpConnection.GetStream();
			this.encapsulatePackets = EncapsulatePackets;
		}

		private class QueuedItem
		{
			public byte[] Packet;
			public EventHandler Callback;

			public QueuedItem(byte[] Packet, EventHandler Callback)
			{
				this.Packet = Packet;
				this.Callback = Callback;
			}
		}

		/// <summary>
		/// Starts receiving on the connection.
		/// </summary>
		public void Start()
		{
			this.readState = 0;
			this.packetSize = 0;
			this.offset = 0;
			this.stream.BeginRead(this.incomingBuffer, 0, BufferSize, this.EndReadTcp, null);
		}

		/// <summary>
		/// Underlying TCP connection
		/// </summary>
		public TcpClient Tcp
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
			if (this.idleTimer != null)
			{
				this.idleTimer.Dispose();
				this.idleTimer = null;
			}

			if (this.tcpConnection != null)
			{
				this.stream.Dispose();
				this.stream = null;

				this.tcpConnection.Close();
				this.tcpConnection = null;
			}

			this.Closed();
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		public void SendTcp(byte[] Packet)
		{
			this.SendTcp(Packet, null);
		}

		/// <summary>
		/// Sends a packet to the peer at the other side of the TCP connection. Transmission is done asynchronously and is
		/// buffered if a sending operation is being performed.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="Callback">Optional method to call when packet has been sent.</param>
		public void SendTcp(byte[] Packet, EventHandler Callback)
		{
			byte[] EncodedPacket = this.EncodePacket(Packet, false);
			QueuedItem Item = new QueuedItem(EncodedPacket, Callback);

			lock (this.outgoingPackets)
			{
				if (this.writing)
					this.outgoingPackets.AddLast(Item);
				else
				{
					this.writing = true;
					this.stream.BeginWrite(EncodedPacket, 0, EncodedPacket.Length, this.EndWriteTcp, Item);
					this.lastTcpPacket = DateTime.Now;
				}
			}
		}

		private byte[] EncodePacket(byte[] Packet, bool IncludePacketNumber)
		{
			if (!this.encapsulatePackets)
				return Packet;

			ushort PacketNr = 0;
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

		private void EndWriteTcp(IAsyncResult ar)
		{
			lock (this.outgoingPackets)
			{
				try
				{
					this.stream.EndWrite(ar);

					QueuedItem Item = (QueuedItem)ar.AsyncState;

					BinaryEventHandler h = this.OnSent;
					if (h != null)
					{
						try
						{
							h(this, Item.Packet);
						}
						catch (Exception ex)
						{
							Events.Log.Critical(ex);
						}
					}

					if (Item.Callback != null)
					{
						try
						{
							Item.Callback(this, new EventArgs());
						}
						catch (Exception ex)
						{
							Events.Log.Critical(ex);
						}
					}

					if (this.outgoingPackets.First != null)
					{
						Item = this.outgoingPackets.First.Value;
						this.outgoingPackets.RemoveFirst();
						this.stream.BeginWrite(Item.Packet, 0, Item.Packet.Length, this.EndWriteTcp, Item);
					}
					else
						this.writing = false;
				}
				catch (Exception)
				{
					this.Closed();
				}
			}
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
		private LinkedList<byte[]> historicPackets = new LinkedList<byte[]>();

		/// <summary>
		/// Event raised when a packet has been sent.
		/// </summary>
		public event BinaryEventHandler OnSent = null;

		private void EndReadTcp(IAsyncResult ar)
		{
			if (this.stream == null)
				return;

			BinaryEventHandler h = null;
			int Pos;
			int NrLeft;
			byte b;

			try
			{
				int NrRead = this.stream.EndRead(ar);
				if (NrRead <= 0)
					this.Closed();
				else
				{
					this.lastTcpPacket = DateTime.Now;

					if (this.encapsulatePackets)
					{
						Pos = 0;
						while (Pos < NrRead)
						{
							switch (this.readState)
							{
								case 0:
									b = this.incomingBuffer[Pos++];
									this.packetSize |= (b & 127) << this.offset;
									this.offset += 7;
									if ((b & 128) == 0)
									{
										this.packetBuffer = new byte[this.packetSize];
										this.packetPos = 0;
										this.readState++;
									}
									break;

								case 1:
									NrLeft = NrRead - Pos;
									if (NrLeft > this.packetSize - this.packetPos)
										NrLeft = this.packetSize - this.packetPos;

									Array.Copy(this.incomingBuffer, Pos, this.packetBuffer, this.packetPos, NrLeft);
									Pos += NrLeft;
									this.packetPos += NrLeft;

									if (this.packetPos >= this.packetSize)
									{
										h = this.OnReceived;
										if (h != null)
										{
											try
											{
												h(this, this.packetBuffer);
											}
											catch (Exception ex)
											{
												Events.Log.Critical(ex);
											}
										}

										this.readState = 0;
										this.packetSize = 0;
										this.offset = 0;
										this.packetBuffer = null;
									}
									break;
							}
						}
					}
					else
					{
						this.packetSize = NrRead;
						this.packetBuffer = new byte[this.packetSize];

						Array.Copy(this.incomingBuffer, 0, this.packetBuffer, 0, NrRead);

						h = this.OnReceived;
						if (h != null)
						{
							try
							{
								h(this, this.packetBuffer);
							}
							catch (Exception ex)
							{
								Events.Log.Critical(ex);
							}
						}
					}

					if (this.stream != null)
						this.stream.BeginRead(this.incomingBuffer, 0, BufferSize, this.EndReadTcp, null);
				}
			}
			catch (Exception)
			{
				this.Closed();
			}
		}

		/// <summary>
		/// Event received when binary data has been received.
		/// </summary>
		public event BinaryEventHandler OnReceived = null;

		private void Closed()
		{
			if (!this.closed)
			{
				this.closed = true;
				this.writing = false;
				this.outgoingPackets.Clear();

				EventHandler h = this.OnClosed;
				if (h != null)
				{
					try
					{
						h(this, new EventArgs());
					}
					catch (Exception ex)
					{
						Events.Log.Critical(ex);
					}
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

		internal void UdpDatagramReceived(object Sender, UdpDatagramEventArgs e)
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
							if (FirstPacket == null)
							{
								FirstPacket = Packet;
								FirstPacketNr = PacketNr;
							}
							else
							{
								if (LostPackets == null)
									LostPackets = new LinkedList<KeyValuePair<ushort, byte[]>>();

								LostPackets.AddFirst(new KeyValuePair<ushort, byte[]>(PacketNr, Packet));   // Reverse order
							}
						}
					}

					if (FirstPacket != null)
						this.lastReceivedPacket = FirstPacketNr;
				}

				BinaryEventHandler h = this.OnReceived;
				if (h != null)
				{
					if (LostPackets != null)
					{
						foreach (KeyValuePair<ushort, byte[]> P in LostPackets)
						{
							try
							{
								h(this, P.Value);
							}
							catch (Exception ex)
							{
								Events.Log.Critical(ex);
							}
						}
					}

					if (FirstPacket != null)
					{
						try
						{
							h(this, FirstPacket);
						}
						catch (Exception ex)
						{
							Events.Log.Critical(ex);
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

				BinaryEventHandler h = this.OnReceived;
				if (h != null)
				{
					try
					{
						h(this, Packet);
					}
					catch (Exception ex)
					{
						Events.Log.Critical(ex);
					}
				}
			}
		}

		private ushort lastReceivedPacket = 0;
		private object udpReceiveLock = new object();

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
					this.Closed();
					this.Dispose();
				}
			}
		}

		private Timer idleTimer = null;
		private DateTime lastTcpPacket = DateTime.Now;

	}
}
