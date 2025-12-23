using System;
using System.Threading.Tasks;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Implements the TOTP calculator algorithm, as defined in RFC 6238:
	/// https://datatracker.ietf.org/doc/html/rfc6238
	/// </summary>
	public class TotpCalculator
	{
		/// <summary>
		/// Default time-step, in seconds (30).
		/// </summary>
		public const int DefaultTimeStepSeconds = 30;

		private readonly HotpCalculator hotp;
		private readonly int timeStepSeconds;

		/// <summary>
		/// Implements the TOTP calculator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		public TotpCalculator(byte[] Secret)
			: this(HotpCalculator.DefaultNrDigits, Secret, HotpCalculator.DefaultHashFunction)
		{
		}

		/// <summary>
		/// Implements the TOTP calculator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		public TotpCalculator(int NrDigits, byte[] Secret)
			: this(NrDigits, Secret, HotpCalculator.DefaultHashFunction)
		{
		}

		/// <summary>
		/// Implements the TOTP calculator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		public TotpCalculator(int NrDigits, byte[] Secret, HashFunction HashFunction)
			: this(NrDigits, Secret, HashFunction, DefaultTimeStepSeconds)
		{
		}

		/// <summary>
		/// Implements the TOTP calculator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		public TotpCalculator(int NrDigits, byte[] Secret, HashFunction HashFunction,
			int TimeStepSeconds)
		{
			CheckTimeStepSeconds(TimeStepSeconds);

			this.timeStepSeconds = TimeStepSeconds;
			this.hotp = new HotpCalculator(NrDigits, Secret, HashFunction);
		}

		internal static void CheckTimeStepSeconds(int TimeStepSeconds)
		{
			if (TimeStepSeconds <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(TimeStepSeconds),
					"Time step must be a positive integer.");
			}
		}

		/// <summary>
		/// Tries to create a TOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		public static Task<TotpCalculator> TryCreate(string OtpEndpoint)
		{
			return TryCreate(HotpCalculator.DefaultNrDigits, OtpEndpoint, DefaultTimeStepSeconds);
		}

		/// <summary>
		/// Tries to create a TOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		public static Task<TotpCalculator> TryCreate(int NrDigits, string OtpEndpoint)
		{
			return TryCreate(NrDigits, OtpEndpoint, DefaultTimeStepSeconds);
		}

		/// <summary>
		/// Tries to create a TOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		public static async Task<TotpCalculator> TryCreate(int NrDigits, string OtpEndpoint,
			int TimeStepSeconds)
		{
			OtpSecret Secret = await OtpSecret.GetSecret(OtpEndpoint);
			if (Secret is null)
				return null;

			return new TotpCalculator(NrDigits, Secret.Secret, Secret.HashFunction, TimeStepSeconds);
		}

		/// <summary>
		/// Number of digits to present.
		/// </summary>
		public int NrDigits => this.hotp.NrDigits;

		/// <summary>
		/// Hash function to use in computation.
		/// </summary>
		public HashFunction HashFunction => this.hotp.HashFunction;

		/// <summary>
		/// Time step in seconds.
		/// </summary>
		public int TimeStepSeconds => this.timeStepSeconds;

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <returns>One-time password.</returns>
		public int Compute()
		{
			return this.Compute(DateTime.UtcNow);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <returns>One-time password.</returns>
		public int Compute(DateTime Timestamp)
		{
			long Counter = (long)(Timestamp.ToUniversalTime().Subtract(UnixEpoch).TotalSeconds / this.timeStepSeconds);
			return this.hotp.Compute(Counter);
		}

		/// <summary>
		/// Unix Date and Time epoch, starting at 1970-01-01T00:00:00Z
		/// </summary>
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <returns>One-time password.</returns>
		public static int Compute(byte[] Secret, DateTime Timestamp)
		{
			return Compute(HotpCalculator.DefaultNrDigits, Secret, Timestamp);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <returns>One-time password.</returns>
		public static int Compute(int NrDigits, byte[] Secret, DateTime Timestamp)
		{
			return Compute(NrDigits, Secret, HotpCalculator.DefaultHashFunction, Timestamp);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <returns>One-time password.</returns>
		public static int Compute(int NrDigits, byte[] Secret, HashFunction HashFunction,
			DateTime Timestamp)
		{
			return Compute(NrDigits, Secret, HashFunction, DefaultTimeStepSeconds, Timestamp);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <returns>One-time password.</returns>
		public static int Compute(int NrDigits, byte[] Secret, HashFunction HashFunction,
			int TimeStepSeconds, DateTime Timestamp)
		{
			CheckTimeStepSeconds(TimeStepSeconds);

			long Counter = (long)(Timestamp.ToUniversalTime().Subtract(UnixEpoch).TotalSeconds / TimeStepSeconds);
			return HotpCalculator.Compute(NrDigits, Secret, HashFunction, Counter);
		}
	}
}
