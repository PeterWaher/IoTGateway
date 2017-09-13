using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Waher.IoTGateway.Svc.ServiceManagement.Classes;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;

namespace Waher.IoTGateway.Svc.ServiceManagement
{
	internal delegate void ServiceControlHandler(ServiceControlCommand control, uint eventType, IntPtr eventData, IntPtr eventContext);

	/// <summary>
	/// Handles interaction with Windows Service API.
	/// </summary>
	public static class Win32
    {
		internal const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
		internal const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool CloseServiceHandle(IntPtr handle);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool StartServiceCtrlDispatcherW([MarshalAs(UnmanagedType.LPArray)] ServiceTableEntry[] serviceTable);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern ServiceStatusHandle RegisterServiceCtrlHandlerExW(string serviceName, ServiceControlHandler serviceControlHandler, IntPtr context);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool SetServiceStatus(ServiceStatusHandle statusHandle, ref ServiceStatus pServiceStatus);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern ServiceControlManager OpenSCManagerW(string machineName, string databaseName, ServiceControlManagerAccessRights dwAccess);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern ServiceHandle CreateServiceW(
			ServiceControlManager serviceControlManager,
			string serviceName,
			string displayName,
			ServiceControlAccessRights desiredControlAccess,
			ServiceType serviceType,
			ServiceStartType startType,
			ErrorSeverity errorSeverity,
			string binaryPath,
			string loadOrderGroup,
			IntPtr outUIntTagId,
			string dependencies,
			string serviceUserName,
			string servicePassword);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool ChangeServiceConfigW(
			ServiceHandle service,
			ServiceType serviceType,
			ServiceStartType startType,
			ErrorSeverity errorSeverity,
			string binaryPath,
			string loadOrderGroup,
			IntPtr outUIntTagId,
			string dependencies,
			string serviceUserName,
			string servicePassword,
			string displayName);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern ServiceHandle OpenServiceW(ServiceControlManager serviceControlManager, string serviceName,
			ServiceControlAccessRights desiredControlAccess);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool StartServiceW(ServiceHandle service, uint argc, IntPtr wargv);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool DeleteService(ServiceHandle service);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool ChangeServiceConfig2W(ServiceHandle service, ServiceConfigInfoTypeLevel infoTypeLevel, IntPtr info);


		// https://msdn.microsoft.com/en-us/library/windows/desktop/ms686016(v=vs.85).aspx
		// https://msdn.microsoft.com/en-us/library/windows/desktop/ms683242(v=vs.85).aspx

		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
		public delegate bool HandlerRoutine(CtrlTypes CtrlType);

	}
}
