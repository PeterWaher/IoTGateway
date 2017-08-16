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
		private Cache<IPEndPoint, State> dtlsStates;
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

			this.dtlsStates = new Cache<IPEndPoint, State>(int.MaxValue, TimeSpan.MaxValue, new TimeSpan(1, 0, 0));
			this.dtlsStates.Removed += DtlsStates_Removed;
			this.dtls.OnApplicationDataReceived += Dtls_OnApplicationDataReceived;
			this.dtls.OnHandshakeFailed += Dtls_OnHandshakeFailed;
			this.dtls.OnHandshakeSuccessful += Dtls_OnHandshakeSuccessful;
			this.dtls.OnSessionFailed += Dtls_OnSessionFailed;
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
			if (this.dtls != null)
			{
				this.dtls.Dispose();
				this.dtls = null;
			}

			if (this.udp != null)
			{
				this.udp.Dispose();
				this.udp = null;
			}
		}

		private void Dtls_OnStateChanged(object Sender, StateChangedEventArgs e)
		{
			if (this.dtlsStates.TryGetValue((IPEndPoint)e.RemoteEndpoint, out State State))
				State.DtlsState = e.State;
		}

		private void Dtls_OnSessionFailed(object Sender, FailureEventArgs e)
		{
			if (this.dtlsStates.TryGetValue((IPEndPoint)e.RemoteEndpoint, out State State))
			{
				State.Queue.Clear();
				this.dtlsStates.Remove(State.RemoteEndpoint);
			}
		}

		private void Dtls_OnHandshakeSuccessful(object sender, RemoteEndpointEventArgs e)
		{
			if (this.dtlsStates.TryGetValue((IPEndPoint)e.RemoteEndpoint, out State State))
			{
				State.Queue.Clear();
				this.dtlsStates.Remove(State.RemoteEndpoint);
			}
		}

		private void Dtls_OnHandshakeFailed(object Sender, FailureEventArgs e)
		{
			if (this.dtlsStates.TryGetValue((IPEndPoint)e.RemoteEndpoint, out State State))
			{
				State.Queue.Clear();
				this.dtlsStates.Remove(State.RemoteEndpoint);
			}
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

		private void DtlsStates_Removed(object Sender, CacheItemEventArgs<IPEndPoint, State> e)
		{
			if (e.Value.DtlsState == DtlsState.SessionEstablished ||
				e.Value.DtlsState == DtlsState.Handshake)
			{
				this.dtls.CloseSession(e.Value.RemoteEndpoint);
			}
		}

		/// <summary>
		/// Sends a packet to a remote endpoint.
		/// </summary>
		/// <param name="Packet">Packet to send.</param>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public void Send(byte[] Packet, IPEndPoint RemoteEndpoint)
		{
			if (this.dtlsStates.TryGetValue(RemoteEndpoint, out State State))
			{
				switch (State.DtlsState)
				{
					case DtlsState.SessionEstablished:
						this.dtls.SendApplicationData(Packet, RemoteEndpoint);
						break;

					case DtlsState.Handshake:
						lock (State.Queue)
						{
							State.Queue.AddLast(Packet);
						}
						break;

					case DtlsState.Closed:
					case DtlsState.Failed:
					case DtlsState.Created:
					default:
						lock (State.Queue)
						{
							State.Queue.AddLast(Packet);
						}

						// TODO: Credentials
						this.dtls.StartHandshake(RemoteEndpoint);
						break;
				}
			}
			else
			{
				State = new State()
				{
					RemoteEndpoint = RemoteEndpoint,
					Queue = new LinkedList<byte[]>(),
					DtlsState = DtlsState.Handshake
				};

				State.Queue.AddLast(Packet);

				// TODO: Credentials
				this.dtls.StartHandshake(RemoteEndpoint);
			}
		}

		private class State
		{
			public IPEndPoint RemoteEndpoint;
			public LinkedList<byte[]> Queue;
			public DtlsState DtlsState;
		}

	}
}
