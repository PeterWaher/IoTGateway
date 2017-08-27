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
		/// <param name="Id">ID of object.</param>
		/// <param name="SubId">ID of object instance.</param>
		public Lwm2mObjectInstance(int Id, int SubId)
			: base("/" + Id.ToString() + "/" + SubId.ToString())
		{
			this.subId = SubId;
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
