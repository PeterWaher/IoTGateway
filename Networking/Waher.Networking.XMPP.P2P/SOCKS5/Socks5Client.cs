using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Networking.XMPP.P2P.SOCKS5
{
	/// <summary>
	/// SOCKS5 connection state.
	/// </summary>
	public enum Socks5State
	{
		/// <summary>
		/// Offline
		/// </summary>
		Offline,

		/// <summary>
		/// Connecting
		/// </summary>
		Connecting,

		/// <summary>
		/// Initializing
		/// </summary>
		Initializing,

		/// <summary>
		/// Authenticating
		/// </summary>
		Authenticating,

		/// <summary>
		/// Authenticated
		/// </summary>
		Authenticated,

		/// <summary>
		/// Connected
		/// </summary>
		Connected,

		/// <summary>
		/// Error state
		/// </summary>
		Error
	}

	/// <summary>
	/// Client used for SOCKS5 communication.
	/// 
	/// SOCKS5 is defined in RFC 1928.
	/// </summary>
	public class Socks5Client : Sniffable, IDisposable
	{
		private BinaryTcpClient client;
		private Socks5State state = Socks5State.Offline;
		private readonly object synchObj = new object();
		private readonly string host;
		private readonly int port;
		private readonly string jid;
		private bool closeWhenDone = false;
		private bool disposed = false;
		private object callbackState;
		private object tag = null;
		private bool isWriting = false;

		/// <summary>
		/// Client used for SOCKS5 communication.
		/// </summary>
		/// <param name="Host">Host of SOCKS5 stream host.</param>
		/// <param name="Port">Port of SOCKS5 stream host.</param>
		/// <param name="JID">JID of SOCKS5 stream host.</param>
		/// <param name="Sniffers">Optional set of sniffers.</param>
		public Socks5Client(string Host, int Port, string JID, params ISniffer[] Sniffers)
			: base(Sniffers)
		{
			this.host = Host;
			this.port = Port;
			this.jid = JID;

			this.State = Socks5State.Connecting;
			this.Information("Connecting to " + this.host + ":" + this.port.ToString());

			this.client = new BinaryTcpClient();
			this.Connect();
		}

		private async void Connect()
		{
			try
			{
				this.client.OnReceived += this.Client_OnReceived;
				this.client.OnSent += this.Client_OnSent;
				this.client.OnError += this.Client_OnError;
				this.client.OnDisconnected += this.Client_OnDisconnected;
				this.client.OnWriteQueueEmpty += this.Client_OnWriteQueueEmpty;

				await this.client.ConnectAsync(this.host, this.port);
				if (this.disposed)
					return;

				this.Information("Connected to " + this.host + ":" + this.port.ToString());

				this.state = Socks5State.Initializing;
				this.SendPacket(new byte[] { 5, 1, 0 });
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				this.State = Socks5State.Error;
			}
		}

		private void Client_OnWriteQueueEmpty(object sender, EventArgs e)
		{
			bool DoDispose;

			lock (this.synchObj)
			{
				this.isWriting = false;
				DoDispose = this.closeWhenDone;
			}

			if (DoDispose)
				this.Dispose();
			else
			{
				EventHandler h = this.OnWriteQueueEmpty;
				if (h != null)
				{
					try
					{
						h(this, e);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		private void Client_OnDisconnected(object sender, EventArgs e)
		{
			this.State = Socks5State.Offline;
		}

		private void Client_OnError(object Sender, Exception Exception)
		{
			this.State = Socks5State.Error;
		}

		private Task<bool> Client_OnSent(object Sender, byte[] Packet)
		{
			this.TransmitBinary(Packet);
			return Task.FromResult<bool>(true);
		}

		private Task<bool> Client_OnReceived(object Sender, byte[] Packet)
		{
			this.ReceiveBinary(Packet);

			try
			{
				this.ParseIncoming(Packet);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Task.FromResult<bool>(true);
		}

		/// <summary>
		/// Current state.
		/// </summary>
		public Socks5State State
		{
			get { return this.state; }
			internal set
			{
				if (this.state != value)
				{
					this.state = value;
					this.Information("State changed to " + this.state.ToString());

					EventHandler h = this.OnStateChange;
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
			}
		}

		internal object CallbackState
		{
			get { return this.callbackState; }
			set { this.callbackState = value; }
		}

		/// <summary>
		/// Tag
		/// </summary>
		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

		/// <summary>
		/// Event raised whenever the state changes.
		/// </summary>
		public event EventHandler OnStateChange = null;

		/// <summary>
		/// Host of SOCKS5 stream host.
		/// </summary>
		public string Host
		{
			get { return this.host; }
		}

		/// <summary>
		/// Port of SOCKS5 stream host.
		/// </summary>
		public int Port
		{
			get { return this.port; }
		}

		/// <summary>
		/// JID of SOCKS5 stream host.
		/// </summary>
		public string JID
		{
			get { return this.jid; }
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.disposed = true;
			this.State = Socks5State.Offline;

			this.client?.Dispose();
			this.client = null;
		}

		/// <summary>
		/// Send binary data.
		/// </summary>
		/// <param name="Data">Data</param>
		public void Send(byte[] Data)
		{
			if (this.state != Socks5State.Connected)
				throw new IOException("SOCKS5 connection not open.");

			this.SendPacket(Data);
		}

		private void SendPacket(byte[] Data)
		{
			lock (this.synchObj)
			{
				this.isWriting = true;
			}

			Data = (byte[])Data.Clone();
			this.client.Send(Data);
		}

		/// <summary>
		/// Event raised when the write queue is empty.
		/// </summary>
		public event EventHandler OnWriteQueueEmpty = null;

		/// <summary>
		/// Closes the stream when all bytes have been sent.
		/// </summary>
		public void CloseWhenDone()
		{
			lock (this.synchObj)
			{
				if (this.isWriting)
				{
					this.closeWhenDone = true;
					return;
				}
			}

			this.Dispose();
		}

		private void ParseIncoming(byte[] Data)
		{
			if (this.state == Socks5State.Connected)
			{
				DataReceivedEventHandler h = this.OnDataReceived;
				if (h != null)
				{
					try
					{
						h(this, new DataReceivedEventArgs(Data, this, this.callbackState));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			else if (this.state == Socks5State.Initializing)
			{
				if (Data.Length < 2 || Data[0] < 5)
				{
					this.ToError();
					return;
				}

				byte Method = Data[1];

				switch (Method)
				{
					case 0: // No authentication.
						this.State = Socks5State.Authenticated;
						break;

					default:
						this.ToError();
						return;
				}
			}
			else
			{
				if (Data.Length < 5 || Data[0] < 5)
				{
					this.ToError();
					return;
				}

				byte REP = Data[1];

				switch (REP)
				{
					case 0: // Succeeded
						this.State = Socks5State.Connected;
						break;

					case 1:
						this.Error("General SOCKS server failure.");
						this.ToError();
						break;

					case 2:
						this.Error("Connection not allowed by ruleset.");
						this.ToError();
						break;

					case 3:
						this.Error("Network unreachable.");
						this.ToError();
						break;

					case 4:
						this.Error("Host unreachable.");
						this.ToError();
						break;

					case 5:
						this.Error("Connection refused.");
						this.ToError();
						break;

					case 6:
						this.Error("TTL expired.");
						this.ToError();
						break;

					case 7:
						this.Error("Command not supported.");
						this.ToError();
						break;

					case 8:
						this.Error("Address type not supported.");
						this.ToError();
						break;

					default:
						this.Error("Unrecognized error code returned: " + REP.ToString());
						this.ToError();
						break;
				}

				byte ATYP = Data[3];
				int i = 4;
				int c = Data.Length;
				IPAddress Addr = null;
				string DomainName = null;

				switch (ATYP)
				{
					case 1: // IPv4.
						if (i + 4 > c)
						{
							this.Error("Expected more bytes.");
							this.ToError();
							return;
						}

						byte[] A = new byte[4];
						Array.Copy(Data, i, A, 0, 4);
						i += 4;
						Addr = new IPAddress(A);
						break;

					case 3: // Domain name.
						byte NrBytes = Data[i++];
						if (i + NrBytes > c)
						{
							this.Error("Expected more bytes.");
							this.ToError();
							return;
						}

						DomainName = Encoding.ASCII.GetString(Data, i, NrBytes);
						i += NrBytes;
						break;

					case 4: // IPv6.
						if (i + 16 > c)
						{
							this.Error("Expected more bytes.");
							this.ToError();
							return;
						}

						A = new byte[16];
						Array.Copy(Data, i, A, 0, 16);
						i += 16;
						Addr = new IPAddress(A);
						break;

					default:
						this.ToError();
						return;
				}

				if (i + 2 != c)
				{
					this.Error("Invalid number of bytes received.");
					this.ToError();
					return;
				}

				int Port = Data[i++];
				Port <<= 8;
				Port |= Data[i++];

				ResponseEventHandler h = this.OnResponse;
				if (h != null)
				{
					try
					{
						h(this, new ResponseEventArgs(REP, Addr, DomainName, Port));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when a response has been returned.
		/// </summary>
		public event ResponseEventHandler OnResponse = null;

		/// <summary>
		/// Event raised when binary data has been received over an established connection.
		/// </summary>
		public event DataReceivedEventHandler OnDataReceived = null;

		private void ToError()
		{
			this.State = Socks5State.Error;
			this.client.Dispose();
		}

		private void Request(Command Command, IPAddress DestinationAddress, int Port)
		{
			using (MemoryStream Req = new MemoryStream())
			{
				Req.WriteByte(5);
				Req.WriteByte((byte)Command);
				Req.WriteByte(0);

				if (DestinationAddress.AddressFamily == AddressFamily.InterNetwork)
					Req.WriteByte(1);
				else if (DestinationAddress.AddressFamily == AddressFamily.InterNetworkV6)
					Req.WriteByte(4);
				else
					throw new ArgumentException("Invalid address family.", nameof(DestinationAddress));

				byte[] Addr = DestinationAddress.GetAddressBytes();
				Req.Write(Addr, 0, Addr.Length);
				Req.WriteByte((byte)(Port >> 8));
				Req.WriteByte((byte)Port);

				this.SendPacket(Req.ToArray());
			}
		}

		private void Request(Command Command, string DestinationDomainName, int Port)
		{
			using (MemoryStream Req = new MemoryStream())
			{
				Req.WriteByte(5);
				Req.WriteByte((byte)Command);
				Req.WriteByte(0);
				Req.WriteByte(3);

				byte[] Bytes = Encoding.ASCII.GetBytes(DestinationDomainName);
				int c = Bytes.Length;
				if (c > 255)
					throw new IOException("Domain name too long.");

				Req.WriteByte((byte)c);
				Req.Write(Bytes, 0, Bytes.Length);
				Req.WriteByte((byte)(Port >> 8));
				Req.WriteByte((byte)Port);

				this.SendPacket(Req.ToArray());
			}
		}

		/// <summary>
		/// Connects to the target.
		/// </summary>
		/// <param name="DestinationAddress">Destination Address. Must be a IPv4 or IPv6 address.</param>
		/// <param name="Port">Port number.</param>
		/// <exception cref="IOException">If client not connected (yet).</exception>
		public void CONNECT(IPAddress DestinationAddress, int Port)
		{
			this.Request(Command.CONNECT, DestinationAddress, Port);
		}

		/// <summary>
		/// Connects to the target.
		/// </summary>
		/// <param name="DestinationDomainName">Destination Domain Name.</param>
		/// <param name="Port">Port number.</param>
		/// <exception cref="IOException">If client not connected (yet).</exception>
		public void CONNECT(string DestinationDomainName, int Port)
		{
			this.Request(Command.CONNECT, DestinationDomainName, Port);
		}

		/// <summary>
		/// XMPP-specific SOCKS5 connection, as described in XEP-0065:
		/// https://xmpp.org/extensions/xep-0065.html
		/// </summary>
		/// <param name="StreamID">Stream ID</param>
		/// <param name="RequesterJID">Requester JID</param>
		/// <param name="TargetJID">Target JID</param>
		public void CONNECT(string StreamID, string RequesterJID, string TargetJID)
		{
			string s = StreamID + RequesterJID + TargetJID;
			byte[] Hash = Hashes.ComputeSHA1Hash(Encoding.UTF8.GetBytes(s));
			StringBuilder sb = new StringBuilder();

			foreach (byte b in Hash)
				sb.Append(b.ToString("x2"));

			this.CONNECT(sb.ToString(), 0);
		}

		/// <summary>
		/// Binds to the target.
		/// </summary>
		/// <param name="DestinationAddress">Destination Address. Must be a IPv4 or IPv6 address.</param>
		/// <param name="Port">Port number.</param>
		/// <exception cref="IOException">If client not connected (yet).</exception>
		public void BIND(IPAddress DestinationAddress, int Port)
		{
			this.Request(Command.BIND, DestinationAddress, Port);
		}

		/// <summary>
		/// Binds to the target.
		/// </summary>
		/// <param name="DestinationDomainName">Destination Domain Name.</param>
		/// <param name="Port">Port number.</param>
		/// <exception cref="IOException">If client not connected (yet).</exception>
		public void BIND(string DestinationDomainName, int Port)
		{
			this.Request(Command.BIND, DestinationDomainName, Port);
		}

		/// <summary>
		/// Establish an association within the UDP relay process.
		/// </summary>
		/// <param name="DestinationAddress">Destination Address. Must be a IPv4 or IPv6 address.</param>
		/// <param name="Port">Port number.</param>
		/// <exception cref="IOException">If client not connected (yet).</exception>
		public void UDP_ASSOCIATE(IPAddress DestinationAddress, int Port)
		{
			this.Request(Command.UDP_ASSOCIATE, DestinationAddress, Port);
		}

		/// <summary>
		/// Establish an association within the UDP relay process.
		/// </summary>
		/// <param name="DestinationDomainName">Destination Domain Name.</param>
		/// <param name="Port">Port number.</param>
		/// <exception cref="IOException">If client not connected (yet).</exception>
		public void UDP_ASSOCIATE(string DestinationDomainName, int Port)
		{
			this.Request(Command.UDP_ASSOCIATE, DestinationDomainName, Port);
		}


	}
}
