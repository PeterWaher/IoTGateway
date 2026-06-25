using System;
using System.Collections.Generic;

namespace Waher.IoTGateway.Setup
{
	public partial class XmppConfiguration
	{
		private readonly static Dictionary<string, KeyValuePair<string, string>> clp = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.CurrentCultureIgnoreCase)
		{
			{ "cybercity.online", new KeyValuePair<string, string>("7941b22aee8a65ef959cd882f072d9abfcbd7798a01adcddffaf45e16f1d1089", "91ac3a987832db33e3c464c75470cc2ae8d4393a27f97bfbc8993769e03ee380") },
			{ "lab.tagroot.io", new KeyValuePair<string, string>("528a32a35f576b3d0f12f354c00b7a414f9f346c136040251b3d4e60e4874439", "058ed419efbd50341c61970cc3c0225efda5bd1b220b63612831636489c72672") },
			{ "tagroot.io", new KeyValuePair<string, string>("67f2056797fe632b7070313b4f1bae6a9960b67d5235148133f6de273b23449d", "da1b4320fcc90b60687c09be168411adbaef4f44d792d29f63aff14062024e37") },
			{ "waher.se", new KeyValuePair<string, string>("f7befbbdecd6bf1e46ad485ac2f75dabc37135ec920d7077fb34dd6b2051261c", "a96132b8c6083c81965b4874a81c4e9130cd798515df37266339ab7d43a57bef") }
		};

		/// <summary>
		/// Date when solution was built.
		/// </summary>
		public static readonly string BuildDate = "2026-06-20";

		/// <summary>
		/// Time when solution was built.
		/// </summary>
		public static readonly string BuildTime = "19:33:27";
	}
}
