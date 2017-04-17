using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Waher.Content.Test
{
	[TestFixture]
    public class JsonTests
    {
		// JSON Parsing diagram: http://json.org/

		[Test]
		public void Test_01_True()
		{
			Assert.AreEqual(true, JSON.Parse("true"));
		}

		[Test]
		public void Test_02_False()
		{
			Assert.AreEqual(false, JSON.Parse("false"));
		}

		[Test]
		public void Test_03_Null()
		{
			Assert.AreEqual(null, JSON.Parse("null"));
		}

		[Test]
		public void Test_04_Number_1()
		{
			Assert.AreEqual(12, JSON.Parse("12"));
		}

		[Test]
		public void Test_05_Number_2()
		{
			Assert.AreEqual(12.3, JSON.Parse("12.3"));
		}

		[Test]
		public void Test_05_Number_3()
		{
			Assert.AreEqual(12e3, JSON.Parse("12e3"));
		}

		[Test]
		public void Test_06_Number_4()
		{
			Assert.AreEqual(12e-3, JSON.Parse("12e-3"));
		}

		[Test]
		public void Test_07_Number_5()
		{
			Assert.AreEqual(12.3E+4, JSON.Parse("12.3E+4"));
		}

		[Test]
		public void Test_08_Number_6()
		{
			Assert.AreEqual(.3, JSON.Parse(".3"));	// Not strictly JSON.
		}

		[Test]
		public void Test_09_String_1()
		{
			Assert.AreEqual("Hello", JSON.Parse("\"Hello\"")); 
		}

		[Test]
		public void Test_10_String_2()
		{
			Assert.AreEqual("He\"llo", JSON.Parse("\"He\\\"llo\""));
		}

		[Test]
		public void Test_11_String_3()
		{
			Assert.AreEqual("He\\llo", JSON.Parse("\"He\\\\llo\""));
		}

		[Test]
		public void Test_12_String_4()
		{
			Assert.AreEqual("He/llo", JSON.Parse("\"He\\/llo\""));
		}

		[Test]
		public void Test_13_String_5()
		{
			Assert.AreEqual("He\bllo", JSON.Parse("\"He\\bllo\""));
		}

		[Test]
		public void Test_14_String_6()
		{
			Assert.AreEqual("He\fllo", JSON.Parse("\"He\\fllo\""));
		}

		[Test]
		public void Test_15_String_7()
		{
			Assert.AreEqual("He\nllo", JSON.Parse("\"He\\nllo\""));
		}

		[Test]
		public void Test_16_String_8()
		{
			Assert.AreEqual("He\rllo", JSON.Parse("\"He\\rllo\""));
		}

		[Test]
		public void Test_17_String_9()
		{
			Assert.AreEqual("He\tllo", JSON.Parse("\"He\\tllo\""));
		}

		[Test]
		public void Test_18_String_10()
		{
			Assert.AreEqual("He\u1234llo", JSON.Parse("\"He\\u1234llo\""));
		}

		[Test]
		public void Test_19_ParseObject()
		{
			string Json = "{\n  \"success\": true,\n  \"challenge_ts\": \"2017-04-17T06:41:36Z\",\n  \"hostname\": \"localhost\"\n}";

			Dictionary<string, object> Obj = (Dictionary<string, object>)JSON.Parse(Json);

			Assert.AreEqual(3, Obj.Count);
			Assert.AreEqual(true, Obj["success"]);
			Assert.AreEqual("2017-04-17T06:41:36Z", Obj["challenge_ts"]);
			Assert.AreEqual("localhost", Obj["hostname"]);
		}

		[Test]
		public void Test_20_ParseArray()
		{
			string Json = "[1, \"Hello\", {\"a\":1,\"b\":2}]";

			object[] A = (object[])JSON.Parse(Json);

			Assert.AreEqual(3, A.Length);
			Assert.AreEqual(1, A[0]);
			Assert.AreEqual("Hello", A[1]);

			Dictionary<string, object> Obj = (Dictionary<string, object>)A[2];

			Assert.AreEqual(2, Obj.Count);
			Assert.AreEqual(1, Obj["a"]);
			Assert.AreEqual(2, Obj["b"]);
		}
	}
}
