namespace Waher.Content.Zip.Test
{
	[TestClass]
	public sealed class ZipTests
	{
		[TestMethod]
		[DataRow("Data/Test.txt", "Output/Test_01_UnprotectedText.zip")]
		[DataRow("Data/Test.xml", "Output/Test_01_UnprotectedXml.zip")]
		public async Task Test_01_CreateUnprotectedZipFile(string SourceFile, 
			string OutputFile)
		{
			await Zip.CreateZipFile(SourceFile, OutputFile, true);
		}

		[TestMethod]
		[DataRow("Data/Test.txt", "Test02", "Output/Test_02_ProtectedText.zip")]
		[DataRow("Data/Test.xml", "Test02", "Output/Test_02_ProtectedXml.zip")]
		public async Task Test_02_CreateProtectedZipFile(string SourceFile, string Password,
			string OutputFile)
		{
			await Zip.CreateZipFile(SourceFile, OutputFile, true, Password);
		}

		[TestMethod]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Output/Test_03_UnprotectedMultiple.zip")]
		public async Task Test_03_CreateUnprotectedZipFileWithMultipleFiles(
			string[] SourceFiles, string OutputFile)
		{
			await Zip.CreateZipFile(SourceFiles, OutputFile, true);
		}

		[TestMethod]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", "Output/Test_04_ProtectedMultiple.zip")]
		public async Task Test_04_CreateProtectedZipFileWithMultipleFiles(
			string[] SourceFiles, string Password, string OutputFile)
		{
			await Zip.CreateZipFile(SourceFiles, OutputFile, true, Password);
		}
	}
}
