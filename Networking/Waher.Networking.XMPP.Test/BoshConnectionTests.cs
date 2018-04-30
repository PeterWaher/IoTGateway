using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class BoshConnectionTests : XmppConnectionTests
	{
		public BoshConnectionTests()
		{
		}

		public override XmppCredentials GetCredentials()
		{
			return new XmppCredentials()
			{
				//Host = "waher.se",
				//HttpEndpoint = "https://waher.se/http-bind",
				Host = "localhost",
				HttpEndpoint = "https://localhost/http-bind",
				TrustServer = true,
				Account = "test",
				Password = "testpwd"
			};
		}
	}
}
