using Waher.Content;

namespace Waher.Security.TOTP.Test
{
	[TestClass]
	public sealed class OtpAuthUriTests
	{
		[TestMethod]
		[DataRow("otpauth://totp/Example:alice@google.com?secret=JBSWY3DPEHPK3PXP&issuer=Example",
			CredentialAlgorithm.TOTP, "Example:alice@google.com", "JBSWY3DPEHPK3PXP", "Example",
			"alice@google.com", HashFunction.SHA1, 6, 30)]
		[DataRow("otpauth://totp/ACME%20Co:john.doe@email.com?secret=HXDMVJECJJWSRB3HWIZR4IFUGFTMXBOZ&issuer=ACME%20Co&algorithm=SHA1&digits=6&period=30",
			CredentialAlgorithm.TOTP, "ACME Co:john.doe@email.com", "HXDMVJECJJWSRB3HWIZR4IFUGFTMXBOZ", "ACME Co",
			"john.doe@email.com", HashFunction.SHA1, 6, 30)]
		[DataRow("otpauth://totp/example:alice@google.com?secret=jbswy3dpehpk3pxp&issuer=example&algorithm=sha256",
			CredentialAlgorithm.TOTP, "example:alice@google.com", "JBSWY3DPEHPK3PXP", "example",
			"alice@google.com", HashFunction.SHA256, 6, 30)]
		[DataRow("otpauth://totp/acme%20co:john.doe@email.com?secret=hxdmvjecjjwsrb3hwizr4ifugftmxboz&issuer=acme%20co&algorithm=sha512&digits=6&period=30",
			CredentialAlgorithm.TOTP, "acme co:john.doe@email.com", "HXDMVJECJJWSRB3HWIZR4IFUGFTMXBOZ", "acme co",
			"john.doe@email.com", HashFunction.SHA512, 6, 30)]
		public void Test_01_Parse(string Uri, CredentialAlgorithm OtpAlgorithm, string Label,
			string Secret, string Issuer, string Account, HashFunction Algorithm, int Digits, 
			int CounterPeriod)
		{
			ExternalCredential? Credentials = ExternalCredential.TryParse(Uri);
			Assert.IsNotNull(Credentials);
			Assert.AreEqual(OtpAlgorithm, Credentials.Type);
			Assert.AreEqual(Label, Credentials.Label);
			Assert.AreEqual(Secret, Base32.Encode(Credentials.Secret));
			Assert.AreEqual(Issuer, Credentials.Issuer);
			Assert.AreEqual(Account, Credentials.Account);
			Assert.AreEqual(Algorithm, Credentials.HashFunction);
			Assert.AreEqual(Digits, Credentials.NrDigits);

			switch (OtpAlgorithm)
			{
				case CredentialAlgorithm.HOTP:
					Assert.AreEqual(CounterPeriod, Credentials.Counter);
					break;
			
				case CredentialAlgorithm.TOTP:
					Assert.AreEqual(CounterPeriod, Credentials.TimeStepSeconds);
					break;
			}
		}
	}
}
