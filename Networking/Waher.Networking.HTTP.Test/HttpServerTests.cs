using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.HTTP.Authentication;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class HttpServerTests : IUserSource
	{
		private static HttpServer server;
		private static ConsoleEventSink sink = null;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Runtime.Inventory.Types.Initialize(
				typeof(HttpServerTests).Assembly,
				typeof(Script.Expression).Assembly,
				typeof(Content.Images.ImageCodec).Assembly,
				typeof(CommonTypes).Assembly);
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			sink = new ConsoleEventSink();
			Log.Register(sink);

			X509Certificate2 Certificate = Resources.LoadCertificate("Waher.Networking.HTTP.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			server = new HttpServer(8080, 8088, Certificate, new TextWriterSniffer(Console.Out, BinaryPresentationMethod.ByteCount));

			ServicePointManager.ServerCertificateValidationCallback = delegate (Object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			server?.Dispose();
			server = null;

			if (sink != null)
			{
				Log.Unregister(sink);
				sink.Dispose();
				sink = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (UserName == "User")
				return Task.FromResult<IUser>(new User());
			else
				return Task.FromResult<IUser>(null);
		}

		[TestMethod]
		public void Test_01_GET_HTTP_ContentLength()
		{
			server.Register("/test01.txt", (req, resp) => resp.Return("hej på dej"));

			using (CookieWebClient Client = new CookieWebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test01.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[TestMethod]
		public void Test_02_GET_HTTP_Chunked()
		{
			server.Register("/test02.txt", async (req, resp) =>
			{
				int i;

				resp.ContentType = "text/plain";
				for (i = 0; i < 1000; i++)
					await resp.Write(new string('a', 100));
			});

			using (CookieWebClient Client = new CookieWebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test02.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual(100000, s.Length);
				Assert.AreEqual(new string('a', 100000), s);
			}
		}

		[TestMethod]
		public void Test_03_GET_HTTP_Encoding()
		{
			server.Register("/test03.png", async (req, resp) =>
			{
				await resp.Return(new SKBitmap(320, 200));
			});

			using (CookieWebClient Client = new CookieWebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test03.png");
				SKBitmap Bmp = SKBitmap.Decode(Data);
				Assert.AreEqual(320, Bmp.Width);
				Assert.AreEqual(200, Bmp.Height);
			}
		}

		[TestMethod]
		public void Test_04_GET_HTTPS()
		{
			server.Register("/test04.txt", (req, resp) => resp.Return("hej på dej"));

			using (CookieWebClient Client = new CookieWebClient())
			{
				byte[] Data = Client.DownloadData("https://localhost:8088/test04.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[TestMethod]
		public void Test_05_Authentication_Basic()
		{
			server.Register("/test05.txt", (req, resp) => resp.Return("hej på dej"), new BasicAuthentication("Test05", this));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Client.Credentials = new NetworkCredential("User", "Password");
				byte[] Data = Client.DownloadData("http://localhost:8080/test05.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[TestMethod]
		public void Test_06_Authentication_Digest()
		{
			server.Register("/test06.txt", (req, resp) => resp.Return("hej på dej"), new DigestAuthentication("Test06", this));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Client.Credentials = new NetworkCredential("User", "Password");
				byte[] Data = Client.DownloadData("http://localhost:8080/test06.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[TestMethod]
		public void Test_07_EmbeddedResource()
		{
			server.Register(new HttpEmbeddedResource("/test07.png", "Waher.Networking.HTTP.Test.Data.Frog-300px.png", typeof(HttpServerTests).Assembly));

			using (CookieWebClient Client = new CookieWebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test07.png");
				SKBitmap Bmp = SKBitmap.Decode(Data);
				Assert.AreEqual(300, Bmp.Width);
				Assert.AreEqual(184, Bmp.Height);
			}
		}

		[TestMethod]
		public void Test_08_FolderResource_GET()
		{
			server.Register(new HttpFolderResource("/Test08", "Data", false, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/Test08/BarnSwallowIsolated-300px.png");
				SKBitmap Bmp = SKBitmap.Decode(Data);
				Assert.AreEqual(300, Bmp.Width);
				Assert.AreEqual(264, Bmp.Height);
			}
		}

		[TestMethod]
		public void Test_09_FolderResource_PUT_File()
		{
			server.Register(new HttpFolderResource("/Test09", "Data", true, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				string s1 = new string('Ω', 100000);
				Client.UploadData("http://localhost:8080/Test09/string.txt", "PUT", Utf8.GetBytes(s1));

				byte[] Data = Client.DownloadData("http://localhost:8080/Test09/string.txt");
				string s2 = Utf8.GetString(Data);

				Assert.AreEqual(s1, s2);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(WebException))]
		public void Test_10_FolderResource_PUT_File_NotAllowed()
		{
			server.Register(new HttpFolderResource("/Test10", "Data", false, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				byte[] Data = Client.UploadData("http://localhost:8080/Test10/string.txt", "PUT", Utf8.GetBytes(new string('Ω', 100000)));
			}
		}

		[TestMethod]
		public void Test_11_FolderResource_DELETE_File()
		{
			server.Register(new HttpFolderResource("/Test11", "Data", true, true, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				Client.UploadData("http://localhost:8080/Test11/string.txt", "PUT", Utf8.GetBytes(new string('Ω', 100000)));

				Client.UploadData("http://localhost:8080/Test11/string.txt", "DELETE", new byte[0]);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(WebException))]
		public void Test_12_FolderResource_DELETE_File_NotAllowed()
		{
			server.Register(new HttpFolderResource("/Test12", "Data", true, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				Client.UploadData("http://localhost:8080/Test12/string.txt", "PUT", Utf8.GetBytes(new string('Ω', 100000)));

				Client.UploadData("http://localhost:8080/Test12/string.txt", "DELETE", new byte[0]);
			}
		}

		[TestMethod]
		public void Test_13_FolderResource_PUT_CreateFolder()
		{
			server.Register(new HttpFolderResource("/Test13", "Data", true, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				string s1 = new string('Ω', 100000);
				Client.UploadData("http://localhost:8080/Test13/Folder/string.txt", "PUT", Utf8.GetBytes(s1));

				byte[] Data = Client.DownloadData("http://localhost:8080/Test13/Folder/string.txt");
				string s2 = Utf8.GetString(Data);

				Assert.AreEqual(s1, s2);
			}
		}

		[TestMethod]
		public void Test_14_FolderResource_DELETE_Folder()
		{
			server.Register(new HttpFolderResource("/Test14", "Data", true, true, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				Client.UploadData("http://localhost:8080/Test14/Folder/string.txt", "PUT", Utf8.GetBytes(new string('Ω', 100000)));

				Client.UploadData("http://localhost:8080/Test14/Folder", "DELETE", new byte[0]);
			}
		}

		[TestMethod]
		public void Test_15_GET_Single_Closed_Range()
		{
			server.Register(new HttpFolderResource("/Test15", "Data", false, false, true, false));

			HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/Test15/Text.txt");
			Request.AddRange(100, 119);

			using (WebResponse Response = Request.GetResponse())
			{
				byte[] Data = new byte[Response.ContentLength];

				using (Stream f = Response.GetResponseStream())
				{
					Assert.AreEqual(20, f.Read(Data, 0, (int)Response.ContentLength));
					string s = Encoding.UTF8.GetString(Data);
					Assert.AreEqual("89012345678901234567", s);
				}
			}
		}

		[TestMethod]
		public void Test_16_GET_Single_Open_Range1()
		{
			server.Register(new HttpFolderResource("/Test16", "Data", false, false, true, false));

			HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/Test16/Text.txt");
			Request.AddRange(980);

			using (WebResponse Response = Request.GetResponse())
			{
				byte[] Data = new byte[Response.ContentLength];

				using (Stream f = Response.GetResponseStream())
				{
					Assert.AreEqual(23, f.Read(Data, 0, (int)Response.ContentLength));
					string s = Encoding.UTF8.GetString(Data);
					Assert.AreEqual("89012345678901234567890", s);
				}
			}
		}

		[TestMethod]
		public void Test_17_GET_Single_Open_Range2()
		{
			server.Register(new HttpFolderResource("/Test17", "Data", false, false, true, false));

			HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/Test17/Text.txt");
			Request.AddRange(-20);

			using (WebResponse Response = Request.GetResponse())
			{
				byte[] Data = new byte[Response.ContentLength];

				using (Stream f = Response.GetResponseStream())
				{
					Assert.AreEqual(20, f.Read(Data, 0, (int)Response.ContentLength));
					string s = Encoding.UTF8.GetString(Data);
					Assert.AreEqual("12345678901234567890", s);
				}
			}
		}

		[TestMethod]
		public void Test_18_GET_MultipleRanges()
		{
			server.Register(new HttpFolderResource("/Test18", "Data", false, false, true, false));

			HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/Test18/Text.txt");
			Request.AddRange(100, 199);
			Request.AddRange(-100);

			using (WebResponse Response = Request.GetResponse())
			{
				byte[] Data = new byte[500];

				using (Stream f = Response.GetResponseStream())
				{
					int NrRead = f.Read(Data, 0, 500);
					string s = Encoding.UTF8.GetString(Data, 0, NrRead);
					string s2 = File.ReadAllText("Data/MultiRangeResponse.txt");

					int i = s.IndexOf("--");
					int j = s.IndexOf("\r\n", i);
					string Boundary = s.Substring(i + 2, j - i - 2);

					Assert.AreEqual(s2, s.Replace(Boundary, "463d71b7a34048709e1bb217940feea6"));
				}
			}
		}

		[TestMethod]
		public void Test_19_PUT_Range()
		{
			server.Register(new HttpFolderResource("/Test19", "Data", true, false, true, false));

			HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/Test19/String2.txt");
			Request.Method = "PUT";
			Request.Headers.Add("Content-Range: bytes 20-39/40");
			byte[] Data = new byte[20];
			int i;

			for (i = 0; i < 20; i++)
				Data[i] = (byte)'1';

			Stream f = Request.GetRequestStream();
			f.Write(Data, 0, 20);

			WebResponse Response = Request.GetResponse();
			Response.Close();

			for (i = 0; i < 20; i++)
				Data[i] = (byte)'2';

			Request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/Test19/String2.txt");
			Request.Method = "PUT";
			Request.Headers.Add("Content-Range: bytes 0-19/40");

			f = Request.GetRequestStream();
			f.Write(Data, 0, 20);

			Response = Request.GetResponse();
			Response.Close();

			using (CookieWebClient Client = new CookieWebClient())
			{
				Data = Client.DownloadData("http://localhost:8080/Test19/String2.txt");
				string s = Encoding.ASCII.GetString(Data);

				Assert.AreEqual("2222222222222222222211111111111111111111", s);
			}
		}

		[TestMethod]
		public void Test_20_HEAD()
		{
			server.Register("/test20.png", async (req, resp) =>
			{
				await resp.Return(new SKBitmap(320, 200));
			});

			HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/test20.png");
			Request.Method = "HEAD";
			WebResponse Response = Request.GetResponse();

			Assert.IsTrue(Response.ContentLength > 0);

			Stream f = Response.GetResponseStream();
			byte[] Data = new byte[1];

			Assert.AreEqual(0, f.Read(Data, 0, 1));
		}

		[TestMethod]
		public void Test_21_Cookies()
		{
			server.Register("/test21_1.txt", async (req, resp) =>
			{
				resp.SetCookie(new Cookie("word1", "hej", "localhost", "/"));
				resp.SetCookie(new Cookie("word2", "på", "localhost", "/"));
				resp.SetCookie(new Cookie("word3", "dej", "localhost", "/"));

				await resp.Return("hejsan");
			});

			server.Register("/test21_2.txt", async (req, resp) =>
			{
				await resp.Return(req.Header.Cookie["word1"] + " " + req.Header.Cookie["word2"] + " " + req.Header.Cookie["word3"]);
			});

			using (CookieWebClient Client = new CookieWebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test21_1.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hejsan", s);

				Data = Client.DownloadData("http://localhost:8080/test21_2.txt");
				s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(WebException))]
		public void Test_22_Conditional_GET_IfModifiedSince_1()
		{
			DateTime LastModified = File.GetLastWriteTime("Data\\BarnSwallowIsolated-300px.png");

			server.Register(new HttpFolderResource("/Test22", "Data", false, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Client.IfModifiedSince = LastModified.AddMinutes(1);
				byte[] Data = Client.DownloadData("http://localhost:8080/Test22/BarnSwallowIsolated-300px.png");
			}
		}

		[TestMethod]
		public void Test_23_Conditional_GET_IfModifiedSince_2()
		{
			DateTime LastModified = File.GetLastWriteTime("Data\\BarnSwallowIsolated-300px.png");

			server.Register(new HttpFolderResource("/Test23", "Data", false, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Client.IfModifiedSince = LastModified.AddMinutes(-1);
				byte[] Data = Client.DownloadData("http://localhost:8080/Test23/BarnSwallowIsolated-300px.png");
				SKBitmap Bmp = SKBitmap.Decode(Data);
				Assert.AreEqual(300, Bmp.Width);
				Assert.AreEqual(264, Bmp.Height);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(WebException))]
		public void Test_24_Conditional_PUT_IfUnmodifiedSince_1()
		{
			DateTime LastModified = File.GetLastWriteTime("Data\\Temp.txt");

			server.Register(new HttpFolderResource("/Test24", "Data", true, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				string s1 = new string('Ω', 100000);
				Client.IfUnmodifiedSince = LastModified.AddMinutes(-1);
				Client.UploadData("http://localhost:8080/Test24/Temp.txt", "PUT", Utf8.GetBytes(s1));
			}
		}

		[TestMethod]
		public void Test_25_Conditional_PUT_IfUnmodifiedSince_2()
		{
			DateTime LastModified = File.GetLastWriteTime("Data\\Temp.txt");

			server.Register(new HttpFolderResource("/Test25", "Data", true, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Encoding Utf8 = new UTF8Encoding(true);
				string s1 = new string('Ω', 100000);
				Client.IfUnmodifiedSince = LastModified.AddMinutes(1);
				Client.UploadData("http://localhost:8080/Test25/Temp.txt", "PUT", Utf8.GetBytes(s1));
			}
		}

		[TestMethod]
		[ExpectedException(typeof(WebException))]
		public void Test_26_NotAcceptable()
		{
			server.Register(new HttpFolderResource("/Test26", "Data", false, false, true, false));

			using (CookieWebClient Client = new CookieWebClient())
			{
				Client.Accept = "text/x-test4";
				byte[] Data = Client.DownloadData("http://localhost:8080/Test26/Text.txt");
			}
		}

		[TestMethod]
		public void Test_27_Content_Conversion()
		{
			HttpFolderResource Resource = new HttpFolderResource("/Test27", "Data", false, false, true, false);
			Resource.AllowTypeConversion("text/plain", "text/x-test1", "text/x-test2", "text/x-test3");

			server.Register(Resource);

			using (CookieWebClient Client = new CookieWebClient())
			{
				Client.Accept = "text/x-test3";
				byte[] Data = Client.DownloadData("http://localhost:8080/Test27/Text.txt");
				MemoryStream ms = new MemoryStream(Data);
				StreamReader r = new StreamReader(ms);
				string s = r.ReadToEnd();
				Assert.AreEqual("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890\r\nConverter 1 was here.\r\nConverter 2 was here.\r\nConverter 3 was here.", s);
			}
		}

	}
}
