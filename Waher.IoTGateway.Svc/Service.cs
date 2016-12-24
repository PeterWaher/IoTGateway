using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

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
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			if (!Gateway.Start(false))
				throw new Exception("Gateway being started in another process.");
		}

		protected override void OnStop()
		{
			Gateway.Stop();
		}
	}
}
