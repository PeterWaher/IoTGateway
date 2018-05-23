using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Security.JWS;

namespace Waher.Security.ACME.Test
{
	[TestClass]
	public class DerTests
	{
		private DerEncoder derOutput;

		[TestInitialize]
		public void TestInitialize()
		{
			this.derOutput = new DerEncoder();
		}

		private void AssertEqualTo(byte[] Expected)
		{
			byte[] Bin = this.derOutput.ToArray();
			int i, c = Bin.Length, d = Expected.Length, Len = Math.Min(c, d);

			for (i = 0; i < Len; i++)
			{
				if (Expected[i] != Bin[i])
					throw this.CreateException("Byte " + i.ToString() + " contained " + Bin[i].ToString() + ", not " + Expected[i].ToString() + ", as expected.", Expected, Bin);
			}

			if (c != d)
				throw this.CreateException("Output length not of expected length.", Expected, Bin);
		}

		private Exception CreateException(string Msg, byte[] Expected, byte[] Bin)
		{
			StringBuilder Message = new StringBuilder();

			Message.AppendLine(Msg);
			Message.AppendLine();

			Message.AppendLine("Generated:");
			this.Append(Message, Bin);
			Message.AppendLine();

			Message.AppendLine();
			Message.AppendLine("Expected:");
			this.Append(Message, Expected);

			return new Exception(Message.ToString());
		}

		private void Append(StringBuilder Message, byte[] Bin)
		{
			int i = 0;
			foreach (byte b in Bin)
			{
				if (i++ == 0)
					Message.AppendLine();
				else
				{
					i &= 15;
					Message.Append(' ');
				}

				Message.Append(b.ToString());
			}
		}

		[TestMethod]
		public void DER_Test_01_BOOLEAN_1()
		{
			this.derOutput.BOOLEAN(false);
			this.AssertEqualTo(new byte[] { 1, 1, 0 });
		}

		[TestMethod]
		public void DER_Test_02_BOOLEAN_2()
		{
			this.derOutput.BOOLEAN(true);
			this.AssertEqualTo(new byte[] { 1, 1, 255 });
		}

		[TestMethod]
		public void DER_Test_03_INTEGER_1()
		{
			this.derOutput.INTEGER(0x8f);
			this.AssertEqualTo(new byte[] { 2, 2, 0, 0x8f });
		}

		[TestMethod]
		public void DER_Test_04_INTEGER_2()
		{
			this.derOutput.INTEGER(3);
			this.AssertEqualTo(new byte[] { 2, 1, 3 });
		}

		[TestMethod]
		public void DER_Test_05_INTEGER_3()
		{
			this.derOutput.INTEGER(-200);
			this.AssertEqualTo(new byte[] { 2, 2, 0xff, 0x38 });
		}

		[TestMethod]
		public void DER_Test_06_INTEGER_4()
		{
			this.derOutput.INTEGER(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);
			this.AssertEqualTo(new byte[] { 2, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
		}

		[TestMethod]
		public void DER_Test_07_INTEGER_5()
		{
			this.derOutput.INTEGER(new byte[]
			{
				165,155,88,36,33,201,225,90,85,
				92,119,213,34,91,45,65,57,78,226,
				160,180,222,24,215,249,153,15,202,247,
				88,119,84,56,250,166,192,121,60,23,
				152,37,10,96,116,225,85,23,121,105,
				116,170,148,252,242,32,28,34,120,198,
				107,88,89,81,65,215,89,212,140,215,
				2,85,81,13,108,0,163,173,10,121,
				202,158,1,121,231,80,175,185,125,122,
				19,35,245,32,90,200,107,123,203,73,
				251,31,211,48,118,144,41,31,201,112,
				133,143,134,166,144,134,194,110,205,150,
				239,1,209,128,243,64,75,197,211
			}, false);
			this.AssertEqualTo(new byte[] { 2, 129, 129, 0, 165, 155, 88, 36, 33, 201, 225, 90, 85, 92, 119, 213, 34, 91, 45, 65, 57, 78, 226, 160, 180, 222, 24, 215, 249, 153, 15, 202, 247, 88, 119, 84, 56, 250, 166, 192, 121, 60, 23, 152, 37, 10, 96, 116, 225, 85, 23, 121, 105, 116, 170, 148, 252, 242, 32, 28, 34, 120, 198, 107, 88, 89, 81, 65, 215, 89, 212, 140, 215, 2, 85, 81, 13, 108, 0, 163, 173, 10, 121, 202, 158, 1, 121, 231, 80, 175, 185, 125, 122, 19, 35, 245, 32, 90, 200, 107, 123, 203, 73, 251, 31, 211, 48, 118, 144, 41, 31, 201, 112, 133, 143, 134, 166, 144, 134, 194, 110, 205, 150, 239, 1, 209, 128, 243, 64, 75, 197, 211 });
		}

		[TestMethod]
		public void DER_Test_08_BITSTRING_1()
		{
			BitArray Bits = new BitArray(20);
			int i;

			for (i = 0; i < 20; i++)
				Bits[i] = ((i & 1) == 0);

			this.derOutput.BITSTRING(Bits);
			this.AssertEqualTo(new byte[] { 3, 4, 4, 0xaa, 0xaa, 0xa0 });
		}

		[TestMethod]
		public void DER_Test_09_BITSTRING_2()
		{
			this.derOutput.StartBITSTRING();
			this.derOutput.StartSEQUENCE();
			this.derOutput.INTEGER(new byte[]
			{
				165,155,88,36,33,201,225,90,85,
				92,119,213,34,91,45,65,57,78,226,
				160,180,222,24,215,249,153,15,202,247,
				88,119,84,56,250,166,192,121,60,23,
				152,37,10,96,116,225,85,23,121,105,
				116,170,148,252,242,32,28,34,120,198,
				107,88,89,81,65,215,89,212,140,215,
				2,85,81,13,108,0,163,173,10,121,
				202,158,1,121,231,80,175,185,125,122,
				19,35,245,32,90,200,107,123,203,73,
				251,31,211,48,118,144,41,31,201,112,
				133,143,134,166,144,134,194,110,205,150,
				239,1,209,128,243,64,75,197,211
			}, false);  // Modulus
			this.derOutput.INTEGER(new byte[] { 1, 0, 1 }, false);  // Exponent
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndBITSTRING();

			this.AssertEqualTo(new byte[] { 3, 129, 141, 0, 48, 129, 137, 2, 129, 129, 0, 165, 155, 88, 36, 33, 201, 225, 90, 85, 92, 119, 213, 34, 91, 45, 65, 57, 78, 226, 160, 180, 222, 24, 215, 249, 153, 15, 202, 247, 88, 119, 84, 56, 250, 166, 192, 121, 60, 23, 152, 37, 10, 96, 116, 225, 85, 23, 121, 105, 116, 170, 148, 252, 242, 32, 28, 34, 120, 198, 107, 88, 89, 81, 65, 215, 89, 212, 140, 215, 2, 85, 81, 13, 108, 0, 163, 173, 10, 121, 202, 158, 1, 121, 231, 80, 175, 185, 125, 122, 19, 35, 245, 32, 90, 200, 107, 123, 203, 73, 251, 31, 211, 48, 118, 144, 41, 31, 201, 112, 133, 143, 134, 166, 144, 134, 194, 110, 205, 150, 239, 1, 209, 128, 243, 64, 75, 197, 211, 2, 3, 1, 0, 1 });
		}

		[TestMethod]
		public void DER_Test_10_BITSTRING_3()
		{
			this.derOutput.BITSTRING(new byte[]
			{
				136,101,224,251,197,171,231,187,138,144,
				17,128,142,89,51,225,153,199,169,169,
				11,62,56,13,58,52,38,76,31,68,
				55,254,6,218,236,104,45,42,235,199,
				147,127,101,225,96,235,42,225,249,7,
				67,75,128,209,165,104,131,119,249,136,
				156,66,107,113,233,230,12,91,37,14,
				179,87,169,9,209,49,44,185,166,142,
				132,73,40,246,199,105,10,131,206,157,
				16,102,218,47,215,68,61,131,234,35,
				97,241,255,51,209,40,70,176,90,163,
				14,1,71,200,93,232,250,85,50,143,
				229,2,232,125,80,91,182,231
			});

			this.AssertEqualTo(new byte[] { 3, 129, 129, 0, 136, 101, 224, 251, 197, 171, 231, 187, 138, 144, 17, 128, 142, 89, 51, 225, 153, 199, 169, 169, 11, 62, 56, 13, 58, 52, 38, 76, 31, 68, 55, 254, 6, 218, 236, 104, 45, 42, 235, 199, 147, 127, 101, 225, 96, 235, 42, 225, 249, 7, 67, 75, 128, 209, 165, 104, 131, 119, 249, 136, 156, 66, 107, 113, 233, 230, 12, 91, 37, 14, 179, 87, 169, 9, 209, 49, 44, 185, 166, 142, 132, 73, 40, 246, 199, 105, 10, 131, 206, 157, 16, 102, 218, 47, 215, 68, 61, 131, 234, 35, 97, 241, 255, 51, 209, 40, 70, 176, 90, 163, 14, 1, 71, 200, 93, 232, 250, 85, 50, 143, 229, 2, 232, 125, 80, 91, 182, 231 });
		}

		[TestMethod]
		public void DER_Test_11_OCTET_STRING_1()
		{
			this.derOutput.OCTET_STRING(new byte[] { 0x1e, 0x08, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72 });
			this.AssertEqualTo(new byte[] { 4, 10, 0x1e, 0x08, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72 });
		}

		[TestMethod]
		public void DER_Test_12_NULL_1()
		{
			this.derOutput.NULL();
			this.AssertEqualTo(new byte[] { 5, 0 });
		}

		[TestMethod]
		public void DER_Test_13_OBJECT_IDENTIFIER_1()
		{
			this.derOutput.OBJECT_IDENTIFIER("1.3.6.1.4.1.311.21.20");
			this.AssertEqualTo(new byte[] { 6, 9, 0x2b, 0x06, 0x01, 0x04, 0x01, 0x82, 0x37, 0x15, 0x14 });
		}

		[TestMethod]
		public void DER_Test_14_UNICODE_STRING_1()
		{
			this.derOutput.UNICODE_STRING("CertificateTemplate");
			this.AssertEqualTo(new byte[] { 0x1e, 0x26, 00, 0x43, 0x00, 0x65, 0x00, 0x72, 0x00, 0x74, 0x00, 0x69, 0x00, 0x66, 0x00, 0x69, 0x00, 0x63, 0x00, 0x61, 0x00, 0x74, 0x00, 0x65, 0x00, 0x54, 0x00, 0x65, 0x00, 0x6d, 0x00, 0x70, 0x00, 0x6c, 0x00, 0x61, 0x00, 0x74, 0x00, 0x65 });
		}

		[TestMethod]
		public void DER_Test_15_IA5_STRING_1()
		{
			this.derOutput.IA5_STRING("6.0.5361.2");
			this.AssertEqualTo(new byte[] { 0x16, 0x0a, 0x36, 0x2e, 0x30, 0x2e, 0x35, 0x33, 0x36, 0x31, 0x2e, 0x32 });
		}

		[TestMethod]
		public void DER_Test_16_PRINTABLE_STRING_1()
		{
			this.derOutput.PRINTABLE_STRING("TestCN");
			this.AssertEqualTo(new byte[] { 0x13, 0x06, 0x54, 0x65, 0x73, 0x74, 0x43, 0x4e });
		}

		[TestMethod]
		public void DER_Test_17_UTF8_STRING_1()
		{
			this.derOutput.UTF8_STRING("Hellö");
			this.AssertEqualTo(new byte[] { 0x0c, 0x06, 72, 101, 108, 108, 195, 182 });
		}

		[TestMethod]
		public void DER_Test_18_SEQUENCE_1()
		{
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");
			this.derOutput.NULL();
			this.derOutput.EndSEQUENCE();

			this.AssertEqualTo(new byte[] { 0x30, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00 });
		}

		[TestMethod]
		public void DER_Test_19_SET_1()
		{
			this.derOutput.StartSET();
			this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");
			this.derOutput.NULL();
			this.derOutput.EndSET();

			this.AssertEqualTo(new byte[] { 0x31, 0x0d, 0x06, 0x09, 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01, 0x05, 0x00 });
		}

		[TestMethod]
		public void DER_Test_20_CSR_1_Replicate()
		{
			// Example taken from: https://www.sslshopper.com/what-is-a-csr-certificate-signing-request.html

			this.derOutput.StartSEQUENCE();     // CertificationRequest
			this.derOutput.StartSEQUENCE();     // CertificationRequestInfo 
			this.derOutput.INTEGER(0);          // Version

			this.derOutput.StartSEQUENCE();     // subject
			this.derOutput.StartSET();
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("2.5.4.6");    // Country Name
			this.derOutput.PRINTABLE_STRING("US");
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndSET();

			this.derOutput.StartSET();
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("2.5.4.8");    // State or Province Name
			this.derOutput.PRINTABLE_STRING("California");
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndSET();

			this.derOutput.StartSET();
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("2.5.4.7");    // Locality Name
			this.derOutput.PRINTABLE_STRING("Mountain View");
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndSET();

			this.derOutput.StartSET();
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("2.5.4.10");    // Organization Name
			this.derOutput.PRINTABLE_STRING("Google Inc");
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndSET();

			this.derOutput.StartSET();
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("2.5.4.11");    // Organizational Unit Name
			this.derOutput.PRINTABLE_STRING("Information Technology");
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndSET();

			this.derOutput.StartSET();
			this.derOutput.StartSEQUENCE();
			this.derOutput.OBJECT_IDENTIFIER("2.5.4.3");    // Common Name
			this.derOutput.PRINTABLE_STRING("www.google.com");
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndSET();
			this.derOutput.EndSEQUENCE();       // end of subject

			this.derOutput.StartSEQUENCE();     // subjectPKInfo
			this.derOutput.StartSEQUENCE();     // algorithm
			this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");   // RSA Encryption
			this.derOutput.NULL();  // No parameters
			this.derOutput.EndSEQUENCE();       // end of algorithm
			this.derOutput.StartBITSTRING();    // subjectPublicKey
			this.derOutput.StartSEQUENCE();
			this.derOutput.INTEGER(new byte[]	// Modulus
			{
				165,155,88,36,33,201,225,90,85,
				92,119,213,34,91,45,65,57,78,226,
				160,180,222,24,215,249,153,15,202,247,
				88,119,84,56,250,166,192,121,60,23,
				152,37,10,96,116,225,85,23,121,105,
				116,170,148,252,242,32,28,34,120,198,
				107,88,89,81,65,215,89,212,140,215,
				2,85,81,13,108,0,163,173,10,121,
				202,158,1,121,231,80,175,185,125,122,
				19,35,245,32,90,200,107,123,203,73,
				251,31,211,48,118,144,41,31,201,112,
				133,143,134,166,144,134,194,110,205,150,
				239,1,209,128,243,64,75,197,211
			}, false);
			this.derOutput.INTEGER(new byte[] { 1, 0, 1 }, false);  // Exponent
			this.derOutput.EndSEQUENCE();
			this.derOutput.EndBITSTRING();      // end of subjectPublicKey
			this.derOutput.EndSEQUENCE();       // end of subjectPKInfo

			this.derOutput.EndOfContent(Asn1TypeClass.ContextSpecific);     // attributes
			this.derOutput.EndSEQUENCE();       // end of CertificationRequestInfo

			this.derOutput.StartSEQUENCE();     // signatureAlgorithm
			this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.5");   // algorithm (SHA-1 with RSA Encryption)
			this.derOutput.NULL();              // parameters
			this.derOutput.EndSEQUENCE();       // End of signatureAlgorithm

			this.derOutput.BITSTRING(new byte[]	// signature
			{
				136,101,224,251,197,171,231,187,138,144,
				17,128,142,89,51,225,153,199,169,169,
				11,62,56,13,58,52,38,76,31,68,
				55,254,6,218,236,104,45,42,235,199,
				147,127,101,225,96,235,42,225,249,7,
				67,75,128,209,165,104,131,119,249,136,
				156,66,107,113,233,230,12,91,37,14,
				179,87,169,9,209,49,44,185,166,142,
				132,73,40,246,199,105,10,131,206,157,
				16,102,218,47,215,68,61,131,234,35,
				97,241,255,51,209,40,70,176,90,163,
				14,1,71,200,93,232,250,85,50,143,
				229,2,232,125,80,91,182,231
			});

			this.derOutput.EndSEQUENCE();       // end of CertificationRequest

			this.AssertEqualTo(Convert.FromBase64String("MIIByjCCATMCAQAwgYkxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpDYWxpZm9ybmlhMRYwFAYDVQQHEw1Nb3VudGFpbiBWaWV3MRMwEQYDVQQKEwpHb29nbGUgSW5jMR8wHQYDVQQLExZJbmZvcm1hdGlvbiBUZWNobm9sb2d5MRcwFQYDVQQDEw53d3cuZ29vZ2xlLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEApZtYJCHJ4VpVXHfVIlstQTlO4qC03hjX+ZkPyvdYd1Q4+qbAeTwXmCUKYHThVRd5aXSqlPzyIBwieMZrWFlRQddZ1IzXAlVRDWwAo60KecqeAXnnUK+5fXoTI/UgWshre8tJ+x/TMHaQKR/JcIWPhqaQhsJuzZbvAdGA80BLxdMCAwEAAaAAMA0GCSqGSIb3DQEBBQUAA4GBAIhl4PvFq+e7ipARgI5ZM+GZx6mpCz44DTo0JkwfRDf+BtrsaC0q68eTf2XhYOsq4fkHQ0uA0aVog3f5iJxCa3Hp5gxbJQ6zV6kJ0TEsuaaOhEko9sdpCoPOnRBm2i/XRD2D6iNh8f8z0ShGsFqjDgFHyF3o+lUyj+UC6H1QW7bn"));
		}

		[TestMethod]
		public void DER_Test_21_CSR_2_Generate_1024_SHA1()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(1024))
			{
				RSAParameters Parameters = RSA.ExportParameters(false);

				// Example taken from: https://www.sslshopper.com/what-is-a-csr-certificate-signing-request.html

				this.derOutput.StartSEQUENCE();     // CertificationRequestInfo 
				this.derOutput.INTEGER(0);          // Version

				this.derOutput.StartSEQUENCE();     // subject
				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.6");    // Country Name
				this.derOutput.PRINTABLE_STRING("US");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.8");    // State or Province Name
				this.derOutput.PRINTABLE_STRING("California");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.7");    // Locality Name
				this.derOutput.PRINTABLE_STRING("Mountain View");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.10");    // Organization Name
				this.derOutput.PRINTABLE_STRING("Google Inc");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.11");    // Organizational Unit Name
				this.derOutput.PRINTABLE_STRING("Information Technology");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.3");    // Common Name
				this.derOutput.PRINTABLE_STRING("www.google.com");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();
				this.derOutput.EndSEQUENCE();       // end of subject

				this.derOutput.StartSEQUENCE();     // subjectPKInfo
				this.derOutput.StartSEQUENCE();     // algorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");   // RSA Encryption
				this.derOutput.NULL();  // No parameters
				this.derOutput.EndSEQUENCE();       // end of algorithm
				this.derOutput.StartBITSTRING();    // subjectPublicKey
				this.derOutput.StartSEQUENCE();
				this.derOutput.INTEGER(Parameters.Modulus, false);
				this.derOutput.INTEGER(Parameters.Exponent, false);
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndBITSTRING();      // end of subjectPublicKey
				this.derOutput.EndSEQUENCE();       // end of subjectPKInfo

				this.derOutput.EndOfContent(Asn1TypeClass.ContextSpecific);     // attributes
				this.derOutput.EndSEQUENCE();       // end of CertificationRequestInfo


				byte[] CertificationRequestInfo = this.derOutput.ToArray();

				this.derOutput.Clear();
				this.derOutput.StartSEQUENCE();     // CertificationRequest
				this.derOutput.Raw(CertificationRequestInfo);

				this.derOutput.StartSEQUENCE();     // signatureAlgorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.5");   // algorithm (SHA-1 with RSA Encryption)
				this.derOutput.NULL();              // parameters
				this.derOutput.EndSEQUENCE();       // End of signatureAlgorithm

				this.derOutput.BITSTRING(RSA.SignData(CertificationRequestInfo, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));

				this.derOutput.EndSEQUENCE();       // end of CertificationRequest

				byte[] CSR = this.derOutput.ToArray();

				this.PrintCSR(CSR);
			}
		}

		[TestMethod]
		public void DER_Test_22_CSR_3_Generate_2048_SHA1()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048))
			{
				RSAParameters Parameters = RSA.ExportParameters(false);

				// Example taken from: https://www.sslshopper.com/what-is-a-csr-certificate-signing-request.html

				this.derOutput.StartSEQUENCE();     // CertificationRequestInfo 
				this.derOutput.INTEGER(0);          // Version

				this.derOutput.StartSEQUENCE();     // subject
				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.6");    // Country Name
				this.derOutput.PRINTABLE_STRING("US");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.8");    // State or Province Name
				this.derOutput.PRINTABLE_STRING("California");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.7");    // Locality Name
				this.derOutput.PRINTABLE_STRING("Mountain View");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.10");    // Organization Name
				this.derOutput.PRINTABLE_STRING("Google Inc");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.11");    // Organizational Unit Name
				this.derOutput.PRINTABLE_STRING("Information Technology");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();

				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.3");    // Common Name
				this.derOutput.PRINTABLE_STRING("www.google.com");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();
				this.derOutput.EndSEQUENCE();       // end of subject

				this.derOutput.StartSEQUENCE();     // subjectPKInfo
				this.derOutput.StartSEQUENCE();     // algorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");   // RSA Encryption
				this.derOutput.NULL();  // No parameters
				this.derOutput.EndSEQUENCE();       // end of algorithm
				this.derOutput.StartBITSTRING();    // subjectPublicKey
				this.derOutput.StartSEQUENCE();
				this.derOutput.INTEGER(Parameters.Modulus, false);
				this.derOutput.INTEGER(Parameters.Exponent, false);
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndBITSTRING();      // end of subjectPublicKey
				this.derOutput.EndSEQUENCE();       // end of subjectPKInfo

				this.derOutput.EndOfContent(Asn1TypeClass.ContextSpecific);     // attributes
				this.derOutput.EndSEQUENCE();       // end of CertificationRequestInfo


				byte[] CertificationRequestInfo = this.derOutput.ToArray();

				this.derOutput.Clear();
				this.derOutput.StartSEQUENCE();     // CertificationRequest
				this.derOutput.Raw(CertificationRequestInfo);

				this.derOutput.StartSEQUENCE();     // signatureAlgorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.5");   // algorithm (SHA-1 with RSA Encryption)
				this.derOutput.NULL();              // parameters
				this.derOutput.EndSEQUENCE();       // End of signatureAlgorithm

				this.derOutput.BITSTRING(RSA.SignData(CertificationRequestInfo, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));

				this.derOutput.EndSEQUENCE();       // end of CertificationRequest

				byte[] CSR = this.derOutput.ToArray();

				this.PrintCSR(CSR);
			}
		}

		[TestMethod]
		public void DER_Test_23_CSR_4_Generate_Simple_SHA1()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048))
			{
				RSAParameters Parameters = RSA.ExportParameters(false);

				// Example taken from: https://www.sslshopper.com/what-is-a-csr-certificate-signing-request.html

				this.derOutput.StartSEQUENCE();     // CertificationRequestInfo 
				this.derOutput.INTEGER(0);          // Version

				this.derOutput.StartSEQUENCE();     // subject
				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.3");    // Common Name
				this.derOutput.PRINTABLE_STRING("www.example.com");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();
				this.derOutput.EndSEQUENCE();       // end of subject

				this.derOutput.StartSEQUENCE();     // subjectPKInfo
				this.derOutput.StartSEQUENCE();     // algorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");   // RSA Encryption
				this.derOutput.NULL();  // No parameters
				this.derOutput.EndSEQUENCE();       // end of algorithm
				this.derOutput.StartBITSTRING();    // subjectPublicKey
				this.derOutput.StartSEQUENCE();
				this.derOutput.INTEGER(Parameters.Modulus, false);
				this.derOutput.INTEGER(Parameters.Exponent, false);
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndBITSTRING();      // end of subjectPublicKey
				this.derOutput.EndSEQUENCE();       // end of subjectPKInfo

				this.derOutput.EndOfContent(Asn1TypeClass.ContextSpecific);     // attributes
				this.derOutput.EndSEQUENCE();       // end of CertificationRequestInfo


				byte[] CertificationRequestInfo = this.derOutput.ToArray();

				this.derOutput.Clear();
				this.derOutput.StartSEQUENCE();     // CertificationRequest
				this.derOutput.Raw(CertificationRequestInfo);

				this.derOutput.StartSEQUENCE();     // signatureAlgorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.5");   // algorithm (SHA-1 with RSA Encryption)
				this.derOutput.NULL();              // parameters
				this.derOutput.EndSEQUENCE();       // End of signatureAlgorithm

				this.derOutput.BITSTRING(RSA.SignData(CertificationRequestInfo, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));

				this.derOutput.EndSEQUENCE();       // end of CertificationRequest

				byte[] CSR = this.derOutput.ToArray();

				this.PrintCSR(CSR);
			}
		}

		[TestMethod]
		public void DER_Test_23_CSR_5_Generate_Simple_SHA256()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048))
			{
				RSAParameters Parameters = RSA.ExportParameters(false);

				// Example taken from: https://www.sslshopper.com/what-is-a-csr-certificate-signing-request.html

				this.derOutput.StartSEQUENCE();     // CertificationRequestInfo 
				this.derOutput.INTEGER(0);          // Version

				this.derOutput.StartSEQUENCE();     // subject
				this.derOutput.StartSET();
				this.derOutput.StartSEQUENCE();
				this.derOutput.OBJECT_IDENTIFIER("2.5.4.3");    // Common Name
				this.derOutput.PRINTABLE_STRING("www.example.com");
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndSET();
				this.derOutput.EndSEQUENCE();       // end of subject

				this.derOutput.StartSEQUENCE();     // subjectPKInfo
				this.derOutput.StartSEQUENCE();     // algorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.1");   // RSA Encryption
				this.derOutput.NULL();  // No parameters
				this.derOutput.EndSEQUENCE();       // end of algorithm
				this.derOutput.StartBITSTRING();    // subjectPublicKey
				this.derOutput.StartSEQUENCE();
				this.derOutput.INTEGER(Parameters.Modulus, false);
				this.derOutput.INTEGER(Parameters.Exponent, false);
				this.derOutput.EndSEQUENCE();
				this.derOutput.EndBITSTRING();      // end of subjectPublicKey
				this.derOutput.EndSEQUENCE();       // end of subjectPKInfo

				this.derOutput.EndOfContent(Asn1TypeClass.ContextSpecific);     // attributes
				this.derOutput.EndSEQUENCE();       // end of CertificationRequestInfo


				byte[] CertificationRequestInfo = this.derOutput.ToArray();

				this.derOutput.Clear();
				this.derOutput.StartSEQUENCE();     // CertificationRequest
				this.derOutput.Raw(CertificationRequestInfo);

				this.derOutput.StartSEQUENCE();     // signatureAlgorithm
				this.derOutput.OBJECT_IDENTIFIER("1.2.840.113549.1.1.11");   // algorithm (SHA-256 with RSA Encryption)
				this.derOutput.NULL();              // parameters
				this.derOutput.EndSEQUENCE();       // End of signatureAlgorithm

				this.derOutput.BITSTRING(RSA.SignData(CertificationRequestInfo, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));

				this.derOutput.EndSEQUENCE();       // end of CertificationRequest

				byte[] CSR = this.derOutput.ToArray();

				this.PrintCSR(CSR);
			}
		}

		[TestMethod]
		public void DER_Test_24_CSR_6_Generic_SHA256()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048))
			{
				CertificateRequest CertificateRequest = new CertificateRequest(new RsaSha256(RSA))
				{
					Country = "SE",
					StateOrProvince = "Stockholm",
					Locality = "Locality",
					Organization = "Example Ltd",
					OrganizationalUnit = "Development",
					CommonName = "www.example.com",
					SubjectAlternativeNames = new string[] { "example.com" },
					Surname = "Smith",
					Description = "Domain certificate",
					Name = "Mr Smith",
					GivenName = "Mr"
				};

				byte[] CSR = CertificateRequest.BuildCSR();

				this.PrintCSR(CSR);
			}
		}

		[TestMethod]
		public void DER_Test_24_CSR_7_Generic_SHA384()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
			{
				CertificateRequest CertificateRequest = new CertificateRequest(new RsaSha384(RSA))
				{
					Country = "SE",
					StateOrProvince = "Stockholm",
					Locality = "Locality",
					Organization = "Example Ltd",
					OrganizationalUnit = "Development",
					CommonName = "www.example.com",
					SubjectAlternativeNames = new string[] { "example.com" },
					Surname = "Smith",
					Description = "Domain certificate",
					Name = "Mr Smith",
					GivenName = "Mr"
				};

				byte[] CSR = CertificateRequest.BuildCSR();

				this.PrintCSR(CSR);
			}
		}

		[TestMethod]
		public void DER_Test_24_CSR_8_Generic_SHA512()
		{
			using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))
			{
				CertificateRequest CertificateRequest = new CertificateRequest(new RsaSha512(RSA))
				{
					Country = "SE",
					StateOrProvince = "Stockholm",
					Locality = "Locality",
					Organization = "Example Ltd",
					OrganizationalUnit = "Development",
					CommonName = "www.example.com",
					SubjectAlternativeNames = new string[] { "example.com" },
					Surname = "Smith",
					Description = "Domain certificate",
					Name = "Mr Smith",
					GivenName = "Mr"
				};

				byte[] CSR = CertificateRequest.BuildCSR();

				this.PrintCSR(CSR);
			}
		}

		private void PrintCSR(byte[] CSR)
		{
			Console.Out.WriteLine("-----BEGIN CERTIFICATE REQUEST-----");
			Console.Out.WriteLine(Convert.ToBase64String(CSR, Base64FormattingOptions.InsertLineBreaks));
			Console.Out.WriteLine("-----END CERTIFICATE REQUEST-----");
		}

	}
}
