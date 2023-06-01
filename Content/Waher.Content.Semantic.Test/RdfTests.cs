using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Waher.Content.Semantic.Test
{
	[TestClass]
	public class RdfTests : SemanticTests
	{
		[DataTestMethod]
		[DataRow("Manifest.rdf")]
		[DataRow("example01.rdf")]
		[DataRow("example02.rdf")]
		[DataRow("example03.rdf")]
		[DataRow("example04.rdf")]
		[DataRow("example05.rdf")]
		[DataRow("example06.rdf")]
		[DataRow("horst_01.test001.rdf")]
		[DataRow("horst_01.test002.rdf")]
		[DataRow("horst_01.test003.rdf")]
		[DataRow("horst_01.test004.rdf")]
		[DataRow("rdfs_container_membership_superProperty.not1C.rdf", null)]
		[DataRow("rdfs_container_membership_superProperty.not1P.rdf", null)]
		[DataRow("rdfs_domain_and_range.nonconclusions005.rdf", null)]
		[DataRow("rdfs_domain_and_range.nonconclusions006.rdf", null)]
		[DataRow("rdfs_domain_and_range.premises005.rdf", null)]
		[DataRow("rdfs_domain_and_range.premises006.rdf", null)]
		[DataRow("tex_01.test001.rdf", null)]
		[DataRow("tex_01.test002.rdf", null)]
		[DataRow("xmlsch_02.test001.rdf", null)]
		[DataRow("xmlsch_02.test002.rdf", null)]
		[DataRow("xmlsch_02.test003.rdf", null)]
		public async Task Test_01_Examples(string FileName)
		{
			await PerformTest(FileName);
		}

		[DataTestMethod]
		[DataRow("example07.rdf", "example07.nt", null)]
		[DataRow("example08.rdf", "example08.nt", null)]
		[DataRow("example09.rdf", "example09.nt", null)]
		[DataRow("example10.rdf", "example10.nt", null)]
		[DataRow("example11.rdf", "example11.nt", null)]
		[DataRow("example12.rdf", "example12.nt", null)]
		[DataRow("example13.rdf", "example13.nt", null)]
		[DataRow("example14.rdf", "example14.nt", null)]
		[DataRow("example15.rdf", "example15.nt", null)]
		[DataRow("example16.rdf", "example16.nt", null)]
		[DataRow("example17.rdf", "example17.nt", null)]
		[DataRow("example18.rdf", "example18.nt", null)]
		[DataRow("example19.rdf", "example19.nt", null)]
		[DataRow("example20.rdf", "example20.nt", null)]
		[DataRow("amp_in_url.test001.rdf", "amp_in_url.test001.nt", null)]
		[DataRow("datatypes.test001.rdf", "datatypes.test001.nt", null)]
		[DataRow("datatypes.test002.rdf", "datatypes.test002.nt", null)]
		[DataRow("rdf_charmod_literals.test001.rdf", "rdf_charmod_literals.test001.nt", null)]
		[DataRow("rdf_charmod_uris.test001.rdf", "rdf_charmod_uris.test001.nt", null)]
		[DataRow("rdf_charmod_uris.test002.rdf", "rdf_charmod_uris.test002.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test001.rdf", "rdf_containers_syntax_vs_schema.test001.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test002.rdf", "rdf_containers_syntax_vs_schema.test002.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test003.rdf", "rdf_containers_syntax_vs_schema.test003.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test004.rdf", "rdf_containers_syntax_vs_schema.test004.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test005.rdf", "rdf_containers_syntax_vs_schema.test005.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test006.rdf", "rdf_containers_syntax_vs_schema.test006.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test007.rdf", "rdf_containers_syntax_vs_schema.test007.nt", null)]
		[DataRow("rdf_containers_syntax_vs_schema.test008.rdf", "rdf_containers_syntax_vs_schema.test008.nt", null)]
		[DataRow("rdf_element_not_mandatory.test001.rdf", "rdf_element_not_mandatory.test001.nt", null)]
		[DataRow("rdfms_difference_between_ID_and_about.test1.rdf", "rdfms_difference_between_ID_and_about.test1.nt", null)]
		[DataRow("rdfms_difference_between_ID_and_about.test2.rdf", "rdfms_difference_between_ID_and_about.test2.nt", null)]
		[DataRow("rdfms_difference_between_ID_and_about.test3.rdf", "rdfms_difference_between_ID_and_about.test3.nt", null)]
		[DataRow("rdfms_duplicate_member_props.test001.rdf", "rdfms_duplicate_member_props.test001.nt", null)]
		[DataRow("rdfms_empty_property_elements.test001.rdf", "rdfms_empty_property_elements.test001.nt", null)]
		[DataRow("rdfms_empty_property_elements.test002.rdf", "rdfms_empty_property_elements.test002.nt", null)]
		[DataRow("rdfms_empty_property_elements.test003.rdf", "rdfms_empty_property_elements.test003.nt", null)]
		[DataRow("rdfms_empty_property_elements.test004.rdf", "rdfms_empty_property_elements.test004.nt", null)]
		[DataRow("rdfms_empty_property_elements.test005.rdf", "rdfms_empty_property_elements.test005.nt", null)]
		[DataRow("rdfms_empty_property_elements.test006.rdf", "rdfms_empty_property_elements.test006.nt", null)]
		[DataRow("rdfms_empty_property_elements.test007.rdf", "rdfms_empty_property_elements.test007.nt", null)]
		[DataRow("rdfms_empty_property_elements.test008.rdf", "rdfms_empty_property_elements.test008.nt", null)]
		[DataRow("rdfms_empty_property_elements.test009.rdf", "rdfms_empty_property_elements.test009.nt", null)]
		[DataRow("rdfms_empty_property_elements.test010.rdf", "rdfms_empty_property_elements.test010.nt", null)]
		[DataRow("rdfms_empty_property_elements.test011.rdf", "rdfms_empty_property_elements.test011.nt", null)]
		[DataRow("rdfms_empty_property_elements.test012.rdf", "rdfms_empty_property_elements.test012.nt", null)]
		[DataRow("rdfms_empty_property_elements.test013.rdf", "rdfms_empty_property_elements.test013.nt", null)]
		[DataRow("rdfms_empty_property_elements.test014.rdf", "rdfms_empty_property_elements.test014.nt", null)]
		[DataRow("rdfms_empty_property_elements.test015.rdf", "rdfms_empty_property_elements.test015.nt", null)]
		[DataRow("rdfms_empty_property_elements.test016.rdf", "rdfms_empty_property_elements.test016.nt", null)]
		[DataRow("rdfms_empty_property_elements.test017.rdf", "rdfms_empty_property_elements.test017.nt", null)]
		[DataRow("rdfms_identity_anon_resources.test001.rdf", "rdfms_identity_anon_resources.test001.nt", null)]
		[DataRow("rdfms_identity_anon_resources.test002.rdf", "rdfms_identity_anon_resources.test002.nt", null)]
		[DataRow("rdfms_identity_anon_resources.test003.rdf", "rdfms_identity_anon_resources.test003.nt", null)]
		[DataRow("rdfms_identity_anon_resources.test004.rdf", "rdfms_identity_anon_resources.test004.nt", null)]
		[DataRow("rdfms_identity_anon_resources.test005.rdf", "rdfms_identity_anon_resources.test005.nt", null)]
		[DataRow("rdfms_literal_is_xml_structure.test001.rdf", "rdfms_literal_is_xml_structure.test001.nt", null)]
		[DataRow("rdfms_literal_is_xml_structure.test002.rdf", "rdfms_literal_is_xml_structure.test002.nt", null)]
		[DataRow("rdfms_literal_is_xml_structure.test003.rdf", "rdfms_literal_is_xml_structure.test003.nt", null)]
		[DataRow("rdfms_literal_is_xml_structure.test004.rdf", "rdfms_literal_is_xml_structure.test004.nt", null)]
		[DataRow("rdfms_literal_is_xml_structure.test005.rdf", "rdfms_literal_is_xml_structure.test005.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test001.rdf", "rdfms_nested_bagIDs.test001.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test002.rdf", "rdfms_nested_bagIDs.test002.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test003.rdf", "rdfms_nested_bagIDs.test003.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test004.rdf", "rdfms_nested_bagIDs.test004.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test005.rdf", "rdfms_nested_bagIDs.test005.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test006.rdf", "rdfms_nested_bagIDs.test006.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test007.rdf", "rdfms_nested_bagIDs.test007.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test008.rdf", "rdfms_nested_bagIDs.test008.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test009.rdf", "rdfms_nested_bagIDs.test009.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test010.rdf", "rdfms_nested_bagIDs.test010.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test011.rdf", "rdfms_nested_bagIDs.test011a.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test011.rdf", "rdfms_nested_bagIDs.test011b.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test012.rdf", "rdfms_nested_bagIDs.test012a.nt", null)]
		[DataRow("rdfms_nested_bagIDs.test012.rdf", "rdfms_nested_bagIDs.test012b.nt", null)]
		[DataRow("rdfms_not_id_and_resource_attr.test001.rdf", "rdfms_not_id_and_resource_attr.test001.nt", null)]
		[DataRow("rdfms_not_id_and_resource_attr.test002.rdf", "rdfms_not_id_and_resource_attr.test002.nt", null)]
		[DataRow("rdfms_not_id_and_resource_attr.test003.rdf", "rdfms_not_id_and_resource_attr.test003.nt", null)]
		[DataRow("rdfms_not_id_and_resource_attr.test004.rdf", "rdfms_not_id_and_resource_attr.test004.nt", null)]
		[DataRow("rdfms_not_id_and_resource_attr.test005.rdf", "rdfms_not_id_and_resource_attr.test005.nt", null)]
		[DataRow("rdfms_para196.test001.rdf", "rdfms_para196.test001.nt", null)]
		[DataRow("rdfms_rdf_names_use.test001.rdf", "rdfms_rdf_names_use.test001.nt", null)]
		[DataRow("rdfms_rdf_names_use.test002.rdf", "rdfms_rdf_names_use.test002.nt", null)]
		[DataRow("rdfms_rdf_names_use.test003.rdf", "rdfms_rdf_names_use.test003.nt", null)]
		[DataRow("rdfms_rdf_names_use.test004.rdf", "rdfms_rdf_names_use.test004.nt", null)]
		[DataRow("rdfms_rdf_names_use.test005.rdf", "rdfms_rdf_names_use.test005.nt", null)]
		[DataRow("rdfms_rdf_names_use.test006.rdf", "rdfms_rdf_names_use.test006.nt", null)]
		[DataRow("rdfms_rdf_names_use.test007.rdf", "rdfms_rdf_names_use.test007.nt", null)]
		[DataRow("rdfms_rdf_names_use.test008.rdf", "rdfms_rdf_names_use.test008.nt", null)]
		[DataRow("rdfms_rdf_names_use.test009.rdf", "rdfms_rdf_names_use.test009.nt", null)]
		[DataRow("rdfms_rdf_names_use.test010.rdf", "rdfms_rdf_names_use.test010.nt", null)]
		[DataRow("rdfms_rdf_names_use.test011.rdf", "rdfms_rdf_names_use.test011.nt", null)]
		[DataRow("rdfms_rdf_names_use.test012.rdf", "rdfms_rdf_names_use.test012.nt", null)]
		[DataRow("rdfms_rdf_names_use.test013.rdf", "rdfms_rdf_names_use.test013.nt", null)]
		[DataRow("rdfms_rdf_names_use.test014.rdf", "rdfms_rdf_names_use.test014.nt", null)]
		[DataRow("rdfms_rdf_names_use.test015.rdf", "rdfms_rdf_names_use.test015.nt", null)]
		[DataRow("rdfms_rdf_names_use.test016.rdf", "rdfms_rdf_names_use.test016.nt", null)]
		[DataRow("rdfms_rdf_names_use.test017.rdf", "rdfms_rdf_names_use.test017.nt", null)]
		[DataRow("rdfms_rdf_names_use.test018.rdf", "rdfms_rdf_names_use.test018.nt", null)]
		[DataRow("rdfms_rdf_names_use.test019.rdf", "rdfms_rdf_names_use.test019.nt", null)]
		[DataRow("rdfms_rdf_names_use.test020.rdf", "rdfms_rdf_names_use.test020.nt", null)]
		[DataRow("rdfms_rdf_names_use.test021.rdf", "rdfms_rdf_names_use.test021.nt", null)]
		[DataRow("rdfms_rdf_names_use.test022.rdf", "rdfms_rdf_names_use.test022.nt", null)]
		[DataRow("rdfms_rdf_names_use.test023.rdf", "rdfms_rdf_names_use.test023.nt", null)]
		[DataRow("rdfms_rdf_names_use.test024.rdf", "rdfms_rdf_names_use.test024.nt", null)]
		[DataRow("rdfms_rdf_names_use.test025.rdf", "rdfms_rdf_names_use.test025.nt", null)]
		[DataRow("rdfms_rdf_names_use.test026.rdf", "rdfms_rdf_names_use.test026.nt", null)]
		[DataRow("rdfms_rdf_names_use.test027.rdf", "rdfms_rdf_names_use.test027.nt", null)]
		[DataRow("rdfms_rdf_names_use.test028.rdf", "rdfms_rdf_names_use.test028.nt", null)]
		[DataRow("rdfms_rdf_names_use.test029.rdf", "rdfms_rdf_names_use.test029.nt", null)]
		[DataRow("rdfms_rdf_names_use.test030.rdf", "rdfms_rdf_names_use.test030.nt", null)]
		[DataRow("rdfms_rdf_names_use.test031.rdf", "rdfms_rdf_names_use.test031.nt", null)]
		[DataRow("rdfms_rdf_names_use.test032.rdf", "rdfms_rdf_names_use.test032.nt", null)]
		[DataRow("rdfms_rdf_names_use.test033.rdf", "rdfms_rdf_names_use.test033.nt", null)]
		[DataRow("rdfms_rdf_names_use.test034.rdf", "rdfms_rdf_names_use.test034.nt", null)]
		[DataRow("rdfms_rdf_names_use.test035.rdf", "rdfms_rdf_names_use.test035.nt", null)]
		[DataRow("rdfms_rdf_names_use.test036.rdf", "rdfms_rdf_names_use.test036.nt", null)]
		[DataRow("rdfms_rdf_names_use.test037.rdf", "rdfms_rdf_names_use.test037.nt", null)]
		[DataRow("rdfms_rdf_names_use.warn001.rdf", "rdfms_rdf_names_use.warn001.nt", null)]
		[DataRow("rdfms_rdf_names_use.warn002.rdf", "rdfms_rdf_names_use.warn002.nt", null)]
		[DataRow("rdfms_rdf_names_use.warn003.rdf", "rdfms_rdf_names_use.warn003.nt", null)]
		[DataRow("rdfms_reification_required.test001.rdf", "rdfms_reification_required.test001.nt", null)]
		[DataRow("rdfms_seq_representation.test001.rdf", "rdfms_seq_representation.test001.nt", null)]
		[DataRow("rdfms_syntax_incomplete.test001.rdf", "rdfms_syntax_incomplete.test001.nt", null)]
		[DataRow("rdfms_syntax_incomplete.test002.rdf", "rdfms_syntax_incomplete.test002.nt", null)]
		[DataRow("rdfms_syntax_incomplete.test003.rdf", "rdfms_syntax_incomplete.test003.nt", null)]
		[DataRow("rdfms_syntax_incomplete.test004.rdf", "rdfms_syntax_incomplete.test004.nt", null)]
		[DataRow("rdfms_uri_substructure.test001.rdf", "rdfms_uri_substructure.test001.nt", null)]
		[DataRow("rdfms_xmllang.test001.rdf", "rdfms_xmllang.test001.nt", null)]
		[DataRow("rdfms_xmllang.test002.rdf", "rdfms_xmllang.test002.nt", null)]
		[DataRow("rdfms_xmllang.test003.rdf", "rdfms_xmllang.test003.nt", null)]
		[DataRow("rdfms_xmllang.test004.rdf", "rdfms_xmllang.test004.nt", null)]
		[DataRow("rdfms_xmllang.test005.rdf", "rdfms_xmllang.test005.nt", null)]
		[DataRow("rdfms_xmllang.test006.rdf", "rdfms_xmllang.test006.nt", null)]
		[DataRow("rdfms_xml_literal_namespaces.test001.rdf", "rdfms_xml_literal_namespaces.test001.nt", null)]
		[DataRow("rdfms_xml_literal_namespaces.test002.rdf", "rdfms_xml_literal_namespaces.test002.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test001.rdf", "rdf_ns_prefix_confusion.test001.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test002.rdf", "rdf_ns_prefix_confusion.test002.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test003.rdf", "rdf_ns_prefix_confusion.test003.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test004.rdf", "rdf_ns_prefix_confusion.test004.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test005.rdf", "rdf_ns_prefix_confusion.test005.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test006.rdf", "rdf_ns_prefix_confusion.test006.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test007.rdf", "rdf_ns_prefix_confusion.test007.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test008.rdf", "rdf_ns_prefix_confusion.test008.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test009.rdf", "rdf_ns_prefix_confusion.test009.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test010.rdf", "rdf_ns_prefix_confusion.test010.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test011.rdf", "rdf_ns_prefix_confusion.test011.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test012.rdf", "rdf_ns_prefix_confusion.test012.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test013.rdf", "rdf_ns_prefix_confusion.test013.nt", null)]
		[DataRow("rdf_ns_prefix_confusion.test014.rdf", "rdf_ns_prefix_confusion.test014.nt", null)]
		[DataRow("rdfs_domain_and_range.test001.rdf", "rdfs_domain_and_range.test001.nt", null)]
		[DataRow("rdfs_domain_and_range.test002.rdf", "rdfs_domain_and_range.test002.nt", null)]
		[DataRow("rdfs_domain_and_range.test003.rdf", "rdfs_domain_and_range.test003.nt", null)]
		[DataRow("rdfs_domain_and_range.test004.rdf", "rdfs_domain_and_range.test004.nt", null)]
		[DataRow("rdfs_no_cycles_in_subClassOf.test001.rdf", "rdfs_no_cycles_in_subClassOf.test001.nt", null)]
		[DataRow("rdfs_no_cycles_in_subPropertyOf.test001.rdf", "rdfs_no_cycles_in_subPropertyOf.test001.nt", null)]
		[DataRow("unrecognised_xml_attributes.test001.rdf", "unrecognised_xml_attributes.test001.nt", null)]
		[DataRow("unrecognised_xml_attributes.test002.rdf", "unrecognised_xml_attributes.test002.nt", null)]
		[DataRow("xmlbase.test001.rdf", "xmlbase.test001.nt", null)]
		[DataRow("xmlbase.test002.rdf", "xmlbase.test002.nt", null)]
		[DataRow("xmlbase.test003.rdf", "xmlbase.test003.nt", null)]
		[DataRow("xmlbase.test004.rdf", "xmlbase.test004.nt", null)]
		[DataRow("xmlbase.test005.rdf", "xmlbase.test005.nt", null)]
		[DataRow("xmlbase.test006.rdf", "xmlbase.test006.nt", null)]
		[DataRow("xmlbase.test007.rdf", "xmlbase.test007.nt", null)]
		[DataRow("xmlbase.test008.rdf", "xmlbase.test008.nt", null)]
		[DataRow("xmlbase.test009.rdf", "xmlbase.test009.nt", null)]
		[DataRow("xmlbase.test010.rdf", "xmlbase.test010.nt", null)]
		[DataRow("xmlbase.test011.rdf", "xmlbase.test011.nt", null)]
		[DataRow("xmlbase.test012.rdf", "xmlbase.test012.nt", null)]
		[DataRow("xmlbase.test013.rdf", "xmlbase.test013.nt", null)]
		[DataRow("xmlbase.test014.rdf", "xmlbase.test014.nt", null)]
		[DataRow("xmlbase.test015.rdf", "xmlbase.test015.nt", null)]
		[DataRow("xmlbase.test016.rdf", "xmlbase.test016.nt", null)]
		[DataRow("xml_canon.test001.rdf", "xml_canon.test001.nt", null)]
		public async Task Test_02_PassTests(string FileName, string Expected, string BaseUri)
		{
			await PerformTest(FileName, Expected, BaseUri);
		}

		[DataTestMethod]
		[ExpectedException(typeof(UriFormatException))]
		[DataRow("rdf_charmod_literals.error001.rdf", null)]
		[DataRow("rdf_charmod_literals.error002.rdf", null)]
		[DataRow("rdf_charmod_uris.error001.rdf", null)]
		[DataRow("rdf_containers_syntax_vs_schema.error001.rdf", null)]
		[DataRow("rdf_containers_syntax_vs_schema.error002.rdf", null)]
		[DataRow("rdfms_abouteach.error001.rdf", null)]
		[DataRow("rdfms_abouteach.error002.rdf", null)]
		[DataRow("rdfms_difference_between_ID_and_about.error1.rdf", null)]
		[DataRow("rdfms_empty_property_elements.error001.rdf", null)]
		[DataRow("rdfms_empty_property_elements.error002.rdf", null)]
		[DataRow("rdfms_empty_property_elements.error003.rdf", null)]
		[DataRow("rdfms_parseType.error001.rdf", null)]
		[DataRow("rdfms_parseType.error002.rdf", null)]
		[DataRow("rdfms_parseType.error003.rdf", null)]
		[DataRow("rdfms_rdf_id.error001.rdf", null)]
		[DataRow("rdfms_rdf_id.error002.rdf", null)]
		[DataRow("rdfms_rdf_id.error003.rdf", null)]
		[DataRow("rdfms_rdf_id.error004.rdf", null)]
		[DataRow("rdfms_rdf_id.error005.rdf", null)]
		[DataRow("rdfms_rdf_id.error006.rdf", null)]
		[DataRow("rdfms_rdf_id.error007.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error001.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error002.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error003.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error004.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error005.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error006.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error007.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error008.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error009.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error010.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error011.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error012.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error013.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error014.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error015.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error016.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error017.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error018.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error019.rdf", null)]
		[DataRow("rdfms_rdf_names_use.error020.rdf", null)]
		[DataRow("rdfms_syntax_incomplete.error001.rdf", null)]
		[DataRow("rdfms_syntax_incomplete.error002.rdf", null)]
		[DataRow("rdfms_syntax_incomplete.error003.rdf", null)]
		[DataRow("rdfms_syntax_incomplete.error004.rdf", null)]
		[DataRow("rdfms_syntax_incomplete.error005.rdf", null)]
		[DataRow("rdfms_syntax_incomplete.error006.rdf", null)]
		[DataRow("rdfms_uri_substructure.error001.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error001.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error002.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error003.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error004.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error005.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error006.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error007.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error008.rdf", null)]
		[DataRow("rdf_ns_prefix_confusion.error009.rdf", null)]
		[DataRow("xmlbase.error001.rdf", null)]
		public async Task Test_03_Bad(string FileName, string BaseUri)
		{
			await PerformTest(FileName, BaseUri);
		}

		private static async Task PerformTest(string FileName)
		{
			RdfDocument Parsed = await LoadRdfDocument(FileName, null);
			await Print(Parsed);
		}

		private static async Task PerformTest(string FileName, string BaseUri)
		{
			RdfDocument Parsed = await LoadRdfDocument(FileName, new Uri(BaseUri + FileName));
			await Print(Parsed);
		}

		private static async Task Print(RdfDocument Parsed)
		{
			KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Parsed, Encoding.UTF8);
			Assert.AreEqual("application/rdf+xml; charset=utf-8", P.Value);

			byte[] Data = P.Key;
			string s = Encoding.UTF8.GetString(Data);

			Console.Out.WriteLine(s);
			Console.Out.WriteLine();

			foreach (ISemanticTriple Triple in Parsed)
				Console.WriteLine(Triple.ToString());
		}

		private static async Task<RdfDocument> LoadRdfDocument(string FileName, Uri? BaseUri)
		{
			byte[] Data = Resources.LoadResource(typeof(RdfTests).Namespace + ".Data.Rdf." + FileName);
			object Decoded = await InternetContent.DecodeAsync("application/rdf+xml", Data, BaseUri);
			if (Decoded is not RdfDocument Parsed)
				throw new Exception("Unable to decode RDF document.");

			return Parsed;
		}

		private static async Task<TurtleDocument> LoadTurtleDocument(string FileName, Uri? BaseUri)
		{
			byte[] Data = Resources.LoadResource(typeof(TurtleTests).Namespace + ".Data.Rdf." + FileName);
			object Decoded = await InternetContent.DecodeAsync("text/turtle", Data, BaseUri);
			if (Decoded is not TurtleDocument Parsed)
				throw new Exception("Unable to decode Turtle document.");

			return Parsed;
		}

		private static async Task PerformTest(string FileName, string ExpectedFileName, string BaseUri)
		{
			RdfDocument Parsed = await LoadRdfDocument(FileName, BaseUri is null ? null : new Uri(BaseUri + FileName));
			TurtleDocument ParsedExpected = await LoadTurtleDocument(ExpectedFileName, BaseUri is null ? null : new Uri(BaseUri + ExpectedFileName));

			await Print(Parsed);
			Console.Out.WriteLine();
			await TurtleTests.Print(ParsedExpected);

			CompareTriples(Parsed, ParsedExpected);
		}

	}
}