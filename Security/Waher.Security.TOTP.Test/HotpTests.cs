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
	}
}
