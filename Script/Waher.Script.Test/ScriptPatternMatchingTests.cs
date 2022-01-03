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
			await ScriptEvaluationTests.Test("{'a':A,'b':B,'c':Required(C),'d':Optional(D)}:={'a':s,'c':true,'b':b};[A,B,C,D]", 
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
				"									<Connect>\r\n" +
				"										<ApplicationName><[Required(Str(ApplicationName))]></ApplicationName>\r\n" +
				"										<Purpose><[Required(Str(Purpose))]></Purpose>\r\n" +
				"										<ClientSideToken><[Optional(Str(ClientSideToken))]></ClientSideToken>\r\n" +
				"									</Connect>:=Posted;\r\n";

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

	}
}