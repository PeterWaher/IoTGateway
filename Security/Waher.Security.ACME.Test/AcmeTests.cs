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
			Assert.IsNotNull(Directory.KeyChange);
			Assert.IsNotNull(Directory.NewAccount);
			Assert.IsNull(Directory.NewAuthz);
			Assert.IsNotNull(Directory.NewNonce);
			Assert.IsNotNull(Directory.NewOrder);
			Assert.IsNotNull(Directory.RevokeCert);
			Assert.IsNotNull(Directory.TermsOfService);
			Assert.IsNotNull(Directory.Website);
		}

		[TestMethod]
		public async Task ACME_Test_02_CreateAccount()
		{
			AcmeAccount Account = await this.client.CreateAccount(new string[] { "mailto:unit.test@example.com" }, true);
			Assert.IsNotNull(Account);
			Assert.AreEqual(AcmeAccountStatus.valid, Account.Status);
			Assert.IsNotNull(Account.Contact);
			Assert.IsTrue(Account.Contact.Length > 0);
			Assert.AreEqual("mailto:unit.test@example.com", Account.Contact[0]);
		}

		[TestMethod]
		public async Task ACME_Test_03_GetAccount()
		{
			AcmeAccount Account = await this.client.GetAccount();
			Assert.IsNotNull(Account);
			Assert.AreEqual(AcmeAccountStatus.valid, Account.Status);
			Assert.IsNotNull(Account.Contact);
			Assert.IsTrue(Account.Contact.Length > 0);
			Assert.AreEqual("mailto:unit.test@example.com", Account.Contact[0]);
		}

		[TestMethod]
		public async Task ACME_Test_04_UpdateAccount()
		{
			AcmeAccount Account = await this.client.GetAccount();
			Account = await Account.Update(new string[] { "mailto:unit.test@example.com", "mailto:unit.test2@example.com" });
			Assert.IsNotNull(Account);
			Assert.AreEqual(AcmeAccountStatus.valid, Account.Status);
			Assert.IsNotNull(Account.Contact);
			Assert.IsTrue(Account.Contact.Length > 1);
			Assert.AreEqual("mailto:unit.test@example.com", Account.Contact[0]);
			Assert.AreEqual("mailto:unit.test2@example.com", Account.Contact[1]);
		}

		[TestMethod]
		public async Task ACME_Test_05_NewKey()
		{
			AcmeAccount Account = await this.client.GetAccount();
			await Account.NewKey();
		}

		[TestMethod]
		public async Task ACME_Test_06_OrderCertificate()
		{
			AcmeOrder Order = await this.OrderCertificate();
			Assert.IsNotNull(Order);
			Assert.IsTrue(Order.AuthorizationUris.Length > 0);
			Assert.IsNull(Order.Certificate);
			Assert.IsNotNull(Order.Expires);
			Assert.IsNotNull(Order.Finalize);
			Assert.IsTrue(Order.Identifiers.Length > 0);
			Assert.IsNotNull(Order.Location);
			Assert.IsNull(Order.NotAfter);
			Assert.IsNull(Order.NotBefore);
			Assert.AreEqual(AcmeOrderStatus.pending, Order.Status);
		}

		private async Task<AcmeOrder> OrderCertificate()
		{
			AcmeAccount Account = await this.client.GetAccount();
			return await Account.OrderCertificate(new string[] { "example.com", "www.example.com" }, null, null);
		}

		[TestMethod]
		public async Task ACME_Test_07_PollOrder()
		{
			AcmeOrder Order = await this.OrderCertificate();
			AcmeOrder Order2 = await Order.Poll();

			Assert.IsNotNull(Order2);
			Assert.AreEqual(Order.AuthorizationUris.Length, Order2.AuthorizationUris.Length);
			Assert.IsNull(Order2.Certificate);
			Assert.AreEqual(Order.Expires, Order2.Expires);
			Assert.AreEqual(Order.Finalize, Order2.Finalize);
			Assert.AreEqual(Order.Identifiers.Length, Order2.Identifiers.Length);
			Assert.AreEqual(Order.Location, Order2.Location);
			Assert.IsNull(Order2.NotAfter);
			Assert.IsNull(Order2.NotBefore);
			Assert.AreEqual(Order.Status, Order2.Status);
		}

		[TestMethod]
		public async Task ACME_Test_08_Authorizations()
		{
			AcmeOrder Order = await this.OrderCertificate();
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();

			Assert.IsNotNull(Authorizations);
			Assert.AreEqual(Order.AuthorizationUris.Length, Authorizations.Length);
		}

		[TestMethod]
		public async Task ACME_Test_09_PollAuthorization()
		{
			AcmeOrder Order = await this.OrderCertificate();
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();

			AcmeAuthorization Authorization = await Authorizations[0].Poll();
			Assert.IsNotNull(Authorization);
		}

		[TestMethod]
		public async Task ACME_Test_10_DeactivateAuthorizations()
		{
			AcmeOrder Order = await this.OrderCertificate();
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();
			int i, c = Authorizations.Length;

			for (i = 0; i < c; i++)
				Authorizations[i] = await Authorizations[i].Deactivate();
		}

		[TestMethod]
		public async Task ACME_Test_90_DeactivateAccount()
		{
			AcmeAccount Account = await this.client.GetAccount();
			Account = await Account.Deactivate();
		}

	}
}
