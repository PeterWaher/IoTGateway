using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class BoshStanzaTests : XmppStanzaTests
	{
		public BoshStanzaTests()
		{
		}

		[ClassInitialize]
		public new static void ClassInitialize(TestContext _)
		{
			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public new static void ClassCleanup()
		{
			DisposeSnifferAndLog();
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
