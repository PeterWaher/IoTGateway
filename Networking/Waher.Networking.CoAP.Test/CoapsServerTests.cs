using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP.ContentFormats;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Runtime.Inventory;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapsServerTests : CoapServerTests
	{
		protected override void SetupClientServer()
		{
			this.server = new CoapEndpoint(null, new int[] { CoapEndpoint.DefaultCoapsPort }, null, null, false, true, new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));
			this.client = new CoapEndpoint(null, new int[] { CoapEndpoint.DefaultCoapsPort + 2 }, null, null, true, false);
			this.credentials = new PresharedKey("testid", new byte[] { 1, 2, 3, 4 });
		}
	}
}
