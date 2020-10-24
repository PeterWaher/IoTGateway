using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Waher.Events;
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

			this.dtlsStates = new Cache<IPEndPoint, DtlsOverUdpState>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromHours(1));
			this.dtlsStates.Removed += DtlsStates_Removed;
			this.dtls.OnApplicationDataReceived += Dtls_OnApplicationDataReceived;
			this.dtls.OnHandshakeFailed += Dtls_OnHandshakeFailed;
			this.dtls.OnHandshakeSuccessful += Dtls_OnHandshakeSuccessful;
			this.dtls.OnSessionFailed += Dtls_OnSessionFailed;
			this.dtls.OnIncomingHandshakeStarted += Dtls_OnIncomingHandshakeStarted;
			this.dtls.OnStateChanged += Dtls_OnStateChanged;
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
		public DtlsEndpoint DTLS
		{
			get { return this.dtls; }
		}

		/// <summary>
		/// Tag owner can use to associate object with another.
		/// </summary>
		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
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

		private void Dtls_OnStateChanged(object Sender, StateChangedEventArgs e)
		{
			if (this.dtlsStates.TryGetValue((IPEndPoint)e.RemoteEndpoint, out DtlsOverUdpState State))
				State.CurrentState = e.State;
		}

		private void Dtls_OnIncomingHandshakeStarted(object Sender, RemoteEndpointEventArgs e)
		{
			IPEndPoint EP = (IPEndPoint)e.RemoteEndpoint;

			if (!this.dtlsStates.ContainsKey(EP))
			{
				DtlsOverUdpState State = new DtlsOverUdpState()
				{
					RemoteEndpoint = EP,
					Queue = new LinkedList<Tuple<byte[], UdpTransmissionEventHandler, object>>(),
					CurrentState = DtlsState.Handshake
				};

				this.dtlsStates.Add(EP, State);
			}
		}

		private void Dtls_OnSessionFailed(object Sender, FailureEventArgs e)
		{
			IPEndPoint EP = (IPEndPoint)e.RemoteEndpoint;

			if (this.dtlsStates.TryGetValue(EP, out DtlsOverUdpState State))
			{
				this.dtlsStates.Remove(State.RemoteEndpoint);
				State.Done(this, false);
			}
		}

		private void Dtls_OnHandshakeSuccessful(object sender, RemoteEndpointEventArgs e)
		{
			IPEndPoint EP = (IPEndPoint)e.RemoteEndpoint;

			if (this.dtlsStates.TryGetValue(EP, out DtlsOverUdpState State))
				State.Done(this, true);
			else
			{
				State = new DtlsOverUdpState()
				{
					RemoteEndpoint = EP,
					Queue = new LinkedList<Tuple<byte[], UdpTransmissionEventHandler, object>>(),
					CurrentState = DtlsState.SessionEstablished
				};

				this.dtlsStates.Add(EP, State);
			}
		}

		private void Dtls_OnHandshakeFailed(object Sender, FailureEventArgs e)
		{
			this.Dtls_OnSessionFailed(Sender, e);
		}

		private void Dtls_OnApplicationDataReceived(object Sender, ApplicationDataEventArgs e)
		{
			try
			{
				this.OnDatagramReceived?.Invoke(this, new UdpDatagramEventArgs(this,
					(IPEndPoint)e.RemoteEndpoint, e.ApplicationData));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when decrypted application data has been successfully been received
		/// over a DTLS session.
		/// </summary>
		public event UdpDatagramEventHandler OnDatagramReceived = null;

		private void DtlsStates_Removed(object Sender, CacheItemEventArgs<IPEndPoint, DtlsOverUdpState> e)
		{
			if (e.Value.CurrentState == Security.DTLS.DtlsState.SessionEstablished ||
				e.Value.CurrentState == Security.DTLS.DtlsState.Handshake)
			{
				this.dtls.CloseSession(e.Value.RemoteEndpoint);
			}
		}

		/// <summary>
		/// Sends a packet to a remote endpoint.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="Credentials">Optional credentials.</param>
		/// <param name="Callback">Method to call when operation concludes.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Send(byte[] Packet, IPEndPoint RemoteEndpoint, IDtlsCredentials Credentials,
			UdpTransmissionEventHandler Callback, object State)
		{
			if (this.dtlsStates.TryGetValue(RemoteEndpoint, out DtlsOverUdpState DtlsState))
			{
				switch (DtlsState.CurrentState)
				{
					case Security.DTLS.DtlsState.SessionEstablished:
						this.dtls.SendApplicationData(Packet, RemoteEndpoint);
						break;

					case Security.DTLS.DtlsState.Handshake:
						DtlsState.AddToQueue(Packet, Callback, State);
						break;

					case Security.DTLS.DtlsState.Closed:
					case Security.DTLS.DtlsState.Failed:
					case Security.DTLS.DtlsState.Created:
					default:
						DtlsState.AddToQueue(Packet, Callback, State);
						this.dtls.StartHandshake(RemoteEndpoint, Credentials);
						break;
				}
			}
			else
			{
				DtlsState = new DtlsOverUdpState()
				{
					RemoteEndpoint = RemoteEndpoint,
					Queue = new LinkedList<Tuple<byte[], UdpTransmissionEventHandler, object>>(),
					CurrentState = Security.DTLS.DtlsState.Handshake
				};

				DtlsState.AddToQueue(Packet, Callback, State);
				this.dtlsStates.Add(RemoteEndpoint, DtlsState);

				this.dtls.StartHandshake(RemoteEndpoint, Credentials);
			}
		}


	}
}
