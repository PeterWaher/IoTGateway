using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Reflection;
using System.Threading.Tasks;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class WritingTests
	{
		private static readonly Dictionary<string, Assembly> assemblies = [];

		[TestMethod]
		[DataRow("V06_unicode_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V07_name16_astral_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V07_name16_astral_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V07_name16_astral_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V08_payload_order_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V08_payload_order_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V08_payload_order_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V09_minus1_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V09_minus1_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V09_minus1_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V10_minus128_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V10_minus128_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V10_minus128_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V11_minus129_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V11_minus129_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V11_minus129_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V12_plus127_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V12_plus127_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V12_plus127_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V13_plus255_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V13_plus255_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V13_plus255_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V14_plus256_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V14_plus256_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V14_plus256_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V15_big_positive_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V15_big_positive_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V15_big_positive_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V16_big_negative_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V16_big_negative_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V16_big_negative_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V17_message1000_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V17_message1000_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V17_message1000_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V18_message1001_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V18_message1001_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V18_message1001_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V19_utf8_fragment_split_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V19_utf8_fragment_split_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V19_utf8_fragment_split_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V20_payload_long_unicode_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V20_payload_long_unicode_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V20_payload_long_unicode_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V21_payload_many_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V21_payload_many_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V21_payload_many_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V22_controls_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V22_controls_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V22_controls_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V23_one_astral_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V23_one_astral_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V23_one_astral_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V24_payload1000_1001_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V24_payload1000_1001_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V24_payload1000_1001_DER.bin", AsnEncodingRules.DER)]
		[DataRow("B01_explicit_default.bin", AsnEncodingRules.BER)]
		[DataRow("B02_outer_indefinite_mixed.bin", AsnEncodingRules.BER)]
		[DataRow("B03_speed_indefinite.bin", AsnEncodingRules.BER)]
		[DataRow("B04_payload_indefinite.bin", AsnEncodingRules.BER)]
		[DataRow("B05_constructed_name.bin", AsnEncodingRules.BER)]
		[DataRow("B06_nested_constructed_message.bin", AsnEncodingRules.BER)]
		[DataRow("B07_constructed_payload_string.bin", AsnEncodingRules.BER)]
		[DataRow("B08_nonminimal_outer_length.bin", AsnEncodingRules.BER)]
		[DataRow("B09_explicit_default_cer_framing.bin", AsnEncodingRules.BER)]
		[DataRow("C01_primitive_1001.bin", AsnEncodingRules.BER)]
		[DataRow("C02_constructed_short.bin", AsnEncodingRules.BER)]
		[DataRow("C03_fragment999.bin", AsnEncodingRules.BER)]
		[DataRow("C04_definite_payload.bin", AsnEncodingRules.BER)]
		[DataRow("C05_definite_choice_wrapper.bin", AsnEncodingRules.BER)]
		[DataRow("V01_minimum_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V01_minimum_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V01_minimum_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V02_mph100_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V02_mph100_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V02_mph100_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V03_kmph128_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V03_kmph128_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V03_kmph128_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V04_message_empty_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V04_message_empty_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V04_message_empty_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V05_name16_ascii_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V05_name16_ascii_CER.bin", AsnEncodingRules.CER)]
		[DataRow("V05_name16_ascii_DER.bin", AsnEncodingRules.DER)]
		[DataRow("V06_unicode_BER.bin", AsnEncodingRules.BER)]
		[DataRow("V06_unicode_CER.bin", AsnEncodingRules.CER)]
		public async Task Test_01_WorldSchema(string Binary, AsnEncodingRules Rules)
		{
			Construct Parsed = await ReadingTests.Read("World-Schema.asn1",
				"Test_01_WorldSchema", "World_Schema", "Rocket",
				"Binaries\\World-Schema\\" + Binary, Rules);

			byte[] Encoded = Write(Parsed, Rules);
			Assert.IsNotNull(Encoded);
		}

		internal static byte[] Write(Construct Parsed, AsnEncodingRules Rules)
		{
			AsnWriter Writer = new(Rules);
			Parsed.Write(Writer);

			byte[] Encoded = Writer.Encode();
			Assert.IsNotNull(Encoded);

			Console.Out.WriteLine(Convert.ToBase64String(Encoded,
				Base64FormattingOptions.InsertLineBreaks));

			object Parsed2 = ReadingTests.Create(Parsed.GetType(), Encoded, Rules);

			Assert.AreEqual(Parsed, Parsed2);

			return Encoded;
		}

	}
}
