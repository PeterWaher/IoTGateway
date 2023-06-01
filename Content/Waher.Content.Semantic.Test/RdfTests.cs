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
		[DataRow("horst-01/test001.rdf")]
		[DataRow("horst-01/test002.rdf")]
		[DataRow("horst-01/test003.rdf")]
		[DataRow("horst-01/test004.rdf")]
		[DataRow("rdfs-container-membership-superProperty/not1C.rdf")]
		[DataRow("rdfs-container-membership-superProperty/not1P.rdf")]
		[DataRow("rdfs-domain-and-range/nonconclusions005.rdf")]
		[DataRow("rdfs-domain-and-range/nonconclusions006.rdf")]
		[DataRow("rdfs-domain-and-range/premises005.rdf")]
		[DataRow("rdfs-domain-and-range/premises006.rdf")]
		[DataRow("tex-01/test001.rdf")]
		[DataRow("tex-01/test002.rdf")]
		[DataRow("xmlsch-02/test001.rdf")]
		[DataRow("xmlsch-02/test002.rdf")]
		[DataRow("xmlsch-02/test003.rdf")]
		public async Task Test_01_Examples(string FileName)
		{
			await PerformTest(FileName);
		}

		[DataTestMethod]
		[DataRow("example07.rdf", "example07/nt", null)]
		[DataRow("example08.rdf", "example08/nt", null)]
		[DataRow("example09.rdf", "example09/nt", null)]
		[DataRow("example10.rdf", "example10/nt", null)]
		[DataRow("example11.rdf", "example11/nt", null)]
		[DataRow("example12.rdf", "example12/nt", null)]
		[DataRow("example13.rdf", "example13/nt", null)]
		[DataRow("example14.rdf", "example14/nt", null)]
		[DataRow("example15.rdf", "example15/nt", null)]
		[DataRow("example16.rdf", "example16/nt", null)]
		[DataRow("example17.rdf", "example17/nt", null)]
		[DataRow("example18.rdf", "example18/nt", null)]
		[DataRow("example19.rdf", "example19/nt", null)]
		[DataRow("example20.rdf", "example20/nt", null)]
		[DataRow("amp-in-url/test001.rdf", "amp-in-url/test001/nt", null)]
		[DataRow("datatypes/test001.rdf", "datatypes/test001/nt", null)]
		[DataRow("datatypes/test002.rdf", "datatypes/test002/nt", null)]
		[DataRow("rdf-charmod-literals/test001.rdf", "rdf-charmod-literals/test001/nt", null)]
		[DataRow("rdf-charmod-uris/test001.rdf", "rdf-charmod-uris/test001/nt", null)]
		[DataRow("rdf-charmod-uris/test002.rdf", "rdf-charmod-uris/test002/nt", null)]
		[DataRow("rdf-containers-syntax-vs-schema/test001.rdf", "rdf-containers-syntax-vs-schema/test001/nt", null)]
		[DataRow("rdf-containers-syntax-vs-schema/test002.rdf", "rdf-containers-syntax-vs-schema/test002/nt", null)]
		[DataRow("rdf-containers-syntax-vs-schema/test003.rdf", "rdf-containers-syntax-vs-schema/test003/nt", null)]
		[DataRow("rdf-containers-syntax-vs-schema/test004.rdf", "rdf-containers-syntax-vs-schema/test004/nt", null)]
		[DataRow("rdf-containers-syntax-vs-schema/test006.rdf", "rdf-containers-syntax-vs-schema/test006/nt", null)]
		[DataRow("rdf-containers-syntax-vs-schema/test007.rdf", "rdf-containers-syntax-vs-schema/test007/nt", null)]
		[DataRow("rdf-containers-syntax-vs-schema/test008.rdf", "rdf-containers-syntax-vs-schema/test008/nt", null)]
		[DataRow("rdf-element-not-mandatory/test001.rdf", "rdf-element-not-mandatory/test001/nt", null)]
		[DataRow("rdfms-difference-between-ID-and-about/test1.rdf", "rdfms-difference-between-ID-and-about/test1/nt", null)]
		[DataRow("rdfms-difference-between-ID-and-about/test2.rdf", "rdfms-difference-between-ID-and-about/test2/nt", null)]
		[DataRow("rdfms-difference-between-ID-and-about/test3.rdf", "rdfms-difference-between-ID-and-about/test3/nt", null)]
		[DataRow("rdfms-duplicate-member-props/test001.rdf", "rdfms-duplicate-member-props/test001/nt", null)]
		[DataRow("rdfms-empty-property-elements/test001.rdf", "rdfms-empty-property-elements/test001/nt", null)]
		[DataRow("rdfms-empty-property-elements/test002.rdf", "rdfms-empty-property-elements/test002/nt", null)]
		[DataRow("rdfms-empty-property-elements/test003.rdf", "rdfms-empty-property-elements/test003/nt", null)]
		[DataRow("rdfms-empty-property-elements/test004.rdf", "rdfms-empty-property-elements/test004/nt", null)]
		[DataRow("rdfms-empty-property-elements/test005.rdf", "rdfms-empty-property-elements/test005/nt", null)]
		[DataRow("rdfms-empty-property-elements/test006.rdf", "rdfms-empty-property-elements/test006/nt", null)]
		[DataRow("rdfms-empty-property-elements/test007.rdf", "rdfms-empty-property-elements/test007/nt", null)]
		[DataRow("rdfms-empty-property-elements/test008.rdf", "rdfms-empty-property-elements/test008/nt", null)]
		[DataRow("rdfms-empty-property-elements/test009.rdf", "rdfms-empty-property-elements/test009/nt", null)]
		[DataRow("rdfms-empty-property-elements/test010.rdf", "rdfms-empty-property-elements/test010/nt", null)]
		[DataRow("rdfms-empty-property-elements/test011.rdf", "rdfms-empty-property-elements/test011/nt", null)]
		[DataRow("rdfms-empty-property-elements/test012.rdf", "rdfms-empty-property-elements/test012/nt", null)]
		[DataRow("rdfms-empty-property-elements/test013.rdf", "rdfms-empty-property-elements/test013/nt", null)]
		[DataRow("rdfms-empty-property-elements/test014.rdf", "rdfms-empty-property-elements/test014/nt", null)]
		[DataRow("rdfms-empty-property-elements/test015.rdf", "rdfms-empty-property-elements/test015/nt", null)]
		[DataRow("rdfms-empty-property-elements/test016.rdf", "rdfms-empty-property-elements/test016/nt", null)]
		[DataRow("rdfms-empty-property-elements/test017.rdf", "rdfms-empty-property-elements/test017/nt", null)]
		[DataRow("rdfms-identity-anon-resources/test001.rdf", "rdfms-identity-anon-resources/test001/nt", null)]
		[DataRow("rdfms-identity-anon-resources/test002.rdf", "rdfms-identity-anon-resources/test002/nt", null)]
		[DataRow("rdfms-identity-anon-resources/test003.rdf", "rdfms-identity-anon-resources/test003/nt", null)]
		[DataRow("rdfms-identity-anon-resources/test004.rdf", "rdfms-identity-anon-resources/test004/nt", null)]
		[DataRow("rdfms-identity-anon-resources/test005.rdf", "rdfms-identity-anon-resources/test005/nt", null)]
		[DataRow("rdfms-literal-is-xml-structure/test001.rdf", "rdfms-literal-is-xml-structure/test001/nt", null)]
		[DataRow("rdfms-literal-is-xml-structure/test002.rdf", "rdfms-literal-is-xml-structure/test002/nt", null)]
		[DataRow("rdfms-literal-is-xml-structure/test003.rdf", "rdfms-literal-is-xml-structure/test003/nt", null)]
		[DataRow("rdfms-literal-is-xml-structure/test004.rdf", "rdfms-literal-is-xml-structure/test004/nt", null)]
		[DataRow("rdfms-literal-is-xml-structure/test005.rdf", "rdfms-literal-is-xml-structure/test005/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test001.rdf", "rdfms-nested-bagIDs/test001/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test002.rdf", "rdfms-nested-bagIDs/test002/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test003.rdf", "rdfms-nested-bagIDs/test003/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test004.rdf", "rdfms-nested-bagIDs/test004/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test005.rdf", "rdfms-nested-bagIDs/test005/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test006.rdf", "rdfms-nested-bagIDs/test006/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test007.rdf", "rdfms-nested-bagIDs/test007/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test008.rdf", "rdfms-nested-bagIDs/test008/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test009.rdf", "rdfms-nested-bagIDs/test009/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test010.rdf", "rdfms-nested-bagIDs/test010/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test011.rdf", "rdfms-nested-bagIDs/test011a/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test011.rdf", "rdfms-nested-bagIDs/test011b/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test012.rdf", "rdfms-nested-bagIDs/test012a/nt", null)]
		[DataRow("rdfms-nested-bagIDs/test012.rdf", "rdfms-nested-bagIDs/test012b/nt", null)]
		[DataRow("rdfms-not-id-and-resource-attr/test001.rdf", "rdfms-not-id-and-resource-attr/test001/nt", null)]
		[DataRow("rdfms-not-id-and-resource-attr/test002.rdf", "rdfms-not-id-and-resource-attr/test002/nt", null)]
		[DataRow("rdfms-not-id-and-resource-attr/test003.rdf", "rdfms-not-id-and-resource-attr/test003/nt", null)]
		[DataRow("rdfms-not-id-and-resource-attr/test004.rdf", "rdfms-not-id-and-resource-attr/test004/nt", null)]
		[DataRow("rdfms-not-id-and-resource-attr/test005.rdf", "rdfms-not-id-and-resource-attr/test005/nt", null)]
		[DataRow("rdfms-para196/test001.rdf", "rdfms-para196/test001/nt", null)]
		[DataRow("rdfms-rdf-names-use/test001.rdf", "rdfms-rdf-names-use/test001/nt", null)]
		[DataRow("rdfms-rdf-names-use/test002.rdf", "rdfms-rdf-names-use/test002/nt", null)]
		[DataRow("rdfms-rdf-names-use/test003.rdf", "rdfms-rdf-names-use/test003/nt", null)]
		[DataRow("rdfms-rdf-names-use/test004.rdf", "rdfms-rdf-names-use/test004/nt", null)]
		[DataRow("rdfms-rdf-names-use/test005.rdf", "rdfms-rdf-names-use/test005/nt", null)]
		[DataRow("rdfms-rdf-names-use/test006.rdf", "rdfms-rdf-names-use/test006/nt", null)]
		[DataRow("rdfms-rdf-names-use/test007.rdf", "rdfms-rdf-names-use/test007/nt", null)]
		[DataRow("rdfms-rdf-names-use/test008.rdf", "rdfms-rdf-names-use/test008/nt", null)]
		[DataRow("rdfms-rdf-names-use/test009.rdf", "rdfms-rdf-names-use/test009/nt", null)]
		[DataRow("rdfms-rdf-names-use/test010.rdf", "rdfms-rdf-names-use/test010/nt", null)]
		[DataRow("rdfms-rdf-names-use/test011.rdf", "rdfms-rdf-names-use/test011/nt", null)]
		[DataRow("rdfms-rdf-names-use/test012.rdf", "rdfms-rdf-names-use/test012/nt", null)]
		[DataRow("rdfms-rdf-names-use/test013.rdf", "rdfms-rdf-names-use/test013/nt", null)]
		[DataRow("rdfms-rdf-names-use/test014.rdf", "rdfms-rdf-names-use/test014/nt", null)]
		[DataRow("rdfms-rdf-names-use/test015.rdf", "rdfms-rdf-names-use/test015/nt", null)]
		[DataRow("rdfms-rdf-names-use/test016.rdf", "rdfms-rdf-names-use/test016/nt", null)]
		[DataRow("rdfms-rdf-names-use/test017.rdf", "rdfms-rdf-names-use/test017/nt", null)]
		[DataRow("rdfms-rdf-names-use/test018.rdf", "rdfms-rdf-names-use/test018/nt", null)]
		[DataRow("rdfms-rdf-names-use/test019.rdf", "rdfms-rdf-names-use/test019/nt", null)]
		[DataRow("rdfms-rdf-names-use/test020.rdf", "rdfms-rdf-names-use/test020/nt", null)]
		[DataRow("rdfms-rdf-names-use/test021.rdf", "rdfms-rdf-names-use/test021/nt", null)]
		[DataRow("rdfms-rdf-names-use/test022.rdf", "rdfms-rdf-names-use/test022/nt", null)]
		[DataRow("rdfms-rdf-names-use/test023.rdf", "rdfms-rdf-names-use/test023/nt", null)]
		[DataRow("rdfms-rdf-names-use/test024.rdf", "rdfms-rdf-names-use/test024/nt", null)]
		[DataRow("rdfms-rdf-names-use/test025.rdf", "rdfms-rdf-names-use/test025/nt", null)]
		[DataRow("rdfms-rdf-names-use/test026.rdf", "rdfms-rdf-names-use/test026/nt", null)]
		[DataRow("rdfms-rdf-names-use/test027.rdf", "rdfms-rdf-names-use/test027/nt", null)]
		[DataRow("rdfms-rdf-names-use/test028.rdf", "rdfms-rdf-names-use/test028/nt", null)]
		[DataRow("rdfms-rdf-names-use/test029.rdf", "rdfms-rdf-names-use/test029/nt", null)]
		[DataRow("rdfms-rdf-names-use/test030.rdf", "rdfms-rdf-names-use/test030/nt", null)]
		[DataRow("rdfms-rdf-names-use/test031.rdf", "rdfms-rdf-names-use/test031/nt", null)]
		[DataRow("rdfms-rdf-names-use/test032.rdf", "rdfms-rdf-names-use/test032/nt", null)]
		[DataRow("rdfms-rdf-names-use/test033.rdf", "rdfms-rdf-names-use/test033/nt", null)]
		[DataRow("rdfms-rdf-names-use/test034.rdf", "rdfms-rdf-names-use/test034/nt", null)]
		[DataRow("rdfms-rdf-names-use/test035.rdf", "rdfms-rdf-names-use/test035/nt", null)]
		[DataRow("rdfms-rdf-names-use/test036.rdf", "rdfms-rdf-names-use/test036/nt", null)]
		[DataRow("rdfms-rdf-names-use/test037.rdf", "rdfms-rdf-names-use/test037/nt", null)]
		[DataRow("rdfms-rdf-names-use/warn001.rdf", "rdfms-rdf-names-use/warn001/nt", null)]
		[DataRow("rdfms-rdf-names-use/warn002.rdf", "rdfms-rdf-names-use/warn002/nt", null)]
		[DataRow("rdfms-rdf-names-use/warn003.rdf", "rdfms-rdf-names-use/warn003/nt", null)]
		[DataRow("rdfms-reification-required/test001.rdf", "rdfms-reification-required/test001/nt", null)]
		[DataRow("rdfms-seq-representation/test001.rdf", "rdfms-seq-representation/test001/nt", null)]
		[DataRow("rdfms-syntax-incomplete/test001.rdf", "rdfms-syntax-incomplete/test001/nt", null)]
		[DataRow("rdfms-syntax-incomplete/test002.rdf", "rdfms-syntax-incomplete/test002/nt", null)]
		[DataRow("rdfms-syntax-incomplete/test003.rdf", "rdfms-syntax-incomplete/test003/nt", null)]
		[DataRow("rdfms-syntax-incomplete/test004.rdf", "rdfms-syntax-incomplete/test004/nt", null)]
		[DataRow("rdfms-uri-substructure/test001.rdf", "rdfms-uri-substructure/test001/nt", null)]
		[DataRow("rdfms-xmllang/test001.rdf", "rdfms-xmllang/test001/nt", null)]
		[DataRow("rdfms-xmllang/test002.rdf", "rdfms-xmllang/test002/nt", null)]
		[DataRow("rdfms-xmllang/test003.rdf", "rdfms-xmllang/test003/nt", null)]
		[DataRow("rdfms-xmllang/test004.rdf", "rdfms-xmllang/test004/nt", null)]
		[DataRow("rdfms-xmllang/test005.rdf", "rdfms-xmllang/test005/nt", null)]
		[DataRow("rdfms-xmllang/test006.rdf", "rdfms-xmllang/test006/nt", null)]
		[DataRow("rdfms-xml-literal-namespaces/test001.rdf", "rdfms-xml-literal-namespaces/test001/nt", null)]
		[DataRow("rdfms-xml-literal-namespaces/test002.rdf", "rdfms-xml-literal-namespaces/test002/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test001.rdf", "rdf-ns-prefix-confusion/test001/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test002.rdf", "rdf-ns-prefix-confusion/test002/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test003.rdf", "rdf-ns-prefix-confusion/test003/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test004.rdf", "rdf-ns-prefix-confusion/test004/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test005.rdf", "rdf-ns-prefix-confusion/test005/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test006.rdf", "rdf-ns-prefix-confusion/test006/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test007.rdf", "rdf-ns-prefix-confusion/test007/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test008.rdf", "rdf-ns-prefix-confusion/test008/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test009.rdf", "rdf-ns-prefix-confusion/test009/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test010.rdf", "rdf-ns-prefix-confusion/test010/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test011.rdf", "rdf-ns-prefix-confusion/test011/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test012.rdf", "rdf-ns-prefix-confusion/test012/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test013.rdf", "rdf-ns-prefix-confusion/test013/nt", null)]
		[DataRow("rdf-ns-prefix-confusion/test014.rdf", "rdf-ns-prefix-confusion/test014/nt", null)]
		[DataRow("rdfs-domain-and-range/test001.rdf", "rdfs-domain-and-range/test001/nt", null)]
		[DataRow("rdfs-domain-and-range/test002.rdf", "rdfs-domain-and-range/test002/nt", null)]
		[DataRow("rdfs-domain-and-range/test003.rdf", "rdfs-domain-and-range/test003/nt", null)]
		[DataRow("rdfs-domain-and-range/test004.rdf", "rdfs-domain-and-range/test004/nt", null)]
		[DataRow("rdfs-no-cycles-in-subClassOf/test001.rdf", "rdfs-no-cycles-in-subClassOf/test001/nt", null)]
		[DataRow("rdfs-no-cycles-in-subPropertyOf/test001.rdf", "rdfs-no-cycles-in-subPropertyOf/test001/nt", null)]
		[DataRow("unrecognised-xml-attributes/test001.rdf", "unrecognised-xml-attributes/test001/nt", null)]
		[DataRow("unrecognised-xml-attributes/test002.rdf", "unrecognised-xml-attributes/test002/nt", null)]
		[DataRow("xmlbase/test001.rdf", "xmlbase/test001/nt", null)]
		[DataRow("xmlbase/test002.rdf", "xmlbase/test002/nt", null)]
		[DataRow("xmlbase/test003.rdf", "xmlbase/test003/nt", null)]
		[DataRow("xmlbase/test004.rdf", "xmlbase/test004/nt", null)]
		[DataRow("xmlbase/test005.rdf", "xmlbase/test005/nt", null)]
		[DataRow("xmlbase/test006.rdf", "xmlbase/test006/nt", null)]
		[DataRow("xmlbase/test007.rdf", "xmlbase/test007/nt", null)]
		[DataRow("xmlbase/test008.rdf", "xmlbase/test008/nt", null)]
		[DataRow("xmlbase/test009.rdf", "xmlbase/test009/nt", null)]
		[DataRow("xmlbase/test010.rdf", "xmlbase/test010/nt", null)]
		[DataRow("xmlbase/test011.rdf", "xmlbase/test011/nt", null)]
		[DataRow("xmlbase/test012.rdf", "xmlbase/test012/nt", null)]
		[DataRow("xmlbase/test013.rdf", "xmlbase/test013/nt", null)]
		[DataRow("xmlbase/test014.rdf", "xmlbase/test014/nt", null)]
		[DataRow("xmlbase/test015.rdf", "xmlbase/test015/nt", null)]
		[DataRow("xmlbase/test016.rdf", "xmlbase/test016/nt", null)]
		[DataRow("xml-canon/test001.rdf", "xml-canon/test001/nt", null)]
		public async Task Test_02_PassTests(string FileName, string Expected, string BaseUri)
		{
			await PerformTest(FileName, Expected, BaseUri);
		}

		[DataTestMethod]
		[ExpectedException(typeof(UriFormatException))]
		[DataRow("rdf-charmod-literals/error001.rdf", null)]
		[DataRow("rdf-charmod-literals/error002.rdf", null)]
		[DataRow("rdf-charmod-uris/error001.rdf", null)]
		[DataRow("rdf-containers-syntax-vs-schema/error001.rdf", null)]
		[DataRow("rdf-containers-syntax-vs-schema/error002.rdf", null)]
		[DataRow("rdfms-abouteach/error001.rdf", null)]
		[DataRow("rdfms-abouteach/error002.rdf", null)]
		[DataRow("rdfms-difference-between-ID-and-about/error1.rdf", null)]
		[DataRow("rdfms-empty-property-elements/error001.rdf", null)]
		[DataRow("rdfms-empty-property-elements/error002.rdf", null)]
		[DataRow("rdfms-empty-property-elements/error003.rdf", null)]
		[DataRow("rdfms-parseType/error001.rdf", null)]
		[DataRow("rdfms-parseType/error002.rdf", null)]
		[DataRow("rdfms-parseType/error003.rdf", null)]
		[DataRow("rdfms-rdf-id/error001.rdf", null)]
		[DataRow("rdfms-rdf-id/error002.rdf", null)]
		[DataRow("rdfms-rdf-id/error003.rdf", null)]
		[DataRow("rdfms-rdf-id/error004.rdf", null)]
		[DataRow("rdfms-rdf-id/error005.rdf", null)]
		[DataRow("rdfms-rdf-id/error006.rdf", null)]
		[DataRow("rdfms-rdf-id/error007.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error001.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error002.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error003.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error004.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error005.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error006.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error007.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error008.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error009.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error010.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error011.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error012.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error013.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error014.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error015.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error016.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error017.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error018.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error019.rdf", null)]
		[DataRow("rdfms-rdf-names-use/error020.rdf", null)]
		[DataRow("rdfms-syntax-incomplete/error001.rdf", null)]
		[DataRow("rdfms-syntax-incomplete/error002.rdf", null)]
		[DataRow("rdfms-syntax-incomplete/error003.rdf", null)]
		[DataRow("rdfms-syntax-incomplete/error004.rdf", null)]
		[DataRow("rdfms-syntax-incomplete/error005.rdf", null)]
		[DataRow("rdfms-syntax-incomplete/error006.rdf", null)]
		[DataRow("rdfms-uri-substructure/error001.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error001.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error002.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error003.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error004.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error005.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error006.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error007.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error008.rdf", null)]
		[DataRow("rdf-ns-prefix-confusion/error009.rdf", null)]
		[DataRow("xmlbase/error001.rdf", null)]
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
			BaseUri ??= new Uri("http://www.w3.org/2000/10/rdf-tests/rdfcore/" + FileName);
			
			byte[] Data = Resources.LoadResource(typeof(RdfTests).Namespace + ".Data.Rdf." + FileName.Replace('-', '_').Replace('/', '.'));
			object Decoded = await InternetContent.DecodeAsync("application/rdf+xml", Data, BaseUri);
			if (Decoded is not RdfDocument Parsed)
				throw new Exception("Unable to decode RDF document.");

			return Parsed;
		}

		private static async Task<TurtleDocument> LoadTurtleDocument(string FileName, Uri? BaseUri)
		{
			BaseUri ??= new Uri("http://www.w3.org/2000/10/rdf-tests/rdfcore/" + FileName);
			
			byte[] Data = Resources.LoadResource(typeof(TurtleTests).Namespace + ".Data.Rdf." + FileName.Replace('-', '_').Replace('/', '.'));
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