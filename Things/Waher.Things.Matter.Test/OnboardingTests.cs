using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Things.Matter.Onboarding;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Tests the onboarding payload codecs (QR codes and manual pairing codes).
	/// </summary>
	[TestClass]
	public sealed class OnboardingTests
	{
		[TestMethod]
		public void Test_01_Base38()
		{
			byte[] Decoded = SetupPayload.Base38Decode("KKHF3W2S013OPM3EJX11");
			Assert.AreEqual("Hello World!", Encoding.ASCII.GetString(Decoded));
			Assert.AreEqual("KKHF3W2S013OPM3EJX11", SetupPayload.Base38Encode(Decoded));
			Assert.AreEqual("A0", SetupPayload.Base38Encode(new byte[] { 10 }));
		}

		[TestMethod]
		public void Test_02_QrCode_TestDevice()
		{
			// QR code printed by the matter.js reference device with the public test credentials.
			SetupPayload P = SetupPayload.Parse("MT:Y.K90AFN00KA0648G00");

			Assert.AreEqual(0, P.Version);
			Assert.AreEqual((ushort)0xfff1, P.VendorId);
			Assert.AreEqual((ushort)0x8000, P.ProductId);
			Assert.AreEqual(CommissioningFlow.Standard, P.Flow);
			Assert.AreEqual(DiscoveryCapabilities.OnNetwork, P.Capabilities);
			Assert.AreEqual((ushort)3840, P.Discriminator);
			Assert.AreEqual(20202021U, P.Passcode);
			Assert.IsFalse(P.IsShortDiscriminator);

			Assert.AreEqual("MT:Y.K90AFN00KA0648G00", P.ToQrCode());
			Assert.AreEqual("34970112332", P.ToManualCode());
		}

		[TestMethod]
		public void Test_03_QrCode_SdkDefault()
		{
			SetupPayload P = SetupPayload.Parse("MT:M5L90MP500K64J00000");

			Assert.AreEqual((ushort)12, P.VendorId);
			Assert.AreEqual((ushort)1, P.ProductId);
			Assert.AreEqual(DiscoveryCapabilities.SoftAP, P.Capabilities);
			Assert.AreEqual((ushort)128, P.Discriminator);
			Assert.AreEqual(2048U, P.Passcode);
			Assert.AreEqual("MT:M5L90MP500K64J00000", P.ToQrCode());
		}

		[TestMethod]
		public void Test_04_ManualCode()
		{
			SetupPayload P = SetupPayload.Parse("3497-011-2332");

			Assert.AreEqual(20202021U, P.Passcode);
			Assert.IsTrue(P.IsShortDiscriminator);
			Assert.AreEqual((byte)15, P.ShortDiscriminator);
			Assert.AreEqual("34970112332", P.ToManualCode());
			Assert.AreEqual("3497-011-2332", SetupPayload.FormatManualCode(P.ToManualCode()));
		}

		[TestMethod]
		public void Test_05_ManualCode_VidPid()
		{
			SetupPayload P = new()
			{
				VendorId = 0xfff1,
				ProductId = 0x8001,
				Discriminator = 3840,
				Passcode = 20202021,
				Flow = CommissioningFlow.Custom
			};

			string Code = P.ToManualCode();
			Assert.AreEqual(21, Code.Length);

			SetupPayload P2 = SetupPayload.Parse(Code);
			Assert.AreEqual(P.Passcode, P2.Passcode);
			Assert.AreEqual(P.VendorId, P2.VendorId);
			Assert.AreEqual(P.ProductId, P2.ProductId);
			Assert.AreEqual(P.ShortDiscriminator, P2.ShortDiscriminator);
		}

		[TestMethod]
		public void Test_06_InvalidCodes()
		{
			Assert.ThrowsExactly<MatterDecodingException>(() => SetupPayload.Parse("02684354589"));    // Wrong check digit
			Assert.ThrowsExactly<MatterDecodingException>(() => SetupPayload.Parse("2456"));           // Too short
			Assert.ThrowsExactly<MatterDecodingException>(() => SetupPayload.Parse("MT:%%%"));
			Assert.IsFalse(SetupPayload.IsValidPasscode(12345678));
			Assert.IsFalse(SetupPayload.IsValidPasscode(0));
			Assert.IsTrue(SetupPayload.IsValidPasscode(20202021));
		}

		[TestMethod]
		public void Test_07_Verhoeff()
		{
			Assert.AreEqual('3', Verhoeff.ComputeCheckDigit("236"));
			Assert.IsTrue(Verhoeff.Validate("2363"));
			Assert.IsTrue(Verhoeff.Validate("34970112332"));
		}
	}
}
