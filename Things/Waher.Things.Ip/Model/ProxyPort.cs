using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Security;
using Waher.Security.LoginMonitor;
using Waher.Security.Users;

namespace Waher.Things.Ip.Model
{
	/// <summary>
	/// Node acting as a TCP/IP proxy opening a port for incoming communication and proxying it to another port on a remote machine .
	/// </summary>
	public class ProxyPort : CommunicationLayer, IDisposable
	{
		private readonly LinkedList<TcpListener> tcpListeners = new LinkedList<TcpListener>();
		private readonly Dictionary<Guid, ProxyClientConncetion> connections = new Dictionary<Guid, ProxyClientConncetion>();
		private readonly IpCidr[] remoteIps;
		private readonly IpHostPortProxy node;
		private readonly string host;
		private readonly int port;
		private readonly int listeningPort;
		private readonly bool tls;
		private readonly bool trustServer;
		private readonly bool authorizedAccess;
		private long nrBytesDownlink = 0;
		private long nrBytesUplink = 0;
		private bool closed = false;

		private ProxyPort(IpHostPortProxy Node, string Host, int Port, bool Tls, bool TrustServer, int ListeningPort, bool AuthorizedAccess,
			IpCidr[] RemoteIps, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.node = Node;
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.trustServer = TrustServer;
			this.listeningPort = ListeningPort;
			this.authorizedAccess = AuthorizedAccess;
			this.remoteIps = RemoteIps;
		}

		/// <summary>
		/// Creates a port proxy object.
		/// </summary>
		/// <param name="Node">Proxy node.</param>
		/// <param name="Host">Host</param>
		/// <param name="Port">Port number</param>
		/// <param name="Tls">If TLS should be used</param>
		/// <param name="TrustServer">If server should be trusted</param>
		/// <param name="ListeningPort">Listening port</param>
		/// <param name="AuthorizedAccess">If authorized access is required.</param>
		/// <param name="RemoteIps">Permitted remote IP ranges.</param>
		/// <returns>Proxy port object.</returns>
		public static async Task<ProxyPort> Create(IpHostPortProxy Node, string Host, int Port, bool Tls, bool TrustServer, int ListeningPort,
			bool AuthorizedAccess, IpCidr[] RemoteIps)
		{
			ProxyPort Result = new ProxyPort(Node, Host, Port, Tls, TrustServer, ListeningPort, AuthorizedAccess, RemoteIps);
			await Result.Open();
			return Result;
		}

		private async Task Open()
		{
			foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (Interface.OperationalStatus != OperationalStatus.Up)
					continue;

				IPInterfaceProperties Properties = Interface.GetIPProperties();

				foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
				{
					if ((UnicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4) ||
						(UnicastAddress.Address.AddressFamily == AddressFamily.InterNetworkV6 && Socket.OSSupportsIPv6))
					{
						IPEndPoint DesiredEndpoint = new IPEndPoint(UnicastAddress.Address, this.listeningPort);

						try
						{
							TcpListener Listener = new TcpListener(UnicastAddress.Address, this.listeningPort);

							Listener.Start();
							Task T = this.ListenForIncomingConnections(Listener);

							lock (this.tcpListeners)
							{
								this.tcpListeners.AddLast(Listener);
							}

							await this.node.RemoveErrorAsync(DesiredEndpoint.ToString());
						}
						catch (SocketException)
						{
							await this.node.LogErrorAsync(DesiredEndpoint.ToString(), "Unable to open Proxy port for listening.");
						}
						catch (Exception ex)
						{
							await this.node.LogErrorAsync(DesiredEndpoint.ToString(), ex.Message);
						}
					}
				}
			}
		}

		private async Task ListenForIncomingConnections(TcpListener Listener)
		{
			try
			{
				while (!this.closed)
				{
					try
					{
						TcpClient Client;

						try
						{
							Client = await Listener.AcceptTcpClientAsync();
							if (this.closed)
								return;
						}
						catch (InvalidOperationException)
						{
							lock (this.tcpListeners)
							{
								LinkedListNode<TcpListener> Node = this.tcpListeners?.First;

								while (!(Node is null))
								{
									if (Node.Value == Listener)
									{
										this.tcpListeners.Remove(Node);
										break;
									}

									Node = Node.Next;
								}
							}

							return;
						}

						if (!(Client is null))
						{
							if (!(this.remoteIps is null))
							{
								bool Match = false;

								if (Client.Client.RemoteEndPoint is IPEndPoint IPEndPoint)
								{
									foreach (IpCidr Range in this.remoteIps)
									{
										if (Range.Matches(IPEndPoint.Address))
										{
											Match = true;
											break;
										}
									}
								}

								if (!Match)
								{
									this.Error("Remote IP not approved. Conncetion reused.");
									Client.Dispose();
									continue;
								}
							}

							BinaryTcpClient Incoming = new BinaryTcpClient(Client, false);
							BinaryTcpClient Outgoing = null;

							Incoming.Bind(true);

							this.Information("Connection accepted from " + Incoming.RemoteEndPoint + ".");


							if (!Types.TryGetModuleParameter("X509", out object Obj) || !(Obj is X509Certificate Certificate))
								Certificate = null;

							try
							{
								Outgoing = new BinaryTcpClient(false);
								if (!await Outgoing.ConnectAsync(this.host, this.port, true))
								{
									await this.node.LogErrorAsync("UnableToConnect", "Unable to connect to remote endpoint.");
									Incoming.DisposeWhenDone();
									continue;
								}

								if (this.tls)
									await Outgoing.UpgradeToTlsAsClient(Certificate, Crypto.TlsOnly, this.trustServer);
							}
							catch (Exception ex)
							{
								await this.node.LogErrorAsync("UnableToConnect", "Unable to connect to remote endpoint: " + ex.Message);
								this.Exception(ex);
								Incoming.DisposeWhenDone();

								if (!(Outgoing is null))
									await Outgoing.DisposeAsync();

								continue;
							}

							await this.node.RemoveErrorAsync("UnableToConnect");

							if ((this.tls || this.authorizedAccess) && !(Certificate is null))
							{
								await this.node.RemoveWarningAsync("NoCertificate");

								Task _ = this.SwitchToTls(Incoming, Outgoing, Certificate);
							}
							else
							{
								if (this.tls)
									await this.node.LogWarningAsync("NoCertificate", "No registered certificate found. Listening port is unencrypted.");

								ProxyClientConncetion Connection = new ProxyClientConncetion(this, Incoming, Outgoing, this.Sniffers);
								Outgoing.Continue();
								Incoming.Continue();

								lock (this.connections)
								{
									this.connections[Connection.Id] = Connection;
								}
							}
						}
					}
					catch (SocketException)
					{
						// Ignore
					}
					catch (ObjectDisposedException)
					{
						// Ignore
					}
					catch (NullReferenceException)
					{
						// Ignore
					}
					catch (Exception ex)
					{
						if (this.closed || this.tcpListeners is null)
							break;

						bool Found = false;

						foreach (TcpListener P in this.tcpListeners)
						{
							if (P == Listener)
							{
								Found = true;
								break;
							}
						}

						if (Found)
							Log.Exception(ex);
						else
							break;  // Removed, for instance due to network change
					}
				}
			}
			catch (Exception ex)
			{
				if (this.closed || this.tcpListeners is null)
					return;

				Log.Exception(ex);
			}
		}

		private async Task SwitchToTls(BinaryTcpClient Incoming, BinaryTcpClient Outgoing, X509Certificate Certificate)
		{
			string RemoteEndpoint = Incoming.RemoteEndPoint.RemovePortNumber();

			if (LoginAuditor.CanStartTls(RemoteEndpoint))
			{
				try
				{
					this.Information("Switching to TLS.");

					await Incoming.UpgradeToTlsAsServer(Certificate, Crypto.SecureTls, ClientCertificates.Optional);

					if (this.authorizedAccess)
					{
						if (Incoming.RemoteCertificate is null)
						{
							this.Error("No remote certificate found. mTLS is required.");
							await Incoming.DisposeAsync();
							await Outgoing.DisposeAsync();
							return;
						}

						if (!Incoming.RemoteCertificateValid)
						{
							this.Error("Remote certificate not valid.");
							await Incoming.DisposeAsync();
							await Outgoing.DisposeAsync();
							return;
						}

						string[] Identities = IpHostPortProxy.GetCertificateIdentities(Incoming.RemoteCertificate);
						User User = null;

						foreach (string Identity in IpHostPortProxy.GetCertificateIdentities(Certificate))
						{
							User = await Users.GetUser(Identity, false);
							if (!(User is null))
								break;
						}

						string RemoteEndPoint = Incoming.RemoteEndPoint.RemovePortNumber();

						if (User is null)
						{
							string Msg = "Invalid login: No user found matching certificate subject.";
							LoginAuditor.Fail(Msg, User.UserName, RemoteEndPoint, "PROXY");
							this.Error(Msg);
							await Incoming.DisposeAsync();
							await Outgoing.DisposeAsync();
							return;
						}
						else
							LoginAuditor.Success("Successful login using remote certificate.", User.UserName, RemoteEndPoint, "PROXY");
					}

					if (this.HasSniffers)
					{
						this.Information("TLS established" +
							". Cipher Strength: " + Incoming.CipherStrength.ToString() +
							", Hash Strength: " + Incoming.HashStrength.ToString() +
							", Key Exchange Strength: " + Incoming.KeyExchangeStrength.ToString());

						if (!(Incoming.RemoteCertificate is null))
						{
							if (this.HasSniffers)
							{
								StringBuilder sb = new StringBuilder();

								sb.Append("Remote Certificate received. Valid: ");
								sb.Append(Incoming.RemoteCertificateValid.ToString());
								sb.Append(", Subject: ");
								sb.Append(Incoming.RemoteCertificate.Subject);
								sb.Append(", Issuer: ");
								sb.Append(Incoming.RemoteCertificate.Issuer);
								sb.Append(", S/N: ");
								sb.Append(Convert.ToBase64String(Incoming.RemoteCertificate.GetSerialNumber()));
								sb.Append(", Hash: ");
								sb.Append(Convert.ToBase64String(Incoming.RemoteCertificate.GetCertHash()));

								this.Information(sb.ToString());
							}
						}
					}

					ProxyClientConncetion Connection = new ProxyClientConncetion(this, Incoming, Outgoing, this.Sniffers);
					Outgoing.Continue();
					Incoming.Continue();

					lock (this.connections)
					{
						this.connections[Connection.Id] = Connection;
					}
				}
				catch (AuthenticationException ex)
				{
					await this.LoginFailure(ex, Incoming, Outgoing, RemoteEndpoint);
				}
				catch (Win32Exception ex)
				{
					if (ex is SocketException)
					{
						await Incoming.DisposeAsync();
						await Outgoing.DisposeAsync();
					}
					else
						await this.LoginFailure(ex, Incoming, Outgoing, RemoteEndpoint);
				}
				catch (IOException)
				{
					await Incoming.DisposeAsync();
					await Outgoing.DisposeAsync();
				}
				catch (Exception ex)
				{
					await Incoming.DisposeAsync();
					await Outgoing.DisposeAsync();
					Log.Exception(ex);
				}
			}
			else
			{
				await Incoming.DisposeAsync();
				await Outgoing.DisposeAsync();
			}
		}

		private async Task LoginFailure(Exception ex, BinaryTcpClient Incoming, BinaryTcpClient Outgoing, string RemoteIpEndpoint)
		{
			Exception ex2 = Log.UnnestException(ex);
			await LoginAuditor.ReportTlsHackAttempt(RemoteIpEndpoint, "TLS handshake failed: " + ex2.Message, "PROXY");

			await Incoming.DisposeAsync();
			await Outgoing.DisposeAsync();
		}

		private void Close()
		{
			TcpListener[] Listeners;
			ProxyClientConncetion[] Connections;

			this.closed = true;

			lock (this.tcpListeners)
			{
				Listeners = new TcpListener[this.tcpListeners.Count];
				this.tcpListeners.CopyTo(Listeners, 0);
				this.tcpListeners.Clear();
			}

			lock (this.connections)
			{
				Connections = new ProxyClientConncetion[this.connections.Count];
				this.connections.Values.CopyTo(Connections, 0);
				this.connections.Clear();
			}

			foreach (TcpListener Listener in Listeners)
			{
				try
				{
					Listener.Stop();
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			foreach (ProxyClientConncetion Connection in Connections)
			{
				try
				{
					Connection.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.Close();
		}

		/// <summary>
		/// Removes a proxy client connection.
		/// </summary>
		/// <param name="Connection">Connection to remove.</param>
		public void Remove(ProxyClientConncetion Connection)
		{
			lock (this.connections)
			{
				this.connections.Remove(Connection.Id);
			}

			Connection.Dispose();
		}

		/// <summary>
		/// Increment uplink counter.
		/// </summary>
		/// <param name="NrBytes">Number of bytes</param>
		public void IncUplink(int NrBytes)
		{
			this.nrBytesUplink += NrBytes;
		}

		/// <summary>
		/// Increment downlink counter.
		/// </summary>
		/// <param name="NrBytes">Number of bytes</param>
		public void IncDownlink(int NrBytes)
		{
			this.nrBytesDownlink += NrBytes;
		}

		/// <summary>
		/// Number of bytes send uplink
		/// </summary>
		public long NrBytesUplink => this.nrBytesUplink;

		/// <summary>
		/// Number of bytes send downlink
		/// </summary>
		public long NrBytesDownlink => this.nrBytesDownlink;

		/// <summary>
		/// Number of connections.
		/// </summary>
		public int NrConnctions
		{
			get
			{
				lock (this.connections)
				{
					return this.connections.Count;
				}
			}
		}

	}
}