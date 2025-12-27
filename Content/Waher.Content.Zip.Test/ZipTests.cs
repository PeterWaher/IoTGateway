namespace Waher.Content.Zip.Test
{
	[TestClass]
	public sealed class ZipTests
	{
		[TestMethod]
		[DataRow("Data/Test.txt", "Output/UnprotectedText.zip")]
		[DataRow("Data/Test.xml", "Output/UnprotectedXml.zip")]
		public async Task Test_01_CreateUnprotectedZipFile(string SourceFile, 
			string OutputFile)
		{
			await Zip.CreateZipFile(SourceFile, OutputFile, true);
		}

		[TestMethod]
		[DataRow("Data/Test.txt", "Test", "Output/ProtectedText.zip")]
		[DataRow("Data/Test.xml", "Test", "Output/ProtectedXml.zip")]
		public async Task Test_02_CreateProtectedZipFile(string SourceFile, string Password,
			string OutputFile)
		{
			await Zip.CreateZipFile(SourceFile, OutputFile, true, Password);
		}

		[TestMethod]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Output/UnprotectedMultiple.zip")]
		public async Task Test_03_CreateUnprotectedZipFileWithMultipleFiles(
			string[] SourceFiles, string OutputFile)
		{
			await Zip.CreateZipFile(SourceFiles, OutputFile, true);
		}

		[TestMethod]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test", "Output/ProtectedMultiple.zip")]
		public async Task Test_04_CreateProtectedZipFileWithMultipleFiles(
			string[] SourceFiles, string Password, string OutputFile)
		{
			await Zip.CreateZipFile(SourceFiles, OutputFile, true, Password);
		}
	}
}
