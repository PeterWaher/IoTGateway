using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Waher.IoTGateway.Svc.ServiceManagement.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ServiceTableEntry
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		internal string serviceName;

		internal IntPtr serviceMainFunction;
	}
}
