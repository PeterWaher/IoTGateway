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

namespace Waher.Things.Ip.Model
{
	public class ProxyPort : Sniffable, IDisposable
	{
		private readonly LinkedList<TcpListener> tcpListeners = new LinkedList<TcpListener>();
		private readonly Dictionary<Guid, ProxyClientConncetion> connections = new Dictionary<Guid, ProxyClientConncetion>();
		private readonly IpHostPortProxy node;
		private readonly string host;
		private readonly int port;
		private readonly int listeningPort;
		private readonly bool tls;
		private readonly bool trustServer;
		private long nrBytesDownlink = 0;
		private long nrBytesUplink = 0;
		private bool closed = false;

		private ProxyPort(IpHostPortProxy Node, string Host, int Port, bool Tls, bool TrustServer, int ListeningPort)
		{
			this.node = Node;
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.trustServer = TrustServer;
			this.listeningPort = ListeningPort;
		}

		public static async Task<ProxyPort> Create(IpHostPortProxy Node, string Host, int Port, bool Tls, bool TrustServer, int ListeningPort)
		{
			ProxyPort Result = new ProxyPort(Node, Host, Port, Tls, TrustServer, ListeningPort);
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
							this.Information("Connection accepted from " + Client.Client.RemoteEndPoint.ToString() + ".");

							BinaryTcpClient Incoming = new BinaryTcpClient(Client);
							BinaryTcpClient Outgoing;

							Incoming.Bind(true);

							if (!Types.TryGetModuleParameter("X509", out object Obj) || !(Obj is X509Certificate Certificate))
								Certificate = null;

							try
							{
								Outgoing = new BinaryTcpClient();
								if (!await Outgoing.ConnectAsync(this.host, this.port, true))
								{
									Incoming.DisposeWhenDone();
									return;
								}

								if (this.tls)
									await Outgoing.UpgradeToTlsAsClient(Certificate, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, this.trustServer);
							}
							catch (Exception)
							{
								Incoming.DisposeWhenDone();
								return;
							}

							if (this.tls && !(Certificate is null))
							{
								await this.node.RemoveWarningAsync("NoCertificate");

								Task T = this.SwitchToTls(Incoming, Outgoing, Certificate);
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
							Log.Critical(ex);
						else
							break;  // Removed, for instance due to network change
					}
				}
			}
			catch (Exception ex)
			{
				if (this.closed || this.tcpListeners is null)
					return;

				Log.Critical(ex);
			}
		}

		private async Task SwitchToTls(BinaryTcpClient Incoming, BinaryTcpClient Outgoing, X509Certificate Certificate)
		{
			string RemoteIpEndpoint;
			EndPoint EP = Incoming.Client.Client.RemoteEndPoint;

			if (EP is IPEndPoint IpEP)
				RemoteIpEndpoint = IpEP.Address.ToString();
			else
				RemoteIpEndpoint = EP.ToString();

			if (Security.LoginMonitor.LoginAuditor.CanStartTls(RemoteIpEndpoint))
			{
				try
				{
					this.Information("Switching to TLS.");

					await Incoming.UpgradeToTlsAsServer(Certificate, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12);

					if (this.HasSniffers)
					{
						this.Information("TLS established.");

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
					await this.LoginFailure(ex, Incoming, RemoteIpEndpoint);
				}
				catch (Win32Exception ex)
				{
					if (ex is SocketException)
						Incoming.Dispose();
					else
						await this.LoginFailure(ex, Incoming, RemoteIpEndpoint);
				}
				catch (IOException)
				{
					Incoming.Dispose();
				}
				catch (Exception ex)
				{
					Incoming.Dispose();
					Log.Critical(ex);
				}
			}
			else
				Incoming.Dispose();
		}

		private async Task LoginFailure(Exception ex, BinaryTcpClient Client, string RemoteIpEndpoint)
		{
			Exception ex2 = Log.UnnestException(ex);
			await Security.LoginMonitor.LoginAuditor.ReportTlsHackAttempt(RemoteIpEndpoint, "TLS handshake failed: " + ex2.Message, "PROXY");

			Client.Dispose();
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

		public void Dispose()
		{
			this.Close();
		}

		public void Remove(ProxyClientConncetion Connection)
		{
			lock (this.connections)
			{
				this.connections.Remove(Connection.Id);
			}

			Connection.Dispose();
		}

		public void IncUplink(int NrBytes)
		{
			this.nrBytesUplink += NrBytes;
		}

		public void IncDownlink(int NrBytes)
		{
			this.nrBytesDownlink += NrBytes;
		}

		public long NrBytesUplink => this.nrBytesUplink;
		public long NrBytesDownlink => this.nrBytesDownlink;

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