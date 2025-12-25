using System;
using System.Threading.Tasks;
using Waher.Runtime.Counters;
using Waher.Runtime.Threading;
using Waher.Security.LoginMonitor;

namespace Waher.Security.TOTP
{
	/// <summary>
	/// Implements the TOTP validator algorithm, as defined in RFC 6238:
	/// https://datatracker.ietf.org/doc/html/rfc6238
	/// </summary>
	public class TotpValidator
	{
		/// <summary>
		/// Protocol name (TOTP).
		/// </summary>
		public const string ProtocolName = "TOTP";

		private readonly int nrDigits;
		private readonly byte[] secret;
		private readonly HashFunction hashFunction;
		private readonly LoginAuditor auditor;
		private readonly string endpoint;
		private readonly int timeStepSeconds;
		private readonly long t0;

		/// <summary>
		/// Implements the TOTP validator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		public TotpValidator(byte[] Secret, string OtpEndpoint, LoginAuditor Auditor)
			: this(HotpCalculator.DefaultNrDigits, Secret,
				  HotpCalculator.DefaultHashFunction, TotpCalculator.DefaultTimeStepSeconds,
				  OtpEndpoint, Auditor)
		{
		}

		/// <summary>
		/// Implements the TOTP validator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		public TotpValidator(int NrDigits, byte[] Secret, string OtpEndpoint, LoginAuditor Auditor)
			: this(NrDigits, Secret, HotpCalculator.DefaultHashFunction,
				  TotpCalculator.DefaultTimeStepSeconds, OtpEndpoint, Auditor)
		{
		}

		/// <summary>
		/// Implements the TOTP validator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		public TotpValidator(int NrDigits, byte[] Secret, HashFunction HashFunction,
			int TimeStepSeconds, string OtpEndpoint, LoginAuditor Auditor)
			: this(NrDigits, Secret, HashFunction, TimeStepSeconds, TotpCalculator.DefaultT0,
				  OtpEndpoint, Auditor)
		{
		}

		/// <summary>
		/// Implements the TOTP validator algorithm, as defined in RFC 6238:
		/// https://datatracker.ietf.org/doc/html/rfc6238
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		/// <param name="T0">Unix Time when starting counting steps.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		public TotpValidator(int NrDigits, byte[] Secret, HashFunction HashFunction,
			int TimeStepSeconds, long T0, string OtpEndpoint, LoginAuditor Auditor)
		{
			HotpCalculator.CheckNrDigits(NrDigits);
			HotpCalculator.CheckSecret(Secret);
			TotpCalculator.CheckTimeStepSeconds(TimeStepSeconds, T0);

			this.nrDigits = NrDigits;
			this.secret = Secret;
			this.hashFunction = HashFunction;
			this.endpoint = OtpEndpoint;
			this.auditor = Auditor;
			this.timeStepSeconds = TimeStepSeconds;
			this.t0 = T0;
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
		/// Time step in seconds.
		/// </summary>
		public int TimeStepSeconds => this.timeStepSeconds;

		/// <summary>
		/// OTP Endpoint
		/// </summary>
		public string OtpEndpoint => this.endpoint;

		/// <summary>
		/// Login auditor.
		/// </summary>
		public LoginAuditor Auditor => this.auditor;

		/// <summary>
		/// Tries to create an TOTP calculator for the given endpoint.
		/// </summary>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>TOTP Calculator, if endpoint was found, null otherwise.</returns>
		public static async Task<TotpValidator> TryCreate(string OtpEndpoint, LoginAuditor Auditor)
		{
			ExternalCredential Secret = await ExternalCredential.GetSecret(OtpEndpoint, CredentialAlgorithm.TOTP);
			if (Secret is null || !Secret.HashFunction.HasValue)
				return null;

			return new TotpValidator(Secret.NrDigits, Secret.Secret, Secret.HashFunction.Value,
				Secret.TimeStepSeconds, OtpEndpoint, Auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <returns>Validation result.</returns>
		public Task<ValidationResult> Validate(string OtpEndpoint, string RemoteEndPoint,
			DateTime Timestamp, int PassCode)
		{
			return Validate(this.nrDigits, this.secret, this.hashFunction, OtpEndpoint,
				RemoteEndPoint, Timestamp, this.timeStepSeconds, PassCode, this.auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>Validation result.</returns>
		public static Task<ValidationResult> Validate(byte[] Secret, string OtpEndpoint,
			string RemoteEndPoint, DateTime Timestamp, int TimeStepSeconds, int PassCode,
			LoginAuditor Auditor)
		{
			return Validate(HotpCalculator.DefaultNrDigits, Secret, OtpEndpoint,
				RemoteEndPoint, Timestamp, TimeStepSeconds, PassCode, Auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>Validation result.</returns>
		public static Task<ValidationResult> Validate(int NrDigits, byte[] Secret,
			string OtpEndpoint, string RemoteEndPoint, DateTime Timestamp, int TimeStepSeconds,
			int PassCode, LoginAuditor Auditor)
		{
			return Validate(NrDigits, Secret, HotpCalculator.DefaultHashFunction,
				OtpEndpoint, RemoteEndPoint, Timestamp, TimeStepSeconds, PassCode, Auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>Validation result.</returns>
		public static async Task<ValidationResult> Validate(int NrDigits, byte[] Secret,
			HashFunction HashFunction, string OtpEndpoint, string RemoteEndPoint,
			DateTime Timestamp, int TimeStepSeconds, int PassCode, LoginAuditor Auditor)
		{
			return await Validate(NrDigits, Secret, HashFunction, OtpEndpoint,
				RemoteEndPoint, Timestamp, TimeStepSeconds, TotpCalculator.DefaultT0,
				PassCode, Auditor);
		}

		/// <summary>
		/// Calculates the expected one-time-password for the given counter value.
		/// </summary>
		/// <param name="NrDigits">Number of digits to present.</param>
		/// <param name="Secret">Shared secret.</param>
		/// <param name="HashFunction">Hash function to use in computation.</param>
		/// <param name="OtpEndpoint">OTP Endpoint</param>
		/// <param name="RemoteEndPoint">String-representation of remote endpoint.</param>
		/// <param name="Timestamp">Compute code for this point in time.</param>
		/// <param name="TimeStepSeconds">Time step in seconds.</param>
		/// <param name="T0">Unix Time when starting counting steps.</param>
		/// <param name="PassCode">Pass code to validate.</param>
		/// <param name="Auditor">Login auditor.</param>
		/// <returns>Validation result.</returns>
		public static async Task<ValidationResult> Validate(int NrDigits, byte[] Secret,
			HashFunction HashFunction, string OtpEndpoint, string RemoteEndPoint,
			DateTime Timestamp, int TimeStepSeconds, long T0, int PassCode, 
			LoginAuditor Auditor)
		{
			using Semaphore Lock = await Semaphores.BeginWrite(ProtocolName + ":" + OtpEndpoint);

			string Key = ProtocolName + "." + OtpEndpoint;
			long LastCounter = await RuntimeCounters.GetCount(Key);
			long Counter = TotpCalculator.CalcCounter(Timestamp, TimeStepSeconds, T0);

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

			if (!Ok)    // Check previous time step
			{
				Counter--;
				Expected = HotpCalculator.Compute(NrDigits, Secret, HashFunction, Counter);
				Ok = Expected == PassCode;

				if (Ok)
					await RuntimeCounters.DecrementCounter(Key);
			}

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
