using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Waher.Security.PKCS.Certificates.CRMF;

namespace Waher.Security.PKCS.Test
{
	[TestClass]
	public class CertificateRequestTests
	{
		[TestInitialize]
		public void TestInitialize()
		{
		}

		[TestMethod]
		public void Test_01_EncodeDecode()
		{
			using RSACryptoServiceProvider RSA = new(4096);
			CertificateRequest Csr = new(new RsaSha256(RSA))
			{
				CommonName = "Unit Test",
				Surname = "Test",
				SerialNumber = "1234",
				Country = "XX",
				StateOrProvince = "XXX",
				Locality = "XXXX",
				StreetAddress = "Street 1",
				Organization = "Org",
				OrganizationalUnit = "R&D",
				Title = "Bit",
				Description = "Request made for test purposes",
				PostalAddress = "Somewhere",
				PostalCode = "12345",
				PostOfficeBox = "P.O. Box 123",
				PhysicalDeliveryOfficeName = "Of. 1",
				TelephoneNumber = "+12345678",
				RegisteredAddress = "Street 2",
				PresentationAddress = "Street 3",
				Name = "Unit Test",
				GivenName = "Unit",
				Initials = "U.T.",
				DistinguishedName = "UNIT",
				HouseIdentifier = "Bld A",
				SubjectAlternativeNames = ["Test_01_EncodeDecode"],
				EMailAddress = "unit.test@example.org"
			};

			byte[] Encoded = Csr.BuildCSR();

			AsnReader Reader = new(Encoded, AsnEncodingRules.DER);
			CertRequest CertRequest = CertRequest.Create(Reader);
		}

	}
}
