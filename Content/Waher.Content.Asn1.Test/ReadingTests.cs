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
		[DataRow("V06_unicode_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V07_name16_astral_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V07_name16_astral_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V07_name16_astral_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V08_payload_order_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V08_payload_order_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V08_payload_order_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V09_minus1_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V09_minus1_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V09_minus1_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V10_minus128_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V10_minus128_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V10_minus128_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V11_minus129_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V11_minus129_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V11_minus129_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V12_plus127_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V12_plus127_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V12_plus127_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V13_plus255_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V13_plus255_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V13_plus255_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V14_plus256_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V14_plus256_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V14_plus256_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V15_big_positive_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V15_big_positive_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V15_big_positive_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V16_big_negative_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V16_big_negative_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V16_big_negative_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V17_message1000_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V17_message1000_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V17_message1000_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V18_message1001_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V18_message1001_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V18_message1001_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V19_utf8_fragment_split_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V19_utf8_fragment_split_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V19_utf8_fragment_split_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V20_payload_long_unicode_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V20_payload_long_unicode_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V20_payload_long_unicode_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V21_payload_many_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V21_payload_many_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V21_payload_many_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V22_controls_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V22_controls_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V22_controls_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V23_one_astral_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V23_one_astral_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V23_one_astral_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V24_payload1000_1001_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V24_payload1000_1001_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V24_payload1000_1001_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("X_both_speed_alternatives_CER.bin", AsnEncodingRules.CER, "CHOICE wrapper contains both alternatives; reject unread inner content.")]
		[DataRow("X_both_speed_alternatives_DER.bin", AsnEncodingRules.DER, "CHOICE wrapper contains both alternatives; reject unread inner content.")]
		[DataRow("X_duplicate_fuel_CER.bin", AsnEncodingRules.CER, "Duplicate fuel field.")]
		[DataRow("X_duplicate_fuel_DER.bin", AsnEncodingRules.DER, "Duplicate fuel field.")]
		[DataRow("X_duplicate_message_CER.bin", AsnEncodingRules.CER, "Duplicate DEFAULT message field.")]
		[DataRow("X_duplicate_message_DER.bin", AsnEncodingRules.DER, "Duplicate DEFAULT message field.")]
		[DataRow("X_duplicate_name_CER.bin", AsnEncodingRules.CER, "Duplicate name field.")]
		[DataRow("X_duplicate_name_DER.bin", AsnEncodingRules.DER, "Duplicate name field.")]
		[DataRow("X_duplicate_speed_CER.bin", AsnEncodingRules.CER, "OPTIONAL speed appears twice.")]
		[DataRow("X_duplicate_speed_DER.bin", AsnEncodingRules.DER, "OPTIONAL speed appears twice.")]
		[DataRow("X_empty_integer_CER.bin", AsnEncodingRules.CER, "INTEGER contents must contain at least one octet.")]
		[DataRow("X_empty_integer_DER.bin", AsnEncodingRules.DER, "INTEGER contents must contain at least one octet.")]
		[DataRow("X_empty_rocket_CER.bin", AsnEncodingRules.CER, "All required fields are absent.")]
		[DataRow("X_empty_rocket_DER.bin", AsnEncodingRules.DER, "All required fields are absent.")]
		[DataRow("X_empty_speed_CER.bin", AsnEncodingRules.CER, "Present CHOICE wrapper contains no alternative.")]
		[DataRow("X_empty_speed_DER.bin", AsnEncodingRules.DER, "Present CHOICE wrapper contains no alternative.")]
		[DataRow("X_explicit_mph_integer_CER.bin", AsnEncodingRules.CER, "mph is IMPLICIT INTEGER; an extra [0] EXPLICIT layer is wrong.")]
		[DataRow("X_explicit_mph_integer_DER.bin", AsnEncodingRules.DER, "mph is IMPLICIT INTEGER; an extra [0] EXPLICIT layer is wrong.")]
		[DataRow("X_missing_fuel_CER.bin", AsnEncodingRules.CER, "Required fuel is missing.")]
		[DataRow("X_missing_fuel_DER.bin", AsnEncodingRules.DER, "Required fuel is missing.")]
		[DataRow("X_missing_name_CER.bin", AsnEncodingRules.CER, "Required name is missing.")]
		[DataRow("X_missing_name_DER.bin", AsnEncodingRules.DER, "Required name is missing.")]
		[DataRow("X_missing_payload_CER.bin", AsnEncodingRules.CER, "Required payload is missing; empty and absent differ.")]
		[DataRow("X_missing_payload_DER.bin", AsnEncodingRules.DER, "Required payload is missing; empty and absent differ.")]
		[DataRow("X_nonminimal_tag_number_CER.bin", AsnEncodingRules.CER, "High-tag-number form used for tag number zero.")]
		[DataRow("X_nonminimal_tag_number_DER.bin", AsnEncodingRules.DER, "High-tag-number form used for tag number zero.")]
		[DataRow("X_payload_context_item_CER.bin", AsnEncodingRules.CER, "Payload entries are universal UTF8Strings; no positional automatic tags.")]
		[DataRow("X_payload_context_item_DER.bin", AsnEncodingRules.DER, "Payload entries are universal UTF8Strings; no positional automatic tags.")]
		[DataRow("X_payload_integer_item_CER.bin", AsnEncodingRules.CER, "Payload contains INTEGER instead of UTF8String.")]
		[DataRow("X_payload_integer_item_DER.bin", AsnEncodingRules.DER, "Payload contains INTEGER instead of UTF8String.")]
		[DataRow("X_primitive_indefinite_name_CER.bin", AsnEncodingRules.CER, "Indefinite length is forbidden for primitive UTF8String.")]
		[DataRow("X_primitive_indefinite_name_DER.bin", AsnEncodingRules.DER, "Indefinite length is forbidden for primitive UTF8String.")]
		[DataRow("X_primitive_outer_sequence_CER.bin", AsnEncodingRules.CER, "Outer SEQUENCE must be constructed.")]
		[DataRow("X_primitive_outer_sequence_DER.bin", AsnEncodingRules.DER, "Outer SEQUENCE must be constructed.")]
		[DataRow("X_primitive_payload_CER.bin", AsnEncodingRules.CER, "SEQUENCE OF must have the constructed bit set.")]
		[DataRow("X_primitive_payload_DER.bin", AsnEncodingRules.DER, "SEQUENCE OF must have the constructed bit set.")]
		[DataRow("X_primitive_speed_wrapper_CER.bin", AsnEncodingRules.CER, "An explicit wrapper must be constructed.")]
		[DataRow("X_primitive_speed_wrapper_DER.bin", AsnEncodingRules.DER, "An explicit wrapper must be constructed.")]
		[DataRow("X_redundant_negative_sign_CER.bin", AsnEncodingRules.CER, "Redundant leading FF is forbidden for INTEGER even under BER.")]
		[DataRow("X_redundant_negative_sign_DER.bin", AsnEncodingRules.DER, "Redundant leading FF is forbidden for INTEGER even under BER.")]
		[DataRow("X_redundant_positive_sign_CER.bin", AsnEncodingRules.CER, "Redundant leading zero is forbidden for INTEGER even under BER.")]
		[DataRow("X_redundant_positive_sign_DER.bin", AsnEncodingRules.DER, "Redundant leading zero is forbidden for INTEGER even under BER.")]
		[DataRow("X_set_instead_sequence_CER.bin", AsnEncodingRules.CER, "Outer tag is SET rather than SEQUENCE.")]
		[DataRow("X_set_instead_sequence_DER.bin", AsnEncodingRules.DER, "Outer tag is SET rather than SEQUENCE.")]
		[DataRow("X_speed_extra_null_CER.bin", AsnEncodingRules.CER, "Extra value follows the selected speed inside the explicit wrapper.")]
		[DataRow("X_speed_extra_null_DER.bin", AsnEncodingRules.DER, "Extra value follows the selected speed inside the explicit wrapper.")]
		[DataRow("X_speed_without_wrapper_CER.bin", AsnEncodingRules.CER, "Selected speed is missing its [3] EXPLICIT wrapper.")]
		[DataRow("X_speed_without_wrapper_DER.bin", AsnEncodingRules.DER, "Selected speed is missing its [3] EXPLICIT wrapper.")]
		[DataRow("X_standalone_eoc_CER.bin", AsnEncodingRules.CER, "End-of-contents appears outside the completed Rocket.")]
		[DataRow("X_standalone_eoc_DER.bin", AsnEncodingRules.DER, "End-of-contents appears outside the completed Rocket.")]
		[DataRow("X_trailing_top_level_value_CER.bin", AsnEncodingRules.CER, "A second top-level value follows Rocket; enforce complete-message consumption.")]
		[DataRow("X_trailing_top_level_value_DER.bin", AsnEncodingRules.DER, "A second top-level value follows Rocket; enforce complete-message consumption.")]
		[DataRow("X_truncated_CER.bin", AsnEncodingRules.CER, "Last byte of the complete encoding is missing.")]
		[DataRow("X_truncated_DER.bin", AsnEncodingRules.DER, "Last byte of the complete encoding is missing.")]
		[DataRow("X_universal_fuel_CER.bin", AsnEncodingRules.CER, "Fuel must use Context-specific [2], not Universal ENUMERATED.")]
		[DataRow("X_universal_fuel_DER.bin", AsnEncodingRules.DER, "Fuel must use Context-specific [2], not Universal ENUMERATED.")]
		[DataRow("X_universal_message_CER.bin", AsnEncodingRules.CER, "Present message must use Context-specific [1].")]
		[DataRow("X_universal_message_DER.bin", AsnEncodingRules.DER, "Present message must use Context-specific [1].")]
		[DataRow("X_universal_name_CER.bin", AsnEncodingRules.CER, "Name must use its implicit Context-specific [0] tag.")]
		[DataRow("X_universal_name_DER.bin", AsnEncodingRules.DER, "Name must use its implicit Context-specific [0] tag.")]
		[DataRow("X_universal_payload_CER.bin", AsnEncodingRules.CER, "Payload's implicit [4] replaces the Universal SEQUENCE identifier.")]
		[DataRow("X_universal_payload_DER.bin", AsnEncodingRules.DER, "Payload's implicit [4] replaces the Universal SEQUENCE identifier.")]
		[DataRow("X_universal_speed_integer_CER.bin", AsnEncodingRules.CER, "mph/kmph require implicit [0]/[1], not Universal INTEGER.")]
		[DataRow("X_universal_speed_integer_DER.bin", AsnEncodingRules.DER, "mph/kmph require implicit [0]/[1], not Universal INTEGER.")]
		[DataRow("X_unknown_speed_CER.bin", AsnEncodingRules.CER, "CHOICE alternative [2] is undefined.")]
		[DataRow("X_unknown_speed_DER.bin", AsnEncodingRules.DER, "CHOICE alternative [2] is undefined.")]
		[DataRow("X_unknown_trailing_field_CER.bin", AsnEncodingRules.CER, "Rocket has no extension marker; unknown extra field is forbidden.")]
		[DataRow("X_unknown_trailing_field_DER.bin", AsnEncodingRules.DER, "Rocket has no extension marker; unknown extra field is forbidden.")]
		[DataRow("X_utf8_overlong_CER.bin", AsnEncodingRules.CER, "Overlong UTF-8 sequence.")]
		[DataRow("X_utf8_overlong_DER.bin", AsnEncodingRules.DER, "Overlong UTF-8 sequence.")]
		[DataRow("X_utf8_surrogate_CER.bin", AsnEncodingRules.CER, "UTF-8 encodes a surrogate, which is not a Unicode scalar value.")]
		[DataRow("X_utf8_surrogate_DER.bin", AsnEncodingRules.DER, "UTF-8 encodes a surrogate, which is not a Unicode scalar value.")]
		[DataRow("X_utf8_truncated_CER.bin", AsnEncodingRules.CER, "Incomplete three-octet UTF-8 sequence.")]
		[DataRow("X_utf8_truncated_DER.bin", AsnEncodingRules.DER, "Incomplete three-octet UTF-8 sequence.")]
		[DataRow("X_wrong_field_order_CER.bin", AsnEncodingRules.CER, "SEQUENCE fields are out of declaration order.")]
		[DataRow("X_wrong_field_order_DER.bin", AsnEncodingRules.DER, "SEQUENCE fields are out of declaration order.")]
		[DataRow("B01_explicit_default.bin", AsnEncodingRules.BER, null)]
		[DataRow("B02_outer_indefinite_mixed.bin", AsnEncodingRules.BER, null)]
		[DataRow("B03_speed_indefinite.bin", AsnEncodingRules.BER, null)]
		[DataRow("B04_payload_indefinite.bin", AsnEncodingRules.BER, null)]
		[DataRow("B05_constructed_name.bin", AsnEncodingRules.BER, null)]
		[DataRow("B06_nested_constructed_message.bin", AsnEncodingRules.BER, null)]
		[DataRow("B07_constructed_payload_string.bin", AsnEncodingRules.BER, null)]
		[DataRow("B08_nonminimal_outer_length.bin", AsnEncodingRules.BER, null)]
		[DataRow("B09_explicit_default_cer_framing.bin", AsnEncodingRules.BER, null)]
		[DataRow("C01_primitive_1001.bin", AsnEncodingRules.BER, null)]
		[DataRow("C02_constructed_short.bin", AsnEncodingRules.BER, null)]
		[DataRow("C03_fragment999.bin", AsnEncodingRules.BER, null)]
		[DataRow("C04_definite_payload.bin", AsnEncodingRules.BER, null)]
		[DataRow("C05_definite_choice_wrapper.bin", AsnEncodingRules.BER, null)]
		[DataRow("N01_empty_name_CER.bin", AsnEncodingRules.CER, "Name violates SIZE(1..16): zero characters.")]
		[DataRow("N01_empty_name_DER.bin", AsnEncodingRules.DER, "Name violates SIZE(1..16): zero characters.")]
		[DataRow("N02_name17_ascii_CER.bin", AsnEncodingRules.CER, "Name violates SIZE(1..16): 17 characters.")]
		[DataRow("N02_name17_ascii_DER.bin", AsnEncodingRules.DER, "Name violates SIZE(1..16): 17 characters.")]
		[DataRow("N03_name17_astral_CER.bin", AsnEncodingRules.CER, "Name violates SIZE(1..16): 17 Unicode scalar values.")]
		[DataRow("N03_name17_astral_DER.bin", AsnEncodingRules.DER, "Name violates SIZE(1..16): 17 Unicode scalar values.")]
		[DataRow("N04_fuel3_CER.bin", AsnEncodingRules.CER, "ENUMERATED has no extension marker: undefined value 3 is forbidden.")]
		[DataRow("N04_fuel3_DER.bin", AsnEncodingRules.DER, "ENUMERATED has no extension marker: undefined value 3 is forbidden.")]
		[DataRow("N05_fuel_negative_CER.bin", AsnEncodingRules.CER, "ENUMERATED value -1 is outside the defined alternatives.")]
		[DataRow("N05_fuel_negative_DER.bin", AsnEncodingRules.DER, "ENUMERATED value -1 is outside the defined alternatives.")]
		[DataRow("V01_minimum_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V01_minimum_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V01_minimum_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V02_mph100_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V02_mph100_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V02_mph100_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V03_kmph128_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V03_kmph128_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V03_kmph128_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V04_message_empty_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V04_message_empty_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V04_message_empty_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V05_name16_ascii_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V05_name16_ascii_CER.bin", AsnEncodingRules.CER, null)]
		[DataRow("V05_name16_ascii_DER.bin", AsnEncodingRules.DER, null)]
		[DataRow("V06_unicode_BER.bin", AsnEncodingRules.BER, null)]
		[DataRow("V06_unicode_CER.bin", AsnEncodingRules.CER, null)]
		public async Task Test_01_WorldSchema(string Binary, AsnEncodingRules Rules, 
			string ExpectedError)
		{
			if (string.IsNullOrEmpty(ExpectedError))
			{
				await Read("World-Schema.asn1", "Test_01_WorldSchema", "World_Schema",
					"Rocket", "Binaries\\World-Schema\\" + Binary, Rules);
			}
			else
			{
				Assert.Throws<Exception>(async () =>
				{
					await Read("World-Schema.asn1", "Test_01_WorldSchema", "World_Schema",
						"Rocket", "Binaries\\World-Schema\\" + Binary, Rules);
				}, ExpectedError);
			}
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
