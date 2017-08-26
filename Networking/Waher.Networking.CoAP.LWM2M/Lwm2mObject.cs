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
		public Lwm2mObject(int Id)
			: base("/" + Id.ToString())
		{
			this.id = Id;
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
		/// Registers an LWM2M object instance on the client.
		/// </summary>
		/// <param name="Instance">Object.</param>
		public void Add(Lwm2mObjectInstance Instance)
		{
			if (Instance.SubId < 0)
				throw new ArgumentException("Invalid object ID.", nameof(Instance));

			if (Instance.Object != null)
				throw new ArgumentException("Instance already added to an object.", nameof(Instance));

			lock (this.instances)
			{
				if (this.instances.ContainsKey(Instance.SubId))
				{
					throw new ArgumentException("An object with ID " + Instance.SubId +
						" already is registered.", nameof(Instance));
				}

				this.instances[Instance.SubId] = Instance;
			}

			Instance.Object = this;

			this.client?.RegisterUpdateIfRegistered();
		}

		/// <summary>
		/// Unregisters an LWM2M object instance from the client.
		/// </summary>
		/// <param name="Instance">Object.</param>
		public bool Remove(Lwm2mObjectInstance Instance)
		{
			if (Instance.Object != this)
				return false;

			lock (this.instances)
			{
				if (this.instances.TryGetValue(Instance.SubId, out Lwm2mObjectInstance Inst) && Inst == Instance)
				{
					Instance.Object = null;
					if (!this.instances.Remove(Instance.SubId))
						return false;
				}
				else
					return false;
			}

			this.client?.RegisterUpdateIfRegistered();

			return true;
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public virtual void DeleteBootstrapInfo()
		{
			foreach (Lwm2mObjectInstance Instance in this.Instances)
				Instance.DeleteBootstrapInfo();
		}

	}
}
