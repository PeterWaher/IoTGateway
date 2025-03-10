﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Security.PKCS;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Waher.Security.ACME.Test
{
	[TestClass]
	public class AcmeTests
	{
		private const string directory = "https://acme-staging-v02.api.letsencrypt.org/directory";
		private AcmeClient client;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(JSON).Assembly);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			RSAParameters Parameters;

			try
			{
				CspParameters CspParams = new()
				{
					Flags = CspProviderFlags.UseMachineKeyStore,
					KeyContainerName = directory
				};

				/*using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096, CspParams))
				{
					RSA.PersistKeyInCsp = false;
					RSA.Clear();
				}*/

				using RSACryptoServiceProvider RSA = new(4096, CspParams);
				Parameters = RSA.ExportParameters(true);
			}
			catch (CryptographicException ex)
			{
				throw new CryptographicException("Unable to get access to cryptographic key for " + directory + ". Was application initially run using another user?", ex);
			}

			this.client = new AcmeClient(new Uri(directory), Parameters);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.client?.Dispose();
			this.client = null;
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
			AcmeAccount Account = await this.client.CreateAccount(new string[] { "mailto:unit.test@waher.se" }, true);
			Assert.IsNotNull(Account);
			Assert.AreEqual(AcmeAccountStatus.valid, Account.Status);
			Assert.IsNotNull(Account.Contact);
			Assert.IsTrue(Account.Contact.Length > 0);
			Assert.AreEqual("mailto:unit.test@waher.se", Account.Contact[0]);
		}

		[TestMethod]
		public async Task ACME_Test_03_GetAccount()
		{
			AcmeAccount Account = await this.client.GetAccount();
			Assert.IsNotNull(Account);
			Assert.AreEqual(AcmeAccountStatus.valid, Account.Status);
			Assert.IsNotNull(Account.Contact);
			Assert.IsTrue(Account.Contact.Length > 0);
			Assert.AreEqual("mailto:unit.test@waher.se", Account.Contact[0]);
		}

		[TestMethod]
		public async Task ACME_Test_04_UpdateAccount()
		{
			AcmeAccount Account = await this.client.GetAccount();
			Account = await Account.Update(new string[] { "mailto:unit.test@waher.se", "mailto:unit.test2@waher.se" });
			Assert.IsNotNull(Account);
			Assert.AreEqual(AcmeAccountStatus.valid, Account.Status);
			Assert.IsNotNull(Account.Contact);
			Assert.IsTrue(Account.Contact.Length > 1);
			Assert.IsTrue(Array.IndexOf<string>(Account.Contact, "mailto:unit.test@waher.se") >= 0);
			Assert.IsTrue(Array.IndexOf<string>(Account.Contact, "mailto:unit.test2@waher.se") >= 0);
		}

		[TestMethod]
		public async Task ACME_Test_05_NewKey()
		{
			AcmeAccount Account = await this.client.GetAccount();
			await Account.NewKey();

			CspParameters CspParams = new()
			{
				Flags = CspProviderFlags.UseMachineKeyStore,
				KeyContainerName = directory
			};

			using RSACryptoServiceProvider RSA = new(4096, CspParams);
			RSA.ImportParameters(this.client.ExportAccountKey(true));
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

			try
			{
				return await Account.OrderCertificate(Domains, null, null);
			}
			catch (AcmeMalformedException)	// Not sure why this is necessary. Perhaps because it takes time to propagate the keys correctly on the remote end?
			{
				await Task.Delay(5000);
				return await Account.OrderCertificate(Domains, null, null);
			}
		}

		[TestMethod]
		public async Task ACME_Test_07_PollOrder()
		{
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			AcmeOrder Order2 = await Order.Poll();

			Assert.IsNotNull(Order2);
			Assert.AreEqual(Order.AuthorizationUris.Length, Order2.AuthorizationUris.Length);
			Assert.IsNull(Order2.Certificate);
			Assert.AreEqual(Order.Expires?.Date, Order2.Expires?.Date);
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

			Print(Authorizations);
		}

		private static void Print(params AcmeAuthorization[] Authorizations)
		{
			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				ConsoleOut.WriteLine(Authorization.Type);
				ConsoleOut.WriteLine(Authorization.Value);
				ConsoleOut.WriteLine(Authorization.Status);
				ConsoleOut.WriteLine();

				Print(Authorization.Value, Authorization.Challenges);
			}
		}

		private static void Print(string DomainName, params AcmeChallenge[] Challenges)
		{
			foreach (AcmeChallenge Challenge in Challenges)
			{
				ConsoleOut.WriteLine(Challenge.Location);
				ConsoleOut.WriteLine(Challenge.Status.ToString());
				ConsoleOut.WriteLine(Challenge.Token);

				if (Challenge is AcmeHttpChallenge HttpChallenge)
				{
					ConsoleOut.WriteLine(HttpChallenge.ResourceName);
					ConsoleOut.WriteLine(HttpChallenge.KeyAuthorization);
				}
				else if (Challenge is AcmeDnsChallenge DnsChallenge)
				{
					ConsoleOut.WriteLine(DnsChallenge.ValidationDomainNamePrefix + DomainName);
					ConsoleOut.WriteLine(DnsChallenge.KeyAuthorization);
				}

				if (Challenge.Validated.HasValue)
					ConsoleOut.WriteLine(Challenge.Validated.Value);

				ConsoleOut.WriteLine();
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
				ConsoleOut.WriteLine(Order.Location);
				ConsoleOut.WriteLine(Order.Status);
				ConsoleOut.WriteLine();

				AcmeAuthorization[] Authorizations = await Order.GetAuthorizations();
				Print(Authorizations);
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
					if (Challenge is AcmeHttpChallenge)
					{
						AcmeChallenge Challenge2 = await Challenge.AcknowledgeChallenge();

						Print(Authorization.Value, Challenge2);
					}
				}
			}

			System.Threading.Thread.Sleep(5000);

			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				AcmeAuthorization Authorization2 = await Authorization.Poll();
				Print(Authorization2);
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
					if (Challenge is AcmeDnsChallenge)
					{
						AcmeChallenge Challenge2 = await Challenge.AcknowledgeChallenge();

						Print(Authorization.Value, Challenge2);
					}
				}
			}

			System.Threading.Thread.Sleep(5000);

			foreach (AcmeAuthorization Authorization in Authorizations)
			{
				AcmeAuthorization Authorization2 = await Authorization.Poll();
				Print(Authorization2);
			}
		}

		[TestMethod]
		public async Task ACME_Test_15_FinalizeOrder()
		{
			using RSACryptoServiceProvider RSA = new(4096);
			AcmeOrder Order = await this.OrderCertificate("example.com", "www.example.com");
			await Order.FinalizeOrder(new CertificateRequest(new RsaSha256(RSA))
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

		[TestMethod]
		[Ignore]
		public async Task ACME_Test_90_DeactivateAccount()
		{
			AcmeAccount Account = await this.client.GetAccount();
			await Account.Deactivate();
		}

	}
}

#pragma warning restore CA1416 // Validate platform compatibility
