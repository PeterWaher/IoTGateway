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
	public class Lwm2mClient
	{
		private SortedDictionary<int, Lwm2mObject> objects = new SortedDictionary<int, Lwm2mObject>();
		private CoapEndpoint coapEndpoint;
		private Lwm2mServerReference[] serverReferences;
		private string clientName;
		private int lifetimeSeconds = 0;

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

			lock (this.objects)
			{
				if (this.objects.ContainsKey(Object.Id))
					throw new ArgumentException("An object with ID " + Object.Id + " already is registered.", nameof(Object));

				this.objects[Object.Id] = Object;
			}
		}

		/// <summary>
		/// Unregisters an LWM2M object from the client.
		/// </summary>
		/// <param name="Object">Object.</param>
		public bool Remove(Lwm2mObject Object)
		{
			lock (this.objects)
			{
				if (this.objects.TryGetValue(Object.Id, out Lwm2mObject Obj) && Obj == Object)
					return this.objects.Remove(Object.Id);
				else
					return false;
			}
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
		/// Registers client with server.
		/// </summary>
		/// <param name="LifetimeSeconds">Lifetime, in seconds.</param>
		public void Register(int LifetimeSeconds)
		{
			if (LifetimeSeconds <= 0)
				throw new ArgumentException("Expected positive integer.", nameof(LifetimeSeconds));

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

			string LinkFormat = sb.ToString();
			byte[] Payload = Encoding.UTF8.GetBytes(LinkFormat);

			foreach (Lwm2mServerReference Server in this.serverReferences)
			{
				this.coapEndpoint.POST(Server.Uri + "rd?ep=" + this.clientName +
					"&lt=" + LifetimeSeconds.ToString() + "&lwm2m=1.0", true, Payload, 64,
					Server.Credentials, this.RegisterResponse, Server,
					new CoapOptionContentFormat(CoreLinkFormat.ContentFormatCode));
			}
		}

		/// <summary>
		/// Re-registers the client with server. Must be called before the current
		/// registration times out.
		/// </summary>
		public void RegisterUpdate()
		{
			foreach (Lwm2mServerReference Server in this.serverReferences)
			{
				this.coapEndpoint.POST(Server.Uri + "rd?ep=" + this.clientName +
					"&lt=" + this.lifetimeSeconds.ToString() + "&lwm2m=1.0", true, null, 64,
					Server.Credentials, this.RegisterResponse, Server,
					new CoapOptionContentFormat(CoreLinkFormat.ContentFormatCode));
			}
		}

		private void RegisterResponse(object Sender, CoapResponseEventArgs e)
		{
			Lwm2mServerReference Server = (Lwm2mServerReference)e.State;

			Server.Registered = e.Ok;

			try
			{
				if (e.Ok)
					this.OnRegistrationSuccessful?.Invoke(this, new Lwm2mServerReferenceEventArgs(Server));
				else
					this.OnRegistrationFailed?.Invoke(this, new Lwm2mServerReferenceEventArgs(Server));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

		}

		/// <summary>
		/// Event raised when a server registration has been successful.
		/// </summary>
		public event Lwm2mServerReferenceEventHandler OnRegistrationSuccessful = null;

		/// <summary>
		/// Event raised when a server registration has been failed.
		/// </summary>
		public event Lwm2mServerReferenceEventHandler OnRegistrationFailed = null;


		// TODO: Start()
	}
}
