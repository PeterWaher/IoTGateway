using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M object instances.
	/// </summary>
	public abstract class Lwm2mObjectInstance : CoapResource
	{
		private Lwm2mObject obj = null;
		private int subId;

		/// <summary>
		/// Base class for all LWM2M objects.
		/// </summary>
		/// <param name="Parent">Parent object.</param>
		/// <param name="SubId">ID of object.</param>
		public Lwm2mObjectInstance(Lwm2mObject Parent, int SubId)
			: base(Parent.Path + "/" + SubId.ToString())
		{
			this.subId = SubId;

			Parent.Add(this);
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
		/// Sub-ID of object instance.
		/// </summary>
		public int SubId
		{
			get { return this.subId; }
		}

		/// <summary>
		/// Deletes any Bootstrap information.
		/// </summary>
		public virtual void DeleteBootstrapInfo()
		{
		}

	}
}
