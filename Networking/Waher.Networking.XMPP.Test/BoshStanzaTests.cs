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
	public class BoshStanzaTests : XmppStanzaTests
	{
		public BoshStanzaTests()
		{
		}

		public override XmppCredentials GetCredentials1()
		{
			return new XmppCredentials()
			{
				Host = "waher.se",
				UriEndpoint = "https://waher.se/http-bind",
				Account = "xmppclient.test01",
				Password = "testpassword"
			};
		}

		public override XmppCredentials GetCredentials2()
		{
			return new XmppCredentials()
			{
				Host = "waher.se",
				UriEndpoint = "https://waher.se/http-bind",
				Account = "xmppclient.test02",
				Password = "testpassword"
			};
		}
	}
}
