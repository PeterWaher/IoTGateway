using System;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Implements the TOTP algorithm, as defined in RFC 6238:
	/// https://datatracker.ietf.org/doc/html/rfc6238
	/// </summary>
	public class TotpCalculator
	{
		private readonly HotpCalculator hotp;
		private readonly int timeStepSeconds;

		/// <summary>
		/// Implements the TOTP algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		public TotpCalculator(byte[] Secret)
			: this(6, Secret, HashFunction.SHA1)
		{
		}

		/// <summary>
		/// Implements the TOTP algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		public TotpCalculator(int NrDigits, byte[] Secret)
			: this(NrDigits, Secret, HashFunction.SHA1)
		{
		}

		/// <summary>
		/// Implements the TOTP algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		public TotpCalculator(int NrDigits, byte[] Secret, HashFunction HashFunction)
			: this(NrDigits, Secret, HashFunction, 30)
		{
		}

		/// <summary>
		/// Implements the TOTP algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		public TotpCalculator(int NrDigits, byte[] Secret, HashFunction HashFunction,
			int TimeStepSeconds)
		{
			if (TimeStepSeconds <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(TimeStepSeconds),
					"Time step must be a positive integer.");
			}

			this.timeStepSeconds = TimeStepSeconds;
			this.hotp = new HotpCalculator(NrDigits, Secret, HashFunction);
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
	}
}
