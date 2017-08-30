using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Networking.CoAP.Options;
using Waher.Networking.CoAP.LWM2M.ContentFormats;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M object instances.
	/// </summary>
	[CollectionName("Lwm2mObjectInstances")]
	[TypeName(TypeNameSerialization.FullName)]
	[Index("Id", "InstanceId")]
	public abstract class Lwm2mObjectInstance : CoapResource, ICoapGetMethod
	{
		private SortedDictionary<int, Lwm2mResource> resources = new SortedDictionary<int, Lwm2mResource>();
		private Lwm2mObject obj = null;
		private string objectId = null;
		private ushort id;
		private ushort instanceId;

		/// <summary>
		/// Base class for all LWM2M objects.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="InstanceId">ID of object instance.</param>
		/// <param name="Resources">Resources.</param>
		public Lwm2mObjectInstance(ushort Id, ushort InstanceId, params Lwm2mResource[] Resources)
			: base("/" + Id.ToString() + "/" + InstanceId.ToString())
		{
			this.id = Id;
			this.instanceId = InstanceId;

			foreach (Lwm2mResource Resource in Resources)
				this.Add(Resource);
		}

		/// <summary>
		/// Object ID in database.
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// LWM2M object.
		/// </summary>
		public Lwm2mObject Object
		{
			get { return this.obj; }
			internal set { this.obj = value; }
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
					this.Path = "/" + this.id.ToString() + "/" + this.instanceId.ToString();
				}
			}
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
					this.Path = "/" + this.id.ToString() + "/" + this.instanceId.ToString();
				}
			}
		}

		/// <summary>
		/// Registered resources.
		/// </summary>
		public Lwm2mResource[] Resources
		{
			get
			{
				Lwm2mResource[] Result;

				lock (this.resources)
				{
					Result = new Lwm2mResource[this.resources.Count];
					this.resources.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		/// <summary>
		/// If the object instance has resources registered on it.
		/// </summary>
		public bool HasResources
		{
			get
			{
				lock (this.resources)
				{
					return this.resources.Count > 0;
				}
			}
		}

		/// <summary>
		/// How notifications are sent, if at all.
		/// </summary>
		public override Notifications Notifications => Notifications.Acknowledged;

		/// <summary>
		/// Adds a resource.
		/// </summary>
		/// <param name="Resource">Resource.</param>
		public void Add(Lwm2mResource Resource)
		{
			lock (this.resources)
			{
				if (Resource.ResourceId < 0)
					throw new ArgumentException("Invalid resource ID.", nameof(Resource));

				if (this.resources.ContainsKey(Resource.ResourceId))
				{
					throw new ArgumentException("A resource with ID " + Resource.ResourceId +
						" is already registered.", nameof(Resource));
				}

				this.resources[Resource.ResourceId] = Resource;
				Resource.ObjectInstance = this;
			}
		}

		/// <summary>
		/// Removes all resources.
		/// </summary>
		protected void ClearResources()
		{
			Lwm2mResource[] Resources;

			lock (this.resources)
			{
				Resources = new Lwm2mResource[this.resources.Count];
				this.resources.Values.CopyTo(Resources, 0);
				this.resources.Clear();
			}

			foreach (Lwm2mResource Resource in Resources)
				this.obj?.Client?.CoapEndpoint.Unregister(Resource);
		}

		/// <summary>
		/// Loads any Bootstrap information.
		/// </summary>
		public virtual Task LoadBootstrapInfo()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public virtual Task DeleteBootstrapInfo()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Applies any Bootstrap information.
		/// </summary>
		public virtual Task ApplyBootstrapInfo()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Encodes available object links.
		/// </summary>
		/// <param name="IncludeSecurity">If security objects are to be included.</param>
		/// <param name="Output">Link output.</param>
		public void EncodeObjectLinks(bool IncludeSecurity, StringBuilder Output)
		{
			if (this.id > 0 || IncludeSecurity)
			{
				Output.Append(',');
				Output.Append("</");
				Output.Append(this.id.ToString());
				Output.Append('/');
				Output.Append(this.instanceId.ToString());
				Output.Append('>');

				this.EncodeLinkParameters(Output);
			}
		}

		/// <summary>
		/// Encodes any link parameters to the object link.
		/// </summary>
		/// <param name="Output">Link output.</param>
		public virtual void EncodeLinkParameters(StringBuilder Output)
		{
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.Path + ": " + this.GetType().FullName;
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

			if (!string.IsNullOrEmpty(Request.SubPath))
			{
				Response.RST(CoapCode.NotFound);
				return;
			}

			if (Request.IsAcceptable(Tlv.ContentFormatCode))
				Writer = new TlvWriter();
			else if (Request.IsAcceptable(Json.ContentFormatCode))
				Writer = new JsonWriter(this.Path + "/");
			else
			{
				Response.RST(CoapCode.NotAcceptable);
				return;
			}

			this.Export(Writer);

			byte[] Payload = Writer.ToArray();

			if (Payload.Length == 0)
				Response.RST(CoapCode.NotFound);
			else
			{
				Response.Respond(CoapCode.Content, Payload, 64,
					new CoapOptionContentFormat(Writer.ContentFormat));
			}
		}

		/// <summary>
		/// Exports all resources.
		/// </summary>
		/// <param name="Writer">Output</param>
		public virtual void Export(ILwm2mWriter Writer)
		{
			foreach (Lwm2mResource Resource in this.Resources)
				Resource.Write(Writer);
		}

		/// <summary>
		/// Event raised after the resource has been registered
		/// </summary>
		public event EventHandler OnAfterRegister = null;

		internal virtual void AfterRegister(Lwm2mClient Client)
		{
			try
			{
				this.OnAfterRegister?.Invoke(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			foreach (Lwm2mResource Resource in this.Resources)
			{
				Client.CoapEndpoint.Register(Resource);
				Resource.AfterRegister();
			}
		}

	}
}
