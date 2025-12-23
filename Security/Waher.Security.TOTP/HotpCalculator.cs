using System;
using System.Threading.Tasks;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Implements the HOTP calculator algorithm, as defined in RFC 4226:
	/// https://datatracker.ietf.org/doc/html/rfc4226
	/// </summary>
	public class HotpCalculator
	{
		/// <summary>
		/// Default number of digits (6).
		/// </summary>
		public const int DefaultNrDigits = 6;

		/// <summary>
		/// Default Hash Function (SHA-1)
		/// </summary>
		public const HashFunction DefaultHashFunction = HashFunction.SHA1;

		private readonly int nrDigits;
		private readonly byte[] secret;
		private readonly HashFunction hashFunction;

		/// <summary>
		/// Implements the HOTP calculator algorithm, as defined in RFC 4226:
		/// https://datatracker.ietf.org/doc/html/rfc4226
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		public HotpCalculator(byte[] Secret)
			: this(DefaultNrDigits, Secret, DefaultHashFunction)
		{
		}

		/// <summary>
		/// Implements the HOTP calculator algorithm, as defined in RFC 4226:
		/// https://datatracker.ietf.org/doc/html/rfc4226
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		public HotpCalculator(int NrDigits, byte[] Secret)
			: this(NrDigits, Secret, DefaultHashFunction)
		{
		}

		/// <summary>
		/// Implements the HOTP calculator algorithm, as defined in RFC 4226:
		/// https://datatracker.ietf.org/doc/html/rfc4226
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		public HotpCalculator(int NrDigits, byte[] Secret, HashFunction HashFunction)
		{
			CheckNrDigits(NrDigits);
			CheckSecret(Secret);

			this.nrDigits = NrDigits;
			this.secret = Secret;
			this.hashFunction = HashFunction;
		}

		internal static void CheckNrDigits(int NrDigits)
		{
			if (NrDigits < 6)
			{
				throw new ArgumentOutOfRangeException(nameof(NrDigits),
					"Number of digits must be at least 6.");
			}

			if (NrDigits > 8)
			{
				throw new ArgumentOutOfRangeException(nameof(NrDigits),
					"Number of digits must be at most 8.");
			}
		}

		internal static void CheckSecret(byte[] Secret)
		{
			if (Secret is null)
				throw new ArgumentNullException(nameof(Secret));

			if (Secret.Length == 0)
				throw new ArgumentException("Secret must have a length greater than zero.", nameof(Secret));
		}

		/// <summary>
		/// Number of digits to present.
		/// </summary>
		public int NrDigits => this.nrDigits;

		/// <summary>
		/// Hash function to use in computation.
		/// </summary>
		public HashFunction HashFunction => this.hashFunction;

		/// <summary>
		/// Tries to create an HOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <returns>HOTP Calculator, if endpoint was found, null otherwise.</returns>
		public static Task<HotpCalculator> TryCreate(string OtpEndpoint)
		{
			return TryCreate(DefaultNrDigits, OtpEndpoint);
		}

		/// <summary>
		/// Tries to create an HOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <returns>HOTP Calculator, if endpoint was found, null otherwise.</returns>
		public static async Task<HotpCalculator> TryCreate(int NrDigits, string OtpEndpoint)
		{
			OtpSecret Secret = await OtpSecret.GetSecret(OtpEndpoint, OtpType.HOTP);
			if (Secret is null)
				return null;

			return new HotpCalculator(NrDigits, Secret.Secret, Secret.HashFunction);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="Counter">Counter value.</param>
		/// <returns>One-time password.</returns>
		public int Compute(long Counter)
		{
			return Compute(this.nrDigits, this.secret, this.hashFunction, Counter);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="Counter">Counter value.</param>
		/// <returns>One-time password.</returns>
		public static int Compute(byte[] Secret, long Counter)
		{
			return Compute(DefaultNrDigits, Secret, Counter);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="Counter">Counter value.</param>
		/// <returns>One-time password.</returns>
		public static int Compute(int NrDigits, byte[] Secret, long Counter)
		{
			return Compute(NrDigits, Secret, DefaultHashFunction, Counter);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="Counter">Counter value.</param>
		/// <returns>One-time password.</returns>
		public static int Compute(int NrDigits, byte[] Secret, HashFunction HashFunction,
			long Counter)
		{
			CheckNrDigits(NrDigits);
			CheckSecret(Secret);

			byte[] Data = new byte[8];
			int i;

			for (i = 7; i >= 0; i--)
			{
				Data[i] = (byte)Counter;
				Counter >>= 8;
			}

			byte[] HMAC = Hashes.ComputeHMACHash(HashFunction, Secret, Data);
			int Offset = HMAC[^1] & 0x0f;
			int Nr = 0;

			for (i = 0; i < 4; i++)
			{
				Nr <<= 8;
				Nr |= HMAC[Offset++];
			}

			Nr &= 0x7fffffff;

			int Divisor = (int)Math.Pow(10, NrDigits);

			return Nr % Divisor;
		}
	}
}
