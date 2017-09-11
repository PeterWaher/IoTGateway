using System;
using System.Collections.Generic;
using System.IO;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using Waher.Events;
using Waher.Events.WindowsEventLog;
using Waher.IoTGateway;

namespace Waher.IoTGateway.Svc
{
	class Service : IMicroService
	{
		public void Start()
		{
			Log.Register(new WindowsEventLog("IoTGateway", "IoTGateway", 512));
			Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));

			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			if (!Gateway.Start(false))
				throw new Exception("Gateway being started in another process.");
		}

		public void Stop()
		{
			Gateway.Stop();
			Log.Terminate();
		}
	}
}
