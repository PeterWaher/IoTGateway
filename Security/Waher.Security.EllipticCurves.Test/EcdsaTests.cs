using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Waher.Security.EllipticCurves.Test
{
    [TestClass]
    public class EcdsaTests
    {
        [TestMethod]
        [DataRow(
			"MIICoAIBATAKBggqhkjOPQQDBDBQMSYwJAYDVQQDDB1Td2VkaXNoIENvdW50cnkgU2lnbmluZyBDQSB2MjEZMBcGA1UECgwQUG9saXNteW5kaWdoZXRlbjELMAkGA1UEBhMCU0UXDTI2MDIxNjEzMTgxM1oXDTI2MDUxNzEzMTgxMlowggHsMCcCCANsoFZ4eGNRFw0yNTEwMjIxMDQxMDNaMAwwCgYDVR0VBAMKAQQwJwIIe3FJL20GIi8XDTI1MTAyMjEwMzk0NFowDDAKBgNVHRUEAwoBBDAnAghjoRKmkz7/ORcNMjUxMDIyMTAzOTA1WjAMMAoGA1UdFQQDCgEEMCcCCFJZ7xk+L9m4Fw0yNTEwMjIxMDQzMDFaMAwwCgYDVR0VBAMKAQQwJwIIGuvd3ERQxeEXDTI1MTAyMjEwMzU0MFowDDAKBgNVHRUEAwoBBDAnAgg90thsXKfyABcNMjUxMDIyMTAzNjE2WjAMMAoGA1UdFQQDCgEEMCcCCACg1xGMbnvvFw0yNTEwMjIxMDQyMTVaMAwwCgYDVR0VBAMKAQQwJwIIS3nUNqjVq88XDTI1MTAyMjEwMzQ0NFowDDAKBgNVHRUEAwoBBDAnAgg2rgs8NQZ1OhcNMjUxMDIyMTA0MDE2WjAMMAoGA1UdFQQDCgEEMCcCCGFcgZP3fHBwFw0yNTEwMjIxMDM3MzRaMAwwCgYDVR0VBAMKAQQwJwIIZH37pGQgapgXDTI1MTAyMjEwMzgxMlowDDAKBgNVHRUEAwoBBDAnAggDaLbL5ocnIBcNMjUxMDIyMTA0MTQyWjAMMAoGA1UdFQQDCgEEoC8wLTAfBgNVHSMEGDAWgBTvvBJIvlyugPlkyLIEydAzu9mWUzAKBgNVHRQEAwIBUg==",
			"MA8PWqs7iYmS+V8u2ntn+zjTTWPqFbCqkPBvZxcYME5blWidZdTLX8kMb7SDhlCBXXubCFZK6SxQaOVjRvW5fRwJajAmamCNWoB8GZn/DN29EFYh2WmYGDWbJc87Qgc8GYdFRT7bTkkWWX2g3unL+5QF/rP5a8QO8RgxoduLxTo=",
			"jrUtRO8sr897v4K1OpxqlKg0q9WX1aFvxKPkqr23vnqwOeLIUY9cXLbZEah2st64s1YtY/etGORwVM34GQEt1zoXqqW0EHFZjOwIoVLmTmu8zYKJinashTalybP/iVFYPIlDHItrNYvIhfJIAyUwbHMhQt3j6tTJYeuEc9sRvDU=",
			true)]
        public void Test_01_Verify_BrainPoolP512(string Data, string PublicKey, string Signature, 
            bool BigEndian)
        {
            BrainpoolP512 Curve = new();
			byte[] DataBin = Convert.FromBase64String(Data);
			byte[] PublicKeyBin = Convert.FromBase64String(PublicKey);
			byte[] SignatureBin = Convert.FromBase64String(Signature);

			Assert.IsTrue(ECDSA.Verify(DataBin, PublicKeyBin, BigEndian,
				Hashes.ComputeSHA512Hash, Curve, SignatureBin));
		}
	}
}
