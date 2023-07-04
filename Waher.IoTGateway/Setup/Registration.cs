using System;
using System.Collections.Generic;

namespace Waher.IoTGateway.Setup
{
	public partial class XmppConfiguration
	{
		private readonly static Dictionary<string, KeyValuePair<string, string>> clp = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.CurrentCultureIgnoreCase)
		{
		};

		/// <summary>
		/// Date when solution was built.
		/// </summary>
		public static readonly string BuildDate = "";

		/// <summary>
		/// Time when solution was built.
		/// </summary>
		public static readonly string BuildTime = "";
	}
}
