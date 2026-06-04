using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.Test
{
	[TestClass]
	public sealed class ToonTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(
				typeof(TOON).Assembly,
				typeof(JSON).Assembly
			);
		}

		/// <summary>
		/// Unit tests for encoding Toon content. Tests from:
		/// https://github.com/toon-format/spec/tree/main/tests/fixtures/encode
		/// </summary>
		/// <param name="JsonTestsFileName">File name of JSON file containing tests.</param>
		[TestMethod]
		[DataRow("Data/Encode/primitives.json")]
		[DataRow("Data/Encode/whitespace.json")]
		//[DataRow("Data/Encode/objects.json")]
		//[DataRow("Data/Encode/delimiters.json")]
		//[DataRow("Data/Encode/key-folding.json")]
		//[DataRow("Data/Encode/arrays-primitive.json")]
		//[DataRow("Data/Encode/arrays-tabular.json")]
		//[DataRow("Data/Encode/arrays-objects.json")]
		//[DataRow("Data/Encode/arrays-nested.json")]
		public void Test_01_Encode(string JsonTestsFileName)
		{
			string Json = File.ReadAllText(JsonTestsFileName);
			Dictionary<string, object>? Decoded = JSON.Parse(Json) as Dictionary<string, object>;
			Assert.IsNotNull(Decoded);

			Assert.IsTrue(Decoded.TryGetValue("tests", out object? Tests));
			Assert.IsNotNull(Tests);
			Assert.IsTrue(Tests is IEnumerable<object>);

			foreach (object Test in (IEnumerable<object>)Tests)
			{
				Assert.IsTrue(Test is Dictionary<string, object>);
				Dictionary<string, object> TestDict = (Dictionary<string, object>)Test;

				Assert.IsTrue(TestDict.TryGetValue("name", out object? Name));
				Assert.IsNotNull(Name);
				Assert.IsTrue(Name is string);

				Assert.IsTrue(TestDict.TryGetValue("input", out object? Input));
				Assert.IsNotNull(Input);

				Assert.IsTrue(TestDict.TryGetValue("expected", out object? Expected));
				Assert.IsNotNull(Expected);
				Assert.IsTrue(Expected is string);

				string Output = TOON.Encode(Input, true);

				Assert.AreEqual(Expected, Output, "Test: " + JsonTestsFileName + ", " + Name);
			}
		}
	}
}
