using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.CoAP;
using Waher.Networking.CoAP.ContentFormats;
using Waher.Networking.CoAP.Options;
using Waher.Persistence.Attributes;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M objects.
	/// </summary>
	[CollectionName("Lwm2mObjects")]
	[TypeName(TypeNameSerialization.FullName)]
	[Index("Id")]
	public abstract class Lwm2mObject : CoapResource, ICoapGetMethod
	{
		private SortedDictionary<int, Lwm2mObjectInstance> instances = new SortedDictionary<int, Lwm2mObjectInstance>();
		private Lwm2mClient client = null;
		private string objectId = null;
		private ushort id;

		/// <summary>
		/// Base class for all LWM2M objects.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="Instances">Object instances.</param>
		public Lwm2mObject(ushort Id, params Lwm2mObjectInstance[] Instances)
			: base("/" + Id.ToString())
		{
			this.id = Id;

			foreach (Lwm2mObjectInstance Instance in Instances)
				this.Add(Instance);
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
		/// Adds an object instance.
		/// </summary>
		/// <param name="Instance">Object instance.</param>
		public void Add(Lwm2mObjectInstance Instance)
		{
			lock (this.instances)
			{
				if (Instance.InstanceId < 0)
					throw new ArgumentException("Invalid object instance ID.", nameof(Instance));

				if (this.instances.ContainsKey(Instance.InstanceId))
				{
					throw new ArgumentException("An object instance with ID " + Instance.InstanceId +
						" is already registered.", nameof(Instance));
				}

				this.instances[Instance.InstanceId] = Instance;
				Instance.Object = this;
			}
		}

		/// <summary>
		/// Removes all instances.
		/// </summary>
		protected void ClearInstances()
		{
			Lwm2mObjectInstance[] Instances;

			lock (this.instances)
			{
				Instances = new Lwm2mObjectInstance[this.instances.Count];
				this.instances.Values.CopyTo(Instances, 0);
				this.instances.Clear();
			}

			foreach (Lwm2mObjectInstance Instance in Instances)
			{
				this.client?.CoapEndpoint.Unregister(Instance);

				foreach (Lwm2mResource Resource in Instance.Resources)
					this.client?.CoapEndpoint.Unregister(Resource);
			}
		}

		/// <summary>
		/// LWM2M Client.
		/// </summary>
		public Lwm2mClient Client
		{
			get { return this.client; }
			internal set { this.client = value; }
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
					this.Path = "/" + this.id.ToString();
				}
			}
		}

		/// <summary>
		/// Registered instances.
		/// </summary>
		public Lwm2mObjectInstance[] Instances
		{
			get
			{
				Lwm2mObjectInstance[] Result;

				lock (this.instances)
				{
					Result = new Lwm2mObjectInstance[this.instances.Count];
					this.instances.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		/// <summary>
		/// If the object has instances registered on it.
		/// </summary>
		public bool HasInstances
		{
			get
			{
				lock (this.instances)
				{
					return this.instances.Count > 0;
				}
			}
		}

		/// <summary>
		/// Loads any Bootstrap information.
		/// </summary>
		public virtual async Task LoadBootstrapInfo()
		{
			foreach (Lwm2mObjectInstance Instance in this.Instances)
				await Instance.LoadBootstrapInfo();
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public virtual async Task DeleteBootstrapInfo()
		{
			foreach (Lwm2mObjectInstance Instance in this.Instances)
				await Instance.DeleteBootstrapInfo();
		}

		/// <summary>
		/// Applies any Bootstrap information.
		/// </summary>
		public virtual async Task ApplyBootstrapInfo()
		{
			foreach (Lwm2mObjectInstance Instance in this.Instances)
				await Instance.ApplyBootstrapInfo();
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
				if (this.HasInstances)
				{
					foreach (Lwm2mObjectInstance Instance in this.Instances)
						Instance.EncodeObjectLinks(IncludeSecurity, Output);
				}
				else
				{
					Output.Append(',');
					Output.Append("</");
					Output.Append(this.id.ToString());
					Output.Append('>');

					this.EncodeLinkParameters(Output);
				}
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
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void GET(CoapMessage Request, CoapResponse Response)
		{
			bool FromBootstrapServer = this.client.IsFromBootstrapServer(Request);

			if (this.id == 0 && !FromBootstrapServer)
			{
				Response.RST(CoapCode.Unauthorized);
				return;
			}

			if (!string.IsNullOrEmpty(Request.SubPath))
			{
				Response.RST(CoapCode.NotFound);
				return;
			}

			if (!Request.IsAcceptable(CoreLinkFormat.ContentFormatCode))
			{
				Response.RST(CoapCode.NotAcceptable);
				return;
			}

			StringBuilder Output = new StringBuilder();
			this.EncodeObjectLinks(this.client.State == Lwm2mState.Bootstrap && FromBootstrapServer, Output);

			Response.Respond(CoapCode.Content, Encoding.UTF8.GetBytes(Output.ToString()), 64,
				new CoapOptionContentFormat(CoreLinkFormat.ContentFormatCode));
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.Path + ": " + this.GetType().FullName;
		}

		/// <summary>
		/// Event raised after the resource has been registered
		/// </summary>
		public event EventHandler OnAfterRegister = null;

		/// <summary>
		/// Called after the resource has been registered on a CoAP Endpoint.
		/// </summary>
		public virtual void AfterRegister()
		{
			foreach (Lwm2mObjectInstance Instance in this.Instances)
			{
				this.client.CoapEndpoint.Register(Instance);
				Instance.AfterRegister(this.client);
			}

			try
			{
				this.OnAfterRegister?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}
	}
}
