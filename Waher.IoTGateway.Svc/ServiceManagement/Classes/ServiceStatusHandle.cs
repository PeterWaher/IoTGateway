using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Waher.IoTGateway.Svc.ServiceManagement.Classes
{
	internal class ServiceStatusHandle : SafeHandle
	{
		internal ServiceStatusHandle() : base(IntPtr.Zero, ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle()
		{
			return Win32.CloseServiceHandle(handle);
		}

		public override bool IsInvalid
		{
			[System.Security.SecurityCritical]
			get
			{
				return handle == IntPtr.Zero;
			}
		}
	}
}
