using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Text;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class BenchmarkingTests : HttpServerTestsBase
	{
		private static bool getTestJited = false;
		private static bool putTestJited = false;

		[TestCleanup]
		public Task TestCleanup()
		{
			return this.Cleanup();
		}

		[DataTestMethod]
		[DataRow("HTTP_Benchmark_01_100000_rfc7540.xml", false, false, false, false, 100000)]
		[DataRow("HTTP_Benchmark_01_1000000_rfc7540.xml", false, false, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_01_10000000_rfc7540.xml", false, false, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_01_100000000_rfc7540.xml", false, false, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_01_1000000000_rfc7540.xml", false, false, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_01_deflate_100000_rfc7540.xml", false, true, false, false, 100000)]
		[DataRow("HTTP_Benchmark_01_deflate_1000000_rfc7540.xml", false, true, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_01_deflate_10000000_rfc7540.xml", false, true, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_01_deflate_100000000_rfc7540.xml", false, true, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_01_deflate_1000000000_rfc7540.xml", false, true, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_01_gzip_100000_rfc7540.xml", false, false, true, false, 100000)]
		[DataRow("HTTP_Benchmark_01_gzip_1000000_rfc7540.xml", false, false, true, false, 1000000)]
		[DataRow("HTTP_Benchmark_01_gzip_10000000_rfc7540.xml", false, false, true, false, 10000000)]
		[DataRow("HTTP_Benchmark_01_gzip_100000000_rfc7540.xml", false, false, true, false, 100000000)]
		[DataRow("HTTP_Benchmark_01_gzip_1000000000_rfc7540.xml", false, false, true, false, 1000000000)]
		[DataRow("HTTP_Benchmark_01_br_100000_rfc7540.xml", false, false, false, true, 100000)]
		[DataRow("HTTP_Benchmark_01_br_1000000_rfc7540.xml", false, false, false, true, 1000000)]
		[DataRow("HTTP_Benchmark_01_br_10000000_rfc7540.xml", false, false, false, true, 10000000)]
		[DataRow("HTTP_Benchmark_01_br_100000000_rfc7540.xml", false, false, false, true, 100000000)]
		[DataRow("HTTP_Benchmark_01_br_1000000000_rfc7540.xml", false, false, false, true, 1000000000)]
		[DataRow("HTTP_Benchmark_01_100000_rfc9218.xml", true, false, false, false, 100000)]
		[DataRow("HTTP_Benchmark_01_1000000_rfc9218.xml", true, false, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_01_10000000_rfc9218.xml", true, false, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_01_100000000_rfc9218.xml", true, false, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_01_1000000000_rfc9218.xml", true, false, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_01_deflate_100000_rfc9218.xml", true, true, false, false, 100000)]
		[DataRow("HTTP_Benchmark_01_deflate_1000000_rfc9218.xml", true, true, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_01_deflate_10000000_rfc9218.xml", true, true, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_01_deflate_100000000_rfc9218.xml", true, true, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_01_deflate_1000000000_rfc9218.xml", true, true, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_01_gzip_100000_rfc9218.xml", true, false, true, false, 100000)]
		[DataRow("HTTP_Benchmark_01_gzip_1000000_rfc9218.xml", true, false, true, false, 1000000)]
		[DataRow("HTTP_Benchmark_01_gzip_10000000_rfc9218.xml", true, false, true, false, 10000000)]
		[DataRow("HTTP_Benchmark_01_gzip_100000000_rfc9218.xml", true, false, true, false, 100000000)]
		[DataRow("HTTP_Benchmark_01_gzip_1000000000_rfc9218.xml", true, false, true, false, 1000000000)]
		[DataRow("HTTP_Benchmark_01_br_100000_rfc9218.xml", true, false, false, true, 100000)]
		[DataRow("HTTP_Benchmark_01_br_1000000_rfc9218.xml", true, false, false, true, 1000000)]
		[DataRow("HTTP_Benchmark_01_br_10000000_rfc9218.xml", true, false, false, true, 10000000)]
		[DataRow("HTTP_Benchmark_01_br_100000000_rfc9218.xml", true, false, false, true, 100000000)]
		[DataRow("HTTP_Benchmark_01_br_1000000000_rfc9218.xml", true, false, false, true, 1000000000)]
		public async Task Test_01_GET_Chunked(string SnifferFileName, bool NoRfc7540Priorities,
			bool SupportDeflate, bool SupportGZip, bool SupportBrotli, int TotalSize)
		{
			this.Setup(false, SnifferFileName, false, SupportDeflate, SupportGZip, SupportBrotli);

			HttpResource Resource = this.server.Register("/test01.txt", async (req, resp) =>
			{
				int i = 0;
				int j;

				resp.ContentType = PlainTextCodec.DefaultContentType;
				while (i < TotalSize)
				{
					j = Math.Min(2000, TotalSize - i);
					await resp.Write(new string('a', j));
					i += j;
				}
			});

			try
			{
				if (!getTestJited)
				{
					// JIT pass

					await GetTest(HttpVersion.Version10);
					await GetTest(HttpVersion.Version11);
					await GetTest(HttpVersion.Version20);

					getTestJited = true;

					await Task.Delay(1000);
				}

				// Benchmark pass

				TimeSpan Time10 = await GetTest(HttpVersion.Version10);
				await Task.Delay(1000);

				TimeSpan Time11 = await GetTest(HttpVersion.Version11);
				await Task.Delay(1000);

				TimeSpan Time20 = await GetTest(HttpVersion.Version20);
				await Task.Delay(1000);

				Console.Out.WriteLine("|>> GET Benchmark                                      <<||||");
				Console.Out.WriteLine("|>> Size     <<|>> HTTP 1.0 <<|>> HTTP 1.1 <<|>> HTTP/2   <<|");
				Console.Out.WriteLine("|-------------:|-------------:|-------------:|-------------:|");

				Console.Out.Write("| ");
				Console.Out.Write(PadLeft(TotalSize.ToString(), 10));
				Console.Out.Write(" B | ");

				Console.Out.Write(PadLeft(Time10.TotalSeconds.ToString("F3"), 10));
				Console.Out.Write(" s | ");

				Console.Out.Write(PadLeft(Time11.TotalSeconds.ToString("F3"), 10));
				Console.Out.Write(" s | ");

				Console.Out.Write(PadLeft(Time20.TotalSeconds.ToString("F3"), 10));
				Console.Out.WriteLine(" s |");
			}
			finally
			{
				this.server.Unregister(Resource);
			}
		}

		private static string PadLeft(string s, int Width)
		{
			int c = s.Length;
			if (c < Width)
				s = new string(' ', Width - c) + s;

			return s;
		}

		private static async Task<TimeSpan> GetTest(Version ProtocolVersion)
		{
			using CookieWebClient Client = new(ProtocolVersion);

			DateTime Start = DateTime.Now;
			await Client.DownloadData("http://localhost:8081/test01.txt");
			return DateTime.Now.Subtract(Start);
		}

		[DataTestMethod]
		[DataRow("HTTP_Benchmark_02_100000_rfc7540.xml", false, false, false, false, 100000)]
		[DataRow("HTTP_Benchmark_02_1000000_rfc7540.xml", false, false, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_02_10000000_rfc7540.xml", false, false, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_02_100000000_rfc7540.xml", false, false, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_02_1000000000_rfc7540.xml", false, false, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_02_deflate_100000_rfc7540.xml", false, true, false, false, 100000)]
		[DataRow("HTTP_Benchmark_02_deflate_1000000_rfc7540.xml", false, true, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_02_deflate_10000000_rfc7540.xml", false, true, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_02_deflate_100000000_rfc7540.xml", false, true, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_02_deflate_1000000000_rfc7540.xml", false, true, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_02_gzip_100000_rfc7540.xml", false, false, true, false, 100000)]
		[DataRow("HTTP_Benchmark_02_gzip_1000000_rfc7540.xml", false, false, true, false, 1000000)]
		[DataRow("HTTP_Benchmark_02_gzip_10000000_rfc7540.xml", false, false, true, false, 10000000)]
		[DataRow("HTTP_Benchmark_02_gzip_100000000_rfc7540.xml", false, false, true, false, 100000000)]
		[DataRow("HTTP_Benchmark_02_gzip_1000000000_rfc7540.xml", false, false, true, false, 1000000000)]
		[DataRow("HTTP_Benchmark_02_br_100000_rfc7540.xml", false, false, false, true, 100000)]
		[DataRow("HTTP_Benchmark_02_br_1000000_rfc7540.xml", false, false, false, true, 1000000)]
		[DataRow("HTTP_Benchmark_02_br_10000000_rfc7540.xml", false, false, false, true, 10000000)]
		[DataRow("HTTP_Benchmark_02_br_100000000_rfc7540.xml", false, false, false, true, 100000000)]
		[DataRow("HTTP_Benchmark_02_br_1000000000_rfc7540.xml", false, false, false, true, 1000000000)]
		[DataRow("HTTP_Benchmark_02_100000_rfc9218.xml", true, false, false, false, 100000)]
		[DataRow("HTTP_Benchmark_02_1000000_rfc9218.xml", true, false, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_02_10000000_rfc9218.xml", true, false, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_02_100000000_rfc9218.xml", true, false, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_02_1000000000_rfc9218.xml", true, false, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_02_deflate_100000_rfc9218.xml", true, true, false, false, 100000)]
		[DataRow("HTTP_Benchmark_02_deflate_1000000_rfc9218.xml", true, true, false, false, 1000000)]
		[DataRow("HTTP_Benchmark_02_deflate_10000000_rfc9218.xml", true, true, false, false, 10000000)]
		[DataRow("HTTP_Benchmark_02_deflate_100000000_rfc9218.xml", true, true, false, false, 100000000)]
		[DataRow("HTTP_Benchmark_02_deflate_1000000000_rfc9218.xml", true, true, false, false, 1000000000)]
		[DataRow("HTTP_Benchmark_02_gzip_100000_rfc9218.xml", true, false, true, false, 100000)]
		[DataRow("HTTP_Benchmark_02_gzip_1000000_rfc9218.xml", true, false, true, false, 1000000)]
		[DataRow("HTTP_Benchmark_02_gzip_10000000_rfc9218.xml", true, false, true, false, 10000000)]
		[DataRow("HTTP_Benchmark_02_gzip_100000000_rfc9218.xml", true, false, true, false, 100000000)]
		[DataRow("HTTP_Benchmark_02_gzip_1000000000_rfc9218.xml", true, false, true, false, 1000000000)]
		[DataRow("HTTP_Benchmark_02_br_100000_rfc9218.xml", true, false, false, true, 100000)]
		[DataRow("HTTP_Benchmark_02_br_1000000_rfc9218.xml", true, false, false, true, 1000000)]
		[DataRow("HTTP_Benchmark_02_br_10000000_rfc9218.xml", true, false, false, true, 10000000)]
		[DataRow("HTTP_Benchmark_02_br_100000000_rfc9218.xml", true, false, false, true, 100000000)]
		[DataRow("HTTP_Benchmark_02_br_1000000000_rfc9218.xml", true, false, false, true, 1000000000)]
		public async Task Test_02_PUT(string SnifferFileName, bool NoRfc7540Priorities,
			bool SupportDeflate, bool SupportGZip, bool SupportBrotli, int TotalSize)
		{
			this.Setup(false, SnifferFileName, false, SupportDeflate, SupportGZip, SupportBrotli);

			if (!this.server.TryGetResource("/Test02", false, out _, out _))
				this.server.Register(new HttpFolderResource("/Test02", "Data", true, false, true, false));

			if (!putTestJited)
			{
				// JIT pass

				await PutTest(TotalSize, HttpVersion.Version10);
				await PutTest(TotalSize, HttpVersion.Version11);
				await PutTest(TotalSize, HttpVersion.Version20);

				putTestJited = true;

				await Task.Delay(1000);
			}

			// Benchmark pass

			TimeSpan Time10 = await PutTest(TotalSize, HttpVersion.Version10);
			await Task.Delay(1000);

			TimeSpan Time11 = await PutTest(TotalSize, HttpVersion.Version11);
			await Task.Delay(1000);

			TimeSpan Time20 = await PutTest(TotalSize, HttpVersion.Version20);
			await Task.Delay(1000);

			Console.Out.WriteLine("|>> PUT Benchmark                                      <<||||");
			Console.Out.WriteLine("|>> Size     <<|>> HTTP 1.0 <<|>> HTTP 1.1 <<|>> HTTP/2   <<|");
			Console.Out.WriteLine("|-------------:|-------------:|-------------:|-------------:|");

			Console.Out.Write("| ");
			Console.Out.Write(PadLeft(TotalSize.ToString(), 10));
			Console.Out.Write(" B | ");

			Console.Out.Write(PadLeft(Time10.TotalSeconds.ToString("F3"), 10));
			Console.Out.Write(" s | ");

			Console.Out.Write(PadLeft(Time11.TotalSeconds.ToString("F3"), 10));
			Console.Out.Write(" s | ");

			Console.Out.Write(PadLeft(Time20.TotalSeconds.ToString("F3"), 10));
			Console.Out.WriteLine(" s |");
		}

		private static async Task<TimeSpan> PutTest(int TotalSize, Version ProtocolVersion)
		{
			StringBuilder sb = new();
			int i = 0;

			while (sb.Length < TotalSize)
			{
				if (i > 0)
					sb.Append('_');

				sb.Append(i);
				i++;
			}

			if (sb.Length > TotalSize)
				sb.Length = TotalSize;

			using CookieWebClient Client = new(ProtocolVersion);
			UTF8Encoding Utf8 = new(true);
			string s1 = sb.ToString();
			string FileName = "string_" + TotalSize.ToString() + ".txt";
			byte[] Data0 = Utf8.GetBytes(s1);

			DateTime Start = DateTime.Now;
			await Client.UploadData("http://localhost:8081/Test02/" + FileName, HttpMethod.Put, Data0);
			return DateTime.Now.Subtract(Start);
		}

	}
}
