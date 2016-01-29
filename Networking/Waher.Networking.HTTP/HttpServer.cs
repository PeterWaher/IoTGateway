using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Waher.Events;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Implements a HTTP server.
	/// </summary>
	public class HttpServer : IDisposable
	{
		/// <summary>
		/// Default HTTP Port (80).
		/// </summary>
		public const int DefaultHttpPort = 80;

		/// <summary>
		/// Default HTTPS port (443).
		/// </summary>
		public const int DefaultHttpsPort = 443;

		/// <summary>
		/// Default Connection backlog (10).
		/// </summary>
		public const int DefaultConnectionBacklog = 10;

		/// <summary>
		/// Default buffer size (16384).
		/// </summary>
		public const int DefaultBufferSize = 16384;

		internal static readonly StringComparer caseInsensitiveComparer = StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, true);

		internal static Encoding iso_8859_1 = Encoding.GetEncoding("ISO-8859-1");

		private LinkedList<TcpListener> listeners = new LinkedList<TcpListener>();
		private Dictionary<string, HttpResource> resources = new Dictionary<string, HttpResource>(caseInsensitiveComparer);
		private X509Certificate serverCertificate;

		/// <summary>
		/// Implements a HTTPS server.
		/// </summary>
		public HttpServer()
			: this(new int[] { DefaultHttpPort }, null, null)
		{
		}

		/// <summary>
		/// Implements a HTTPS server.
		/// </summary>
		/// <param name="HttpPort">HTTP Port</param>
		public HttpServer(int HttpPort)
			: this(new int[] { HttpPort }, null, null)
		{
		}

		/// <summary>
		/// Implements a HTTPS server.
		/// </summary>
		/// <param name="ServerCertificate">Server certificate identifying the domain of the server.</param>
		public HttpServer(X509Certificate ServerCertificate)
			: this(new int[] { DefaultHttpPort }, new int[] { DefaultHttpsPort }, ServerCertificate)
		{
		}

		/// <summary>
		/// Implements a HTTPS server.
		/// </summary>
		/// <param name="HttpPort">HTTP Port</param>
		/// <param name="HttpsPort">HTTPS Port</param>
		/// <param name="ServerCertificate">Server certificate identifying the domain of the server.</param>
		public HttpServer(int HttpPort, int HttpsPort, X509Certificate ServerCertificate)
			: this(new int[] { HttpPort }, new int[] { HttpsPort }, ServerCertificate)
		{
		}

		/// <summary>
		/// Implements a HTTPS server.
		/// </summary>
		/// <param name="HttpPorts">HTTP Ports</param>
		/// <param name="HttpsPorts">HTTPS Ports</param>
		/// <param name="ServerCertificate">Server certificate identifying the domain of the server.</param>
		public HttpServer(int[] HttpPorts, int[] HttpsPorts, X509Certificate ServerCertificate)
		{
			TcpListener Listener;

			this.serverCertificate = ServerCertificate;

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
						if (HttpPorts != null)
						{
							foreach (int HttpPort in HttpPorts)
							{
								try
								{
									Listener = new TcpListener(UnicastAddress.Address, HttpPort);
									Listener.Start(DefaultConnectionBacklog);
									Listener.BeginAcceptTcpClient(this.AcceptTcpClientCallback, new object[] { Listener, false });
									this.listeners.AddLast(Listener);
								}
								catch (Exception ex)
								{
									Log.Critical(ex, UnicastAddress.Address.ToString());
								}
							}
						}

						if (HttpsPorts != null)
						{
							foreach (int HttpsPort in HttpsPorts)
							{
								try
								{
									Listener = new TcpListener(UnicastAddress.Address, HttpsPort);
									Listener.Start(DefaultConnectionBacklog);
									Listener.BeginAcceptTcpClient(this.AcceptTcpClientCallback, new object[] { Listener, true });
									this.listeners.AddLast(Listener);
								}
								catch (Exception ex)
								{
									Log.Critical(ex, UnicastAddress.Address.ToString());
								}
							}
						}
					}
				}
			}
		}

		private void AcceptTcpClientCallback(IAsyncResult ar)
		{
			object[] P = (object[])ar.AsyncState;
			TcpListener Listener = (TcpListener)P[0];
			bool Https = (bool)P[1];

			try
			{
				TcpClient Client = Listener.EndAcceptTcpClient(ar);

				if (Https)
				{
					SslStream SslStream = new SslStream(Client.GetStream());
					SslStream.BeginAuthenticateAsServer(this.serverCertificate, false, SslProtocols.Tls, true,
						this.AuthenticateAsServerCallback, new object[] { Client, SslStream });
				}
				else
				{
					NetworkStream Stream = Client.GetStream();
					HttpClientConnection Connection = new HttpClientConnection(this, Client, Stream, DefaultBufferSize);
				}
			}
			catch (SocketException)
			{
				// Ignore
			}
			catch (Exception ex)
			{
				if (this.listeners == null)
					return;

				Log.Critical(ex);
			}
		}

		private void AuthenticateAsServerCallback(IAsyncResult ar)
		{
			object[] P = (object[])ar.AsyncState;
			TcpClient Client = (TcpClient)P[0];
			SslStream SslStream = (SslStream)P[1];

			try
			{
				SslStream.EndAuthenticateAsServer(ar);

				HttpClientConnection Connection = new HttpClientConnection(this, Client, SslStream, DefaultBufferSize);
			}
			catch (SocketException)
			{
				Client.Close();
			}
			catch (Exception ex)
			{
				Client.Close();
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.listeners != null)
			{
				LinkedList<TcpListener> Listeners = this.listeners;
				this.listeners = null;

				foreach (TcpListener Listener in Listeners)
					Listener.Stop();
			}
		}

		/// <summary>
		/// Registers a resource with the server.
		/// </summary>
		/// <param name="Resource">Resource</param>
		/// <exception cref="Exception">If a resource with the same resource name is already registered.</exception>
		public void Register(HttpResource Resource)
		{
			lock (this.resources)
			{
				if (!this.resources.ContainsKey(Resource.ResourceName))
					this.resources[Resource.ResourceName] = Resource;
				else
					throw new Exception("Resource name already registered.");
			}
		}

		/// <summary>
		/// Registers a resource with the server.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public void Register(string ResourceName, HttpMethodHandler GET, params HttpAuthenticationScheme[] AuthenticationSchemes)
		{
			this.Register(ResourceName, GET, true, false, AuthenticationSchemes);
		}

		/// <summary>
		/// Registers a resource with the server.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Synchronous">If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public void Register(string ResourceName, HttpMethodHandler GET, bool Synchronous, params HttpAuthenticationScheme[] AuthenticationSchemes)
		{
			this.Register(ResourceName, GET, Synchronous, false, AuthenticationSchemes);
		}

		/// <summary>
		/// Registers a resource with the server.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="Synchronous">If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).</param>
		/// <param name="HandlesSubPaths">If sub-paths are handled.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public void Register(string ResourceName, HttpMethodHandler GET, bool Synchronous, bool HandlesSubPaths,
			params HttpAuthenticationScheme[] AuthenticationSchemes)
		{
			this.Register(new HttpGetDelegateResource(ResourceName, GET, Synchronous, HandlesSubPaths, AuthenticationSchemes));
		}

		/// <summary>
		/// Registers a resource with the server.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">PSOT method handler.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public void Register(string ResourceName, HttpMethodHandler GET, HttpMethodHandler POST, params HttpAuthenticationScheme[] AuthenticationSchemes)
		{
			this.Register(ResourceName, GET, POST, true, false, AuthenticationSchemes);
		}

		/// <summary>
		/// Registers a resource with the server.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">PSOT method handler.</param>
		/// <param name="Synchronous">If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public void Register(string ResourceName, HttpMethodHandler GET, HttpMethodHandler POST, bool Synchronous,
			params HttpAuthenticationScheme[] AuthenticationSchemes)
		{
			this.Register(ResourceName, GET, POST, Synchronous, false, AuthenticationSchemes);
		}

		/// <summary>
		/// Registers a resource with the server.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="GET">GET method handler.</param>
		/// <param name="POST">PSOT method handler.</param>
		/// <param name="Synchronous">If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).</param>
		/// <param name="HandlesSubPaths">If sub-paths are handled.</param>
		/// <param name="AuthenticationSchemes">Any authentication schemes used to authenticate users before access is granted.</param>
		public void Register(string ResourceName, HttpMethodHandler GET, HttpMethodHandler POST, bool Synchronous, bool HandlesSubPaths,
			params HttpAuthenticationScheme[] AuthenticationSchemes)
		{
			this.Register(new HttpGetPostDelegateResource(ResourceName, GET, POST, Synchronous, HandlesSubPaths, AuthenticationSchemes));
		}

		/// <summary>
		/// Unregisters a resource from the server.
		/// </summary>
		/// <param name="Resource">Resource to unregister.</param>
		/// <returns>If the resource was found and removed.</returns>
		public bool Unregister(HttpResource Resource)
		{
			lock (this.resources)
			{
				HttpResource Resource2;

				if (this.resources.TryGetValue(Resource.ResourceName, out Resource2) && Resource2 == Resource)
				{
					this.resources.Remove(Resource.ResourceName);
					return true;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Tries to get a resource from the server.
		/// </summary>
		/// <param name="ResourceName">Full resource name.</param>
		/// <param name="Resource">Resource matching the full resource name.</param>
		/// <param name="SubPath">Trailing end of full resource name, relative to the best resource that was found.</param>
		/// <returns>If a resource was found matching the full resource name.</returns>
		public bool TryGetResource(string ResourceName, out HttpResource Resource, out string SubPath)
		{
			int i;

			SubPath = string.Empty;

			lock (this.resources)
			{
				while (!string.IsNullOrEmpty(ResourceName))
				{
					if (this.resources.TryGetValue(ResourceName, out Resource))
					{
						if (Resource.HandlesSubPaths || string.IsNullOrEmpty(SubPath))
							return true;
					}

					i = ResourceName.LastIndexOf('/');
					if (i < 0)
						break;

					SubPath = ResourceName.Substring(i) + SubPath;
					ResourceName = ResourceName.Substring(0, i);
				}
			}

			Resource = null;

			return false;
		}

		// TODO: Web Service resources
	}
}
