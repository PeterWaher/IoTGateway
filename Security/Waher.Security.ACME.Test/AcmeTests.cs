using System;
using System.Security.Cryptography;
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
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
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

		private async Task<AcmeOrder> OrderCertificate(params string[] Domains)
		{
			AcmeAccount Account = await this.client.GetAccount();
			return await Account.OrderCertificate(Domains, null, null);
		}

		[TestMethod]
		public async Task ACME_Test_07_PollOrder()
		{
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
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
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();

			Assert.IsNotNull(Authorizations);
			Assert.AreEqual(Order.AuthorizationUris.Length, Authorizations.Length);
		}

		[TestMethod]
		public async Task ACME_Test_09_PollAuthorization()
		{
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();

			AcmeAuthorization Authorization = await Authorizations[0].Poll();
			Assert.IsNotNull(Authorization);
		}

		[TestMethod]
		public async Task ACME_Test_10_DeactivateAuthorizations()
		{
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();
			int i, c = Authorizations.Length;

			for (i = 0; i < c; i++)
				Authorizations[i] = await Authorizations[i].Deactivate();
		}

		[TestMethod]
		public async Task ACME_Test_11_Challenges()
		{
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();

			this.Print(Authorizations);
		}

		private void Print(params AcmeAuthorization[] Authorizations)
		{
			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				Console.Out.WriteLine(Authorization.Type);
				Console.Out.WriteLine(Authorization.Value);
				Console.Out.WriteLine(Authorization.Status);
				Console.Out.WriteLine();

				this.Print(Authorization.Value, Authorization.Challenges);
			}
		}

		private void Print(string DomainName, params AcmeChallenge[] Challenges)
		{
			foreach (AcmeChallenge Challenge in Challenges)
			{
				Console.Out.WriteLine(Challenge.Location);
				Console.Out.WriteLine(Challenge.Status.ToString());
				Console.Out.WriteLine(Challenge.Token);

				if (Challenge is AcmeHttpChallenge HttpChallenge)
				{
					Console.Out.WriteLine(HttpChallenge.ResourceName);
					Console.Out.WriteLine(HttpChallenge.KeyAuthorization);
				}
				else if (Challenge is AcmeDnsChallenge DnsChallenge)
				{
					Console.Out.WriteLine(DnsChallenge.ValidationDomainNamePrefix + DomainName);
					Console.Out.WriteLine(DnsChallenge.KeyAuthorization);
				}

				if (Challenge.Validated.HasValue)
					Console.Out.WriteLine(Challenge.Validated.Value);

				Console.Out.WriteLine();
			}
		}

		[TestMethod]
		[Ignore]
		public async Task ACME_Test_12_GetOrders()
		{
			AcmeAccount Account = await this.client.GetAccount();
			AcmeOrder[] Orders = await Account.GetOrders();
			
			foreach (AcmeOrder Order in Orders)
			{
				Console.Out.WriteLine(Order.Location);
				Console.Out.WriteLine(Order.Status);
				Console.Out.WriteLine();

				AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();
				this.Print(Authorizations);
			}
		}

		[TestMethod]
		public async Task ACME_Test_13_AcknowledgeHttpChallenges()
		{
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();

			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				foreach (AcmeChallenge Challenge in Authorization.Challenges)
				{
					if (Challenge is AcmeHttpChallenge HttpChallenge)
					{
						AcmeChallenge Challenge2 = await Challenge.AcknowledgeChallenge();

						this.Print(Authorization.Value, Challenge2);
					}
				}
			}

			System.Threading.Thread.Sleep(5000);

			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				AcmeAuthorization Authorization2 = await Authorization.Poll();
				this.Print(Authorization2);
			}
		}

		[TestMethod]
		public async Task ACME_Test_14_AcknowledgeDnsChallenges()
		{
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();

			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				foreach (AcmeChallenge Challenge in Authorization.Challenges)
				{
					if (Challenge is AcmeDnsChallenge DnsChallenge)
					{
						AcmeChallenge Challenge2 = await Challenge.AcknowledgeChallenge();

						this.Print(Authorization.Value, Challenge2);
					}
				}
			}

			System.Threading.Thread.Sleep(5000);

			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				AcmeAuthorization Authorization2 = await Authorization.Poll();
				this.Print(Authorization2);
			}
		}

		[TestMethod]
		public async Task ACME_Test_15_FinalizeOrder()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
			{
				AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
				Order = await Order.FinalizeOrder(new CertificateRequest(new RsaSha256(RSA))
				{
					//Country = "SE",
					//StateOrProvince = "Stockholm",
					//Locality = "Locality",
					//Organization = "Example Ltd",
					//OrganizationalUnit = "Development",
					CommonName = "example.com",
					SubjectAlternativeNames = new string[] { "example.com", "www.example.com" },
					EMailAddress = "ex@example.com",
					//Surname = "Smith",
					//Description = "Domain certificate",
					//Name = "Mr Smith",
					//GivenName = "Mr"
				});
			}
		}

		[TestMethod]
		public async Task ACME_Test_90_DeactivateAccount()
		{
			AcmeAccount Account = await this.client.GetAccount();
			Account = await Account.Deactivate();
		}

	}
}
