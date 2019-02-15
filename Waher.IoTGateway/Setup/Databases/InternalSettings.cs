using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup.Databases
{
	/// <summary>
	/// Internal Settings.
	/// </summary>
	[TypeName(TypeNameSerialization.FullName)]
	public class InternalSettings : IDatabaseSettings
	{
		/// <summary>
		/// Internal Settings.
		/// </summary>
		public InternalSettings()
		{
		}
	}
}
