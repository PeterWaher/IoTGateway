using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;

namespace Waher.IoTGateway.Svc.ServiceManagement.Classes
{
	internal class ServiceHandle : SafeHandle
	{
		internal ServiceHandle() : base(IntPtr.Zero, ownsHandle: true)
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

		public virtual void Start(bool throwIfAlreadyRunning = true)
		{
			if (!Win32.StartServiceW(this, 0, IntPtr.Zero))
			{
				int win32Error = Marshal.GetLastWin32Error();

				if (win32Error != Win32.ERROR_SERVICE_ALREADY_RUNNING || throwIfAlreadyRunning)
					throw new Win32Exception(win32Error);
			}
		}

		public virtual void Delete()
		{
			if (!Win32.DeleteService(this))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		public virtual void SetDescription(string description)
		{
			ServiceDescriptionInfo descriptionInfo = new ServiceDescriptionInfo(description ?? string.Empty);
			IntPtr lpDescriptionInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ServiceDescriptionInfo>());
			try
			{
				Marshal.StructureToPtr(descriptionInfo, lpDescriptionInfo, fDeleteOld: false);
				try
				{
					if (!Win32.ChangeServiceConfig2W(this, ServiceConfigInfoTypeLevel.ServiceDescription, lpDescriptionInfo))
						throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				finally
				{
					Marshal.DestroyStructure<ServiceDescriptionInfo>(lpDescriptionInfo);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(lpDescriptionInfo);
			}
		}

		public virtual void SetFailureActions(ServiceFailureActions serviceFailureActions)
		{
			ServiceFailureActionsInfo failureActions = serviceFailureActions is null ? ServiceFailureActionsInfo.Default : new ServiceFailureActionsInfo(serviceFailureActions.ResetPeriod, serviceFailureActions.RebootMessage, serviceFailureActions.RestartCommand, serviceFailureActions.Actions);
			IntPtr lpFailureActions = Marshal.AllocHGlobal(Marshal.SizeOf<ServiceFailureActionsInfo>());
			try
			{
				Marshal.StructureToPtr(failureActions, lpFailureActions, fDeleteOld: false);
				try
				{
					if (!Win32.ChangeServiceConfig2W(this, ServiceConfigInfoTypeLevel.FailureActions, lpFailureActions))
						throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				finally
				{
					Marshal.DestroyStructure<ServiceFailureActionsInfo>(lpFailureActions);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(lpFailureActions);
			}
		}

		public virtual void SetFailureActionFlag(bool enabled)
		{
			ServiceFailureActionsFlag failureActionsFlag = new ServiceFailureActionsFlag(enabled);
			IntPtr lpFailureActionsFlag = Marshal.AllocHGlobal(Marshal.SizeOf<ServiceFailureActionsFlag>());
			try
			{
				Marshal.StructureToPtr(failureActionsFlag, lpFailureActionsFlag, fDeleteOld: false);
				try
				{
					bool result = Win32.ChangeServiceConfig2W(this, ServiceConfigInfoTypeLevel.FailureActionsFlag, lpFailureActionsFlag);
					if (!result)
						throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				finally
				{
					Marshal.DestroyStructure<ServiceFailureActionsFlag>(lpFailureActionsFlag);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(lpFailureActionsFlag);
			}
		}

		public virtual void ChangeConfig(string displayName, string binaryPath, ServiceType serviceType, ServiceStartType startupType, ErrorSeverity errorSeverity, Win32ServiceCredentials credentials)
		{
			bool success = Win32.ChangeServiceConfigW(this, serviceType, startupType, errorSeverity, binaryPath, null, IntPtr.Zero, null, credentials.UserName, credentials.Password, displayName);
			if (!success)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
