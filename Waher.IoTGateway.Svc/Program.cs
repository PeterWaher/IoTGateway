using System;
using Microsoft.Extensions.PlatformAbstractions;
using PeterKottas.DotNetCore.WindowsService;
using Waher.Events;

namespace Waher.IoTGateway.Svc
{
    class Program
    {
        static void Main(string[] args)
        {
			ServiceRunner<Service>.Run(Config =>
			{
				Config.SetName("IoT Gateway Service");
				Config.SetDisplayName("IoT Gateway Service");
				Config.SetDescription("Windows Service hosting the Waher IoT Gateway.");

				Config.Service(ServiceConfig =>
				{
					ServiceConfig.ServiceFactory((Arguments, Controller) =>
					{
						return new Service();
					});

					ServiceConfig.OnStart((Service, Parameters) =>
					{
						Service.Start();
					});

					ServiceConfig.OnStop(Service =>
					{
						Service.Stop();
					});

					ServiceConfig.OnError(ex =>
					{
						Log.Critical(ex);
					});
				});


			});
        }
    }
}
