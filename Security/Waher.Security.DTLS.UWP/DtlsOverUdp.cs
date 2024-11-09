using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Security.DTLS.Events;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Class managing DTLS over UDP.
	/// </summary>
	public class DtlsOverUdp : IDisposable
	{
		private readonly Cache<IPEndPoint, DtlsOverUdpState> dtlsStates;
		private UdpCommunicationLayer udp;
		private DtlsEndpoint dtls;
		private object tag = null;

		/// <summary>
		/// Class managing DTLS over UDP.
		/// </summary>
		/// <param name="UdpClient">UDP connection.</param>
		/// <param name="Mode">DTLS mode.</param>
		/// <param name="Users">User data source, if pre-shared keys should be allowed by a DTLS server endpoint.</param>
		/// <param name="RequiredPrivilege">Required privilege, for the user to be acceptable
		/// in PSK handshakes.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public DtlsOverUdp(UdpClient UdpClient, DtlsMode Mode, IUserSource Users,
			string RequiredPrivilege, params ISniffer[] Sniffers)
		{
			this.udp = new UdpCommunicationLayer(UdpClient);
			this.dtls = new DtlsEndpoint(Mode, this.udp, Users, RequiredPrivilege, Sniffers);

			this.dtlsStates = new Cache<IPEndPoint, DtlsOverUdpState>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromHours(1), true);
			this.dtlsStates.Removed += this.DtlsStates_Removed;
			this.dtls.OnApplicationDataReceived += this.Dtls_OnApplicationDataReceived;
			this.dtls.OnHandshakeFailed += this.Dtls_OnHandshakeFailed;
			this.dtls.OnHandshakeSuccessful += this.Dtls_OnHandshakeSuccessful;
			this.dtls.OnSessionFailed += this.Dtls_OnSessionFailed;
			this.dtls.OnIncomingHandshakeStarted += this.Dtls_OnIncomingHandshakeStarted;
			this.dtls.OnStateChanged += this.Dtls_OnStateChanged;
		}

		/// <summary>
		/// Underlying UDP client.
		/// </summary>
		public UdpClient Client
		{
			get { return this.udp.Client; }
		}

		/// <summary>
		/// DTLS endpoint.
		/// </summary>
		public DtlsEndpoint DTLS => this.dtls;

		/// <summary>
		/// Tag owner can use to associate object with another.
		/// </summary>
		public object Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!(this.dtls is null))
			{
				this.dtls.Dispose();
				this.dtls = null;
			}

			if (!(this.udp is null))
			{
				this.udp.Dispose();
				this.udp = null;
			}
		}

		private Task Dtls_OnStateChanged(object Sender, StateChangedEventArgs e)
		{
			if (this.dtlsStates.TryGetValue((IPEndPoint)e.RemoteEndpoint, out DtlsOverUdpState State))
				State.CurrentState = e.State;
		
			return Task.CompletedTask;
		}

		private Task Dtls_OnIncomingHandshakeStarted(object Sender, RemoteEndpointEventArgs e)
		{
			IPEndPoint EP = (IPEndPoint)e.RemoteEndpoint;

			if (!this.dtlsStates.ContainsKey(EP))
			{
				DtlsOverUdpState State = new DtlsOverUdpState()
				{
					RemoteEndpoint = EP,
					Queue = new LinkedList<Tuple<byte[], EventHandlerAsync<UdpTransmissionEventArgs>, object>>(),
					CurrentState = DtlsState.Handshake
				};

				this.dtlsStates.Add(EP, State);
			}

			return Task.CompletedTask;
		}

		private async Task Dtls_OnSessionFailed(object Sender, FailureEventArgs e)
		{
			IPEndPoint EP = (IPEndPoint)e.RemoteEndpoint;

			if (this.dtlsStates.TryGetValue(EP, out DtlsOverUdpState State))
			{
				this.dtlsStates.Remove(State.RemoteEndpoint);
				await State.Done(this, false);
			}
		}

		private async Task Dtls_OnHandshakeSuccessful(object Sender, RemoteEndpointEventArgs e)
		{
			IPEndPoint EP = (IPEndPoint)e.RemoteEndpoint;

			if (this.dtlsStates.TryGetValue(EP, out DtlsOverUdpState State))
				await State.Done(this, true);
			else
			{
				State = new DtlsOverUdpState()
				{
					RemoteEndpoint = EP,
					Queue = new LinkedList<Tuple<byte[], EventHandlerAsync<UdpTransmissionEventArgs>, object>>(),
					CurrentState = DtlsState.SessionEstablished
				};

				this.dtlsStates.Add(EP, State);
			}
		}

		private Task Dtls_OnHandshakeFailed(object Sender, FailureEventArgs e)
		{
			return this.Dtls_OnSessionFailed(Sender, e);
		}

		private Task Dtls_OnApplicationDataReceived(object Sender, ApplicationDataEventArgs e)
		{
			return this.OnDatagramReceived.Raise(this, new UdpDatagramEventArgs(this, (IPEndPoint)e.RemoteEndpoint, e.ApplicationData));
		}

		/// <summary>
		/// Event raised when decrypted application data has been successfully been received
		/// over a DTLS session.
		/// </summary>
		public event EventHandlerAsync<UdpDatagramEventArgs> OnDatagramReceived = null;

		private Task DtlsStates_Removed(object Sender, CacheItemEventArgs<IPEndPoint, DtlsOverUdpState> e)
		{
			if (e.Value.CurrentState == DtlsState.SessionEstablished ||
				e.Value.CurrentState == DtlsState.Handshake)
			{
				this.dtls.CloseSession(e.Value.RemoteEndpoint);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Sends a packet to a remote endpoint.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Method to call when operation concludes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public async Task Send(byte[] Packet, IPEndPoint RemoteEndpoint, IDtlsCredentials Credentials,
			EventHandlerAsync<UdpTransmissionEventArgs> Callback, object State)
		{
			if (this.dtlsStates.TryGetValue(RemoteEndpoint, out DtlsOverUdpState DtlsState))
			{
				switch (DtlsState.CurrentState)
				{
					case Security.DTLS.DtlsState.SessionEstablished:
						await this.dtls.SendApplicationData(Packet, RemoteEndpoint);
						break;

					case Security.DTLS.DtlsState.Handshake:
						DtlsState.AddToQueue(Packet, Callback, State);
						break;

					case Security.DTLS.DtlsState.Closed:
					case Security.DTLS.DtlsState.Failed:
					case Security.DTLS.DtlsState.Created:
					default:
						DtlsState.AddToQueue(Packet, Callback, State);
						await this.dtls.StartHandshake(RemoteEndpoint, Credentials);
						break;
				}
			}
			else
			{
				DtlsState = new DtlsOverUdpState()
				{
					RemoteEndpoint = RemoteEndpoint,
					Queue = new LinkedList<Tuple<byte[], EventHandlerAsync<UdpTransmissionEventArgs>, object>>(),
					CurrentState = Security.DTLS.DtlsState.Handshake
				};

				DtlsState.AddToQueue(Packet, Callback, State);
				this.dtlsStates.Add(RemoteEndpoint, DtlsState);

				await this.dtls.StartHandshake(RemoteEndpoint, Credentials);
			}
		}


	}
}
