using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
				typeof(ImageCodec).Assembly,
				typeof(XML).Assembly);
		}

		private object Decode(string FileName)
		{
			byte[] Data = File.ReadAllBytes("Data\\" + FileName + ".bin");
			string ContentType = File.ReadAllText("Data\\" + FileName + ".txt");

			return InternetContent.Decode(ContentType, Data, null);
		}

		[TestMethod]
		public void Test_01_Multipart_RelatedContent()
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

		[TestMethod]
		public void Test_02_Multipart_MixedContent()
		{
			object Decoded = this.Decode("0b9e5696-c32a-4418-a246-babb188e5beb");

			MixedContent MixedContent = Decoded as MixedContent;
			Assert.IsNotNull(MixedContent);
			Assert.AreEqual(3, MixedContent.Content.Length);

			RelatedContent RelatedContent = MixedContent.Content[0].Decoded as RelatedContent;
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

			string Attachment1 = MixedContent.Content[1].Decoded as string;
			Assert.IsNotNull(Attachment1);
			Console.Out.WriteLine(Attachment1);
			Console.Out.WriteLine();

			XmlDocument Attachment2 = MixedContent.Content[2].Decoded as XmlDocument;
			Assert.IsNotNull(Attachment2);
			Console.Out.WriteLine(Attachment2.OuterXml);
			Console.Out.WriteLine();
		}

	}
}
