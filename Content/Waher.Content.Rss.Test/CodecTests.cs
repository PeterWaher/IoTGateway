using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;

namespace Waher.Content.Rss.Test
{
	[TestClass]
	public class CodecTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(InternetContent).Assembly,
				typeof(RssDocument).Assembly,
				typeof(XML).Assembly,
				typeof(CodecTests).Assembly);
		}

		[DataTestMethod]
		[DataRow("sample-rss-091.rss")]
		[DataRow("sample-rss-092.rss")]
		[DataRow("sample-rss-2.rss")]
		public void Test_01_CanDecode(string FileName)
		{
			Assert.IsTrue(InternetContent.TryGetContentType(Path.GetExtension(FileName), out string ContentType));
			Assert.AreEqual("application/rss+xml", ContentType);
			Assert.IsTrue(InternetContent.TryGetFileExtension(ContentType, out string FileExtension));
			Assert.AreEqual("rss", FileExtension);
			Assert.IsTrue(InternetContent.Decodes(ContentType, out Grade Grade, out IContentDecoder Decoder));
			Assert.IsTrue(Grade > Grade.NotAtAll);
			Assert.IsTrue(Decoder is RssCodec);
		}

		[DataTestMethod]
		[DataRow("sample-rss-091.rss", 0.91)]
		[DataRow("sample-rss-092.rss", 0.92)]
		[DataRow("sample-rss-2.rss", 2.0)]
		public async Task Test_02_Decode(string FileName, double Version)
		{
			Assert.IsTrue(InternetContent.TryGetContentType(Path.GetExtension(FileName), out string ContentType));
			byte[] Data = await File.ReadAllBytesAsync(Path.Combine(Environment.CurrentDirectory, "Data", FileName));

			object Decoded = await InternetContent.DecodeAsync(ContentType, Data, null);
			Assert.IsNotNull(Decoded);

			RssDocument? Doc = Decoded as RssDocument;
			Assert.IsNotNull(Doc);

			Assert.AreEqual(Version, Doc.Version);
			Assert.IsNotNull(Doc.Channels);
			Assert.IsTrue(Doc.Channels.Length > 0);

			foreach (RssWarning Warning in Doc.Warnings)
				ConsoleOut.WriteLine(Warning.Message);

			foreach (RssChannel Channel in Doc.Channels)
			{
				Assert.IsNotNull(Channel.Items);
				Assert.IsTrue(Channel.Items.Length > 0);
			}

			Assert.AreEqual(0, Doc.Warnings.Length);
		}

		[DataTestMethod]
		[DataRow("sample-rss-091.rss")]
		[DataRow("sample-rss-092.rss")]
		[DataRow("sample-rss-2.rss")]
		public void Test_03_CanEncode(string FileName)
		{
			XmlDocument Xml = new()
			{
				PreserveWhitespace = true
			};
			Xml.Load(Path.Combine(Environment.CurrentDirectory, "Data", FileName));
			RssDocument Doc = new(Xml, null);

			Assert.IsTrue(InternetContent.Encodes(Doc, out Grade Grade, out IContentEncoder Encoder));
			Assert.IsTrue(Grade > Grade.NotAtAll);
			Assert.IsTrue(Encoder is RssCodec);
		}

		[DataTestMethod]
		[DataRow("sample-rss-091.rss")]
		[DataRow("sample-rss-092.rss")]
		[DataRow("sample-rss-2.rss")]
		public async Task Test_04_Encode(string FileName)
		{
			XmlDocument Xml = new()
			{
				PreserveWhitespace = true
			};
			Xml.Load(Path.Combine(Environment.CurrentDirectory, "Data", FileName));
			RssDocument Doc = new(Xml, null);

			ContentResponse P = await InternetContent.EncodeAsync(Doc, Encoding.UTF8);
			P.AssertOk();
			byte[] Data = P.Encoded;
			string ContentType = P.ContentType;

			Assert.IsNotNull(Data);
			Assert.AreEqual("application/rss+xml; charset=utf-8", ContentType);
		}
	}
}