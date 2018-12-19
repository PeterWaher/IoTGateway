using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP;
using Waher.Networking.CoAP.Options;
using Waher.Networking.LWM2M.ContentFormats;
using Waher.Networking.LWM2M.Events;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M resources.
	/// </summary>
	public abstract class Lwm2mResource : CoapResource, ICoapGetMethod, ICoapPutMethod, ICoapPostMethod
	{
		private Lwm2mObjectInstance objInstance = null;
		private string name;
		private ushort id;
		private ushort instanceId;
		private ushort resourceId;
		private bool canWrite;
		private bool persist;

		/// <summary>
		/// Base class for all LWM2M resources.
		/// </summary>
		/// <param name="Name">Name of parameter. If null, parameter values will not be logged</param>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		/// <param name="CanWrite">If the resource allows servers to update the value using write commands.</param>
		/// <param name="Persist">If written values should be persisted by the resource.</param>
		public Lwm2mResource(string Name, ushort Id, ushort InstanceId, ushort ResourceId, bool CanWrite, bool Persist)
			: base("/" + Id.ToString() + "/" + InstanceId.ToString() + "/" + ResourceId.ToString())
		{
			this.name = Name;
			this.id = Id;
			this.instanceId = InstanceId;
			this.resourceId = ResourceId;
			this.canWrite = CanWrite;
			this.persist = Persist;
		}

		/// <summary>
		/// LWM2M object instance.
		/// </summary>
		public Lwm2mObjectInstance ObjectInstance
		{
			get { return this.objInstance; }
			internal set { this.objInstance = value; }
		}

		/// <summary>
		/// How notifications are sent, if at all.
		/// </summary>
		public override Notifications Notifications => Notifications.Acknowledged;

		/// <summary>
		/// Name of resource.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// ID of object.
		/// </summary>
		public ushort Id
		{
			get { return this.id; }
			set
			{
				if (this.id != value)
				{
					this.id = value;
					this.SetPath();
				}
			}
		}

		private void SetPath()
		{
			this.Path = "/" + this.id.ToString() + "/" + this.instanceId.ToString() +
				"/" + this.resourceId.ToString();
		}

		/// <summary>
		/// ID of Object Instance.
		/// </summary>
		public ushort InstanceId
		{
			get { return this.instanceId; }
			set
			{
				if (this.instanceId != value)
				{
					this.instanceId = value;
					this.SetPath();
				}
			}
		}

		/// <summary>
		/// ID of resource.
		/// </summary>
		public ushort ResourceId
		{
			get { return this.resourceId; }
			set
			{
				if (this.resourceId != value)
				{
					this.resourceId = value;
					this.SetPath();
				}
			}
		}

		/// <summary>
		/// If the resource allows servers to update the value using write commands.
		/// </summary>
		public bool CanWrite
		{
			get { return this.canWrite; }
		}

		/// <summary>
		/// If written values should be persisted by the resource.
		/// </summary>
		public bool Persist
		{
			get { return this.persist; }
		}

		/// <summary>
		/// Loads the value of the resource, from persisted storage.
		/// </summary>
		public virtual Task ReadPersistedValue()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Saves the value of the resource, to persisted storage.
		/// </summary>
		/// <returns></returns>
		public virtual Task WritePersistedValue()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public abstract Task Read(TlvRecord Record);

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public abstract void Write(ILwm2mWriter Output);

		/// <summary>
		/// Resets the parameter to its default value.
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// Value of resource.
		/// </summary>
		public abstract object Value
		{
			get;
		}

		/// <summary>
		/// Method called when the resource value has been updated.
		/// </summary>
		protected virtual Task ValueUpdated()
		{
			if (this.objInstance != null)
				return this.objInstance.ValueUpdated(this);
			else
				return Task.CompletedTask;
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public virtual void GET(CoapMessage Request, CoapResponse Response)
		{
			ILwm2mWriter Writer;
			bool FromBootstrapServer = this.objInstance.Object.Client.IsFromBootstrapServer(Request);

			if (this.id == 0 && !FromBootstrapServer)
			{
				Response.RST(CoapCode.Unauthorized);
				return;
			}

			if (Request.Accept is null)
				Writer = new TextWriter();
			else if (Request.IsAcceptable(Tlv.ContentFormatCode))
				Writer = new TlvWriter();
			else if (Request.IsAcceptable(Json.ContentFormatCode))
				Writer = new JsonWriter(this.objInstance.Path + "/");
			else if (Request.IsAcceptable(CoAP.ContentFormats.PlainText.ContentFormatCode))
				Writer = new TextWriter();
			else if (Request.IsAcceptable(CoAP.ContentFormats.Binary.ContentFormatCode))
				Writer = new OpaqueWriter();
			else
			{
				Response.RST(CoapCode.NotAcceptable);
				return;
			}

			if (this.OnBeforeGet != null)
			{
				try
				{
					this.OnBeforeGet.Invoke(this, new CoapRequestEventArgs(Request));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			this.Write(Writer);

			byte[] Payload = Writer.ToArray();

			Response.Respond(CoapCode.Content, Payload, 64,
				new CoapOptionContentFormat(Writer.ContentFormat));
		}

		/// <summary>
		/// Event raised before the response to a GET request is generated.
		/// </summary>
		public event CoapRequestEventHandler OnBeforeGet = null;

		/// <summary>
		/// Event raised after the resource has been registered
		/// </summary>
		public event EventHandler OnAfterRegister = null;

		/// <summary>
		/// Called after the resource has been registered on a CoAP Endpoint.
		/// </summary>
		public virtual void AfterRegister()
		{
			try
			{
				this.OnAfterRegister?.Invoke(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => this.AllowsPUT;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void POST(CoapMessage Request, CoapResponse Response)
		{
			this.PUT(Request, Response);
		}

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public virtual bool AllowsPUT => true;

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public virtual void PUT(CoapMessage Request, CoapResponse Response)
		{
			bool FromBootstrapServer = this.objInstance.Object.Client.IsFromBootstrapServer(Request);

			if (this.id == 0 && !FromBootstrapServer)
			{
				Response.RST(CoapCode.Unauthorized);
				return;
			}

			if (Request.UriQuery != null && Request.UriQuery.Count > 0)    // Write attributes
			{
				if (!FromBootstrapServer)
				{
					Response.RST(CoapCode.Unauthorized);
					return;
				}

				foreach (KeyValuePair<string, string> P in Request.UriQuery)
				{
					switch (P.Key)
					{
						case "pmin":
						case "pmax":
						case "gt":
						case "lt":
						case "st":
							// TODO: Implement support
							break;

						default:
							Response.RST(CoapCode.BadRequest);
							return;
					}
				}
			}

			if (Request.ContentFormat != null)      // Write operation
			{
				if (!this.canWrite && !FromBootstrapServer)
				{
					Response.Respond(CoapCode.BadRequest);
					return;
				}

				object Decoded = Request.Decode();

				if (Decoded is TlvRecord[] Records)
				{
					foreach (TlvRecord Rec in Records)
						this.Read(Rec);
				}
				else
				{
					Response.Respond(CoapCode.NotAcceptable);
					return;
				}

				this.RemoteUpdate(Request);
			}
			else
				this.Execute(Request);

			Response.Respond(CoapCode.Changed);
		}

		internal void RemoteUpdate(CoapMessage Request)
		{
			if (!string.IsNullOrEmpty(this.name))
			{
				Log.Informational("Parameter updated.", this.name, Request.From.ToString(),
					new KeyValuePair<string, object>("Value", this.Value));
			}

			try
			{
				this.OnRemoteUpdate?.Invoke(this, new CoapRequestEventArgs(Request));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Even raised when a new value has been written to the resource from a remote source.
		/// </summary>
		public event CoapRequestEventHandler OnRemoteUpdate = null;

		/// <summary>
		/// Executes an action on the resource.
		/// </summary>
		/// <param name="Request">Request message.</param>
		public virtual void Execute(CoapMessage Request)
		{
			if (!string.IsNullOrEmpty(this.name))
				Log.Informational("Executing action.", this.name, Request.From.ToString());

			try
			{
				this.OnExecute?.Invoke(this, new CoapRequestEventArgs(Request));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when the resource is executed.
		/// </summary>
		public event CoapRequestEventHandler OnExecute = null;

		/// <summary>
		/// Event raised when the resource value has been set.
		/// </summary>
		public event EventHandler OnSet = null;

		/// <summary>
		/// Raises the <see cref="OnSet"/> event.
		/// </summary>
		protected void Set()
		{
			try
			{
				this.OnSet?.Invoke(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
