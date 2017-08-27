using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M objects.
	/// </summary>
	public abstract class Lwm2mObject : CoapResource
	{
		private SortedDictionary<int, Lwm2mObjectInstance> instances = new SortedDictionary<int, Lwm2mObjectInstance>();
		private Lwm2mClient client = null;
		private int id;

		/// <summary>
		/// Base class for all LWM2M objects.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		/// <param name="Instances">Object instances.</param>
		public Lwm2mObject(int Id, params Lwm2mObjectInstance[] Instances)
			: base("/" + Id.ToString())
		{
			this.id = Id;

			foreach (Lwm2mObjectInstance Instance in Instances)
			{
				if (Instance.SubId < 0)
					throw new ArgumentException("Invalid object instance ID.", nameof(Instance));

				if (this.instances.ContainsKey(Instance.SubId))
				{
					throw new ArgumentException("An object instance with ID " + Instance.SubId +
						" already is registered.", nameof(Instance));
				}

				this.instances[Instance.SubId] = Instance;
				Instance.Object = this;
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
		public int Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Registered instances.
		/// </summary>
		public Lwm2mObjectInstance[] Instances
		{
			get
			{
				Lwm2mObjectInstance[] Result;

				Result = new Lwm2mObjectInstance[this.instances.Count];
				this.instances.Values.CopyTo(Result, 0);

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
				return this.instances.Count > 0;
			}
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public virtual void DeleteBootstrapInfo()
		{
			foreach (Lwm2mObjectInstance Instance in this.instances.Values)
				Instance.DeleteBootstrapInfo();
		}

	}
}
