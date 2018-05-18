using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.ACME.Test
{
	[TestClass]
	public class AcmeTests
	{
		private AcmeClient client;

		[TestInitialize]
		public void TestInitialize()
		{
			this.client = new AcmeClient("https://acme-staging-v02.api.letsencrypt.org/directory");
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}
		}

		[TestMethod]
		public async Task ACME_Test_01_GetDirectory()
		{
			AcmeDirectory Directory = await this.client.GetDirectory();
			Assert.IsNotNull(Directory);
			Assert.IsTrue(Directory.CaaIdentities.Length > 0);
			Assert.IsFalse(Directory.ExternalAccountRequired);
			Assert.IsFalse(string.IsNullOrEmpty(Directory.KeyChange));
			Assert.IsFalse(string.IsNullOrEmpty(Directory.NewAccount));
			Assert.IsTrue(string.IsNullOrEmpty(Directory.NewAuthz));
			Assert.IsFalse(string.IsNullOrEmpty(Directory.NewNonce));
			Assert.IsFalse(string.IsNullOrEmpty(Directory.NewOrder));
			Assert.IsFalse(string.IsNullOrEmpty(Directory.RevokeCert));
			Assert.IsFalse(string.IsNullOrEmpty(Directory.TermsOfService));
			Assert.IsFalse(string.IsNullOrEmpty(Directory.Website));
		}
	}
}
