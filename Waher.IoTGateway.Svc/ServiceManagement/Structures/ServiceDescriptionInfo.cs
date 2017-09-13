using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Waher.IoTGateway.Svc.ServiceManagement.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ServiceDescriptionInfo
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		private string serviceDescription;

		public ServiceDescriptionInfo(string serviceDescription)
		{
			this.serviceDescription = serviceDescription;
		}

		public string ServiceDescription
		{
			get { return serviceDescription; }
			set { serviceDescription = value; }
		}
	}
}
