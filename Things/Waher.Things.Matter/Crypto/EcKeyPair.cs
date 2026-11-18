using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Waher.Things.Matter.Crypto
{
	/// <summary>
	/// NIST P-256 key pair, used for ECDSA signatures (raw r||s format, SHA-256) and ECDH.
	/// </summary>
	public class EcKeyPair : IDisposable
	{
		private readonly byte[] privateKey;
		private readonly byte[] publicKey;
		private ECDsa ecdsa;

		/// <summary>
		/// NIST P-256 key pair.
		/// </summary>
		/// <param name="PrivateKey">Private key (32 bytes, big-endian), or null if only the public key is known.</param>
		/// <param name="PublicKey">Public key (65 bytes, uncompressed).</param>
		public EcKeyPair(byte[] PrivateKey, byte[] PublicKey)
		{
			if (PublicKey is null && PrivateKey is null)
				throw new ArgumentException("No key provided.");

			if (PublicKey is null)
				PublicKey = P256.Encode(P256.MultiplyBase(P256.FromBigEndian(PrivateKey)));
			else
				P256.Decode(PublicKey);   // Validates the point.

			this.privateKey = PrivateKey;
			this.publicKey = PublicKey;
		}

		/// <summary>
		/// Creates a new random key pair.
		/// </summary>
		/// <returns>Key pair.</returns>
		public static EcKeyPair Generate()
		{
			BigInteger d = P256.RandomScalar();
			byte[] PrivateKey = P256.ToBigEndian(d, 32);
			byte[] PublicKey = P256.Encode(P256.MultiplyBase(d));

			return new EcKeyPair(PrivateKey, PublicKey);
		}

		/// <summary>
		/// Creates a key pair object containing only a public key.
		/// </summary>
		/// <param name="PublicKey">Public key (65 bytes, uncompressed).</param>
		/// <returns>Key object.</returns>
		public static EcKeyPair FromPublicKey(byte[] PublicKey)
		{
			return new EcKeyPair(null, PublicKey);
		}

		/// <summary>
		/// Private key (32 bytes, big-endian), or null if not available.
		/// </summary>
		public byte[] PrivateKey => this.privateKey;

		/// <summary>
		/// Public key (65 bytes, uncompressed).
		/// </summary>
		public byte[] PublicKey => this.publicKey;

		/// <summary>
		/// If the private key is available.
		/// </summary>
		public bool HasPrivateKey => !(this.privateKey is null);

		private ECDsa GetEcdsa()
		{
			if (this.ecdsa is null)
			{
				ECParameters Parameters = new ECParameters()
				{
					Curve = ECCurve.NamedCurves.nistP256,
					Q = new ECPoint()
					{
						X = MatterCrypto.Slice(this.publicKey, 1, 32),
						Y = MatterCrypto.Slice(this.publicKey, 33, 32)
					}
				};

				if (!(this.privateKey is null))
					Parameters.D = (byte[])this.privateKey.Clone();

				this.ecdsa = ECDsa.Create(Parameters);
			}

			return this.ecdsa;
		}

		/// <summary>
		/// Signs data using ECDSA with SHA-256 (Crypto_Sign).
		/// </summary>
		/// <param name="Data">Data to sign.</param>
		/// <returns>Raw signature (r || s, 64 bytes).</returns>
		public byte[] Sign(byte[] Data)
		{
			if (this.privateKey is null)
				throw new InvalidOperationException("Private key not available.");

			lock (this)
			{
				return this.GetEcdsa().SignData(Data, HashAlgorithmName.SHA256);
			}
		}

		/// <summary>
		/// Verifies an ECDSA signature with SHA-256 (Crypto_Verify).
		/// </summary>
		/// <param name="Data">Signed data.</param>
		/// <param name="Signature">Raw signature (r || s, 64 bytes).</param>
		/// <returns>If signature is valid.</returns>
		public bool Verify(byte[] Data, byte[] Signature)
		{
			if (Signature is null || Signature.Length != 64)
				return false;

			lock (this)
			{
				return this.GetEcdsa().VerifyData(Data, Signature, HashAlgorithmName.SHA256);
			}
		}

		/// <summary>
		/// Verifies an ECDSA signature with SHA-256.
		/// </summary>
		/// <param name="PublicKey">Public key (65 bytes, uncompressed).</param>
		/// <param name="Data">Signed data.</param>
		/// <param name="Signature">Raw signature (r || s, 64 bytes).</param>
		/// <returns>If signature is valid.</returns>
		public static bool Verify(byte[] PublicKey, byte[] Data, byte[] Signature)
		{
			try
			{
				using (EcKeyPair Key = FromPublicKey(PublicKey))
				{
					return Key.Verify(Data, Signature);
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Computes an ECDH shared secret with a peer.
		/// </summary>
		/// <param name="PeerPublicKey">Public key of peer.</param>
		/// <returns>Shared secret (32 bytes).</returns>
		public byte[] Ecdh(byte[] PeerPublicKey)
		{
			if (this.privateKey is null)
				throw new InvalidOperationException("Private key not available.");

			return P256.Ecdh(this.privateKey, PeerPublicKey);
		}

		/// <summary>
		/// Computes the subject key identifier of the public key (SHA-1 of the public key, as used in X.509).
		/// </summary>
		/// <returns>20-byte key identifier.</returns>
		public byte[] KeyIdentifier()
		{
			return ComputeKeyIdentifier(this.publicKey);
		}

		/// <summary>
		/// Computes the key identifier of a public key (SHA-1 of the public key, as used in X.509).
		/// </summary>
		/// <param name="PublicKey">Public key (65 bytes, uncompressed).</param>
		/// <returns>20-byte key identifier.</returns>
		public static byte[] ComputeKeyIdentifier(byte[] PublicKey)
		{
			using (SHA1 Sha1 = SHA1.Create())
			{
				return Sha1.ComputeHash(PublicKey);
			}
		}

		/// <summary>
		/// Converts a DER-encoded ECDSA signature to raw (r || s) form.
		/// </summary>
		/// <param name="Der">DER-encoded signature.</param>
		/// <returns>Raw 64-byte signature.</returns>
		public static byte[] DerToRawSignature(byte[] Der)
		{
			Asn1.DerReader Reader = new Asn1.DerReader(Der);
			Asn1.DerReader Seq = Reader.ReadSequence();
			BigInteger r = P256.FromBigEndian(Seq.ReadIntegerBytes());
			BigInteger s = P256.FromBigEndian(Seq.ReadIntegerBytes());

			return MatterCrypto.Concat(P256.ToBigEndian(r, 32), P256.ToBigEndian(s, 32));
		}

		/// <summary>
		/// Converts a raw (r || s) ECDSA signature to DER form.
		/// </summary>
		/// <param name="Raw">Raw 64-byte signature.</param>
		/// <returns>DER-encoded signature.</returns>
		public static byte[] RawToDerSignature(byte[] Raw)
		{
			if (Raw is null || Raw.Length != 64)
				throw new ArgumentException("Raw signature must be 64 bytes.", nameof(Raw));

			Asn1.DerWriter Writer = new Asn1.DerWriter();
			Writer.StartSequence();
			Writer.WriteUnsignedInteger(MatterCrypto.Slice(Raw, 0, 32));
			Writer.WriteUnsignedInteger(MatterCrypto.Slice(Raw, 32, 32));
			Writer.EndSequence();

			return Writer.ToArray();
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.ecdsa?.Dispose();
			this.ecdsa = null;
		}
	}
}
