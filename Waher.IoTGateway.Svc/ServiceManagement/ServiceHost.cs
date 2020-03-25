using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.IoTGateway.Svc.ServiceManagement.Classes;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;

namespace Waher.IoTGateway.Svc.ServiceManagement
{
	internal delegate void ServiceMainFunction(int numArs, IntPtr argPtrPtr);

	/// <summary>
	/// Handles interaction with Windows Service API.
	/// </summary>
	/// <remarks>
	/// Code is based on the <see cref="https://github.com/dasMulli/dotnet-win32-service"/> project by Martin Andreas Ullrich, with an MIT license.
	/// Modificantions has been made to provide support for custom service commands, Puase, Resume as well as to simplify, structure and adapt it to 
	/// the IoT Gateway project and architecture.
	/// </remarks>
	public class ServiceHost
	{
		private ServiceStatus serviceStatus;
		private ServiceStatusHandle serviceStatusHandle;
		private readonly string serviceName;
		private uint checkpointCounter = 0;
		private int win32ExitCode = 0;
		private Exception exception = null;
		private ManualResetEvent terminating = null;
		private Timer pendingTimer = null;

		internal ServiceHost(string ServiceName)
		{
			this.serviceName = ServiceName;
			this.serviceStatus = new ServiceStatus(ServiceType.Win32OwnProcess, ServiceState.StartPending,
				ServiceAcceptedControlCommandsFlags.None, win32ExitCode: 0, serviceSpecificExitCode: 0, checkPoint: 0, waitHint: 0);
		}

		/// <summary>
		/// Runs the service
		/// </summary>
		public int Run()
		{
			ServiceTableEntry[] serviceTable = new ServiceTableEntry[2]; // second one is null/null to indicate termination
			serviceTable[0].serviceName = this.serviceName;
			serviceTable[0].serviceMainFunction = Marshal.GetFunctionPointerForDelegate((ServiceMainFunction)this.ServiceMainFunction);

			try
			{
				Log.Informational("Starting service control dispatcher.");

				// StartServiceCtrlDispatcherW call returns when ServiceMainFunction exits
				if (!Win32.StartServiceCtrlDispatcherW(serviceTable))
					throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			catch (DllNotFoundException dllException)
			{
				Log.Critical(dllException);
				throw new PlatformNotSupportedException("Unable to call windows service management API.", dllException);
			}

			if (this.exception is null)
				return this.win32ExitCode;
			else
				throw this.exception;
		}

		private void ServiceMainFunction(int numArgs, IntPtr argPtrPtr)
		{
			Log.Informational("Entering service main function.");

			serviceStatusHandle = Win32.RegisterServiceCtrlHandlerExW(this.serviceName,
				(ServiceControlHandler)this.HandleServiceControlCommand, IntPtr.Zero);

			if (serviceStatusHandle.IsInvalid)
			{
				this.exception = new Win32Exception(Marshal.GetLastWin32Error());
				return;
			}

			this.terminating = new ManualResetEvent(false);
			try
			{
				ReportServiceStatus(ServiceState.StartPending, ServiceAcceptedControlCommandsFlags.None);

				Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

				Gateway.GetDatabaseProvider += Program.GetDatabase;
				Gateway.RegistrationSuccessful += Program.RegistrationSuccessful;
				Gateway.OnTerminate += (sender, e) => this.terminating?.Set();

				if (!Gateway.Start(false, true, Program.InstanceName).Result)
					throw new Exception("Gateway being started in another process.");

				ReportServiceStatus(ServiceState.Running,
					ServiceAcceptedControlCommandsFlags.Stop |
					ServiceAcceptedControlCommandsFlags.PauseContinue);

				while (!this.terminating.WaitOne(60000, false))
					;

				ReportServiceStatus(ServiceState.StopPending, ServiceAcceptedControlCommandsFlags.None);

				Gateway.Stop();
				Log.Terminate();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				ReportServiceStatus(ServiceState.Stopped, ServiceAcceptedControlCommandsFlags.None);

				this.terminating?.Dispose();
				this.terminating = null;
			}
		}

		private void ReportServiceStatus(ServiceState state, ServiceAcceptedControlCommandsFlags acceptedControlCommands)
		{
			if (serviceStatus.State == ServiceState.Stopped)
				return;

			serviceStatus.State = state;
			serviceStatus.AcceptedControlCommands = acceptedControlCommands;
			serviceStatus.Win32ExitCode = state == ServiceState.Stopped ? this.win32ExitCode : 0;

			switch (state)
			{
				case ServiceState.ContinuePending:
				case ServiceState.PausePending:
				case ServiceState.StartPending:
				case ServiceState.StopPending:
					serviceStatus.WaitHint = 2000;
					serviceStatus.CheckPoint = ++this.checkpointCounter;

					if (this.pendingTimer is null)
						this.pendingTimer = new Timer(this.ResetPendingStatus, null, 1000, 1000);

					break;

				default:
					serviceStatus.WaitHint = 0;
					serviceStatus.CheckPoint = this.checkpointCounter = 0;

					this.pendingTimer?.Dispose();
					this.pendingTimer = null;
					break;
			}

			Win32.SetServiceStatus(serviceStatusHandle, ref serviceStatus);
		}

		private void ResetPendingStatus(object State)
		{
			ReportServiceStatus(serviceStatus.State, serviceStatus.AcceptedControlCommands);
		}

		private void HandleServiceControlCommand(ServiceControlCommand command, uint eventType, IntPtr eventData, IntPtr eventContext)
		{
			try
			{
				Log.Informational("Service command received: " + command.ToString());

				switch (command)
				{
					case ServiceControlCommand.Stop:
						this.win32ExitCode = 0;
						this.terminating?.Set();
						break;

					case ServiceControlCommand.Interrogate:
						this.ResetPendingStatus(null);
						break;

					case ServiceControlCommand.Pause:
						ReportServiceStatus(ServiceState.PausePending, ServiceAcceptedControlCommandsFlags.None);

						Gateway.Stop();
						Log.Terminate();

						ReportServiceStatus(ServiceState.Paused,
							ServiceAcceptedControlCommandsFlags.Stop |
							ServiceAcceptedControlCommandsFlags.PauseContinue);
						break;

					case ServiceControlCommand.Continue:
						ReportServiceStatus(ServiceState.ContinuePending, ServiceAcceptedControlCommandsFlags.None);

						if (Gateway.Start(false, true, Program.InstanceName).Result)
						{
							ReportServiceStatus(ServiceState.Running,
								ServiceAcceptedControlCommandsFlags.Stop |
								ServiceAcceptedControlCommandsFlags.PauseContinue);
						}
						else
						{
							Log.Error("Unable to resume from paused state.");
							
							ReportServiceStatus(ServiceState.Paused,
								ServiceAcceptedControlCommandsFlags.Stop |
								ServiceAcceptedControlCommandsFlags.PauseContinue);
						}
						break;

					case ServiceControlCommand.DeviceEvent:
					case ServiceControlCommand.HardwareProfileChange:
					case ServiceControlCommand.NetBindAdd:
					case ServiceControlCommand.NetBindDisable:
					case ServiceControlCommand.NetBindEnable:
					case ServiceControlCommand.NetBindRemoved:
					case ServiceControlCommand.Paramchange:
					case ServiceControlCommand.PowerEvent:
					case ServiceControlCommand.SessionChange:
					case ServiceControlCommand.PreShutdown:
					case ServiceControlCommand.Shutdown:
					case ServiceControlCommand.TimeChange:
					case ServiceControlCommand.TriggerEvent:
					case ServiceControlCommand.UserModeReboot:
						// TODO: Implement
						break;

					default:
						if ((int)command >= 128)
							Gateway.ExecuteServiceCommand((int)command);
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
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
				using (ServiceControlManager mgr = ServiceControlManager.Connect(null, null, ServiceControlManagerAccessRights.All))
				{
					if (mgr.TryOpenService(this.serviceName, ServiceControlAccessRights.All, out ServiceHandle existingService,
						out Win32Exception errorException))
					{
						using (existingService)
						{
							existingService.ChangeConfig(DisplayName, Path, ServiceType.Win32OwnProcess,
								StartType, ErrorSeverity.Normal, Credentials);

							if (!string.IsNullOrEmpty(Description))
								existingService.SetDescription(Description);

							if (FailureActions != null)
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
							using (ServiceHandle svc = mgr.CreateService(this.serviceName, DisplayName, Path, ServiceType.Win32OwnProcess,
								StartType, ErrorSeverity.Normal, Credentials))
							{
								if (!string.IsNullOrEmpty(Description))
									svc.SetDescription(Description);

								if (FailureActions != null)
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
						}
						else
							throw errorException;
					}
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
				using (ServiceControlManager mgr = ServiceControlManager.Connect(null, null, ServiceControlManagerAccessRights.All))
				{
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
			}
			catch (DllNotFoundException dllException)
			{
				throw new PlatformNotSupportedException("Unable to call windows service management API.", dllException);
			}
		}

	}
}
