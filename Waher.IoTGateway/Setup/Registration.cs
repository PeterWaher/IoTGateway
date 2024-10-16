using System;
using System.Collections.Generic;

namespace Waher.IoTGateway.Setup
{
	public partial class XmppConfiguration
	{
		private readonly static Dictionary<string, KeyValuePair<string, string>> clp = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.CurrentCultureIgnoreCase)
		{
			{ "abc4.io", new KeyValuePair<string, string>("19fa79fe4d5650f4411e5b4e56dd49ba61b88073ed1e619369662bb31bf79ebd", "953e47f5a2473f81ac5157fe7d50c435ed4d3a5f58e0feecaa3ff268ec246051") },
			{ "cybercity.online", new KeyValuePair<string, string>("49f83b9876ee8fa70c5f5d59934eb6233f562d252abd7741a5ab3e82eabe64cc", "f84b418788e721a02ed9996f6fa951689dac523faae4976df16e2c4966b3ada7") },
			{ "lils.is", new KeyValuePair<string, string>("2628637d93b76ba41c3e12681ea8f6e4d29e476c54286c711fb1f680098e891e", "8b7b7cf0f0566ec6bbd7c3ed271e36f4dac0e657b1e19f44aba4e67d80c8ba43") },
			{ "waher.se", new KeyValuePair<string, string>("467174eda47e09f08aa68861346712f1e2b1fac39e6dcab44e3ea4a6378e7050", "6b51dc06907d818c0da60aec18a8f1c3944e1183d4d8871c75af7d1b13bd69eb") }
		};

		/// <summary>
		/// Date when solution was built.
		/// </summary>
		public static readonly string BuildDate = "2024-10-16";

		/// <summary>
		/// Time when solution was built.
		/// </summary>
		public static readonly string BuildTime = "13:13:21";
	}
}
