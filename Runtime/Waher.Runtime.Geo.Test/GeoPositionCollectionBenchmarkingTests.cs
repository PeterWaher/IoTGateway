﻿using System.Text;
using Waher.Runtime.Profiling;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Runtime.Geo.Test
{
	[TestClass]
	public class GeoPositionCollectionBenchmarkingTests
	{
		private static readonly int[] smallNumberOfItems =
		[
			99,		// To ensure all code is JIT compiled
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			12,
			14,
			16,
			18,
			20,
			25,
			30,
			35,
			40,
			45,
			50,
			55,
			60,
			65,
			70,
			75,
			80,
			85,
			90,
			95,
			100,
		];
		private static readonly int[] largeNumberOfItems =
		[
			99,		// To ensure all code is JIT compiled
			100,
			200,
			500,
			1000,
			2000,
			5000,
			10000,
			20000,
			50000,
			100000,
			200000,
			300000,
			400000,
			500000,
			600000,
			700000,
			800000,
			900000,
			1000000
		];
		private static readonly object syncObj = new();

		[TestMethod]
		public Task Test_01_Add_Small()
		{
			return Test_Add("Test_01_Add_Small", smallNumberOfItems);
		}

		[TestMethod]
		public Task Test_02_Add_Large()
		{
			return Test_Add("Test_02_Add_Large", largeNumberOfItems);
		}

		private static async Task Test_Add(string Name, int[] NumberOfItems)
		{
			Benchmarker2D Benchmarker = new();
			GeoPositionCollection<GeoSpatialObjectReference> GeoPositionCollection;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					GeoPositionCollection = [];

					using Benchmarking Test = Benchmarker.Start("GeoPositionCollection", N);
					
					for (i = 0; i < N; i++)
						GeoPositionCollection.Add(RandomPosition());
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Add()");
		}

		public static GeoSpatialObjectReference RandomPosition()
		{
			return new GeoSpatialObjectReference(
				new GeoSpatialObject(
					Guid.NewGuid().ToString(),
					new GeoPosition(rnd.NextDouble() * 180 - 90, rnd.NextDouble() * 360 - 180),
					true));
		}

		private static readonly Random rnd = new();

		[TestMethod]
		public Task Test_03_Enumerate_Small()
		{
			return Test_Enumerate("Test_03_Enumerate_Small", smallNumberOfItems);
		}

		[TestMethod]
		public Task Test_04_Enumerate_Large()
		{
			return Test_Enumerate("Test_04_Enumerate_Large", largeNumberOfItems);
		}

		private static async Task Test_Enumerate(string Name, int[] NumberOfItems)
		{
			Benchmarker2D Benchmarker = new();
			GeoPositionCollection<GeoSpatialObjectReference> GeoPositionCollection;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					GeoPositionCollection = [];

					for (i = 0; i < N; i++)
						GeoPositionCollection.Add(RandomPosition());

					using Benchmarking Test = Benchmarker.Start("GeoPositionCollection", N);

					foreach (GeoSpatialObjectReference _ in GeoPositionCollection)
						;
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Enumerate()");
		}

		[TestMethod]
		public Task Test_05_Find_Small()
		{
			return Test_Find("Test_05_Find_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_06_Find_Large()
		{
			return Test_Find("Test_06_Find_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_Find(string Name, int[] NumberOfItems, int NrFind)
		{
			Benchmarker2D Benchmarker = new();
			GeoPositionCollection<GeoSpatialObjectReference> GeoPositionCollection;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					GeoPositionCollection = [];

					for (i = 0; i < N; i++)
						GeoPositionCollection.Add(RandomPosition());

					using Benchmarking Test = Benchmarker.Start("GeoPositionCollection", N);

					for (i = 0; i < NrFind; i++)
						GeoPositionCollection.Find(RandomBoundingBox(10, 10));
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Find() ⨯ " + NrFind.ToString());
		}

		public static GeoBoundingBox RandomBoundingBox(int MaxDeltaLat, int MaxDeltaLong)
		{
			double MinLat, MinLong;
			double MaxLat, MaxLong;

			do
			{
				MinLat = rnd.NextDouble() * 180 - 90;
				MinLong = rnd.NextDouble() * 360 - 180;

				MaxLat = MinLat + rnd.NextDouble() * MaxDeltaLat;
				MaxLong = MinLong + rnd.NextDouble() * MaxDeltaLong;
			}
			while (MaxLat > 90);

			if (MaxLong > 180)
				MaxLong -= 360;

			return new GeoBoundingBox(
				new GeoPosition(MinLat, MinLong),
				new GeoPosition(MaxLat, MaxLong));
		}

		private static async Task OutputResults(Benchmarker2D Benchmarker, string BaseFileName,
			string Title)
		{
			StringBuilder Script = new();

			Script.Append("M:=");
			Script.Append(Benchmarker.GetResultScriptMilliseconds());
			Script.AppendLine(";");
			Script.AppendLine("M2:=M[,1..(M.Rows-1)];");
			Script.AppendLine("c:=0;");
			Script.AppendLine("N:=M2[c++,];");
			Script.AppendLine("t1:=M2[c++,];");
			Script.AppendLine("G:=plot2dcurve(N,t1,\"Red\")+scatter2d(N,t1,\"Red\",5)+");
			Script.AppendLine("plot2dcurve(N,zeroes(count(N)),\"Black\");");
			Script.AppendLine("G.LabelX:=\"N\";");
			Script.AppendLine("G.LabelY:=\"Time (ms)\";");
			Script.Append("G.Title:=\"");
			Script.Append(Title);
			Script.AppendLine("\";");
			Script.Append("L:=legend([\"GeoPositionCollection\"");
			Script.Append("],[\"Red\"");
			Script.AppendLine("],\"White\",1);");

			if (!Directory.Exists("OutputPosition"))
				Directory.CreateDirectory("OutputPosition");

			File.WriteAllText("OutputPosition\\Script_" + BaseFileName + ".script", Script.ToString());

			Variables Variables = [];

			await Expression.EvalAsync(Script.ToString(), Variables);

			Graph G = (Graph)Variables["G"];
			Graph Legend = (Graph)Variables["L"];
			GraphSettings Settings = new()
			{
				Width = 1280,
				Height = 720
			};

			await File.WriteAllBytesAsync("OutputPosition\\G_" + BaseFileName + ".png",
				G.CreatePixels(Settings).EncodeAsPng());

			if (!File.Exists("OutputPosition\\Legend.png"))
			{
				await File.WriteAllBytesAsync("OutputPosition\\Legend.png",
					Legend.CreatePixels().EncodeAsPng());
			}
		}
	}
}
