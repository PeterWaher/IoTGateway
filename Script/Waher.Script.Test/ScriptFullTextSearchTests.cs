using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence;
using Waher.Persistence.FullTextSearch;

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
			PropertyDefinition[] Properties;

			Assert.IsNotNull(Properties = Obj as PropertyDefinition[]);
			Assert.AreEqual(2, Properties.Length);

			Assert.AreEqual("P1", Properties[0].Definition);
			Assert.AreEqual(null, Properties[0].ExternalSource);
			Assert.AreEqual(PropertyType.Label, Properties[0].Type);

			Assert.AreEqual("P2", Properties[1].Definition);
			Assert.AreEqual(null, Properties[1].ExternalSource);
			Assert.AreEqual(PropertyType.Label, Properties[1].Type);
		}

		[TestMethod]
		public async Task Test_07_GetFtsProperties_2()
		{
			object Obj = await this.Test("GetFtsProperties()");
			Dictionary<string, object> ByCollection;
			PropertyDefinition[] Properties;

			Assert.IsNotNull(ByCollection = Obj as Dictionary<string, object>);
			Assert.AreEqual(1, ByCollection.Count);

			Assert.IsTrue(ByCollection.TryGetValue("Test", out Obj));

			Assert.IsNotNull(Properties = Obj as PropertyDefinition[]);
			Assert.AreEqual(2, Properties.Length);

			Assert.AreEqual("P1", Properties[0].Definition);
			Assert.AreEqual(null, Properties[0].ExternalSource);
			Assert.AreEqual(PropertyType.Label, Properties[0].Type);

			Assert.AreEqual("P2", Properties[1].Definition);
			Assert.AreEqual(null, Properties[1].ExternalSource);
			Assert.AreEqual(PropertyType.Label, Properties[1].Type);
		}

		[TestMethod]
		public async Task Test_08_GetFtsCollections_1()
		{
			object Obj = await this.Test("GetFtsCollections('TestIndex')");
			string[] Collections;

			Assert.IsNotNull(Collections = Obj as string[]);
			Assert.AreEqual(1, Collections.Length);
			Assert.AreEqual("Test", Collections[0]);
		}

		[TestMethod]
		public async Task Test_09_GetFtsCollections_2()
		{
			object Obj = await this.Test("GetFtsCollections()");
			Dictionary<string, object> ByCollection;
			string[] Collections;

			Assert.IsNotNull(ByCollection = Obj as Dictionary<string, object>);
			Assert.AreEqual(1, ByCollection.Count);

			Assert.IsTrue(ByCollection.TryGetValue("TestIndex", out Obj));

			Assert.IsNotNull(Collections = Obj as string[]);
			Assert.AreEqual(1, Collections.Length);
			Assert.AreEqual("Test", Collections[0]);
		}

		private async Task<object> Test(string Script)
		{
			Variables v = new Variables();
			Expression Exp = new Expression(Script);
			object Obj = await Exp.EvaluateAsync(v);

			return Obj;
		}

		/*
| `FtsFile(Index,FileName)`                                                        | Indexes (or reindexes) a specific file, using the full-text-search collection index defined by `Index`. If the file does not exist, it is removed from the index. | `FtsFolder("FTS",Folder,true)` |
| `FtsFolder(Index,Folder[,Recursive])`                                            | Indexes files in a folder given by `Folder`, using the full-text-search collection index defined by `Index`. Files can be processed recursively in subfolders if `Recursive` is `true` (default is `false`). To keep folder updated, call `FtsFile` when a file is modified, created or deleted. `FtsFolder` only updates files who have not been indexed before, or whose timestamps have changed since last indexation. | `FtsFolder("FTS",Folder,true)` |
| `ReindexFts(Index)                                                               | Reindexes the full-text-search index defined by `Index`. This process may take some time, as all objects in the corresponding collections will be iterated and reindexed. | `ReindexFts("FTS")` |
| `Search(Index,Query,Strict[,Offset,MaxCount[,Order[,Type,PaginationStrategy]]])` | Performs a full-text-search of the query `Query` in the full-text-search index `Index`. `Strict` controls if keywords are as-is (`true`) or prefixes (`false`). Pagination is controlled by `Offset` abd `MaxCount`. The sort order is defined by `Order`. Types searches can be performed, controlled by the optional arguments `Type` and `PaginationStrategy`. | `Search("FTS","Kilroy was here",0,25,"Relevance")` |

		ExpressionEvaluator
		LambdaEvaluator
		 */
	}
}
