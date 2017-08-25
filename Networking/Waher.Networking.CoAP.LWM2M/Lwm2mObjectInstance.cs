using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// Base class for all LWM2M object instances.
	/// </summary>
    public abstract class Lwm2mObjectInstance
    {
		private int subId;

		/// <summary>
		/// Base class for all LWM2M objects.
		/// </summary>
		/// <param name="SubId">ID of object.</param>
		public Lwm2mObjectInstance(int SubId)
		{
			this.subId = SubId;
		}

		/// <summary>
		/// Sub-ID of object instance.
		/// </summary>
		public int SubId
		{
			get { return this.subId; }
		}
    }
}
