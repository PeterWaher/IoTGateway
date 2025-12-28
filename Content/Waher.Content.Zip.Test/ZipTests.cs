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
		[DataRow("Data/Test.txt", "Test02", ZipEncryption.ZipCrypto, "Output/Test_02_ZipCryptoText.zip")]
		[DataRow("Data/Test.xml", "Test02", ZipEncryption.ZipCrypto, "Output/Test_02_ZipCryptoXml.zip")]
		[DataRow("Data/Test.txt", "Test02", ZipEncryption.Aes128Ae1, "Output/Test_02_Aes128Ae1Text.zip")]
		[DataRow("Data/Test.xml", "Test02", ZipEncryption.Aes128Ae1, "Output/Test_02_Aes128Ae1Xml.zip")]
		[DataRow("Data/Test.txt", "Test02", ZipEncryption.Aes192Ae1, "Output/Test_02_Aes192Ae1Text.zip")]
		[DataRow("Data/Test.xml", "Test02", ZipEncryption.Aes192Ae1, "Output/Test_02_Aes192Ae1Xml.zip")]
		[DataRow("Data/Test.txt", "Test02", ZipEncryption.Aes256Ae1, "Output/Test_02_Aes256Ae1Text.zip")]
		[DataRow("Data/Test.xml", "Test02", ZipEncryption.Aes256Ae1, "Output/Test_02_Aes256Ae1Xml.zip")]
		[DataRow("Data/Test.txt", "Test02", ZipEncryption.Aes128Ae2, "Output/Test_02_Aes128Ae2Text.zip")]
		[DataRow("Data/Test.xml", "Test02", ZipEncryption.Aes128Ae2, "Output/Test_02_Aes128Ae2Xml.zip")]
		[DataRow("Data/Test.txt", "Test02", ZipEncryption.Aes192Ae2, "Output/Test_02_Aes192Ae2Text.zip")]
		[DataRow("Data/Test.xml", "Test02", ZipEncryption.Aes192Ae2, "Output/Test_02_Aes192Ae2Xml.zip")]
		[DataRow("Data/Test.txt", "Test02", ZipEncryption.Aes256Ae2, "Output/Test_02_Aes256Ae2Text.zip")]
		[DataRow("Data/Test.xml", "Test02", ZipEncryption.Aes256Ae2, "Output/Test_02_Aes256Ae2Xml.zip")]
		public async Task Test_02_CreateProtectedZipFile(string SourceFile, string Password,
			ZipEncryption Method, string OutputFile)
		{
			await Zip.CreateZipFile(SourceFile, OutputFile, true, Password, Method);
		}

		[TestMethod]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Output/Test_03_UnprotectedMultiple.zip")]
		public async Task Test_03_CreateUnprotectedZipFileWithMultipleFiles(
			string[] SourceFiles, string OutputFile)
		{
			await Zip.CreateZipFile(SourceFiles, OutputFile, true);
		}

		[TestMethod]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", ZipEncryption.ZipCrypto, "Output/Test_04_ZipCryptoMultiple.zip")]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", ZipEncryption.Aes128Ae1, "Output/Test_04_Aes128Ae1Multiple.zip")]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", ZipEncryption.Aes192Ae1, "Output/Test_04_Aes192Ae1Multiple.zip")]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", ZipEncryption.Aes256Ae1, "Output/Test_04_Aes256Ae1Multiple.zip")]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", ZipEncryption.Aes128Ae2, "Output/Test_04_Aes128Ae2Multiple.zip")]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", ZipEncryption.Aes192Ae2, "Output/Test_04_Aes192Ae2Multiple.zip")]
		[DataRow(new string[] { "Data/Test.txt", "Data/Test.xml" }, "Test04", ZipEncryption.Aes256Ae2, "Output/Test_04_Aes256Ae2Multiple.zip")]
		public async Task Test_04_CreateProtectedZipFileWithMultipleFiles(
			string[] SourceFiles, string Password, ZipEncryption Method, string OutputFile)
		{
			await Zip.CreateZipFile(SourceFiles, OutputFile, true, Password, Method);
		}
	}
}
