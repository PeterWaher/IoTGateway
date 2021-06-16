using System;
using System.ComponentModel;
using System.Reflection;
using Waher.IoTGateway.Svc.ServiceManagement.Classes;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;

namespace Waher.IoTGateway.Svc.ServiceManagement
{
	internal delegate void ServiceMainFunction(int numArs, IntPtr argPtrPtr);

	/// <summary>
	/// Handles interaction with Windows Service API.
	/// </summary>
	public class ServiceInstaller
	{
		private readonly string serviceName;

		internal ServiceInstaller(string ServiceName)
		{
			this.serviceName = ServiceName;
		}

		/// <summary>
		/// Installs the service.
		/// </summary>
		/// <param name="DisplayName">Service display name.</param>
		/// <param name="Description">Service description.</param>
		/// <param name="StartType">How the service should be started.</param>
		/// <param name="StartImmediately">If the service should be started immediately.</param>
		/// <param name="FailureActions">Service failure actions.</param>
		/// <param name="Credentials">Credentials to use when running service.</param>
		/// <returns>
		/// Return code:
		/// 
		/// 0: Installed, not started.
		/// 1: Installed, started.
		/// 2: Updated, not started.
		/// 3: Updated, started.
		/// </returns>
		/// <exception cref="Exception">If service could not be installed.</exception>
		public int Install(string DisplayName, string Description, ServiceStartType StartType, bool StartImmediately,
			ServiceFailureActions FailureActions, Win32ServiceCredentials Credentials)
		{
			string Path = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");

			try
			{
				using ServiceControlManager mgr = ServiceControlManager.Connect(null, null, ServiceControlManagerAccessRights.All);
			
				if (mgr.TryOpenService(this.serviceName, ServiceControlAccessRights.All, out ServiceHandle existingService,
					out Win32Exception errorException))
				{
					using (existingService)
					{
						existingService.ChangeConfig(DisplayName, Path, ServiceType.Win32OwnProcess, StartType, 
							ErrorSeverity.Normal, Credentials);

						if (!string.IsNullOrEmpty(Description))
							existingService.SetDescription(Description);

						if (!(FailureActions is null))
						{
							existingService.SetFailureActions(FailureActions);
							existingService.SetFailureActionFlag(true);
						}
						else
							existingService.SetFailureActionFlag(false);

						if (StartImmediately)
						{
							existingService.Start(throwIfAlreadyRunning: false);
							return 3;
						}
						else
							return 2;
					}
				}
				else
				{
					if (errorException.NativeErrorCode == Win32.ERROR_SERVICE_DOES_NOT_EXIST)
					{
						using ServiceHandle svc = mgr.CreateService(this.serviceName, DisplayName, Path, ServiceType.Win32OwnProcess,
							StartType, ErrorSeverity.Normal, Credentials);

						if (!string.IsNullOrEmpty(Description))
							svc.SetDescription(Description);

						if (!(FailureActions is null))
						{
							svc.SetFailureActions(FailureActions);
							svc.SetFailureActionFlag(true);
						}
						else
							svc.SetFailureActionFlag(false);

						if (StartImmediately)
						{
							svc.Start();
							return 1;
						}
						else
							return 0;
					}
					else
						throw errorException;
				}
			}
			catch (DllNotFoundException dllException)
			{
				throw new PlatformNotSupportedException("Unable to call windows service management API.", dllException);
			}
		}

		/// <summary>
		/// Uninstalls the service.
		/// </summary>
		/// <returns>If service was found and uninstalled (true), or if service was not found (false).</returns>
		/// <exception cref="Exception">If unable to uninstall an existing service, or API is not found.</exception>
		public bool Uninstall()
		{
			try
			{
				using ServiceControlManager mgr = ServiceControlManager.Connect(null, null, ServiceControlManagerAccessRights.All);
				
				if (mgr.TryOpenService(this.serviceName, ServiceControlAccessRights.All, out ServiceHandle existingService,
					out Win32Exception errorException))
				{
					using (existingService)
					{
						existingService.Delete();
						return true;
					}
				}
				else
				{
					if (errorException.NativeErrorCode == Win32.ERROR_SERVICE_DOES_NOT_EXIST)
						return false;
					else
						throw errorException;
				}
			}
			catch (DllNotFoundException dllException)
			{
				throw new PlatformNotSupportedException("Unable to call windows service management API.", dllException);
			}
		}

	}
}
