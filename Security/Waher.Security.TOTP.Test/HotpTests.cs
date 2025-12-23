using Waher.Runtime.Counters;

namespace Waher.Security.TOTP.Test
{
	[TestClass]
	public sealed class HotpTests
	{
		[TestMethod]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 0L, 755224)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 1L, 287082)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 2L, 359152)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 3L, 969429)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 4L, 338314)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 5L, 254676)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 6L, 287922)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 7L, 162583)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 8L, 399871)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 9L, 520489)]
		public void Test_01_Calculate(HashFunction HashFunction, int NrDigits, string Secret,
			long Counter, int Password)
		{
			HotpCalculator Hotp = new(NrDigits, Hashes.StringToBinary(Secret), HashFunction);
			int ComputedPassword = Hotp.Compute(Counter);

			Assert.AreEqual(Password, ComputedPassword);
		}

		[TestMethod]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", -1L, 0L, 755224, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 0L, 1L, 287082, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 1L, 2L, 359152, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 2L, 3L, 969429, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 3L, 4L, 338314, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 4L, 5L, 254676, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 5L, 6L, 287922, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 6L, 7L, 162583, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 7L, 8L, 399871, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 8L, 9L, 520489, OtpValidationResult.Valid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 9L, 9L, 520489, OtpValidationResult.CounterInvalid)]
		[DataRow(HashFunction.SHA1, 6, "3132333435363738393031323334353637383930", 9L, 10L, 520489, OtpValidationResult.Invalid)]
		public async Task Test_02_Validate(HashFunction HashFunction, int NrDigits,
			string Secret, long PrevCounter, long Counter, int Password, 
			OtpValidationResult ExpectedResult)
		{
			long UsedCounter = await RuntimeCounters.GetCount("HOTP.UnitTest");
			await RuntimeCounters.IncrementCounter("HOTP.UnitTest", PrevCounter - UsedCounter);

			HotpValidator Hotp = new(NrDigits, Hashes.StringToBinary(Secret), HashFunction,
				"UnitTest", TotpTests.Auditor);
			ValidationResult Result = await Hotp.Validate("UnitTest", "EP", Counter, Password);

			Assert.AreEqual(ExpectedResult, Result.Result);
		}
	}
}
