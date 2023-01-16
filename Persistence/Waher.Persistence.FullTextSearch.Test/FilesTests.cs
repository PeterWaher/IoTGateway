using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Files;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class FilesTests
	{
		[TestMethod]
		public async Task Test_01_IndexFolder()
		{
			FolderIndexationStatistics Result = await Search.IndexFolder("FTS_Files", "Files", true);

			Console.Out.WriteLine("NrAdded: " + Result.NrAdded.ToString());
			Console.Out.WriteLine("NrUpdated: " + Result.NrUpdated.ToString());
			Console.Out.WriteLine("NrDeleted: " + Result.NrDeleted.ToString());
			Console.Out.WriteLine("NrFiles: " + Result.NrFiles.ToString());
			Console.Out.WriteLine("TotalChanges: " + Result.TotalChanges.ToString());

			Assert.AreEqual(Result.TotalChanges, Result.NrAdded + Result.NrUpdated + Result.NrDeleted);
		}

		[TestMethod]
		public async Task Test_02_IndexFile()
		{
			bool Result = await Search.IndexFile("FTS_Files", "Files/1/1.txt");

			Console.Out.WriteLine(Result.ToString());
		}

	}
}