using System;
using System.Collections.Generic;

namespace Waher.IoTGateway.Installers
{
	public partial class CustomActions
	{
		private static Dictionary<string, KeyValuePair<string, string>> clp = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.CurrentCultureIgnoreCase)
		{
			{ "waher.se", new KeyValuePair<string, string>("2c786adfc2087c3e74687fa2457f57a0f138231c2585eccd18a6ff6b2a03f328", "48d373b74e8d83349959f7720d5c5a88ec3933fdb460c454090b55959f470891") },
			{ "cityaware.cl", new KeyValuePair<string, string>("c8096d1d5b598aeb756448b6898cbf8e1f1910a1ccef8060e2cc6a331f6dbef2", "5baed3fc6d43cfd50bdfa18ecf6a865ff92b932ccda7b88e9346125c73414d1c") }
		};
	}
}
