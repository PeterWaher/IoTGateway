using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
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
		public async Task Test_01_OneFile_GZip()
		{
			await Test<GZipContentEncoding>("gzip", false); // To remove JIT times
			await Test<GZipContentEncoding>("gzip", true);
		}

		[TestMethod]
		public async Task Test_02_OneFile_Deflate()
		{
			await Test<DeflateContentEncoding>("deflate", false);   // To remove JIT times
			await Test<DeflateContentEncoding>("deflate", true);
		}

		[TestMethod]
		public async Task Test_03_OneFile_Brotli()
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

		[TestMethod]
		public async Task Test_04_Statistics_GZip()
		{
			await Test<GZipContentEncoding>("GZip", "Red");
		}

		[TestMethod]
		public async Task Test_05_Statistics_Deflate()
		{
			await Test<DeflateContentEncoding>("Deflate", "Green");
		}

		[TestMethod]
		public async Task Test_06_Statistics_Brotli()
		{
			await Test<BrotliContentEncoding>("Brotli", "Blue");
		}

		private static async Task Test<T>(string VariableName, string Color)
			where T : IContentEncoding, new()
		{
			string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..\Waher.IoTGateway.Resources");
			string[] FileNames = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories);
			Stopwatch Watch = new();
			Watch.Start();

			await Test<T>(FileNames[0], false, Watch, false);  // To remove JIT times

			Console.Out.Write(VariableName);
			Console.Out.WriteLine(":=[");

			int i, c = FileNames.Length;

			for (i = 0; i < c; i++)
			{
				string FileName = FileNames[i];

				switch (Path.GetExtension(FileName))
				{
					case ".md":
					case ".css":
					case ".js":
					case ".uml":
					case ".xslt":
					case ".ws":
					case ".dot":
					case ".json":
					case ".txt":
					case ".manifest":
						await Test<T>(FileName, true, Watch, i == c - 1);
						break;
				}
			}

			Console.Out.WriteLine("];");

			Console.Out.Write(VariableName);
			Console.Out.Write("Size:=scatter2d(");
			Console.Out.Write(VariableName);
			Console.Out.Write("[0,],");
			Console.Out.Write(VariableName);
			Console.Out.Write("[1,],\"");
			Console.Out.Write(Color);
			Console.Out.WriteLine("\");");
			Console.Out.Write(VariableName);
			Console.Out.WriteLine("Size.LabelX:='File (Bytes)';");
			Console.Out.Write(VariableName);
			Console.Out.WriteLine("Size.LabelY:='Compressed (Bytes)';");
			Console.Out.Write(VariableName);
			Console.Out.WriteLine("Size.Title:='Size';");

			Console.Out.Write(VariableName);
			Console.Out.Write("Time:=scatter2d(");
			Console.Out.Write(VariableName);
			Console.Out.Write("[0,],");
			Console.Out.Write(VariableName);
			Console.Out.Write("[2,],\"");
			Console.Out.Write(Color);
			Console.Out.WriteLine("\");");
			Console.Out.Write(VariableName);
			Console.Out.WriteLine("Time.LabelX:='File (Bytes)';");
			Console.Out.Write(VariableName);
			Console.Out.WriteLine("Time.LabelY:='Time (ms)';");
			Console.Out.Write(VariableName);
			Console.Out.WriteLine("Time.Title:='Time';");
		}

		private static async Task Test<T>(string FilePath, bool Output, Stopwatch Watch, bool Last)
			where T : IContentEncoding, new()
		{
			long StartTicks = Watch.ElapsedTicks;

			byte[] Data = File.ReadAllBytes(FilePath);

			T ContentEncoding = new();
			MemoryStream Result = new();
			InternalTransfer TransferEncoding = new(Result);
			TransferEncoding Compressor = ContentEncoding.GetEncoder(TransferEncoding, Data.Length, null);

			await Compressor.EncodeAsync(Data, 0, Data.Length);
			await Compressor.FlushAsync();

			byte[] Compressed = Result.ToArray();
			double TicksPerMs = 1000.0 * (Watch.ElapsedTicks - StartTicks) / Stopwatch.Frequency;

			if (Output)
			{
				Console.Out.Write("\t[");
				Console.Out.Write(Data.Length);
				Console.Out.Write(',');
				Console.Out.Write(Compressed.Length);
				Console.Out.Write(',');
				Console.Out.Write(CommonTypes.Encode(TicksPerMs));
				Console.Out.Write(",\"");
				Console.Out.Write(JSON.Encode(FilePath));
				Console.Out.Write("\"]");

				if (!Last)
					Console.Out.Write(',');

				Console.Out.WriteLine();
			}
		}

	}
}
