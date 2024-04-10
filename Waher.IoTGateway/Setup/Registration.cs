using System;
using System.Collections.Generic;

namespace Waher.IoTGateway.Setup
{
	public partial class XmppConfiguration
	{
		private readonly static Dictionary<string, KeyValuePair<string, string>> clp = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.CurrentCultureIgnoreCase)
		{
#error Enter list of featured servers here. Build events copy a Registration.cs file from the parent folder of the solution folder, to avoid placing keys in the repository.
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
