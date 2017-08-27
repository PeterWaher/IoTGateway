using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.CoAP.LWM2M
{
	/// <summary>
	/// LWM2M Security object.
	/// </summary>
    public class Lwm2mSecurityObject : Lwm2mObject
    {
		/// <summary>
		/// LWM2M Security object.
		/// </summary>
		public Lwm2mSecurityObject()
			: base(0, new Lwm2mSecurityObjectInstance())
		{
		}
    }
}
