using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Svc.ServiceManagement.Enumerations
{
	/// <summary>
	/// How the windows service should be started.
	/// </summary>
	public enum ServiceStartType : uint
	{
		StartOnBoot = 0,
		StartOnSystemStart = 1,
		AutoStart = 2,
		StartOnDemand = 3,
		Disabled = 4
	}
}
