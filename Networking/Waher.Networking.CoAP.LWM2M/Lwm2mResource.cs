using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.LWM2M.ContentFormats;
using Waher.Networking.CoAP.LWM2M.Events;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M resources.
	/// </summary>
	public abstract class Lwm2mResource : CoapResource, ICoapGetMethod
	{
		private Lwm2mObjectInstance objInstance = null;
		private ushort id;
		private ushort instanceId;
		private ushort resourceId;

		/// <summary>
		/// Base class for all LWM2M resources.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="ResourceId">ID of resource.</param>
		public Lwm2mResource(ushort Id, ushort InstanceId, ushort ResourceId)
			: base("/" + Id.ToString() + "/" + InstanceId.ToString() + "/" + ResourceId.ToString())
		{
			this.id = Id;
			this.instanceId = InstanceId;
			this.resourceId = ResourceId;
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
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Record">TLV record.</param>
		public abstract void Read(TlvRecord Record);

		/// <summary>
		/// Reads the value from a TLV record.
		/// </summary>
		/// <param name="Output">Output.</param>
		public abstract void Write(ILwm2mWriter Output);

		/// <summary>
		/// Value of resource.
		/// </summary>
		public abstract object Value
		{
			get;
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

			if (Request.IsAcceptable(Tlv.ContentFormatCode))
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

		internal virtual void AfterRegister()
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

	}
}
