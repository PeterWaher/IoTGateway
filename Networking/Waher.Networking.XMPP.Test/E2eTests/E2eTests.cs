using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;

namespace Waher.Networking.XMPP.Test.E2eTests
{
	public abstract class E2eTests : CommunicationTests
	{
		protected IE2eEndpoint[] endpoints1;
		protected IE2eEndpoint[] endpoints2;
		protected EndpointSecurity endpointSecurity1;
		protected EndpointSecurity endpointSecurity2;

		public override void PrepareClient1(XmppClient Client, int SecurityStrength, bool P2p)
		{
			base.PrepareClient1(Client, SecurityStrength, P2p);
			this.endpointSecurity1 = new EndpointSecurity(this.client1, this.serverless1,
				SecurityStrength, this.endpoints1);

			if (P2p)
			{
				this.serverless1.OnResynch += (sender, e) =>
				{
					this.client1.Information("Serveless 1 resynch: " + e.RemoteFullJid);

					this.endpointSecurity1?.SynchronizeE2e(e.RemoteFullJid, (Sender2, e2) =>
					{
						e.Done(e2.Ok);
						return Task.CompletedTask;
					});

					return Task.CompletedTask;
				};
			}
		}

		public override void PrepareClient2(XmppClient Client, int SecurityStrength, bool P2p)
		{
			base.PrepareClient2(Client, SecurityStrength, P2p);
			this.endpointSecurity2 = new EndpointSecurity(this.client2, this.serverless2,
				SecurityStrength, this.endpoints2);

			if (P2p)
			{
				this.serverless2.OnResynch += (sender, e) =>
				{
					this.client2.Information("Serveless 2 resynch: " + e.RemoteFullJid);

					this.endpointSecurity2?.SynchronizeE2e(e.RemoteFullJid, (Sender2, e2) =>
					{
						e.Done(e2.Ok);
						return Task.CompletedTask;
					});

					return Task.CompletedTask;
				};
			}
		}

		protected void PrepareEndpoints(AsymmetricCipher AsymmetricCipherType,
			int SecurityStrength, SymmetricCipher SymmetricCipherType)
		{
			if (AsymmetricCipherType == AsymmetricCipher.Ephemeral)
			{
				this.endpoints1 = null;
				this.endpoints2 = null;
			}
			else
			{
				PrepareEndpoint(AsymmetricCipherType, SecurityStrength, SymmetricCipherType,
					out IE2eEndpoint Endpoint1);
				PrepareEndpoint(AsymmetricCipherType, SecurityStrength, SymmetricCipherType,
					out IE2eEndpoint Endpoint2);

				this.endpoints1 = [Endpoint1];
				this.endpoints2 = [Endpoint2];
			}
		}

		private static void PrepareEndpoint(AsymmetricCipher AsymmetricCipherType,
			int SecurityStrength, SymmetricCipher SymmetricCipherType,
			out IE2eEndpoint Endpoint)
		{
			IE2eSymmetricCipher SymmetricCipher2 = SymmetricCipherType switch
			{
				SymmetricCipher.Aes256 => new Aes256(),
				SymmetricCipher.ChaCha20 => new ChaCha20(),
				SymmetricCipher.AeadChaCha20Poly1305 => new AeadChaCha20Poly1305(),
				_ => throw new ArgumentException("Invalid symmetric cipher type.", nameof(SymmetricCipherType)),
			};

			if (AsymmetricCipherType == AsymmetricCipher.Rsa)
			{
				RSA RSA = RSA.Create();
				RSA.KeySize = SecurityStrength switch
				{
					96 => 1024,
					112 => 2048,
					140 => 4096,
					_ => throw new ArgumentException("Invalid security strength.", nameof(SecurityStrength)),
				};

				Endpoint = new RsaEndpoint(RSA, SymmetricCipher2);
			}
			else
			{
				Endpoint = AsymmetricCipherType switch
				{
					AsymmetricCipher.BrainpoolP160 => new BrainpoolP160Endpoint(SymmetricCipher2),
					AsymmetricCipher.BrainpoolP192 => new BrainpoolP192Endpoint(SymmetricCipher2),
					AsymmetricCipher.BrainpoolP224 => new BrainpoolP224Endpoint(SymmetricCipher2),
					AsymmetricCipher.BrainpoolP256 => new BrainpoolP256Endpoint(SymmetricCipher2),
					AsymmetricCipher.BrainpoolP320 => new BrainpoolP320Endpoint(SymmetricCipher2),
					AsymmetricCipher.BrainpoolP384 => new BrainpoolP384Endpoint(SymmetricCipher2),
					AsymmetricCipher.BrainpoolP512 => new BrainpoolP512Endpoint(SymmetricCipher2),
					AsymmetricCipher.NistP192 => new NistP192Endpoint(SymmetricCipher2),
					AsymmetricCipher.NistP224 => new NistP224Endpoint(SymmetricCipher2),
					AsymmetricCipher.NistP256 => new NistP256Endpoint(SymmetricCipher2),
					AsymmetricCipher.NistP384 => new NistP384Endpoint(SymmetricCipher2),
					AsymmetricCipher.NistP521 => new NistP521Endpoint(SymmetricCipher2),
					AsymmetricCipher.Edwards25519 => new Edwards25519Endpoint(SymmetricCipher2),
					AsymmetricCipher.Edwards448 => new Edwards448Endpoint(SymmetricCipher2),
					AsymmetricCipher.Curve25519 => new Curve25519Endpoint(SymmetricCipher2),
					AsymmetricCipher.Curve448 => new Curve448Endpoint(SymmetricCipher2),
					AsymmetricCipher.ModuleLattice128 => new ModuleLattice128Endpoint(SymmetricCipher2),
					AsymmetricCipher.ModuleLattice192 => new ModuleLattice192Endpoint(SymmetricCipher2),
					AsymmetricCipher.ModuleLattice256 => new ModuleLattice256Endpoint(SymmetricCipher2),
					_ => throw new ArgumentException("Invalid asymmetric cipher type.", nameof(AsymmetricCipherType)),
				};
			}
		}

		public override async Task ConnectClients(int SecurityStrength, bool P2p)
		{
			await base.ConnectClients(SecurityStrength, P2p);

			SubscribedTo(this.client01 ?? this.client1, this.client02 ?? this.client2);
			SubscribedTo(this.client02 ?? this.client2, this.client01 ?? this.client1);
		}

		private static void SubscribedTo(XmppClient From, XmppClient To)
		{
			RosterItem Item1 = From.GetRosterItem(To.BareJID);
			RosterItem Item2 = To.GetRosterItem(From.BareJID);

			if (Item1 is null || (Item1.State != SubscriptionState.Both && Item1.State != SubscriptionState.To) ||
				Item2 is null || (Item2.State != SubscriptionState.Both && Item2.State != SubscriptionState.From))
			{
				ManualResetEvent Done2 = new(false);
				ManualResetEvent Error2 = new(false);

				To.OnPresenceSubscribe += async (Sender, e) =>
				{
					if (e.FromBareJID == From.BareJID)
					{
						await e.Accept();
						Done2.Set();
					}
					else
					{
						await e.Decline();
						Error2.Set();
					}
				};

				From.RequestPresenceSubscription(To.BareJID);

				Assert.AreEqual(0, WaitHandle.WaitAny([Done2, Error2], 5000));
			}
		}
	}
}
