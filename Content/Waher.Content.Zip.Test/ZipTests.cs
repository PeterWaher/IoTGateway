namespace Waher.Content.Zip.Test
{
	[TestClass]
	public sealed class ZipTests
	{
		[TestMethod]
		[DataRow("Data/Test.txt", "Test", "Output/TestText.zip")]
		[DataRow("Data/Test.xml", "Test", "Output/TestXml.zip")]
		public async Task Test_01_CreateProtectedZipFile(string SourceFile, string Password,
			string outputFile)
		{
			await Zip.CreateProtectedZipFile(SourceFile, outputFile, true, Password);
		}
	}
}
