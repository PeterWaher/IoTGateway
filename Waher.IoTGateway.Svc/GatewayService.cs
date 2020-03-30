using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Waher.Events;
using Waher.Persistence;

namespace Waher.IoTGateway.Svc
{
	/// <summary>
	/// Gateway Service
	/// </summary>
	public class GatewayService : ServiceBase
	{
		private bool autoPaused = false;

		/// <summary>
		/// Gateway Service
		/// </summary>
		/// <param name="ServiceName">Service Name</param>
		public GatewayService(string ServiceName)
			: base()
		{
			this.ServiceName = ServiceName;
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

				using (PendingTimer Timer = new PendingTimer(this))
				{
					Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

					Gateway.GetDatabaseProvider += Program.GetDatabase;
					Gateway.RegistrationSuccessful += Program.RegistrationSuccessful;
					Gateway.OnTerminate += this.TerminateService;

					Started = Gateway.Start(false, true, Program.InstanceName).Result;
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
					this.service.RequestAdditionalTime(2000);
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

				using (PendingTimer Timer = new PendingTimer(this))
				{
					Started = Gateway.Start(false, true, Program.InstanceName).Result;
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
					this.Flush();
					return true;

				case PowerBroadcastStatus.ResumeAutomatic:
				case PowerBroadcastStatus.ResumeCritical:
				case PowerBroadcastStatus.ResumeSuspend:
					if (this.autoPaused)
					{
						Log.Notice("Resuming service.");
						this.autoPaused = false;
						this.OnContinue();
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

		protected override void OnSessionChange(SessionChangeDescription changeDescription)
		{
			Log.Notice("Session changed.",
				new KeyValuePair<string, object>("SessionId", changeDescription.SessionId),
				new KeyValuePair<string, object>("Reason", changeDescription.Reason.ToString()));
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
				using (PendingTimer Timer = new PendingTimer(this))
				{
					this.Flush();
					Gateway.Stop().Wait();
					Log.Terminate();
				}
			}
			catch (Exception ex)
			{
				Log.Alert(ex);
			}
		}

		private void Flush()
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
