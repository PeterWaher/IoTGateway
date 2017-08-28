using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Waher.Events;
using Waher.Networking.CoAP.ContentFormats;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Networking.CoAP.Options;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Class implementing an LWM2M client, as defined in:
	/// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf
	/// </summary>
	public class Lwm2mClient : CoapResource, IDisposable, ICoapDeleteMethod
	{
		private SortedDictionary<int, Lwm2mObject> objects = new SortedDictionary<int, Lwm2mObject>();
		private CoapEndpoint coapEndpoint;
		private Lwm2mServerReference[] serverReferences;
		private Lwm2mServerReference bootstrapSever;
		private IPEndPoint[] bootstrapSeverIp = null;
		private Lwm2mState state = Lwm2mState.Deregistered;
		private BootstreapResource bsResource;
		private string clientName;
		private string lastLinks = string.Empty;
		private int lifetimeSeconds = 0;
		private int registrationEpoch = 0;

		/// <summary>
		/// Class implementing an LWM2M client, as defined in:
		/// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf
		/// </summary>
		/// <param name="ClientName">Client name.</param>
		/// <param name="CoapEndpoint">CoAP endpoint to use for LWM2M communication.</param>
		/// <param name="Objects">Objects</param>
		public Lwm2mClient(string ClientName, CoapEndpoint CoapEndpoint,
			params Lwm2mObject[] Objects)
			: base("/")
		{
			this.coapEndpoint = CoapEndpoint;
			this.serverReferences = ServerReferences;
			this.clientName = ClientName;
			this.state = Lwm2mState.Bootstrap;
			this.bsResource = new BootstreapResource(this);

			this.coapEndpoint.Register(this);
			this.coapEndpoint.Register(this.bsResource);

			foreach (Lwm2mObject Object in Objects)
			{
				if (Object.Id < 0)
					throw new ArgumentException("Invalid object ID.", nameof(Object));

				if (this.objects.ContainsKey(Object.Id))
					throw new ArgumentException("An object with ID " + Object.Id + " already is registered.", nameof(Object));

				this.objects[Object.Id] = Object;
				Object.Client = this;

				this.coapEndpoint.Register(Object);

				foreach (Lwm2mObjectInstance Instance in Object.Instances)
					this.coapEndpoint.Register(Instance);
			}
		}

		/// <summary>
		/// Optional title of resource.
		/// </summary>
		public override string Title => this.clientName;

		/// <summary>
		/// Optional array of supported content formats.
		/// </summary>
		public override int[] ContentFormats => new int[] { Tlv.ContentFormatCode };

		/// <summary>
		/// State of LWM2M client.
		/// </summary>
		public Lwm2mState State
		{
			get { return this.state; }
			internal set
			{
				if (this.state != value)
				{
					this.state = value;

					try
					{
						this.OnStateChanged?.Invoke(this, new EventArgs());
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when the LWM2M state changes.
		/// </summary>
		public event EventHandler OnStateChanged = null;

		/// <summary>
		/// Registered objects.
		/// </summary>
		public Lwm2mObject[] Objects
		{
			get
			{
				Lwm2mObject[] Result;

				Result = new Lwm2mObject[this.objects.Count];
				this.objects.Values.CopyTo(Result, 0);

				return Result;
			}
		}

		/// <summary>
		/// If the client has objects registered on it.
		/// </summary>
		public bool HasObjects
		{
			get
			{
				return this.objects.Count > 0;
			}
		}

		/// <summary>
		/// Current CoAP endpoint.
		/// </summary>
		public CoapEndpoint CoapEndpoint
		{
			get { return this.coapEndpoint; }
		}

		/// <summary>
		/// Server references.
		/// </summary>
		public Lwm2mServerReference[] ServerReferences
		{
			get { return this.serverReferences; }
		}

		/// <summary>
		/// Client name.
		/// </summary>
		public string ClientName
		{
			get { return this.clientName; }
		}

		#region Bootstrap

		/// <summary>
		/// Sends a BOOTSTRAP-REQUEST to the current LWM2M Bootstram Server, to request the servers 
		/// initialize bootstrapping. When bootstrapping is completed, server registration is 
		/// performed to the servers reported during bootstrapping.
		/// </summary>
		/// <returns>If a bootstrap server was found, and request initiated.</returns>
		public Task<bool> RequestBootstrap()
		{
			return this.RequestBootstrap(null, null);
		}

		/// <summary>
		/// Sends a BOOTSTRAP-REQUEST to the current LWM2M Bootstram Server, to request the servers 
		/// initialize bootstrapping. When bootstrapping is completed, server registration is 
		/// performed to the servers reported during bootstrapping.
		/// </summary>
		/// <param name="Callback">Callback method when bootstrap request has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If a bootstrap server was found, and request initiated.</returns>
		public async Task<bool> RequestBootstrap(CoapResponseEventHandler Callback, object State)
		{
			foreach (Lwm2mObject Object in this.objects.Values)
			{
				if (Object is Lwm2mSecurityObject SecurityObject)
				{
					foreach (Lwm2mSecurityObjectInstance Instance in SecurityObject.Instances)
					{
						Lwm2mServerReference Ref = this.GetServerReference(Instance, true);
						if (Ref != null)
						{
							if (Instance.ClientHoldOffTimeSeconds.HasValue &&
								Instance.ClientHoldOffTimeSeconds.Value > 0)
							{
								this.coapEndpoint.ScheduleEvent(
									async (P) => await this.RequestBootstrap((Lwm2mServerReference)P, Callback, State),
									DateTime.Now.AddSeconds(Instance.ClientHoldOffTimeSeconds.Value),
									Ref);
							}
							else
								await this.RequestBootstrap(Ref, Callback, State);

							return true;
						}
					}
				}
			}

			return false;
		}

		private Lwm2mServerReference GetServerReference(Lwm2mSecurityObjectInstance Instance,
			bool BootstrapServer)
		{
			if (!Instance.BootstrapServer.HasValue ||
				(Instance.BootstrapServer.Value ^ BootstrapServer) ||
				string.IsNullOrEmpty(Instance.ServerUri))
			{
				return null;
			}

			try
			{
				Uri Uri = new Uri(Instance.ServerUri);
				string s = Uri.PathAndQuery;
				int Port;

				if (!string.IsNullOrEmpty(s) && s != "/")
					return null;

				s = Uri.Scheme.ToLower();

				if (Uri.IsDefaultPort)
				{
					switch (s)
					{
						case "coap":
							Port = CoapEndpoint.DefaultCoapPort;
							break;

						case "coaps":
							Port = CoapEndpoint.DefaultCoapsPort;
							break;

						default:
							return null;
					}
				}
				else
					Port = Uri.Port;

				switch (Instance.SecurityMode)
				{
					case SecurityMode.NoSec:
						if (s != "coap")
							return null;

						return new Lwm2mServerReference(Uri.Host, Port);

					case SecurityMode.PSK:
						if (s != "coaps")
							return null;

						if (Instance.PublicKeyOrIdentity == null ||
							Instance.SecretKey == null)
						{
							return null;
						}

						PresharedKey Credentials = new PresharedKey(Instance.PublicKeyOrIdentity,
							Instance.SecretKey);

						return new Lwm2mServerReference(Uri.Host, Port, Credentials);

					default:
						return null;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Sends a BOOTSTRAP-REQUEST to the LWM2M Bootstram Server, to request the servers 
		/// initialize bootstrapping. When bootstrapping is completed, server registration is 
		/// performed to the servers reported during bootstrapping.
		/// </summary>
		/// <param name="BootstrapServer">Reference to the bootstrap server.</param>
		public Task RequestBootstrap(Lwm2mServerReference BootstrapServer)
		{
			return this.RequestBootstrap(BootstrapServer, null, null);
		}

		/// <summary>
		/// Sends a BOOTSTRAP-REQUEST to the LWM2M Bootstram Server, to request the servers 
		/// initialize bootstrapping. When bootstrapping is completed, server registration is 
		/// performed to the servers reported during bootstrapping.
		/// </summary>
		/// <param name="BootstrapServer">Reference to the bootstrap server.</param>
		/// <param name="Callback">Callback method when bootstrap request has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public async Task RequestBootstrap(Lwm2mServerReference BootstrapServer,
			CoapResponseEventHandler Callback, object State)
		{
			this.bootstrapSever = BootstrapServer;

			Uri BsUri = new Uri(this.bootstrapSever.Uri);
			int Port;

			if (BsUri.IsDefaultPort)
			{
				switch (BsUri.Scheme.ToLower())
				{
					case "coaps":
						Port = CoapEndpoint.DefaultCoapsPort;
						break;

					case "coap":
						Port = CoapEndpoint.DefaultCoapPort;
						break;

					default:
						throw new ArgumentException("Unrecognized URI scheme.", nameof(BootstrapServer));
				}
			}
			else
				Port = BsUri.Port;

			if (IPAddress.TryParse(BsUri.Host, out IPAddress Addr))
				this.bootstrapSeverIp = new IPEndPoint[] { new IPEndPoint(Addr, Port) };
			else
			{
				IPAddress[] Addresses = await Dns.GetHostAddressesAsync(BsUri.Host);
				int i, c = Addresses.Length;

				this.bootstrapSeverIp = new IPEndPoint[c];

				for (i = 0; i < c; i++)
					this.bootstrapSeverIp[i] = new IPEndPoint(Addresses[i], Port);
			}

			await this.coapEndpoint.POST(this.bootstrapSever.Uri + "bs?ep=" + this.clientName, true,
				null, 64, this.bootstrapSever.Credentials, this.BootstrapResponse, new object[] { Callback, State });
		}

		private void BootstrapResponse(object Sender, CoapResponseEventArgs e)
		{
			object[] P = (object[])e.State;
			CoapResponseEventHandler Callback = (CoapResponseEventHandler)P[0];
			object State = P[1];

			if (Callback != null)
			{
				try
				{
					Callback.Invoke(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}


		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void DELETE(CoapMessage Request, CoapResponse Response)
		{
			if (this.state != Lwm2mState.Bootstrap)
			{
				if (this.IsFromBootstrapServer(Request))
					this.State = Lwm2mState.Bootstrap;
				else
				{
					Response.RST(CoapCode.Unauthorized);
					return;
				}
			}

			Task T = this.DeleteBootstrapInfo();
			Response.ACK(CoapCode.Deleted);
		}

		/// <summary>
		/// Checks if a request comes from the bootstrap server.
		/// </summary>
		/// <param name="Request">CoAP Request.</param>
		/// <returns>If the request comes from the bootstrap server.</returns>
		public bool IsFromBootstrapServer(CoapMessage Request)
		{
			return (this.bootstrapSeverIp != null &&
				Array.IndexOf<IPEndPoint>(this.bootstrapSeverIp, Request.From) >= 0);
		}

		/// <summary>
		/// Loads any Bootstrap information.
		/// </summary>
		public virtual async Task LoadBootstrapInfo()
		{
			foreach (Lwm2mObject Object in this.objects.Values)
				await Object.LoadBootstrapInfo();
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public virtual async Task DeleteBootstrapInfo()
		{
			foreach (Lwm2mObject Object in this.objects.Values)
				await Object.DeleteBootstrapInfo();
		}

		/// <summary>
		/// Applies any Bootstrap information.
		/// </summary>
		public virtual async Task ApplyBootstrapInfo()
		{
			foreach (Lwm2mObject Object in this.objects.Values)
				await Object.ApplyBootstrapInfo();
		}

		internal async Task BootstrapCompleted()
		{
			await this.ApplyBootstrapInfo();

			try
			{
				this.OnBootstrapCompleted?.Invoke(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			this.Register();
		}

		internal void BootstrapFailed()
		{
			try
			{
				this.OnBootstrapFailed?.Invoke(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a bootstrap sequence completed.
		/// </summary>
		public event EventHandler OnBootstrapCompleted = null;

		/// <summary>
		/// Event raised when a bootstrap sequence failed.
		/// </summary>
		public event EventHandler OnBootstrapFailed = null;

		internal class BootstreapResource : CoapResource, ICoapPostMethod
		{
			private Lwm2mClient client;

			public BootstreapResource(Lwm2mClient Client)
				: base("/bs")
			{
				this.client = Client;
			}

			public bool AllowsPOST => true;

			public void POST(CoapMessage Request, CoapResponse Response)
			{
				if (this.client.IsFromBootstrapServer(Request))
				{
					Task T = this.client.BootstrapCompleted();
					Response.Respond(CoapCode.Changed);
				}
				else
					Response.RST(CoapCode.Unauthorized);
			}
		}

		#endregion

		#region Registration

		/// <summary>
		/// Registers client with server(s), as defined in bootstrapped information.
		/// </summary>
		public bool Register()
		{
			bool Result = false;

			foreach (Lwm2mObject Object in this.objects.Values)
			{
				if (Object is Lwm2mServerObject ServerObject)
				{
					foreach (Lwm2mServerObjectInstance Instance in ServerObject.Instances)
					{
						if (!Instance.ShortServerId.HasValue)
							continue;

						Lwm2mSecurityObjectInstance SecurityInfo = this.GetSecurityInfo(Instance.ShortServerId.Value);
						if (SecurityInfo == null)
							continue;

						Lwm2mServerReference Ref = this.GetServerReference(SecurityInfo, false);
						if (Ref == null)
							continue;

						this.Register(Instance.LifetimeSeconds.HasValue ? (int)Instance.LifetimeSeconds.Value : 86400, Ref);
						Result = true;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Gets the security information for a given server.
		/// </summary>
		/// <param name="ShortServerId">Short Server ID</param>
		/// <returns>Security information, if found.</returns>
		public Lwm2mSecurityObjectInstance GetSecurityInfo(ushort ShortServerId)
		{
			foreach (Lwm2mObject Object in this.objects.Values)
			{
				if (Object is Lwm2mSecurityObject SecurityObject)
				{
					foreach (Lwm2mSecurityObjectInstance Instance in SecurityObject.Instances)
					{
						if (Instance.ShortServerId.HasValue && Instance.ShortServerId.Value == ShortServerId)
							return Instance;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Registers client with server(s).
		/// </summary>
		/// <param name="LifetimeSeconds">Lifetime, in seconds.</param>
		/// <param name="Servers">Servers to register with.</param>
		public void Register(int LifetimeSeconds, params Lwm2mServerReference[] Servers)
		{
			if (LifetimeSeconds <= 0)
				throw new ArgumentException("Expected positive integer.", nameof(LifetimeSeconds));

			this.serverReferences = Servers;
			this.lastLinks = this.EncodeObjectLinks(false);
			this.lifetimeSeconds = LifetimeSeconds;
			this.State = Lwm2mState.Registration;

			foreach (Lwm2mServerReference Server in this.serverReferences)
				this.Register(Server, this.lastLinks, false);
		}

		private void Register(Lwm2mServerReference Server, string Links, bool Update)
		{
			if (Update && !string.IsNullOrEmpty(Server.LocationPath) &&
				Server.LocationPath.StartsWith("/"))
			{
				if (Links != this.lastLinks)
				{
					this.coapEndpoint.POST(Server.Uri + Server.LocationPath.Substring(1),
						true, Encoding.UTF8.GetBytes(Links), 64, Server.Credentials,
						this.RegisterResponse, new object[] { Server, true },
						new CoapOptionContentFormat(CoreLinkFormat.ContentFormatCode));
				}
				else
				{
					this.coapEndpoint.POST(Server.Uri + Server.LocationPath.Substring(1),
						true, null, 64, Server.Credentials, this.RegisterResponse,
						new object[] { Server, true });
				}
			}
			else
			{
				this.coapEndpoint.POST(Server.Uri + "rd?ep=" + this.clientName +
					"&lt=" + this.lifetimeSeconds.ToString() + "&lwm2m=1.0", true,
					Encoding.UTF8.GetBytes(Links), 64, Server.Credentials,
					this.RegisterResponse, new object[] { Server, false },
					new CoapOptionContentFormat(CoreLinkFormat.ContentFormatCode));
			}
		}

		private string EncodeObjectLinks(bool IncludeSecurity)
		{
			StringBuilder Output = new StringBuilder("</>;ct=11542");

			foreach (Lwm2mObject Obj in this.objects.Values)
				Obj.EncodeObjectLinks(IncludeSecurity, Output);

			return Output.ToString();
		}

		/// <summary>
		/// Re-registers the client with server. Must be called before the current
		/// registration times out.
		/// </summary>
		public void RegisterUpdate()
		{
			if (this.lifetimeSeconds <= 0)
				throw new Exception("Client has not been registered.");

			string Links = this.EncodeObjectLinks(false);
			bool Update = this.state == Lwm2mState.Operation;

			foreach (Lwm2mServerReference Server in this.serverReferences)
				this.Register(Server, Links, Update);

			this.lastLinks = Links;
		}

		internal void RegisterUpdateIfRegistered()
		{
			if (this.state == Lwm2mState.Operation)
				this.RegisterUpdate();
		}

		private void RegisterResponse(object Sender, CoapResponseEventArgs e)
		{
			object[] P = (object[])e.State;
			Lwm2mServerReference Server = (Lwm2mServerReference)P[0];
			bool Update = (bool)P[1];

			Server.Registered = e.Ok;
			this.coapEndpoint.ScheduleEvent(this.RegisterTimeout,
				DateTime.Now.AddSeconds(this.lifetimeSeconds * 0.5), new object[] { Server, e.Ok, this.registrationEpoch });

			try
			{
				if (e.Ok && this.state == Lwm2mState.Registration)
					this.State = Lwm2mState.Operation;

				if (e.Ok)
				{
					if (!Update)
						Server.LocationPath = e.Message.LocationPath;

					this.OnRegistrationSuccessful?.Invoke(this, new Lwm2mServerReferenceEventArgs(Server));
				}
				else
					this.OnRegistrationFailed?.Invoke(this, new Lwm2mServerReferenceEventArgs(Server));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private void RegisterTimeout(object State)
		{
			object[] P = (object[])State;
			Lwm2mServerReference Server = (Lwm2mServerReference)P[0];
			bool Update = (bool)P[1];
			int Epoch = (int)P[2];

			if (Epoch != this.registrationEpoch)
				return;

			if (Update)
				this.Register(Server, this.lastLinks, true);
			else
				this.Register(Server, this.lastLinks, false);
		}

		/// <summary>
		/// Event raised when a server registration has been successful or updated.
		/// </summary>
		public event Lwm2mServerReferenceEventHandler OnRegistrationSuccessful = null;

		/// <summary>
		/// Event raised when a server registration has been failed or unable to update.
		/// </summary>
		public event Lwm2mServerReferenceEventHandler OnRegistrationFailed = null;

		/// <summary>
		/// Deregisters the client from server(s).
		/// </summary>
		public void Deregister()
		{
			this.registrationEpoch++;

			if (this.state != Lwm2mState.Operation)
				return;

			this.State = Lwm2mState.Deregistered;
			this.lifetimeSeconds = 0;

			foreach (Lwm2mServerReference Server in this.serverReferences)
			{
				if (!string.IsNullOrEmpty(Server.LocationPath) && Server.LocationPath.StartsWith("/"))
				{
					this.coapEndpoint.DELETE(Server.Uri + Server.LocationPath.Substring(1),
						true, Server.Credentials, this.DeregisterResponse, Server);
				}
			}
		}

		private void DeregisterResponse(object Sender, CoapResponseEventArgs e)
		{
			Lwm2mServerReference Server = (Lwm2mServerReference)e.State;

			Server.Registered = false;

			try
			{
				if (e.Ok)
					this.OnDeregistrationSuccessful?.Invoke(this, new Lwm2mServerReferenceEventArgs(Server));
				else
					this.OnDeregistrationFailed?.Invoke(this, new Lwm2mServerReferenceEventArgs(Server));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		#endregion

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.coapEndpoint != null)
			{
				if (this.state == Lwm2mState.Operation)
					this.Deregister();

				foreach (Lwm2mObject Object in this.objects.Values)
				{
					this.coapEndpoint.Unregister(Object);

					foreach (Lwm2mObjectInstance Instance in Object.Instances)
						this.coapEndpoint.Unregister(Instance);
				}

				this.coapEndpoint.Unregister(this);
				this.coapEndpoint.Unregister(this.bsResource);
				this.coapEndpoint = null;

				this.objects.Clear();
			}
		}

		/// <summary>
		/// Event raised when a server deregistration has been successful.
		/// </summary>
		public event Lwm2mServerReferenceEventHandler OnDeregistrationSuccessful = null;

		/// <summary>
		/// Event raised when a server deregistration has been failed.
		/// </summary>
		public event Lwm2mServerReferenceEventHandler OnDeregistrationFailed = null;

		// TODO: Start()
	}
}
