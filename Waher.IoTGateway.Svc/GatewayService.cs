using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using Waher.Events;
using Waher.IoTGateway.Svc.ServiceManagement;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.Persistence;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Waher.IoTGateway.Svc
{
	/// <summary>
	/// Gateway Service
	/// </summary>
	public class GatewayService : ServiceBase
	{
		private bool autoPaused = false;
		private bool starting = false;

		/// <summary>
		/// Gateway Service
		/// </summary>
		/// <param name="ServiceName">Service Name</param>
		/// <param name="InstanceName">Name of service instance.</param>
		public GatewayService(string ServiceName, string InstanceName)
			: base()
		{
			this.ServiceName = ServiceName;
			if (!string.IsNullOrEmpty(InstanceName))
				this.ServiceName += " " + InstanceName;

			this.AutoLog = true;
			this.CanHandlePowerEvent = true;
			this.CanHandleSessionChangeEvent = true;
			this.CanPauseAndContinue = true;
			this.CanShutdown = true;
			this.CanStop = true;
		}

		protected override void OnStart(string[] args)
		{
			try
			{
				bool Started;

				if (this.starting)
					Started = false;
				else
				{
					using PendingTimer Timer = new(this);

					Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

					Gateway.GetDatabaseProvider += Program.GetDatabase;
					Gateway.RegistrationSuccessful += Program.RegistrationSuccessful;
					Gateway.OnTerminate += this.TerminateService;

					this.starting = true;
					try
					{
						Started = Gateway.Start(false, true, Program.InstanceName).Result;
					}
					finally
					{
						this.starting = false;
					}
				}

				if (!Started)
				{
					Log.Alert("Gateway being started in another process.");
					ThreadPool.QueueUserWorkItem(_ => this.Stop());
					return;
				}
			}
			catch (Exception ex)
			{
				this.ExitCode = 1;
				Log.Alert(ex);
			}
		}

		private void TerminateService(object Sender, EventArgs e)
		{
			this.ExitCode = 1;
			ThreadPool.QueueUserWorkItem(_ => this.Stop());
		}

		private class PendingTimer : IDisposable
		{
			private readonly GatewayService service;
			private Timer timer;
			private bool disposed = false;

			public PendingTimer(GatewayService Service)
			{
				this.service = Service;
				this.timer = new Timer(this.MoreTime, null, 0, 1000);
			}

			public void Dispose()
			{
				this.disposed = true;
				this.timer?.Dispose();
				this.timer = null;
			}

			private void MoreTime(object State)
			{
				if (!this.disposed)
				{
					try
					{
						this.service.RequestAdditionalTime(2000);
					}
					catch (InvalidOperationException)
					{
						this.timer?.Dispose();
						this.timer = null;
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		protected override void OnPause()
		{
			this.OnStop();
		}

		protected override void OnContinue()
		{
			try
			{
				bool Started;

				if (this.starting)
					Started = false;
				else
				{
					using PendingTimer Timer = new(this);

					this.starting = true;
					try
					{
						Started = Gateway.Start(false, true, Program.InstanceName).Result;
					}
					finally
					{
						this.starting = false;
					}
				}

				if (!Started)
				{
					Log.Alert("Gateway being started in another process.");
					ThreadPool.QueueUserWorkItem(_ => this.Stop());
					return;
				}
			}
			catch (Exception ex)
			{
				this.ExitCode = 1;
				Log.Alert(ex);
			}
		}

		protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
		{
			switch (powerStatus)
			{
				case PowerBroadcastStatus.BatteryLow:
				case PowerBroadcastStatus.OemEvent:
				case PowerBroadcastStatus.PowerStatusChange:
				case PowerBroadcastStatus.QuerySuspend:
					Flush();
					return true;

				case PowerBroadcastStatus.ResumeAutomatic:
				case PowerBroadcastStatus.ResumeCritical:
				case PowerBroadcastStatus.ResumeSuspend:
					if (this.autoPaused)
					{
						this.autoPaused = false;

						if (this.starting)
							Log.Warning("Gateway is in the process of starting, called from another source.");
						else
						{
							Log.Notice("Resuming service.");
							this.OnContinue();
						}
					}
					return true;

				case PowerBroadcastStatus.Suspend:
					this.autoPaused = true;
					Log.Notice("Suspending service.");
					this.OnStop();
					return true;

				case PowerBroadcastStatus.QuerySuspendFailed:
				default:
					return true;
			}
		}

		protected override void OnSessionChange(SessionChangeDescription ChangeDescription)
		{
			int SessionId = ChangeDescription.SessionId;
			List<KeyValuePair<string, object>> Tags = new()
			{
				new("SessionId", SessionId)
			};

			AddWtsUserName(Tags, SessionId);
			AddWtsName(Tags, "Initial Program", SessionId, WtsInfoClass.WTSInitialProgram);
			AddWtsName(Tags, "Application Name", SessionId, WtsInfoClass.WTSApplicationName);
			AddWtsName(Tags, "Working Directory", SessionId, WtsInfoClass.WTSWorkingDirectory);
			AddWtsName(Tags, "OEM ID", SessionId, WtsInfoClass.WTSOEMId);
			AddWtsName(Tags, "Station Name", SessionId, WtsInfoClass.WTSWinStationName);
			AddWtsName(Tags, "Connect State", SessionId, WtsInfoClass.WTSConnectState);
			AddWtsName(Tags, "Client Build Number", SessionId, WtsInfoClass.WTSClientBuildNumber);
			AddWtsName(Tags, "Client Name", SessionId, WtsInfoClass.WTSClientName);
			AddWtsName(Tags, "Client Directory", SessionId, WtsInfoClass.WTSClientDirectory);
			AddWtsName(Tags, "Client Product ID", SessionId, WtsInfoClass.WTSClientProductId);
			AddWtsName(Tags, "Client Hardware ID", SessionId, WtsInfoClass.WTSClientHardwareId);
			AddWtsName(Tags, "Client Address", SessionId, WtsInfoClass.WTSClientAddress);
			AddWtsName(Tags, "Client Display", SessionId, WtsInfoClass.WTSClientDisplay);
			AddWtsName(Tags, "Client Protocol Type", SessionId, WtsInfoClass.WTSClientProtocolType);
			AddWtsName(Tags, "Idle Time", SessionId, WtsInfoClass.WTSIdleTime);
			AddWtsName(Tags, "Logon Time", SessionId, WtsInfoClass.WTSLogonTime);
			AddWtsName(Tags, "Incoming Bytes", SessionId, WtsInfoClass.WTSIncomingBytes);
			AddWtsName(Tags, "Outgoing Bytes", SessionId, WtsInfoClass.WTSOutgoingBytes);
			AddWtsName(Tags, "Incoming Frames", SessionId, WtsInfoClass.WTSIncomingFrames);
			AddWtsName(Tags, "Outgoing Frames", SessionId, WtsInfoClass.WTSOutgoingFrames);
			AddWtsName(Tags, "Client Info", SessionId, WtsInfoClass.WTSClientInfo);
			AddWtsName(Tags, "Session Info", SessionId, WtsInfoClass.WTSSessionInfo);

			switch (ChangeDescription.Reason)
			{
				case SessionChangeReason.ConsoleConnect:
					Log.Alert("User connected to machine via console interface.", Tags.ToArray());
					break;

				case SessionChangeReason.ConsoleDisconnect:
					Log.Alert("User disconnected console interface.", Tags.ToArray());
					break;
				case SessionChangeReason.RemoteConnect:
					Log.Alert("User connected remotely to machine.", Tags.ToArray());
					break;
				case SessionChangeReason.RemoteDisconnect:
					Log.Alert("User disconnected remote interface.", Tags.ToArray());
					break;
				case SessionChangeReason.SessionLock:
					Log.Alert("User session locked.", Tags.ToArray());
					break;
				case SessionChangeReason.SessionLogoff:
					Log.Alert("User logged off.", Tags.ToArray());
					break;
				case SessionChangeReason.SessionLogon:
					Log.Alert("User logged on.", Tags.ToArray());
					break;
				case SessionChangeReason.SessionRemoteControl:
					Log.Alert("User remote control status of session has changed.", Tags.ToArray());
					break;
				case SessionChangeReason.SessionUnlock:
					Log.Alert("User session unlocked.", Tags.ToArray());
					break;

				default:
					Tags.Add(new KeyValuePair<string, object>("Reason", ChangeDescription.Reason.ToString()));
					Log.Alert("Session changed.", Tags.ToArray());
					break;
			}
		}

		private static void AddWtsUserName(List<KeyValuePair<string, object>> Tags, int SessionId)
		{
			string Value = GetUserName(SessionId);
			if (!string.IsNullOrEmpty(Value))
				Tags.Add(new KeyValuePair<string, object>("User Name", Value));
		}

		private static void AddWtsName(List<KeyValuePair<string, object>> Tags, string Key, int SessionId, WtsInfoClass InfoClass)
		{
			string Value = GetWtsName(SessionId, InfoClass);
			if (!string.IsNullOrEmpty(Value))
				Tags.Add(new KeyValuePair<string, object>(Key, Value));
		}

		private static string GetUserName(int SessionId)
		{
			try
			{
				string UserName = GetWtsName(SessionId, WtsInfoClass.WTSUserName);
				if (string.IsNullOrEmpty(UserName))
					return null;

				string Domain = GetWtsName(SessionId, WtsInfoClass.WTSDomainName);
				if (!string.IsNullOrEmpty(Domain))
					UserName = Domain + "\\" + UserName;

				return UserName;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return null;
			}
		}

		private static string GetWtsName(int SessionId, WtsInfoClass InfoClass)
		{
			try
			{
				if (Win32.WTSQuerySessionInformation(IntPtr.Zero, SessionId, InfoClass, out IntPtr Buffer, out int StrLen) && StrLen > 1)
				{
					try
					{
						string Name = Marshal.PtrToStringAnsi(Buffer);
						Win32.WTSFreeMemory(Buffer);
						Buffer = IntPtr.Zero;

						return Name;
					}
					finally
					{
						if (Buffer != IntPtr.Zero)
							Win32.WTSFreeMemory(Buffer);
					}
				}
				else
					return null;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return null;
			}
		}

		protected override void OnShutdown()
		{
			Log.Notice("System is shutting down.");
			this.Stop();
		}

		protected override void OnStop()
		{
			Log.Notice("Service is being stopped.");
			try
			{
				using PendingTimer Timer = new(this);

				Flush();
				Gateway.Stop().Wait();
				Log.Terminate();
			}
			catch (Exception ex)
			{
				Log.Alert(ex);
			}
		}

		private static void Flush()
		{
			if (Database.HasProvider)
				Database.Provider.Flush().Wait();

			if (Ledger.HasProvider)
				Ledger.Provider.Flush().Wait();
		}

		protected override void OnCustomCommand(int command)
		{
			Gateway.ExecuteServiceCommand(command);
		}
	}
}

#pragma warning restore CA1416 // Validate platform compatibility
