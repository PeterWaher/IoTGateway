using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace Waher.Things.Matter.Crypto
{
	/// <summary>
	/// SPAKE2+ (P-256, SHA-256, HKDF, HMAC), as used by Matter PASE (Matter Core Specification, section 3.10,
	/// based on draft-bar-cfrg-spake2plus-01).
	/// </summary>
	public class Spake2Plus
	{
		/// <summary>
		/// Size of the w0/w1 seed values (CRYPTO_W_SIZE_BYTES).
		/// </summary>
		public const int WSize = 40;

		/// <summary>
		/// Point M (compressed): 02886e2f97ace46e55ba9dd7242579f2993b64e16ef3dcab95afd497333d8fa12f
		/// </summary>
		public static readonly P256Point M = P256.Decode(MatterCrypto.FromHex("02886e2f97ace46e55ba9dd7242579f2993b64e16ef3dcab95afd497333d8fa12f"));

		/// <summary>
		/// Point N (compressed): 03d8bbd6c639c62937b04d997f38c3770719c629d7014d49a24b4f98baa1292b49
		/// </summary>
		public static readonly P256Point N = P256.Decode(MatterCrypto.FromHex("03d8bbd6c639c62937b04d997f38c3770719c629d7014d49a24b4f98baa1292b49"));

		private static readonly byte[] confirmationKeysInfo = Encoding.ASCII.GetBytes("ConfirmationKeys");

		private readonly byte[] context;
		private readonly BigInteger w0;
		private readonly BigInteger w1;
		private readonly P256Point l;
		private readonly BigInteger random;
		private readonly bool isProver;
		private readonly byte[] proverIdentity;
		private readonly byte[] verifierIdentity;
		private byte[] x;
		private byte[] y;
		private byte[] ke;
		private byte[] cA;
		private byte[] cB;

		/// <summary>
		/// Creates a SPAKE2+ prover (the commissioner side in PASE).
		/// </summary>
		/// <param name="Context">Protocol context (hash of PASE context and PBKDF messages).</param>
		/// <param name="W0">Value w0.</param>
		/// <param name="W1">Value w1.</param>
		/// <param name="Random">Random scalar x (if null, a random value is generated).</param>
		/// <returns>Prover instance.</returns>
		public static Spake2Plus CreateProver(byte[] Context, BigInteger W0, BigInteger W1, BigInteger? Random = null)
		{
			return new Spake2Plus(Context, W0, W1, default, Random ?? P256.RandomScalar(), true, null, null);
		}

		/// <summary>
		/// Creates a SPAKE2+ prover, with explicit identities (used for validation against generic test vectors).
		/// Matter uses empty identities.
		/// </summary>
		/// <param name="Context">Protocol context.</param>
		/// <param name="W0">Value w0.</param>
		/// <param name="W1">Value w1.</param>
		/// <param name="Random">Random scalar x.</param>
		/// <param name="ProverIdentity">Prover identity.</param>
		/// <param name="VerifierIdentity">Verifier identity.</param>
		/// <returns>Prover instance.</returns>
		public static Spake2Plus CreateProver(byte[] Context, BigInteger W0, BigInteger W1, BigInteger Random,
			byte[] ProverIdentity, byte[] VerifierIdentity)
		{
			return new Spake2Plus(Context, W0, W1, default, Random, true, ProverIdentity, VerifierIdentity);
		}

		/// <summary>
		/// Creates a SPAKE2+ verifier (the commissionee side in PASE).
		/// </summary>
		/// <param name="Context">Protocol context.</param>
		/// <param name="W0">Value w0.</param>
		/// <param name="L">Verifier point L = w1*G.</param>
		/// <param name="Random">Random scalar y (if null, a random value is generated).</param>
		/// <returns>Verifier instance.</returns>
		public static Spake2Plus CreateVerifier(byte[] Context, BigInteger W0, P256Point L, BigInteger? Random = null)
		{
			return new Spake2Plus(Context, W0, BigInteger.Zero, L, Random ?? P256.RandomScalar(), false, null, null);
		}

		/// <summary>
		/// Creates a SPAKE2+ verifier, with explicit identities (used for validation against generic test vectors).
		/// Matter uses empty identities.
		/// </summary>
		/// <param name="Context">Protocol context.</param>
		/// <param name="W0">Value w0.</param>
		/// <param name="L">Verifier point L = w1*G.</param>
		/// <param name="Random">Random scalar y.</param>
		/// <param name="ProverIdentity">Prover identity.</param>
		/// <param name="VerifierIdentity">Verifier identity.</param>
		/// <returns>Verifier instance.</returns>
		public static Spake2Plus CreateVerifier(byte[] Context, BigInteger W0, P256Point L, BigInteger Random,
			byte[] ProverIdentity, byte[] VerifierIdentity)
		{
			return new Spake2Plus(Context, W0, BigInteger.Zero, L, Random, false, ProverIdentity, VerifierIdentity);
		}

		private Spake2Plus(byte[] Context, BigInteger W0, BigInteger W1, P256Point L, BigInteger Random, bool IsProver,
			byte[] ProverIdentity, byte[] VerifierIdentity)
		{
			this.proverIdentity = ProverIdentity ?? Array.Empty<byte>();
			this.verifierIdentity = VerifierIdentity ?? Array.Empty<byte>();
			this.context = Context;
			this.w0 = W0;
			this.w1 = W1;
			this.l = L;
			this.random = Random;
			this.isProver = IsProver;
		}

		/// <summary>
		/// Computes w0 and w1 from a passcode and PBKDF parameters.
		/// </summary>
		/// <param name="Passcode">Setup passcode.</param>
		/// <param name="Salt">PBKDF salt.</param>
		/// <param name="Iterations">PBKDF iterations.</param>
		/// <param name="W0">Resulting w0.</param>
		/// <param name="W1">Resulting w1.</param>
		public static void ComputeW0W1(uint Passcode, byte[] Salt, int Iterations, out BigInteger W0, out BigInteger W1)
		{
			byte[] Pin = BitConverter.GetBytes(Passcode);
			if (!BitConverter.IsLittleEndian)
				Array.Reverse(Pin);

			byte[] Ws = MatterCrypto.Pbkdf2Sha256(Pin, Salt, Iterations, 2 * WSize);

			W0 = P256.ModN(P256.FromBigEndian(MatterCrypto.Slice(Ws, 0, WSize)));
			W1 = P256.ModN(P256.FromBigEndian(MatterCrypto.Slice(Ws, WSize, WSize)));
		}

		/// <summary>
		/// Computes the PAKE passcode verifier (w0 || L), as used when opening a commissioning window.
		/// </summary>
		/// <param name="Passcode">Setup passcode.</param>
		/// <param name="Salt">PBKDF salt.</param>
		/// <param name="Iterations">PBKDF iterations.</param>
		/// <returns>97-byte verifier (32 bytes w0, 65 bytes L).</returns>
		public static byte[] ComputeVerifier(uint Passcode, byte[] Salt, int Iterations)
		{
			ComputeW0W1(Passcode, Salt, Iterations, out BigInteger W0, out BigInteger W1);
			P256Point L = P256.MultiplyBase(W1);

			return MatterCrypto.Concat(P256.ToBigEndian(W0, 32), P256.Encode(L));
		}

		/// <summary>
		/// Computes the share of this party: X (prover) or Y (verifier).
		/// </summary>
		/// <returns>Encoded share (65 bytes).</returns>
		public byte[] ComputeShare()
		{
			P256Point Base = P256.MultiplyBase(this.random);
			P256Point Blind = P256.Multiply(this.w0, this.isProver ? M : N);
			byte[] Result = P256.Encode(P256.Add(Base, Blind));

			if (this.isProver)
				this.x = Result;
			else
				this.y = Result;

			return Result;
		}

		/// <summary>
		/// Processes the share of the other party, and computes keys and confirmation values.
		/// </summary>
		/// <param name="PeerShare">Share of peer (Y for prover, X for verifier).</param>
		public void ProcessPeerShare(byte[] PeerShare)
		{
			P256Point Peer = P256.Decode(PeerShare);
			P256Point Z, V;

			if (this.isProver)
			{
				if (this.x is null)
					this.ComputeShare();

				this.y = PeerShare;

				P256Point T = P256.Add(Peer, P256.Negate(P256.Multiply(this.w0, N)));
				Z = P256.Multiply(this.random, T);
				V = P256.Multiply(this.w1, T);
			}
			else
			{
				if (this.y is null)
					this.ComputeShare();

				this.x = PeerShare;

				P256Point T = P256.Add(Peer, P256.Negate(P256.Multiply(this.w0, M)));
				Z = P256.Multiply(this.random, T);
				V = P256.Multiply(this.random, this.l);
			}

			if (Z.IsInfinity || V.IsInfinity)
				throw new MatterSecurityException("Invalid SPAKE2+ share.");

			byte[] TT;

			using (MemoryStream ms = new MemoryStream())
			{
				AddToTranscript(ms, this.context);
				AddToTranscript(ms, this.proverIdentity);
				AddToTranscript(ms, this.verifierIdentity);
				AddToTranscript(ms, P256.Encode(M));
				AddToTranscript(ms, P256.Encode(N));
				AddToTranscript(ms, this.x);
				AddToTranscript(ms, this.y);
				AddToTranscript(ms, P256.Encode(Z));
				AddToTranscript(ms, P256.Encode(V));
				AddToTranscript(ms, P256.ToBigEndian(this.w0, 32));

				TT = ms.ToArray();
			}

			byte[] K = MatterCrypto.Sha256(TT);
			byte[] Ka = MatterCrypto.Slice(K, 0, 16);
			this.ke = MatterCrypto.Slice(K, 16, 16);

			byte[] Kc = MatterCrypto.Hkdf(Ka, null, confirmationKeysInfo, 32);
			byte[] KcA = MatterCrypto.Slice(Kc, 0, 16);
			byte[] KcB = MatterCrypto.Slice(Kc, 16, 16);

			this.cA = MatterCrypto.HmacSha256(KcA, this.y);
			this.cB = MatterCrypto.HmacSha256(KcB, this.x);
		}

		private static void AddToTranscript(MemoryStream Output, byte[] Data)
		{
			ulong Len = (ulong)Data.Length;
			int i;

			for (i = 0; i < 8; i++)
			{
				Output.WriteByte((byte)Len);
				Len >>= 8;
			}

			Output.Write(Data, 0, Data.Length);
		}

		/// <summary>
		/// Share of the prover (X).
		/// </summary>
		public byte[] X => this.x;

		/// <summary>
		/// Share of the verifier (Y).
		/// </summary>
		public byte[] Y => this.y;

		/// <summary>
		/// Shared encryption key Ke (16 bytes).
		/// </summary>
		public byte[] Ke => this.ke;

		/// <summary>
		/// Confirmation value of the prover: cA = HMAC(KcA, Y).
		/// </summary>
		public byte[] ConfirmationA => this.cA;

		/// <summary>
		/// Confirmation value of the verifier: cB = HMAC(KcB, X).
		/// </summary>
		public byte[] ConfirmationB => this.cB;
	}
}
