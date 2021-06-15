using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.PKCS.Test
{
	[TestClass]
	public class PfxTests
	{
		private PfxEncoder pfxOutput;

		[TestInitialize]
		public void TestInitialize()
		{
			this.pfxOutput = new PfxEncoder();
		}

		[TestMethod]
		public void PFX_Test_01()
		{
			this.pfxOutput.Begin();
			this.pfxOutput.StartSafeContent();


			this.pfxOutput.EndSafeContent();
			byte[] Pfx = this.pfxOutput.End();

			X509Certificate2 _ = new X509Certificate2(Pfx, "Test");
		}

	}
}
