using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Svc.ServiceManagement.Enumerations
{
	public enum ErrorSeverity : uint
	{
		Ignore = 0,
		Normal = 1,
		Severe = 2,
		Crititcal = 3
	}
}
