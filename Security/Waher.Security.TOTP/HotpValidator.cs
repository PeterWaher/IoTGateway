using System;
using System.Threading.Tasks;
using Waher.Runtime.Counters;
using Waher.Runtime.Threading;
using Waher.Security.LoginMonitor;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Implements the HOTP validator algorithm, as defined in RFC 4226:
	/// https://datatracker.ietf.org/doc/html/rfc4226
	/// </summary>
	public class HotpValidator
	{
		/// <summary>
		/// Protocol name (HOTP).
		/// </summary>
		public const string ProtocolName = "HOTP";

		private readonly int nrDigits;
		private readonly byte[] secret;
		private readonly HashFunction hashFunction;
		private readonly LoginAuditor auditor;
		private readonly string endpoint;

		/// <summary>
		/// Implements the HOTP validator algorithm, as defined in RFC 4226:
		/// https://datatracker.ietf.org/doc/html/rfc4226
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		public HotpValidator(byte[] Secret, string OtpEndpoint, LoginAuditor Auditor)
			: this(HotpCalculator.DefaultNrDigits, Secret,
				  HotpCalculator.DefaultHashFunction, OtpEndpoint, Auditor)
		{
		}

		/// <summary>
		/// Implements the HOTP validator algorithm, as defined in RFC 4226:
		/// https://datatracker.ietf.org/doc/html/rfc4226
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		public HotpValidator(int NrDigits, byte[] Secret, string OtpEndpoint, 
			LoginAuditor Auditor)
			: this(NrDigits, Secret, HotpCalculator.DefaultHashFunction, OtpEndpoint, Auditor)
		{
		}

		/// <summary>
		/// Implements the HOTP validator algorithm, as defined in RFC 4226:
		/// https://datatracker.ietf.org/doc/html/rfc4226
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		public HotpValidator(int NrDigits, byte[] Secret, HashFunction HashFunction,
			string OtpEndpoint, LoginAuditor Auditor)
		{
			HotpCalculator.CheckNrDigits(NrDigits);
			HotpCalculator.CheckSecret(Secret);

			this.nrDigits = NrDigits;
			this.secret = Secret;
			this.hashFunction = HashFunction;
			this.endpoint = OtpEndpoint;
			this.auditor = Auditor;
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
		/// OTP Endpoint
		/// </summary>
		public string OtpEndpoint => this.endpoint;

		/// <summary>
		/// Login auditor.
		/// </summary>
		public LoginAuditor Auditor => this.auditor;

		/// <summary>
		/// Tries to create an HOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>HOTP Calculator, if endpoint was found, null otherwise.</returns>
		public static Task<HotpValidator> TryCreate(string OtpEndpoint,
			LoginAuditor Auditor)
		{
			return TryCreate(HotpCalculator.DefaultNrDigits, OtpEndpoint, Auditor);
		}

		/// <summary>
		/// Tries to create an HOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>HOTP Calculator, if endpoint was found, null otherwise.</returns>
		public static async Task<HotpValidator> TryCreate(int NrDigits, string OtpEndpoint, 
			LoginAuditor Auditor)
		{
			OtpSecret Secret = await OtpSecret.GetSecret(OtpEndpoint, OtpType.HOTP);
			if (Secret is null)
				return null;

			return new HotpValidator(NrDigits, Secret.Secret, Secret.HashFunction, 
				OtpEndpoint, Auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Counter">Counter value.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <returns>Validation result.</returns>
		public Task<ValidationResult> Validate(string OtpEndpoint, string RemoteEndPoint,
			long Counter, int PassCode)
		{
			return Validate(this.nrDigits, this.secret, this.hashFunction, OtpEndpoint, 
				RemoteEndPoint, Counter, PassCode, this.auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Counter">Counter value.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>Validation result.</returns>
		public static Task<ValidationResult> Validate(byte[] Secret, string OtpEndpoint, 
			string RemoteEndPoint, long Counter, int PassCode, LoginAuditor Auditor)
		{
			return Validate(HotpCalculator.DefaultNrDigits, Secret, OtpEndpoint, 
				RemoteEndPoint, Counter, PassCode, Auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Counter">Counter value.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>Validation result.</returns>
		public static Task<ValidationResult> Validate(int NrDigits, byte[] Secret,
			string OtpEndpoint, string RemoteEndPoint, long Counter, int PassCode, 
			LoginAuditor Auditor)
		{
			return Validate(NrDigits, Secret, HotpCalculator.DefaultHashFunction,
				OtpEndpoint, RemoteEndPoint, Counter, PassCode, Auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Counter">Counter value.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>Validation result.</returns>
		public static async Task<ValidationResult> Validate(int NrDigits, byte[] Secret,
			HashFunction HashFunction, string OtpEndpoint, string RemoteEndPoint, 
			long Counter, int PassCode, LoginAuditor Auditor)
		{
			using Semaphore Lock = await Semaphores.BeginWrite(ProtocolName + ":" + OtpEndpoint);

			string Key = ProtocolName + "." + OtpEndpoint;
			long LastCounter = await RuntimeCounters.GetCount(Key);
			if (Counter <= LastCounter)
			{
				LoginAuditor.Fail("Counter value not valid (replay attack?).", OtpEndpoint,
					RemoteEndPoint, ProtocolName);
				return new ValidationResult();
			}

			await RuntimeCounters.IncrementCounter(Key, Counter - LastCounter);

			DateTime? EarliestOpportunity = await Auditor.GetEarliestLoginOpportunity(RemoteEndPoint, ProtocolName);
			if (EarliestOpportunity.HasValue)
				return new ValidationResult(EarliestOpportunity.Value);

			int Expected = HotpCalculator.Compute(NrDigits, Secret, HashFunction, Counter);
			bool Ok = Expected == PassCode;

			if (Ok)
			{
				LoginAuditor.Success(ProtocolName + " authentication successful.",
					OtpEndpoint, RemoteEndPoint, ProtocolName);
			}
			else
			{
				LoginAuditor.Fail(ProtocolName + " authentication failed.",
					OtpEndpoint, RemoteEndPoint, ProtocolName);
			}

			return new ValidationResult(Ok);
		}
	}
}
