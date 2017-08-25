using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M objects.
	/// </summary>
	public abstract class Lwm2mObject
	{
		private SortedDictionary<int, Lwm2mObjectInstance> instances = new SortedDictionary<int, Lwm2mObjectInstance>();
		private int id;

		/// <summary>
		/// Base class for all LWM2M objects.
		/// </summary>
		/// <param name="Id">ID of object.</param>
		public Lwm2mObject(int Id)
		{
			this.id = Id;
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

				lock(this.instances)
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

			lock (this.instances)
			{
				if (this.instances.ContainsKey(Instance.SubId))
				{
					throw new ArgumentException("An object with ID " + Instance.SubId +
						" already is registered.", nameof(Instance));
				}

				this.instances[Instance.SubId] = Instance;
			}
		}

		/// <summary>
		/// Unregisters an LWM2M object instance from the client.
		/// </summary>
		/// <param name="Instance">Object.</param>
		public bool Remove(Lwm2mObjectInstance Instance)
		{
			lock (this.instances)
			{
				if (this.instances.TryGetValue(Instance.SubId, out Lwm2mObjectInstance Inst) && Inst == Instance)
					return this.instances.Remove(Instance.SubId);
				else
					return false;
			}
		}

	}
}
