using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Dsn;
using Waher.Content.Html;
using Waher.Content.Images;
using Waher.Content.Multipart;
using Waher.Content.Test.Encodings;
using Waher.Content.Xml;

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
				typeof(DeliveryStatus).Assembly,
				typeof(XML).Assembly);

			Encoding.RegisterProvider(new CodePages());
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

		[TestMethod]
		public void Test_03_Delivery_Status_Notification()
		{
			object Decoded = this.Decode("015dd6dd-ed9f-4139-995b-513f7464dd6f");

			MixedContent MixedContent = Decoded as MixedContent;
			Assert.IsNotNull(MixedContent);
			Assert.AreEqual(2, MixedContent.Content.Length);

			string HumanReadable = MixedContent.Content[0].Decoded as string;
			Assert.IsNotNull(HumanReadable);
			Console.Out.WriteLine(HumanReadable);
			Console.Out.WriteLine();

			DeliveryStatus Dsn = MixedContent.Content[1].Decoded as DeliveryStatus;
			Assert.IsNotNull(Dsn);
			Console.Out.WriteLine(Dsn.Text);
			Console.Out.WriteLine();

			Assert.AreEqual("s554.loopia.se", Dsn.PerMessage.ReportingMta);
			Assert.AreEqual("dns", Dsn.PerMessage.ReportingMtaType);
			Assert.AreEqual(new DateTimeOffset(2019, 01, 23, 22, 15, 51, TimeSpan.FromHours(1)), Dsn.PerMessage.ArrivalDate);
			Assert.AreEqual(2, Dsn.PerMessage.Other.Length);
			Assert.AreEqual("X-Postfix-Queue-ID", Dsn.PerMessage.Other[0].Key);
			Assert.AreEqual("0DC1E1F17438", Dsn.PerMessage.Other[0].Value);
			Assert.AreEqual("X-Postfix-Sender", Dsn.PerMessage.Other[1].Key);

			Assert.AreEqual(1, Dsn.PerRecipients.Length);
			Assert.AreEqual("rfc822", Dsn.PerRecipients[0].FinalRecipientType);
			Assert.AreEqual("rfc822", Dsn.PerRecipients[0].OriginalRecipientType);
			Assert.AreEqual(Content.Dsn.Action.relayed, Dsn.PerRecipients[0].Action);
			Assert.AreEqual(2, Dsn.PerRecipients[0].Status[0]);
			Assert.AreEqual(6, Dsn.PerRecipients[0].Status[1]);
			Assert.AreEqual(0, Dsn.PerRecipients[0].Status[2]);
			Assert.AreEqual("waher.se", Dsn.PerRecipients[0].RemoteMta);
			Assert.AreEqual("dns", Dsn.PerRecipients[0].RemoteMtaType);
			Assert.AreEqual("250 2.6.0    <e272eb6aa58d95a2c8d1ba4554fdaf51@littlesister.se> Message accepted for    delivery.", Dsn.PerRecipients[0].DiagnosticCode);
			Assert.AreEqual("smtp", Dsn.PerRecipients[0].DiagnosticCodeType);
		}

		[TestMethod]
		public void Test_04_BoundaryAtEnd()
		{
			object Decoded = this.Decode("1773459c-3649-4bb4-a33c-a15651665e92");

			ContentAlternatives Alternatives = Decoded as ContentAlternatives;
			Assert.IsNotNull(Alternatives);

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
		public void Test_05_Windows1252()
		{
			object Decoded = this.Decode("84784fa7-51a3-46e4-b8c8-38570416acab");

			ContentAlternatives Alternatives = Decoded as ContentAlternatives;
			Assert.IsNotNull(Alternatives);

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
