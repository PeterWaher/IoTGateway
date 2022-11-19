using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Images;
using Waher.Events;
using Waher.Events.Console;
using Waher.Runtime.Inventory;

namespace Waher.WebService.Tesseract.Test
{
	[TestClass]
	public class TesseractTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			Types.Initialize(typeof(TesseractTests).Assembly,
				typeof(InternetContent).Assembly,
				typeof(ImageCodec).Assembly);

			Log.Register(new ConsoleEventSink());
		}

		[TestMethod]
		public async Task Test_001_DetectService()
		{
			TesseractApi Api = new TesseractApi();

			await Api.Start();
			try
			{
				Assert.IsFalse(string.IsNullOrEmpty(Api.ExecutablePath), "Executable path not found.");
			}
			finally
			{
				await Api.Stop();
			}
		}

		[DataTestMethod]
		[DataRow("100_pass2-uto.jpg", null, "eng")]
		[DataRow("MRZ.jpg", PageSegmentationMode.SingleUniformBlockOfText, "mrz")]
		public async Task Test_002_OcrImage(string FileName, PageSegmentationMode? Mode, string Language)
		{
			byte[] Bin = await Resources.ReadAllBytesAsync(Path.Combine("Data", FileName));
			string ContentType = InternetContent.GetContentType(Path.GetExtension(FileName));
			TesseractApi Api = new TesseractApi();

			await Api.Start();
			try
			{
				string Text = await Api.PerformOcr(Bin, ContentType, Mode, string.Empty);

				Assert.IsFalse(string.IsNullOrEmpty(Text), "Unable to perform OCR.");
				Console.Out.WriteLine(Text);
			}
			finally
			{
				await Api.Stop();
			}
		}
	}
}
