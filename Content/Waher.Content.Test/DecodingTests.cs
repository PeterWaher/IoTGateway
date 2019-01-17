using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Content.Multipart;
using Waher.Content.Xml;
using Waher.Content.Html;
using Waher.Content.Images;

namespace Waher.Content.Test
{
	[TestClass]
    public class DecodingTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Runtime.Inventory.Types.Initialize(
				typeof(CommonTypes).Assembly,
				typeof(HtmlDocument).Assembly,
				typeof(ImageCodec).Assembly);
		}

		private object Decode(string FileName)
		{
			byte[] Data = File.ReadAllBytes("Data\\" + FileName + ".bin");
			string ContentType = File.ReadAllText("Data\\" + FileName + ".txt");

			return InternetContent.Decode(ContentType, Data, null);
		}

		[TestMethod]
		public void Test_01_Multipart()
		{
			object Decoded = this.Decode("6a00b78b-ebb7-4da7-a8cd-65dd09d3ac59");

			RelatedContent RelatedContent = Decoded as RelatedContent;
			Assert.IsNotNull(RelatedContent);
			Assert.AreEqual(2, RelatedContent.Content.Length);

			ContentAlternatives Alternatives = RelatedContent.Content[0].Decoded as ContentAlternatives;
			Assert.IsNotNull(Alternatives);
			Assert.IsTrue(RelatedContent.Content[1].Decoded is SkiaSharp.SKImage);
			File.WriteAllBytes("Data\\" + RelatedContent.Content[1].FileName, RelatedContent.Content[1].TransferDecoded ?? RelatedContent.Content[1].Raw);

			Assert.AreEqual(2, Alternatives.Content.Length);
			string PlainText = Alternatives.Content[0].Decoded as string;
			Assert.IsNotNull(PlainText);
			Console.Out.WriteLine(PlainText);
			Console.Out.WriteLine();

			HtmlDocument Html = Alternatives.Content[1].Decoded as HtmlDocument;
			Assert.IsNotNull(Html);
			Console.Out.WriteLine(Html.HtmlText);
			Console.Out.WriteLine();
		}

	}
}
