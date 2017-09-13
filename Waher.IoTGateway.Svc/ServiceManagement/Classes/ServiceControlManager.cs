using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;

namespace Waher.IoTGateway.Svc.ServiceManagement.Classes
{
	internal class ServiceControlManager : SafeHandle
	{
		internal ServiceControlManager() : base(IntPtr.Zero, ownsHandle: true)
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

		internal static ServiceControlManager Connect(string machineName, string databaseName, ServiceControlManagerAccessRights desiredAccessRights)
		{
			var mgr = Win32.OpenSCManagerW(machineName, databaseName, desiredAccessRights);

			if (mgr.IsInvalid)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return mgr;
		}

		public ServiceHandle CreateService(string serviceName, string displayName, string binaryPath, ServiceType serviceType, ServiceStartType startupType, ErrorSeverity errorSeverity, Win32ServiceCredentials credentials)
		{
			var service = Win32.CreateServiceW(this, serviceName, displayName, ServiceControlAccessRights.All, serviceType, startupType, errorSeverity,
				binaryPath, null, IntPtr.Zero, null, credentials.UserName, credentials.Password);

			if (service.IsInvalid)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return service;
		}

		public ServiceHandle OpenService(string serviceName, ServiceControlAccessRights desiredControlAccess)
		{
			if (!TryOpenService(serviceName, desiredControlAccess, out ServiceHandle service, out Win32Exception errorException))
				throw errorException;

			return service;
		}

		public virtual bool TryOpenService(string serviceName, ServiceControlAccessRights desiredControlAccess, out ServiceHandle serviceHandle, 
			out Win32Exception errorException)
		{
			var service = Win32.OpenServiceW(this, serviceName, desiredControlAccess);

			if (service.IsInvalid)
			{
				errorException = new Win32Exception(Marshal.GetLastWin32Error());
				serviceHandle = null;
				return false;
			}

			serviceHandle = service;
			errorException = null;
			return true;
		}
	}
}
