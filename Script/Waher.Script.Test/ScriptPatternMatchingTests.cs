using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Exceptions;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptPatternMatchingTests
	{
		[TestMethod]
		public async Task Test_01_SimpleAssignments()
		{
			await ScriptEvaluationTests.Test("[A,B,C]:=[a,b,c];[A,B,C]", new object[] {
				ScriptEvaluationTests.a, ScriptEvaluationTests.b, ScriptEvaluationTests.c });

			await ScriptEvaluationTests.Test("[[A,B],[B,C]]:=[[a,b],[b,c]];[A,B,C]", new object[] {
				ScriptEvaluationTests.a, ScriptEvaluationTests.b, ScriptEvaluationTests.c });
		}

		[TestMethod]
		[ExpectedException(typeof(ScriptRuntimeException))]
		public async Task Test_02_Mismatch()
		{
			await ScriptEvaluationTests.Test("[[A,B],[B,C]]:=[[a,b],[c,b]]", new object[] {
				ScriptEvaluationTests.a, ScriptEvaluationTests.b, ScriptEvaluationTests.c });
		}

		[TestMethod]
		public async Task Test_03_RangeChecks()
		{
			await ScriptEvaluationTests.Test("x>5:=b", 6);
			await ScriptEvaluationTests.Test("x>=5:=b", 6);
			await ScriptEvaluationTests.Test("7>x:=b", 6);
			await ScriptEvaluationTests.Test("7>=x:=b", 6);

			await ScriptEvaluationTests.Test("x<7:=b", 6);
			await ScriptEvaluationTests.Test("x<=7:=b", 6);
			await ScriptEvaluationTests.Test("5<x:=b", 6);
			await ScriptEvaluationTests.Test("5<=x:=b", 6);

			await ScriptEvaluationTests.Test("5<x<7:=b", 6);
			await ScriptEvaluationTests.Test("5<=x<7:=b", 6);
			await ScriptEvaluationTests.Test("5<x<=7:=b", 6);
			await ScriptEvaluationTests.Test("5<=x<=7:=b", 6);
		}

		[TestMethod]
		public async Task Test_04_JSON()
		{
			await ScriptEvaluationTests.Test("{'a':A,'b':B,'c':Required(C),'d':Optional(D)}:=" +
				"{'a':s,'c':true,'b':b};[A,B,C,D]",
				new object[] { ScriptEvaluationTests.s, ScriptEvaluationTests.b, true, null });
		}

		[TestMethod]
		public async Task Test_05_XML_1()
		{
			await ScriptEvaluationTests.Test("<test><a><[A]></a><b x=Double(B) y=Required(Double(C)) z=Optional(D)/></test>:=<test><a><[s]></a><b y=c x=b/></test>;[A,B,C,D]",
				new object[] { ScriptEvaluationTests.s, ScriptEvaluationTests.b, ScriptEvaluationTests.c, null });
		}

		[TestMethod]
		public async Task Test_06_XML_2()
		{
			string s =
				"Posted:=<Connect>\r\n" +
				"    <ApplicationName>Postman</ApplicationName>\r\n" +
				"    <Purpose>Testing API</Purpose>\r\n" +
				"    <ClientSideToken>Testing</ClientSideToken>\r\n" +
				"</Connect>;\r\n";

			s +=
				"<Connect>\r\n" +
				"	<ApplicationName><[Required(Str(ApplicationName))]></ApplicationName>\r\n" +
				"	<Purpose><[Required(Str(Purpose))]></Purpose>\r\n" +
				"	<ClientSideToken><[Optional(Str(ClientSideToken))]></ClientSideToken>\r\n" +
				"</Connect>:=Posted;\r\n";

			s += "[ApplicationName,Purpose,ClientSideToken]";

			await ScriptEvaluationTests.Test(s, new object[] { "Postman", "Testing API", "Testing" });
		}

		[TestMethod]
		public async Task Test_07_XML_3()
		{
			string s =
				"Posted:=<Refresh>\r\n" +
				"    <Purpose>Testing API again</Purpose>\r\n" +
				"</Refresh>;\r\n";

			s +=
				"<Refresh>\r\n" +
				"	<ApplicationName><[Optional(Str(ApplicationName))]></ApplicationName>\r\n" +
				"	<Purpose><[Optional(Str(Purpose))]></Purpose>\r\n" +
				"	<ClientSideToken><[Optional(Str(ClientSideToken))]></ClientSideToken>\r\n" +
				"</Refresh>:=Posted;\r\n";

			s += "[ApplicationName,Purpose,ClientSideToken]";

			await ScriptEvaluationTests.Test(s, new object[] { null, "Testing API again", null });
		}

		[TestMethod]
		public async Task Test_08_XML_4()
		{
			string s =
				"Posted:=<Refresh><Purpose>Testing API again</Purpose></Refresh>;\r\n";

			s +=
				"<Refresh>\r\n" +
				"	<ApplicationName><[Optional(Str(ApplicationName))]></ApplicationName>\r\n" +
				"	<Purpose><[Optional(Str(Purpose))]></Purpose>\r\n" +
				"	<ClientSideToken><[Optional(Str(ClientSideToken))]></ClientSideToken>\r\n" +
				"</Refresh>:=Posted;\r\n";

			s += "[ApplicationName,Purpose,ClientSideToken]";

			await ScriptEvaluationTests.Test(s, new object[] { null, "Testing API again", null });
		}

		[TestMethod]
		public async Task Test_09_XML_5()
		{
			string s =
				"Posted:=<Refresh>\r\n" +
				"    <Purpose>Testing API again</Purpose>\r\n" +
				"</Refresh>;\r\n";

			s +=
				"<Refresh>" +
				"<ApplicationName><[Optional(Str(ApplicationName))]></ApplicationName>" +
				"<Purpose><[Optional(Str(Purpose))]></Purpose>" +
				"<ClientSideToken><[Optional(Str(ClientSideToken))]></ClientSideToken>" +
				"</Refresh>:=Posted;";

			s += "[ApplicationName,Purpose,ClientSideToken]";

			await ScriptEvaluationTests.Test(s, new object[] { null, "Testing API again", null });
		}

		[TestMethod]
		public async Task Test_10_XML_6()
		{
			string s = "<a b=Int(B) *><b/><*></a>:=" +
				"<a b='10' c='20' d='30'><b/><c/><d/></a>;B";

			await ScriptEvaluationTests.Test(s, 10);
		}

		[TestMethod]
		public async Task Test_11_XML_7()
		{
			string s = "<a b=Int(B) *><*><c/><*></a>:=" +
				"<a c='20' d='30' b='10'><b/><d/><c/></a>;B";

			await ScriptEvaluationTests.Test(s, 10);
		}

		[TestMethod]
		public async Task Test_12_XML_8()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>" +
				"<Profession>Bus Driver</Profession>" +
				"<EmployedSince>2010-01-02</EmployedSince>" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) age=Optional(Int(Age))>" +
				"<Profession><[Optional(Str(Profession))]></Profession>" +
				"<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>" +
			"</Person>:=Xml;[Name,Age,Profession,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", 50, "Bus Driver", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_13_XML_9()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>" +
				"<Profession>Bus Driver</Profession>" +
				"<EmployedSince>2010-01-02</EmployedSince>" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) *>" +
				"<*>" +
				"<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>" +
				"<*>" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_14_XML_10()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>" +
				"<Profession>Bus Driver</Profession>" +
				"<EmployedSince>2010-01-02</EmployedSince>" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) * *>" +
				"<*>" +
				"<*>" +
				"<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>" +
				"<*>" +
				"<*>" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_15_JSON_2()
		{
			await ScriptEvaluationTests.Test("{*,'a':A,'b':B,'c':Required(C),'d':Optional(D)}:=" +
				"{'a':s,'c':true,'b':b,'e':Now};[A,B,C,D]",
				new object[] { ScriptEvaluationTests.s, ScriptEvaluationTests.b, true, null });
		}

		[TestMethod]
		public async Task Test_16_JSON_3()
		{
			await ScriptEvaluationTests.Test("{'a':A,'b':B,'c':Required(C),'d':Optional(D),*}:=" +
				"{'a':s,'c':true,'b':b,'e':Now};[A,B,C,D]",
				new object[] { ScriptEvaluationTests.s, ScriptEvaluationTests.b, true, null });
		}

		[TestMethod]
		public async Task Test_17_JSON_4()
		{
			await ScriptEvaluationTests.Test("{'a':A,'b':B,*,'c':Required(C),'d':Optional(D),*}:=" +
				"{'a':s,'c':true,'b':b,'e':Now};[A,B,C,D]",
				new object[] { ScriptEvaluationTests.s, ScriptEvaluationTests.b, true, null });
		}

		[TestMethod]
		public async Task Test_18_XML_11()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>\r\n" +
				"\t<Profession>Bus Driver</Profession>\r\n" +
				"\t<EmployedSince>2010-01-02</EmployedSince>\r\n" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) *>\r\n" +
				"\t<*>\r\n" +
				"\t<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>\r\n" +
				"\t<*>\r\n" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_19_XML_12()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>\r\n" +
				"\t<Profession>Bus Driver</Profession>\r\n" +
				"\t<EmployedSince>2010-01-02</EmployedSince>\r\n" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) * *>\r\n" +
				"\t<*>\r\n" +
				"\t<*>\r\n" +
				"\t<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>\r\n" +
				"\t<*>\r\n" +
				"\t<*>\r\n" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_20_XML_13()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>" +
				"<Profession>Bus Driver</Profession>" +
				"<EmployedSince>2010-01-02</EmployedSince>" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) *>\r\n" +
				"\t<*>\r\n" +
				"\t<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>\r\n" +
				"\t<*>\r\n" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_21_XML_14()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>" +
				"<Profession>Bus Driver</Profession>" +
				"<EmployedSince>2010-01-02</EmployedSince>" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) * *>\r\n" +
				"\t<*>\r\n" +
				"\t<*>\r\n" +
				"\t<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>\r\n" +
				"\t<*>\r\n" +
				"\t<*>\r\n" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_22_XML_15()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>\r\n" +
				"\t<Profession>Bus Driver</Profession>\r\n" +
				"\t<EmployedSince>2010-01-02</EmployedSince>\r\n" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) *>" +
				"<*>" +
				"<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>" +
				"<*>" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

		[TestMethod]
		public async Task Test_23_XML_16()
		{
			string s = "Xml:=<Person name='Kalle' age='50'>\r\n" +
				"\t<Profession>Bus Driver</Profession>\r\n" +
				"\t<EmployedSince>2010-01-02</EmployedSince>\r\n" +
			"</Person>;";

			s += "<Person name=Required(Str(Name)) * *>" +
				"<*>" +
				"<*>" +
				"<EmployedSince><[Required(DateTime(EmployedSince))]></EmployedSince>" +
				"<*>" +
				"<*>" +
			"</Person>:=Xml;[Name,EmployedSince]";

			await ScriptEvaluationTests.Test(s, new object[] { "Kalle", new DateTime(2010, 1, 2) });
		}

	}
}