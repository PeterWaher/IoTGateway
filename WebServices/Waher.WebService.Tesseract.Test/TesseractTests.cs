using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Images;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Security.JWS;
using Waher.Security.JWT;

namespace Waher.WebService.Tesseract.Test
{
	[TestClass]
	public class TesseractTests
	{
		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(typeof(TesseractTests).Assembly,
				typeof(InternetContent).Assembly,
				typeof(ImageCodec).Assembly,
				typeof(JwtFactory).Assembly, 
				typeof(JwsAlgorithm).Assembly);

			Log.Register(new ConsoleEventSink());

			await Types.StartAllModules(10000);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Types.StopAllModules();
		}

		[TestMethod]
		public async Task Test_001_DetectService()
		{
			TesseractApi Api = new();

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
			byte[] Bin = await Runtime.IO.Files.ReadAllBytesAsync(Path.Combine("Data", FileName));
			string ContentType = InternetContent.GetContentType(Path.GetExtension(FileName));
			TesseractApi Api = new();

			await Api.Start();
			try
			{
				string Text = await Api.PerformOcr(Bin, ContentType, Mode, Language);

				Assert.IsFalse(string.IsNullOrEmpty(Text), "Unable to perform OCR.");
				ConsoleOut.WriteLine(Text);
			}
			finally
			{
				await Api.Stop();
			}
		}

		[DataTestMethod]
		[DataRow("100_pass2-uto.jpg", null, "eng")]
		[DataRow("MRZ.jpg", PageSegmentationMode.SingleUniformBlockOfText, "mrz")]
		public async Task Test_003_ApiTests(string FileName, PageSegmentationMode? Mode, string Language)
		{
			byte[] Bin = await Runtime.IO.Files.ReadAllBytesAsync(Path.Combine("Data", FileName));
			string ContentType = InternetContent.GetContentType(Path.GetExtension(FileName));
			TesseractApi Api = new();

			using JwtFactory Factory = JwtFactory.CreateHmacSha256();
			Types.SetModuleParameter("JWT", Factory);

			string Token = Factory.Create();

			using HttpServer WebServer = new(8081);
			WebServer.Register(new ApiResource(Api, new JwtAuthentication("Test", null, Factory)));

			await Api.Start();
			try
			{
				Uri Uri = new("http://localhost:8081/Tesseract/Api");
				using HttpClient HttpClient = new();
				using HttpRequestMessage Request = new()
				{
					RequestUri = Uri,
					Method = HttpMethod.Post,
					Content = new ByteArrayContent(Bin)
				};

				Request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
				Request.Headers.Add("X-LANGUAGE", Language);

				if (Mode.HasValue)
					Request.Headers.Add("X-PSM", Mode.Value.ToString());

				Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

				HttpResponseMessage Response = await HttpClient.SendAsync(Request);
				Response.EnsureSuccessStatusCode();

				Bin = await Response.Content.ReadAsByteArrayAsync();
				ContentType = Response.Content.Headers.ContentType.ToString();

				ContentResponse Obj = await InternetContent.DecodeAsync(ContentType, Bin, Uri);
				Obj.AssertOk();

				string Text = Obj.Decoded as string;

				Assert.IsNotNull(Text, "Unexpected response.");

				ConsoleOut.WriteLine(Text);
			}
			finally
			{
				await Api.Stop();
			}
		}
	}
}
