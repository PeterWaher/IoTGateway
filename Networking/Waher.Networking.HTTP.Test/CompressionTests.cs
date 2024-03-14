using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Networking.HTTP.Brotli;
using Waher.Networking.HTTP.ContentEncodings;
using Waher.Networking.HTTP.TransferEncodings;
using Waher.Runtime.Profiling;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class CompressionTests
	{
		[TestMethod]
		public async Task Test_01_GZip()
		{
			await Test<GZipContentEncoding>("gzip", false);	// To remove JIT times
			await Test<GZipContentEncoding>("gzip", true);
		}

		[TestMethod]
		public async Task Test_02_Deflate()
		{
			await Test<DeflateContentEncoding>("deflate", false);   // To remove JIT times
			await Test<DeflateContentEncoding>("deflate", true);
		}

		[TestMethod]
		public async Task Test_03_Brotli()
		{
			await Test<BrotliContentEncoding>("br", false); // To remove JIT times
			await Test<BrotliContentEncoding>("br", true);
		}

		private static async Task Test<T>(string Name, bool Output)
			where T : IContentEncoding, new()
		{
			Profiler Profiler = new(Name, ProfilerThreadType.Sequential);
			Profiler.Start();
			Profiler.NewState("Init");

			string FilePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..\Waher.IoTGateway.Resources\Highlight\highlight.pack.js");

			Profiler.NewState("Load");
			byte[] Data = File.ReadAllBytes(FilePath);

			Profiler.NewState("Setup");

			T ContentEncoding = new();
			MemoryStream Result = new();
			InternalTransfer TransferEncoding = new(Result);
			TransferEncoding Compressor = ContentEncoding.GetEncoder(TransferEncoding, Data.Length, Name);

			Profiler.NewState("Compress");

			await Compressor.EncodeAsync(Data, 0, Data.Length);
			await Compressor.FlushAsync();

			Profiler.NewState("Result");

			byte[] Compressed = Result.ToArray();

			if (Output)
			{
				Console.Out.WriteLine("Input: " + Data.Length + " bytes");
				Console.Out.WriteLine("Output: " + Compressed.Length + " bytes");
				Console.Out.WriteLine();
			}

			Profiler.Stop();

			if (Output)
			{
				Console.Out.WriteLine();
				Console.Out.WriteLine("```uml");
				Console.Out.WriteLine(Profiler.ExportPlantUml(TimeUnit.MilliSeconds));
				Console.Out.WriteLine("```");
			}
		}
	}
}
