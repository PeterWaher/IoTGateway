using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence;
using Waher.Persistence.FullTextSearch;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptFullTextSearchTests
	{
		[ClassInitialize]
		public static async Task Initialize(TestContext _)
		{
			await Database.Clear("Test");
			await (await Database.GetDictionary("TestIndex")).ClearAsync();

			await Database.Clear("FullTextSearchObjects");
			await Database.Clear("FullTextSearchFiles");
			await (await Database.GetDictionary("FullTextSearchCollections")).ClearAsync();
		}

		[TestMethod]
		public async Task Test_01_FtsCollection()
		{
			Assert.AreEqual(true, await this.Test("FtsCollection('TestIndex','Test')"));
		}

		[TestMethod]
		public async Task Test_02_AddFtsProperties_1()
		{
			Assert.AreEqual(true, await this.Test("AddFtsProperties('Test',['P1','P2','P3'])"));
		}

		[TestMethod]
		public async Task Test_03_AddFtsProperties_2()
		{
			Assert.AreEqual(false, await this.Test("AddFtsProperties('Test',['P1','P2','P3'])"));
		}

		[TestMethod]
		public async Task Test_04_RemoveFtsProperties_1()
		{
			Assert.AreEqual(true, await this.Test("RemoveFtsProperties('Test',['P3'])"));
		}

		[TestMethod]
		public async Task Test_05_RemoveFtsProperties_2()
		{
			Assert.AreEqual(false, await this.Test("RemoveFtsProperties('Test',['P3'])"));
		}

		[TestMethod]
		public async Task Test_06_GetFtsProperties_1()
		{
			object Obj = await this.Test("GetFtsProperties('Test')");
			PropertyDefinition Def;
			Array Properties;

			Assert.IsNotNull(Properties = Obj as Array);
			Assert.AreEqual(2, Properties.Length);

			Assert.IsNotNull(Def = Properties.GetValue(0) as PropertyDefinition);
			Assert.AreEqual("P1", Def.Definition);
			Assert.AreEqual(null, Def.ExternalSource);
			Assert.AreEqual(PropertyType.Label, Def.Type);

			Assert.IsNotNull(Def = Properties.GetValue(1) as PropertyDefinition);
			Assert.AreEqual("P2", Def.Definition);
			Assert.AreEqual(null, Def.ExternalSource);
			Assert.AreEqual(PropertyType.Label, Def.Type);
		}

		[TestMethod]
		public async Task Test_07_GetFtsProperties_2()
		{
			object Obj = await this.Test("GetFtsProperties()");
			Dictionary<string, IElement> ByCollection;
			PropertyDefinition Def;
			Array Properties;

			Assert.IsNotNull(ByCollection = Obj as Dictionary<string, IElement>);
			Assert.AreEqual(1, ByCollection.Count);

			Assert.IsTrue(ByCollection.TryGetValue("Test", out IElement E));

			Assert.IsNotNull(Properties = E.AssociatedObjectValue as Array);
			Assert.AreEqual(2, Properties.Length);

			Assert.IsNotNull(Def = Properties.GetValue(0) as PropertyDefinition);
			Assert.AreEqual("P1", Def.Definition);
			Assert.AreEqual(null, Def.ExternalSource);
			Assert.AreEqual(PropertyType.Label, Def.Type);

			Assert.IsNotNull(Def = Properties.GetValue(1) as PropertyDefinition);
			Assert.AreEqual("P2", Def.Definition);
			Assert.AreEqual(null, Def.ExternalSource);
			Assert.AreEqual(PropertyType.Label, Def.Type);
		}

		[TestMethod]
		public async Task Test_08_GetFtsCollections_1()
		{
			object Obj = await this.Test("GetFtsCollections('TestIndex')");
			Array Collections;

			Assert.IsNotNull(Collections = Obj as Array);
			Assert.AreEqual(1, Collections.Length);
			Assert.AreEqual("Test", Collections.GetValue(0));
		}

		[TestMethod]
		public async Task Test_09_GetFtsCollections_2()
		{
			object Obj = await this.Test("GetFtsCollections()");
			Dictionary<string, IElement> ByCollection;
			Array Collections;

			Assert.IsNotNull(ByCollection = Obj as Dictionary<string, IElement>);
			Assert.AreEqual(1, ByCollection.Count);

			Assert.IsTrue(ByCollection.TryGetValue("TestIndex", out IElement E));

			Assert.IsNotNull(Collections = E.AssociatedObjectValue as Array);
			Assert.AreEqual(1, Collections.Length);
			Assert.AreEqual("Test", Collections.GetValue(0));
		}

		[TestMethod]
		public async Task Test_10_Insert_Search_1()
		{
			object Obj = await this.Test("insert into Test object {'P1':'Hello World','P2':'Kilroy was here.','P3':'Fitzroy also'};Sleep(1000);Search('TestIndex','+Kilroy -Fitzroy',false)");
			Array ResultSet;

			Assert.IsNotNull(ResultSet = Obj as Array);
			Assert.IsTrue(ResultSet.Length > 0);
		}

		[TestMethod]
		public async Task Test_11_Reindex_Search_1()
		{
			object Obj = await this.Test("insert into Test object {'P1':'Hello World','P2':'Kilroy was here.','P3':'Fitzroy also'};Sleep(1000);ReindexFts('TestIndex');Sleep(1000);Search('TestIndex','+Kilroy -Fitzroy',false)");
			Array ResultSet;

			Assert.IsNotNull(ResultSet = Obj as Array);
			Assert.IsTrue(ResultSet.Length > 0);
		}

		[TestMethod]
		public async Task Test_12_FtsFolder_Search_1()
		{
			object Obj = await this.Test("FtsFolder('TestIndex','Files',true);Sleep(1000);Search('TestIndex','+Kilroy -Fitzroy',false)");
			Array ResultSet;

			Assert.IsNotNull(ResultSet = Obj as Array);
			Assert.IsTrue(ResultSet.Length >= 10, "Only " + ResultSet.Length.ToString() + " objects found. Expected at least 10.");
		}

		[TestMethod]
		public async Task Test_13_FtsFile_Search_1()
		{
			object Obj = await this.Test("Search('TestIndex','+Kilroy -Fitzroy',false)");
			Array ResultSet;

			Assert.IsNotNull(ResultSet = Obj as Array);
			int c = ResultSet.Length;

			Obj = await this.Test("FtsFile('TestIndex','File/1.txt');Sleep(1000);Search('TestIndex','+Kilroy -Fitzroy',false)");
			Assert.IsNotNull(ResultSet = Obj as Array);

			Assert.AreEqual(c + 1, ResultSet.Length);
		}

		private async Task<object> Test(string Script)
		{
			Variables v = new Variables();
			Expression Exp = new Expression(Script);
			object Obj = await Exp.EvaluateAsync(v);

			return Obj;
		}

		/*
		ExpressionEvaluator
		LambdaEvaluator
		 */
	}
}
