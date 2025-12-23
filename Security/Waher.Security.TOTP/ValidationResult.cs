using System;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Validation result enumeration.
	/// </summary>
	public enum OtpValidationResult
	{
		/// <summary>
		/// Pass Code valid.
		/// </summary>
		Valid,

		/// <summary>
		/// Pass Code invalid.
		/// </summary>
		Invalid,

		/// <summary>
		/// Counter provided invalid.
		/// </summary>
		CounterInvalid,

		/// <summary>
		/// Endpoint is temporarily blocked.
		/// </summary>
		TemporaryBlock,

		/// <summary>
		/// Endpoint is permanently blocked.
		/// </summary>
		PermanentBlock
	}

	/// <summary>
	/// Contains information of a HOTP or TOTP validation attempt.
	/// </summary>
	public class ValidationResult
	{
		private readonly OtpValidationResult result;
		private readonly DateTime blockedUntil;

		/// <summary>
		/// Contains information of a HOTP or TOTP validation attempt.
		/// </summary>
		/// <param name="Ok">If code was valid or not.</param>
		public ValidationResult(bool Ok)
		{
			this.result = Ok ? OtpValidationResult.Valid : OtpValidationResult.Invalid;
			this.blockedUntil = DateTime.MinValue;
		}

		/// <summary>
		/// Contains information of a HOTP or TOTP validation attempt.
		/// </summary>
		/// <param name="BlockedUntil">Timestamp of block.</param>
		public ValidationResult(DateTime BlockedUntil)
		{
			this.result = BlockedUntil == DateTime.MaxValue ?
				OtpValidationResult.PermanentBlock :
				OtpValidationResult.TemporaryBlock;
			this.blockedUntil = BlockedUntil;
		}

		/// <summary>
		/// Contains information of a HOTP or TOTP validation attempt.
		/// </summary>
		public ValidationResult()
		{
			this.result = OtpValidationResult.CounterInvalid;
			this.blockedUntil = DateTime.MinValue;
		}

		/// <summary>
		/// Validation result.
		/// </summary>
		public OtpValidationResult Result => this.result;
	}
}
