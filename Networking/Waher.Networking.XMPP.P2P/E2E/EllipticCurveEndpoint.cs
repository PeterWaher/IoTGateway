﻿using System;
using System.IO;
using Waher.Runtime.Cache;
using Waher.Security;
using Waher.Security.EllipticCurves;

namespace Waher.Networking.XMPP.P2P.E2E
{
	/// <summary>
	/// Abstract base class for Elliptic Curve endpoints.
	/// </summary>
	public abstract class EllipticCurveEndpoint : E2eEndpoint
	{
		private static readonly Cache<string, byte[]> sharedSecrets = new Cache<string, byte[]>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromDays(1), true);

		/// <summary>
		/// Remote public key.
		/// </summary>
		protected readonly byte[] publicKey;
		private readonly EllipticCurve curve;
		private readonly bool hasPrivateKey;
		private readonly string publicKeyBase64;

		/// <summary>
		/// Abstract base class for Elliptic Curve endpoints.
		/// </summary>
		/// <param name="Curve">Curve instance</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public EllipticCurveEndpoint(EllipticCurve Curve, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(DefaultSymmetricCipher)
		{
			this.curve = Curve;
			this.publicKey = Curve.PublicKey;
			this.hasPrivateKey = true;
			this.publicKeyBase64 = Convert.ToBase64String(this.publicKey);
		}

		/// <summary>
		/// Abstract base class for Elliptic Curve endpoints.
		/// </summary>
		/// <param name="PublicKey">Remote public key.</param>
		/// <param name="ReferenceCurve">Reference curve</param>
		/// <param name="DefaultSymmetricCipher">Default symmetric cipher.</param>
		public EllipticCurveEndpoint(byte[] PublicKey, EllipticCurve ReferenceCurve, IE2eSymmetricCipher DefaultSymmetricCipher)
			: base(DefaultSymmetricCipher)
		{
			this.publicKey = PublicKey;
			this.curve = ReferenceCurve;
			this.hasPrivateKey = false;
			this.publicKeyBase64 = Convert.ToBase64String(this.publicKey);
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
		/// Name of elliptic curve
		/// </summary>
		public string CurveName => this.curve.CurveName;

		/// <summary>
		/// Elliptic Curve
		/// </summary>
		public EllipticCurve Curve => this.curve;

		/// <summary>
		/// Previous Elliptic Curve
		/// </summary>
		public EllipticCurve PrevCurve => (this.Previous as EllipticCurveEndpoint)?.Curve;

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
			CipherText = null;
			return GetSharedKey(this, RemoteEndpoint);
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
			return GetSharedKey(this, RemoteEndpoint);
		}

		/// <summary>
		/// If the recipient needs a cipher text to generate the same shared secret.
		/// </summary>
		public override bool SharedSecretUseCipherText => false;

		/// <summary>
		/// Shared secret, for underlying symmetric cipher.
		/// </summary>
		public static byte[] GetSharedKey(EllipticCurveEndpoint LocalKey, IE2eEndpoint RemoteKey)
		{
			string Key = LocalKey.PublicKeyBase64 + ";" + RemoteKey.PublicKeyBase64;

			if (sharedSecrets.TryGetValue(Key, out byte[] SharedKey))
				SharedKey = (byte[])SharedKey.Clone();
			else
			{
				SharedKey = LocalKey.curve.GetSharedKey(RemoteKey.PublicKey, Hashes.ComputeSHA256Hash);
				sharedSecrets[Key] = (byte[])SharedKey.Clone();
			}

			return SharedKey;
		}

		/// <summary>
		/// Signs binary data using the local private key.
		/// </summary>
		/// <param name="Data">Binary data</param>
		/// <returns>Digital signature.</returns>
		public override byte[] Sign(byte[] Data)
		{
			if (!this.hasPrivateKey)
				throw new InvalidOperationException("Signing requires private key.");

			byte[] Signature = this.curve.Sign(Data);

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

			byte[] Signature = this.curve.Sign(Data);

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
			return this.curve.Verify(Data, PublicKey, Signature);
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
			return this.curve.Verify(Data, PublicKey, Signature);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public override bool Verify(byte[] Data, byte[] Signature)
		{
			return this.Verify(Data, this.publicKey, Signature);
		}

		/// <summary>
		/// Verifies a signature.
		/// </summary>
		/// <param name="Data">Data that is signed.</param>
		/// <param name="Signature">Digital signature.</param>
		/// <returns>If signature is valid.</returns>
		public override bool Verify(Stream Data, byte[] Signature)
		{
			return this.Verify(Data, this.publicKey, Signature);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is EllipticCurveEndpoint EcEndpoint &&
				this.curve.CurveName.Equals(EcEndpoint.curve.CurveName) &&
				this.publicKeyBase64.Equals(EcEndpoint.publicKeyBase64);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.curve.CurveName.GetHashCode();
			Result ^= Result << 5 ^ this.publicKeyBase64.GetHashCode();

			return Result;
		}

	}
}
