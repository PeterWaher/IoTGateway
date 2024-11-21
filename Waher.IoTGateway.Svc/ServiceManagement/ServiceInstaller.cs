using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using Waher.Events;
using Waher.IoTGateway.Svc.ServiceManagement.Classes;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.IoTGateway.Svc.ServiceManagement.Structures;
using static Waher.IoTGateway.Svc.ServiceManagement.Win32;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Waher.IoTGateway.Svc.ServiceManagement
{
	internal delegate void ServiceMainFunction(int numArs, IntPtr argPtrPtr);

	/// <summary>
	/// Handles interaction with Windows Service API.
	/// </summary>
	public class ServiceInstaller
	{
		private readonly string serviceName;
		private readonly string instanceName;

		internal ServiceInstaller(string ServiceName, string InstanceName)
		{
			this.serviceName = ServiceName;
			this.instanceName = InstanceName;

			if (!string.IsNullOrEmpty(InstanceName))
				this.serviceName += " " + InstanceName;
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

			if (!string.IsNullOrEmpty(this.instanceName))
				Path += " -instance \"" + this.instanceName + "\"";

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

						if (FailureActions is not null)
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

						if (FailureActions is not null)
						{
							svc.SetFailureActions(FailureActions);
							svc.SetFailureActionFlag(true);
						}
						else
							svc.SetFailureActionFlag(false);

						try
						{
							// Attempt to give the local service account access rights to start
							// the service. This will be used by the update procedure to trigger
							// a start of the service after installation has completed.

							using ServiceController Service = new(this.serviceName);

							bool HandleRetrieved = false;
							Service.ServiceHandle.DangerousAddRef(ref HandleRetrieved);

							try
							{
								IntPtr ServiceHandle = Service.ServiceHandle.DangerousGetHandle();

								QueryServiceObjectSecurity(ServiceHandle, SecurityInfos.DiscretionaryAcl, null, 0, out uint Size);

								if (Size <= 0 || Size > int.MaxValue)
									throw new Exception("Unable to get Service Security Object");

								byte[] Buffer = new byte[Size];
								if (!QueryServiceObjectSecurity(ServiceHandle, SecurityInfos.DiscretionaryAcl, Buffer, Size, out Size))
									throw new Win32Exception(Marshal.GetLastWin32Error());

								RawSecurityDescriptor Descriptor = new(Buffer, 0);

								if (Descriptor.DiscretionaryAcl is not null)
								{
									int i = 0;
									bool Found = false;

									foreach (object Item in Descriptor.DiscretionaryAcl)
									{
										if (Item is CommonAce Ace)
										{
											if (Ace.SecurityIdentifier.IsWellKnown(WellKnownSidType.LocalServiceSid))
											{
												if (Ace.AceQualifier != AceQualifier.AccessAllowed || (Ace.AccessMask & (int)ServiceAccessRights.Start) == 0)
													Descriptor.DiscretionaryAcl.RemoveAce(i);
												else
												{
													Found = true;
													break;
												}
											}
										}

										i++;
									}

									if (!Found)
									{
										SecurityIdentifier LocalServiceSid = new(WellKnownSidType.LocalServiceSid, null);
										CommonAce LocalServiceAce = new(AceFlags.None, AceQualifier.AccessAllowed, (int)ServiceAccessRights.Start, LocalServiceSid, false, null);
										Descriptor.DiscretionaryAcl.InsertAce(Descriptor.DiscretionaryAcl.Count, LocalServiceAce);

										int Len = Descriptor.BinaryLength;
										byte[] Bin = new byte[Len];

										Descriptor.GetBinaryForm(Bin, 0);

										if (!SetServiceObjectSecurity(ServiceHandle, SecurityInfos.DiscretionaryAcl, Bin))
											throw new Win32Exception(Marshal.GetLastWin32Error());
									}
								}
							}
							finally
							{
								if (HandleRetrieved)
									Service.ServiceHandle.DangerousRelease();
							}
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}

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

#pragma warning restore CA1416 // Validate platform compatibility
