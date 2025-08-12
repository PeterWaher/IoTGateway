using System;
using System.IO;
using Waher.Security.PQC;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for Module Lattice endpoints.
	/// </summary>
	public abstract class ModuleLatticeEndpoint : E2eEndpoint
	{
		private const string preHashAlgorithm = "SHAKE-256";

		private readonly ML_KEM keyEncapsulationMechanism;
		private readonly ML_DSA signatureAlgorithm;
		private readonly ML_KEM_Keys keyEncapsulationMechanismKeys;
		private readonly ML_DSA_Keys signatureAlgorithmKeys;
		private readonly bool hasPrivateKey;
		private readonly byte[] publicKey;
		private readonly string publicKeyBase64;

		/// <summary>
		/// Abstract base class for Module Lattice endpoints.
		/// </summary>
		/// <param name="KeyEncapsulationMechanism">Key Encapsulation Mechanism.</param>
		/// <param name="KeyEncapsulationMechanismKeys">Key Encapsulation Mechanism Keys.</param>
		/// <param name="SignatureAlgorithm">Signature Algorithm.</param>
		/// <param name="SignatureAlgorithmKeys">Signature Algorithm Keys.</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLatticeEndpoint(ML_KEM KeyEncapsulationMechanism,
			ML_KEM_Keys KeyEncapsulationMechanismKeys, ML_DSA SignatureAlgorithm,
			ML_DSA_Keys SignatureAlgorithmKeys, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(DefaultSymmetricCipher)
		{
			this.keyEncapsulationMechanism = KeyEncapsulationMechanism;
			this.keyEncapsulationMechanismKeys = KeyEncapsulationMechanismKeys;
			this.signatureAlgorithm = SignatureAlgorithm;
			this.signatureAlgorithmKeys = SignatureAlgorithmKeys;

			this.hasPrivateKey = this.keyEncapsulationMechanismKeys.HasDecapsulationKey &&
				this.signatureAlgorithmKeys.HasPrivateKey;

			if (this.keyEncapsulationMechanismKeys.EncapsulationKey is null ||
				this.signatureAlgorithmKeys.PublicKey is null)
			{
				this.publicKey = null;
				this.publicKeyBase64 = null;
			}
			else
			{
				this.publicKey = new byte[this.keyEncapsulationMechanism.PublicKeyLength +
					this.signatureAlgorithm.PublicKeyLength];

				Array.Copy(this.keyEncapsulationMechanismKeys.EncapsulationKey, 0,
					this.publicKey, 0, this.keyEncapsulationMechanism.PublicKeyLength);

				Array.Copy(this.signatureAlgorithmKeys.PublicKey, 0,
					this.publicKey, this.keyEncapsulationMechanism.PublicKeyLength,
					this.signatureAlgorithm.PublicKeyLength);

				this.publicKeyBase64 = Convert.ToBase64String(this.publicKey);
			}
		}

		/// <summary>
		/// Abstract base class for Module Lattice endpoints.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <param name="KeyEncapsulationMechanism">Key Encapsulation Mechanism.</param>
		/// <param name="SignatureAlgorithm">Signature Algorithm.</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public ModuleLatticeEndpoint(byte[] PublicKey, ML_KEM KeyEncapsulationMechanism,
			ML_DSA SignatureAlgorithm, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(DefaultSymmetricCipher)
		{
			if (PublicKey is null)
				throw new ArgumentNullException(nameof(PublicKey));

			if (PublicKey.Length != KeyEncapsulationMechanism.PublicKeyLength +
				SignatureAlgorithm.PublicKeyLength)
			{
				throw new ArgumentException("Public key length does not match expected length for the specified key encapsulation mechanism and signature algorithm.", nameof(PublicKey));
			}

			this.keyEncapsulationMechanism = KeyEncapsulationMechanism;
			this.signatureAlgorithm = SignatureAlgorithm;
			this.hasPrivateKey = false;
			this.publicKey = PublicKey;
			this.publicKeyBase64 = Convert.ToBase64String(PublicKey);

			byte[] EncapsulationKey = new byte[this.keyEncapsulationMechanism.PublicKeyLength];
			byte[] SignaturePublicKey = new byte[this.signatureAlgorithm.PublicKeyLength];

			Array.Copy(PublicKey, 0, EncapsulationKey, 0, EncapsulationKey.Length);
			Array.Copy(PublicKey, EncapsulationKey.Length, SignaturePublicKey, 0, SignaturePublicKey.Length);

			this.keyEncapsulationMechanismKeys = ML_KEM_Keys.FromEncapsulationKey(EncapsulationKey);
			this.signatureAlgorithmKeys = ML_DSA_Keys.FromPublicKey(SignaturePublicKey);
		}

		/// <summary>
		/// If the key contains a private key.
		/// </summary>
		public bool HasPrivateKey => this.hasPrivateKey;

		/// <summary>
		/// Remote public key.
		/// </summary>
		public override byte[] PublicKey => this.publicKey;

		/// <summary>
		/// Remote public key, as a Base64 string.
		/// </summary>
		public override string PublicKeyBase64 => this.publicKeyBase64;

		/// <summary>
		/// Key Encapsulation Mechanism used for key exchange.
		/// </summary>
		public ML_KEM KeyEncapsulationMechanism => this.keyEncapsulationMechanism;

		/// <summary>
		/// Signature Algorithm used for signing.
		/// </summary>
		public ML_DSA SignatureAlgorithm => this.signatureAlgorithm;

		/// <summary>
		/// Gets a shared secret for encryption, and optionally a corresponding cipher text.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint</param>
		/// <param name="Cipher">Symmetric cipher to use for encryption.</param>
		/// <param name="CipherText">Optional cipher text required by the recipient to
		/// be able to generate the same shared secret.</param>
		/// <returns>Shared secret.</returns>
		public override byte[] GetSharedSecretForEncryption(IE2eEndpoint RemoteEndpoint,
			IE2eSymmetricCipher Cipher, out byte[] CipherText)
		{
			if (!(RemoteEndpoint is ModuleLatticeEndpoint RemoteModuleLatticeEndpoint))
				throw new ArgumentException("Remote endpoint is not a Module Lattice endpoint.", nameof(RemoteEndpoint));

			if (!this.keyEncapsulationMechanismKeys.HasDecapsulationKey)
				throw new InvalidOperationException("Local endpoint does not have a private key for shared secret calculation.");

			if (this.keyEncapsulationMechanism.PublicKeyLength !=
				RemoteModuleLatticeEndpoint.keyEncapsulationMechanism.PublicKeyLength ||
				this.signatureAlgorithm.PublicKeyLength !=
				RemoteModuleLatticeEndpoint.signatureAlgorithm.PublicKeyLength)
			{
				throw new ArgumentException("Key lengths do not match.", nameof(RemoteEndpoint));
			}

			ML_KEM_Encapsulation Encapsulation = this.keyEncapsulationMechanism.Encapsulate(
				RemoteModuleLatticeEndpoint.keyEncapsulationMechanismKeys.EncapsulationKey);

			CipherText = Encapsulation.CipherText;

			return Encapsulation.SharedSecret;
		}

		/// <summary>
		/// Gets a shared secret for decryption.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint</param>
		/// <param name="CipherText">Optional cipher text required by the recipient to
		/// be able to generate the same shared secret.</param>
		/// <returns>Shared secret.</returns>
		public override byte[] GetSharedSecretForDecryption(IE2eEndpoint RemoteEndpoint,
			byte[] CipherText)
		{
			if (!(RemoteEndpoint is ModuleLatticeEndpoint RemoteModuleLatticeEndpoint))
				throw new ArgumentException("Remote endpoint is not a Module Lattice endpoint.", nameof(RemoteEndpoint));

			if (!this.keyEncapsulationMechanismKeys.HasDecapsulationKey)
				throw new InvalidOperationException("Local endpoint does not have a private key for shared secret calculation.");

			if (this.keyEncapsulationMechanism.PublicKeyLength !=
				RemoteModuleLatticeEndpoint.keyEncapsulationMechanism.PublicKeyLength ||
				this.signatureAlgorithm.PublicKeyLength !=
				RemoteModuleLatticeEndpoint.signatureAlgorithm.PublicKeyLength)
			{
				throw new ArgumentException("Key lengths do not match.", nameof(RemoteEndpoint));
			}

			return this.keyEncapsulationMechanism.Decapsulate(
				this.keyEncapsulationMechanismKeys.DecapsulationKey, CipherText);
		}

		/// <summary>
		/// If the recipient needs a cipher text to generate the same shared secret.
		/// </summary>
		public override bool SharedSecretUseCipherText => true;

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Digital signature.</returns>
		public override byte[] Sign(byte[] Data)
		{
			if (!this.hasPrivateKey)
				throw new InvalidOperationException("Signing requires private key.");

			byte[] Signature;
			bool Valid;

			do
			{
				Signature = this.signatureAlgorithm.Sign(this.signatureAlgorithmKeys,
					Data, null, preHashAlgorithm, out _);
				Valid = this.signatureAlgorithm.Verify(this.signatureAlgorithmKeys, Data,
					Signature, null, preHashAlgorithm);
			}
			while (!Valid);

			return Signature;
		}

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Digital signature.</returns>
		public override byte[] Sign(Stream Data)
		{
			if (!this.hasPrivateKey)
				throw new InvalidOperationException("Signing requires private key.");

			byte[] Signature;
			bool Valid;

			do
			{
				Signature = this.signatureAlgorithm.Sign(this.signatureAlgorithmKeys,
					Data, null, preHashAlgorithm, out _);
				Valid = this.signatureAlgorithm.Verify(this.signatureAlgorithmKeys, Data,
					Signature, null, preHashAlgorithm);
			}
			while (!Valid);

			return Signature;
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="PublicKey">Public key</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public bool Verify(byte[] Data, byte[] PublicKey, byte[] Signature)
		{
			return this.signatureAlgorithm.Verify(ML_DSA_Keys.FromPublicKey(PublicKey), Data,
				Signature, null, preHashAlgorithm);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="PublicKey">Public key</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public bool Verify(Stream Data, byte[] PublicKey, byte[] Signature)
		{
			return this.signatureAlgorithm.Verify(ML_DSA_Keys.FromPublicKey(PublicKey), Data,
				Signature, null, preHashAlgorithm);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public override bool Verify(byte[] Data, byte[] Signature)
		{
			return this.signatureAlgorithm.Verify(this.signatureAlgorithmKeys, Data,
				Signature, null, preHashAlgorithm);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public override bool Verify(Stream Data, byte[] Signature)
		{
			return this.signatureAlgorithm.Verify(this.signatureAlgorithmKeys, Data,
				Signature, null, preHashAlgorithm);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is ModuleLatticeEndpoint MlObj &&
				this.publicKeyBase64 == MlObj.publicKeyBase64 &&
				this.hasPrivateKey == MlObj.hasPrivateKey;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.publicKeyBase64.GetHashCode();
			Result ^= Result << 5 ^ this.hasPrivateKey.GetHashCode();

			return Result;
		}

		/// <summary>
		/// Generates private keys from a 96-byte secret.
		/// </summary>
		/// <param name="Secret">96-byte secret.</param>
		/// <param name="KemKeys">Keys for key exchange</param>
		/// <param name="DsaKeys">Keys for digital signatures.</param>
		protected void GeneratePrivateKeys(byte[] Secret, out ML_KEM_Keys KemKeys,
			out ML_DSA_Keys DsaKeys)
		{
			if (Secret is null)
				throw new ArgumentNullException(nameof(Secret));

			if (Secret.Length == 96)
			{
				byte[] SeedKem = new byte[64];
				byte[] SeedDsa = new byte[32];

				Array.Copy(Secret, 0, SeedKem, 0, 64);
				Array.Copy(Secret, 64, SeedDsa, 0, 32);

				KemKeys = ML_KEM.ML_KEM_512.KeyGen_FromSeed(SeedKem, true);
				DsaKeys = ML_DSA.ML_DSA_44.KeyGen_Internal(SeedDsa, true);

				Array.Clear(SeedKem, 0, SeedKem.Length);
				Array.Clear(SeedDsa, 0, SeedDsa.Length);
			}
			else
				throw new ArgumentException("Invalid private key length.", nameof(Secret));
		}

		/// <summary>
		/// Exports the private key (seed) of the endpoint.
		/// </summary>
		/// <returns>Private key (seed)</returns>
		public byte[] ExportPrivateKey()
		{
			if (!(this.keyEncapsulationMechanismKeys?.HasDecapsulationKey ?? false) ||
				!(this.signatureAlgorithmKeys?.HasPrivateKey ?? false))
			{
				throw new InvalidOperationException("Endpoint has no private keys.");
			}

			if (this.keyEncapsulationMechanismKeys.Seed is null ||
				this.signatureAlgorithmKeys.Seed is null)
			{
				throw new InvalidOperationException("Endpoint has no seeds, which is required to export private key.");
			}

			byte[] KemKey = this.keyEncapsulationMechanismKeys.Seed;
			byte[] DsaKey = this.signatureAlgorithmKeys.Seed;

			int c = KemKey.Length;
			int d = DsaKey.Length;

			byte[] Result = new byte[c + d];

			Array.Copy(KemKey, 0, Result, 0, c);
			Array.Copy(DsaKey, 0, Result, c, d);

			return Result;
		}
	}
}
