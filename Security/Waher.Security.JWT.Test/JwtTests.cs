using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.JWT.Test
{
	[TestClass]
	public class JwtTests
	{
		[TestMethod]
		public void JWT_Test_01_Parse_Secure()
		{
			JwtToken Token = new JwtToken("eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQogImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ.dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk");

			Assert.AreEqual("JWT", Token.Type);
			Assert.AreEqual(JwtAlgorithm.HS256, Token.Algorithm);

			Assert.AreEqual(true, Token.TryGetClaim("iss", out object Issuer));
			Assert.AreEqual("joe", Issuer);
			Assert.AreEqual("joe", Token.Issuer);

			Assert.AreEqual(true, Token.TryGetClaim("exp", out object Expires));
			Assert.AreEqual(1300819380, Expires);
			Assert.AreEqual(new DateTime(2011, 3, 22, 18, 43, 0, DateTimeKind.Utc), Token.Expiration);

			Assert.AreEqual(true, Token.TryGetClaim("http://example.com/is_root", out object IsRoot));
			Assert.AreEqual(true, IsRoot);
		}

		[TestMethod]
		public void JWT_Test_02_Parse_Unsecure()
		{
			JwtToken Token = new JwtToken("eyJhbGciOiJub25lIn0.eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQogImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ.");

			Assert.AreEqual(JwtAlgorithm.none, Token.Algorithm);

			Assert.AreEqual(true, Token.TryGetClaim("iss", out object Issuer));
			Assert.AreEqual("joe", Issuer);
			Assert.AreEqual("joe", Token.Issuer);

			Assert.AreEqual(true, Token.TryGetClaim("exp", out object Expires));
			Assert.AreEqual(1300819380, Expires);
			Assert.AreEqual(new DateTime(2011, 3, 22, 18, 43, 0, DateTimeKind.Utc), Token.Expiration);

			Assert.AreEqual(true, Token.TryGetClaim("http://example.com/is_root", out object IsRoot));
			Assert.AreEqual(true, IsRoot);
		}

		[TestMethod]
		public void JWT_Test_03_Validate_ValidToken()
		{
			JwtToken Token = new JwtToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ");

			Assert.AreEqual("JWT", Token.Type);
			Assert.AreEqual(JwtAlgorithm.HS256, Token.Algorithm);

			Assert.AreEqual(true, Token.TryGetClaim("sub", out object Subject));
			Assert.AreEqual("1234567890", Subject);
			Assert.AreEqual("1234567890", Token.Subject);

			Assert.AreEqual(true, Token.TryGetClaim("name", out object Name));
			Assert.AreEqual("John Doe", Name);

			Assert.AreEqual(true, Token.TryGetClaim("admin", out object Admin));
			Assert.AreEqual(true, Admin);

			using (JwtFactory Factory = new JwtFactory(Encoding.ASCII.GetBytes("secret")))
			{
				Assert.AreEqual(true, Factory.IsValid(Token));
			}
		}

		[TestMethod]
		public void JWT_Test_04_Validate_InvalidToken()
		{
			JwtToken Token = new JwtToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ");

			Assert.AreEqual("JWT", Token.Type);
			Assert.AreEqual(JwtAlgorithm.HS256, Token.Algorithm);

			Assert.AreEqual(true, Token.TryGetClaim("sub", out object Subject));
			Assert.AreEqual("1234567890", Subject);
			Assert.AreEqual("1234567890", Token.Subject);

			Assert.AreEqual(true, Token.TryGetClaim("name", out object Name));
			Assert.AreEqual("John Doe", Name);

			Assert.AreEqual(true, Token.TryGetClaim("admin", out object Admin));
			Assert.AreEqual(true, Admin);

			using (JwtFactory Factory = new JwtFactory(Encoding.ASCII.GetBytes("wrong secret")))
			{
				Assert.AreEqual(false, Factory.IsValid(Token));
			}
		}

		[TestMethod]
		public void JWT_Test_05_CreateToken()
		{
			using (JwtFactory Factory = new JwtFactory(Encoding.ASCII.GetBytes("secret")))
			{
				DateTime Expires = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				string TokenStr = Factory.Create(
					new KeyValuePair<string, object>("sub", "test user"),
					new KeyValuePair<string, object>("exp", Expires));
				JwtToken Token = new JwtToken(TokenStr);

				Assert.AreEqual("JWT", Token.Type);
				Assert.AreEqual(JwtAlgorithm.HS256, Token.Algorithm);

				Assert.AreEqual(true, Token.TryGetClaim("sub", out object Subject));
				Assert.AreEqual("test user", Subject);
				Assert.AreEqual("test user", Token.Subject);

				Assert.AreEqual(Expires, Token.Expiration);

				Assert.AreEqual(true, Factory.IsValid(Token));
			}
		}
	}
}
