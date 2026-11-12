using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Runtime.IO;

namespace Waher.Content.Asn1.Test
{
	[TestClass]
	public class ReadingTests
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
		[DataRow("X_both_speed_alternatives_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_both_speed_alternatives_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_duplicate_fuel_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_duplicate_fuel_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_duplicate_message_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_duplicate_message_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_duplicate_name_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_duplicate_name_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_duplicate_speed_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_duplicate_speed_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_empty_integer_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_empty_integer_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_empty_rocket_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_empty_rocket_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_empty_speed_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_empty_speed_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_explicit_mph_integer_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_explicit_mph_integer_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_missing_fuel_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_missing_fuel_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_missing_name_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_missing_name_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_missing_payload_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_missing_payload_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_nonminimal_tag_number_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_nonminimal_tag_number_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_payload_context_item_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_payload_context_item_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_payload_integer_item_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_payload_integer_item_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_primitive_indefinite_name_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_primitive_indefinite_name_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_primitive_outer_sequence_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_primitive_outer_sequence_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_primitive_payload_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_primitive_payload_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_primitive_speed_wrapper_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_primitive_speed_wrapper_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_redundant_negative_sign_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_redundant_negative_sign_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_redundant_positive_sign_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_redundant_positive_sign_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_set_instead_sequence_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_set_instead_sequence_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_speed_extra_null_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_speed_extra_null_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_speed_without_wrapper_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_speed_without_wrapper_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_standalone_eoc_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_standalone_eoc_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_trailing_top_level_value_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_trailing_top_level_value_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_truncated_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_truncated_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_universal_fuel_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_universal_fuel_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_universal_message_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_universal_message_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_universal_name_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_universal_name_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_universal_payload_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_universal_payload_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_universal_speed_integer_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_universal_speed_integer_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_unknown_speed_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_unknown_speed_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_unknown_trailing_field_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_unknown_trailing_field_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_utf8_overlong_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_utf8_overlong_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_utf8_surrogate_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_utf8_surrogate_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_utf8_truncated_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_utf8_truncated_DER.bin", AsnEncodingRules.DER)]
		[DataRow("X_wrong_field_order_CER.bin", AsnEncodingRules.CER)]
		[DataRow("X_wrong_field_order_DER.bin", AsnEncodingRules.DER)]
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
		[DataRow("N01_empty_name_CER.bin", AsnEncodingRules.CER)]
		[DataRow("N01_empty_name_DER.bin", AsnEncodingRules.DER)]
		[DataRow("N02_name17_ascii_CER.bin", AsnEncodingRules.CER)]
		[DataRow("N02_name17_ascii_DER.bin", AsnEncodingRules.DER)]
		[DataRow("N03_name17_astral_CER.bin", AsnEncodingRules.CER)]
		[DataRow("N03_name17_astral_DER.bin", AsnEncodingRules.DER)]
		[DataRow("N04_fuel3_CER.bin", AsnEncodingRules.CER)]
		[DataRow("N04_fuel3_DER.bin", AsnEncodingRules.DER)]
		[DataRow("N05_fuel_negative_CER.bin", AsnEncodingRules.CER)]
		[DataRow("N05_fuel_negative_DER.bin", AsnEncodingRules.DER)]
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
			await Read("World-Schema.asn1", "Test_01_WorldSchema", "World_Schema",
				"Rocket", "Binaries\\World-Schema\\" + Binary, Rules);
		}

		private static async Task Read(string Definition, string Namespace, string ModuleName,
			string TypeName, string Binary, AsnEncodingRules Rules)
		{ 
			if (!assemblies.TryGetValue(Namespace, out Assembly A))
			{
				CSharpTests.CSharpResults Compiled = await CSharpTests.GenerateCSharp(
					Definition, Namespace);

				A = Assembly.Load(Compiled.Dll, Compiled.Pdb);
				assemblies[Namespace] = A;
			}

			Type T = A.GetType(Namespace + "." + ModuleName + "." + TypeName);
			Assert.IsNotNull(T);

			MethodInfo Create = T.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
			Assert.IsNotNull(Create);

			byte[] Bin = await Files.ReadAllBytesAsync(Binary);
			AsnReader Reader = new(Bin, Rules);
			object Parsed = Create.Invoke(null, [Reader]);
			Assert.IsNotNull(Parsed);
		}

	}
}
