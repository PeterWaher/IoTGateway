using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.ContentFormats;
using Waher.Networking.CoAP.CoRE;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Class implementing an LWM2M client, as defined in:
	/// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf
	/// </summary>
	public class Lwm2mClient : IDisposable
	{
		private SortedDictionary<int, Lwm2mObject> objects = new SortedDictionary<int, Lwm2mObject>();
		private CoapEndpoint coapEndpoint;
		private Lwm2mServerReference[] serverReferences;
		private string clientName;
		private string lastLinks = string.Empty;
		private int lifetimeSeconds = 0;
		private int registrationEpoch = 0;
		private bool registered = false;

		/// <summary>
		/// Class implementing an LWM2M client, as defined in:
		/// http://www.openmobilealliance.org/release/LightweightM2M/V1_0-20170208-A/OMA-TS-LightweightM2M-V1_0-20170208-A.pdf
		/// </summary>
		/// <param name="ClientName">Client name.</param>
		/// <param name="CoapEndpoint">CoAP endpoint to use for LWM2M communication.</param>
		/// <param name="ServerReferences">LWM2M server references.</param>
		public Lwm2mClient(string ClientName, CoapEndpoint CoapEndpoint,
			params Lwm2mServerReference[] ServerReferences)
		{
			this.coapEndpoint = CoapEndpoint;
			this.serverReferences = ServerReferences;
			this.clientName = ClientName;
		}

		/// <summary>
		/// Registers an LWM2M object on the client.
		/// </summary>
		/// <param name="Object">Object.</param>
		public void Add(Lwm2mObject Object)
		{
			if (Object.Id < 0)
				throw new ArgumentException("Invalid object ID.", nameof(Object));

			if (Object.Client != null)
				throw new ArgumentException("Object already added to a client.", nameof(Object));

			lock (this.objects)
			{
				if (this.objects.ContainsKey(Object.Id))
					throw new ArgumentException("An object with ID " + Object.Id + " already is registered.", nameof(Object));

				this.objects[Object.Id] = Object;

				Object.Client = this;
			}

			if (this.registered)
				this.RegisterUpdate();
		}

		/// <summary>
		/// Unregisters an LWM2M object from the client.
		/// </summary>
		/// <param name="Object">Object.</param>
		public bool Remove(Lwm2mObject Object)
		{
			if (Object.Client != this)
				return false;

			lock (this.objects)
			{
				if (this.objects.TryGetValue(Object.Id, out Lwm2mObject Obj) && Obj == Object)
				{
					Object.Client = null;
					if (!this.objects.Remove(Object.Id))
						return false;
				}
				else
					return false;
			}

			if (this.registered)
				this.RegisterUpdate();

			return true;
		}

		/// <summary>
		/// Registered objects.
		/// </summary>
		public Lwm2mObject[] Objects
		{
			get
			{
				Lwm2mObject[] Result;

				lock (this.objects)
				{
					Result = new Lwm2mObject[this.objects.Count];
					this.objects.Values.CopyTo(Result, 0);
				}

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
				lock (this.objects)
				{
					return this.objects.Count > 0;
				}
			}
		}

		/// <summary>
		/// Performs a resource discovery for registered server references.
		/// </summary>
		public void Discover()
		{
			foreach (Lwm2mServerReference Server in this.serverReferences)
			{
				this.coapEndpoint.GET(Server.Uri + ".well-known/core", true, Server.Credentials,
					this.DiscoverResponse, Server);
			}
		}

		private void DiscoverResponse(object Sender, CoapResponseEventArgs e)
		{
			Lwm2mServerReference Server = (Lwm2mServerReference)e.State;

			Server.HasBootstrapInterface = null;
			Server.HasRegistrationInterface = null;

			if (e.Ok)
			{
				LinkDocument Doc = e.Message.Decode() as LinkDocument;
				Server.LinkDocument = Doc;

				if (Doc != null)
				{
					foreach (Link Link in Doc.Links)
					{
						switch (Link.Uri.LocalPath)
						{
							case "/bs":
								Server.HasBootstrapInterface = true;
								break;

							case "/rd":
								Server.HasRegistrationInterface = true;
								break;
						}
					}
				}
			}

			if (!Server.HasBootstrapInterface.HasValue)
				Server.HasBootstrapInterface = false;

			if (!Server.HasRegistrationInterface.HasValue)
				Server.HasRegistrationInterface = false;

			try
			{
				this.OnServerDiscovered?.Invoke(this, new Lwm2mServerReferenceEventArgs(Server));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a server reference has been discovered.
		/// </summary>
		public event Lwm2mServerReferenceEventHandler OnServerDiscovered = null;

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

		/// <summary>
		/// Sends a BOOTSTRAP-REQUEST to the LWM2M Server(s), to request the servers initialize
		/// bootstrapping.
		/// </summary>
		public void BootstrapRequest()
		{
			foreach (Lwm2mServerReference Server in this.serverReferences)
			{
				this.coapEndpoint.POST(Server.Uri + "bs?ep=" + this.clientName, true, null, 64, Server.Credentials,
					this.BootstrepRequestResponse, Server);
			}
		}

		private void BootstrepRequestResponse(object Sender, CoapResponseEventArgs e)
		{
			Lwm2mServerReference Server = (Lwm2mServerReference)e.State;

			// TODO
		}

		/// <summary>
		/// Registers client with server(s).
		/// </summary>
		/// <param name="LifetimeSeconds">Lifetime, in seconds.</param>
		public void Register(int LifetimeSeconds)
		{
			if (LifetimeSeconds <= 0)
				throw new ArgumentException("Expected positive integer.", nameof(LifetimeSeconds));

			this.lastLinks = this.EncodeObjectLinks();
			this.lifetimeSeconds = LifetimeSeconds;

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

		private string EncodeObjectLinks()
		{
			StringBuilder sb = new StringBuilder("</>;ct=11543");

			foreach (Lwm2mObject Obj in this.Objects)
			{
				if (Obj.Id == 0)
					continue;

				if (Obj.HasInstances)
				{
					foreach (Lwm2mObjectInstance Instance in Obj.Instances)
					{
						sb.Append(',');
						sb.Append("</");
						sb.Append(Obj.Id.ToString());
						sb.Append('/');
						sb.Append(Instance.SubId.ToString());
						sb.Append('>');
					}
				}
				else
				{
					sb.Append(',');
					sb.Append("</");
					sb.Append(Obj.Id.ToString());
					sb.Append('>');
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Re-registers the client with server. Must be called before the current
		/// registration times out.
		/// </summary>
		public void RegisterUpdate()
		{
			if (this.lifetimeSeconds <= 0)
				throw new Exception("Client has not been registered.");

			string Links = this.EncodeObjectLinks();
			bool Update = this.registered;

			foreach (Lwm2mServerReference Server in this.serverReferences)
				this.Register(Server, Links, Update);

			this.lastLinks = Links;
		}

		internal void RegisterUpdateIfRegistered()
		{
			if (this.registered)
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
				this.registered = e.Ok;

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

			if (!this.registered)
				return;

			this.registered = false;
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

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.coapEndpoint!=null && this.registered)
			{
				this.Deregister();
				this.coapEndpoint = null;
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
