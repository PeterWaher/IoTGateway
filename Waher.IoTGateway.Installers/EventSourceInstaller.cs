using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Waher.IoTGateway.Installers
{
	[RunInstaller(true)]
	public partial class EventSourceInstaller : System.Configuration.Install.Installer
	{
		public EventSourceInstaller()
		{
		}

		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			if (!EventLog.Exists("IoTGateway") || !EventLog.SourceExists("IoTGateway"))
				EventLog.CreateEventSource(new EventSourceCreationData("IoTGateway", "IoTGateway"));
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			if (EventLog.Exists("IoTGateway"))
			{
				try
				{
					EventLog.Delete("IoTGateway");
				}
				catch (Exception)
				{
					// Ignore.
				}
			}

			if (EventLog.SourceExists("IoTGateway"))
			{
				try
				{
					EventLog.DeleteEventSource("IoTGateway");
				}
				catch (Exception)
				{
					// Ignore.
				}
			}
		}
	}
}
