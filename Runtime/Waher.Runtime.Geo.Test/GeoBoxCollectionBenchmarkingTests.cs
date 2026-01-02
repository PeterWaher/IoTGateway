using System.Text;
using Waher.Runtime.Profiling;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Runtime.Geo.Test
{
	[TestClass]
	public class GeoBoxCollectionBenchmarkingTests
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
			30000,
			40000,
			50000,
			60000,
			//100000,
			//200000,
			//300000,
			//400000,
			//500000,
			//600000,
			//700000,
			//800000,
			//900000,
			//1000000
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
			GeoBoxCollection<GeoBoundingBox> GeoBoxCollection;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					GeoBoxCollection = [];

					using Benchmarking Test = Benchmarker.Start("GeoBoxCollection", N);

					for (i = 0; i < N; i++)
					{
						GeoBoundingBox Box = GeoPositionCollectionBenchmarkingTests.RandomBoundingBox(10, 10);
						GeoBoxCollection.Add(Box);
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Add()");
		}

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
			GeoBoxCollection<GeoBoundingBox> GeoBoxCollection;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					GeoBoxCollection = [];

					for (i = 0; i < N; i++)
						GeoBoxCollection.Add(GeoPositionCollectionBenchmarkingTests.RandomBoundingBox(10, 10));

					using Benchmarking Test = Benchmarker.Start("GeoBoxCollection", N);

					foreach (GeoBoundingBox _ in GeoBoxCollection)
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
			GeoBoxCollection<GeoBoundingBox> GeoBoxCollection;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					GeoBoxCollection = [];

					for (i = 0; i < N; i++)
						GeoBoxCollection.Add(GeoPositionCollectionBenchmarkingTests.RandomBoundingBox(10, 10));

					using Benchmarking Test = Benchmarker.Start("GeoBoxCollection", N);

					for (i = 0; i < NrFind; i++)
						GeoBoxCollection.Find(GeoPositionCollectionBenchmarkingTests.RandomPosition().Location);
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Find() ⨯ " + NrFind.ToString());
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
			Script.Append("L:=legend([\"GeoBoxCollection\"");
			Script.Append("],[\"Red\"");
			Script.AppendLine("],\"White\",1);");

			if (!Directory.Exists("OutputBox"))
				Directory.CreateDirectory("OutputBox");

			File.WriteAllText("OutputBox\\Script_" + BaseFileName + ".script", Script.ToString());

			Variables Variables = [];

			await Expression.EvalAsync(Script.ToString(), Variables);

			Graph G = (Graph)Variables["G"];
			Graph Legend = (Graph)Variables["L"];
			GraphSettings Settings = new()
			{
				Width = 1280,
				Height = 720
			};

			await File.WriteAllBytesAsync("OutputBox\\G_" + BaseFileName + ".png",
				G.CreatePixels(Settings).EncodeAsPng());

			if (!File.Exists("OutputBox\\Legend.png"))
			{
				await File.WriteAllBytesAsync("OutputBox\\Legend.png",
					Legend.CreatePixels().EncodeAsPng());
			}
		}

		[TestMethod]
		[Ignore]
		public async Task Test_07_Configuration()
		{
			Benchmarker3D Benchmarker = new();

			lock (syncObj)
			{
				ConfigTest(Benchmarker, "JIT", 2, 2, 10);

				for (int GridSize = 2; GridSize <= 12; GridSize++)
				{
					for (int MaxCellCount = 1; MaxCellCount <= 12; MaxCellCount++)
					{
						for (int N = 1000; N <= 20000; N *= 2)
						{
							ConfigTest(Benchmarker, "GeoBoxCollection(" +
								N.ToString() + ")", GridSize, MaxCellCount, N);
						}
					}
				}
			}

			Benchmarker.Remove("JIT");   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, "Test_07_Configuration", "Configuration");
		}

		private static void ConfigTest(Benchmarker3D Benchmarker, string Name, int GridSize,
			int MaxCellCount, int N)
		{
			GeoBoxCollection<GeoBoundingBox> GeoBoxCollection = new(GridSize, MaxCellCount);
			int i;

			using Benchmarking Test = Benchmarker.Start(Name, GridSize, MaxCellCount);

			for (i = 0; i < N; i++)
				GeoBoxCollection.Add(GeoPositionCollectionBenchmarkingTests.RandomBoundingBox(10, 10));

			for (i = 0; i < 10000; i++)
				GeoBoxCollection.Find(GeoPositionCollectionBenchmarkingTests.RandomPosition().Location);
		}

		private static async Task OutputResults(Benchmarker3D Benchmarker, string BaseFileName,
			string Title)
		{
			StringBuilder Script = new();

			Script.Append("M:=");
			Script.Append(Benchmarker.GetResultScriptMilliseconds());
			Script.AppendLine(";");
			Script.AppendLine("GraphWidth:=1280;");
			Script.AppendLine("GraphHeight:=768;");
			Script.AppendLine();
			Script.AppendLine("foreach P in M do");
			Script.AppendLine("(");
			Script.AppendLine("	A:=P.Value;");
			Script.AppendLine("	GridSizes:=A[0,1..(A.Rows-1)];");
			Script.AppendLine("	MaxCellCounts:=A[1..(A.Columns-1),0];");
			Script.AppendLine("	Times:=A[1..(A.Columns-1),1..(A.Rows-1)];");
			Script.AppendLine("	G:=VerticalBars3D(Columns(MaxCellCounts,count(GridSizes)),Times,Rows(GridSizes,count(MaxCellCounts)));");
			Script.AppendLine("	G.LabelX:=\"Grid Size\";");
			Script.AppendLine("	G.LabelY:=\"Time (ms)\";");
			Script.AppendLine("	G.LabelZ:=\"Max Cell Count\";");
			Script.AppendLine("	G.Title:=P.Key;");
			Script.AppendLine();
			Script.AppendLine("	SaveFile(G,\"OutputBox\\\"+P.Key+\".png\");");
			Script.AppendLine();
			Script.AppendLine("\tMinValue:=Min(Times);");
			Script.AppendLine("	[MinC,MinR]:=IndexOf(Times,MinValue);");
			Script.AppendLine("	printline(P.Key+\", Min: \"+MinValue+\", Column: \"+MinC+\", Row: \"+MinR+\", GridSize: \"+GridSizes[MinC]+\", MaxCellCount: \"+MaxCellCounts[MinR]);");
			Script.AppendLine();
			Script.AppendLine("	MaxValue:=Max(Times);");
			Script.AppendLine("	[MaxC,MaxR]:=IndexOf(Times,MaxValue);");
			Script.AppendLine("	printline(P.Key+\", Max: \"+MaxValue+\", Column: \"+MaxC+\", Row: \"+MaxR+\", GridSize: \"+GridSizes[MaxC]+\", MaxCellCount: \"+MaxCellCounts[MaxR]);");
			Script.AppendLine("	printline();");
			Script.AppendLine(")");

			if (!Directory.Exists("OutputBox"))
				Directory.CreateDirectory("OutputBox");

			File.WriteAllText("OutputBox\\Script_" + BaseFileName + ".script", Script.ToString());

			Variables Variables = [];
			Variables.ConsoleOut = Console.Out;

			await Expression.EvalAsync(Script.ToString(), Variables);
		}

	}
}
