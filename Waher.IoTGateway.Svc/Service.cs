using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Events.WindowsEventLog;

namespace Waher.IoTGateway.Svc
{
	public partial class Service : ServiceBase
	{
		public Service()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			if (!Gateway.Start(false, new WindowsEventLog("IoTGateway", "IoTGateway", 512)))
				throw new Exception("Gateway being started in another process.");
		}

		protected override void OnStop()
		{
			Gateway.Stop();
			Log.Terminate();
		}

		protected override void OnCustomCommand(int command)
		{
			if (!Gateway.ExecuteServiceCommand(command))
				base.OnCustomCommand(command);
		}
	}
}
