using System.Text;
using Waher.Layout.Layout2D.Functions;
using Waher.Runtime.Inventory;
using Waher.Runtime.Profiling;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Runtime.Collections.Test
{
	[TestClass]
	public class ChunkedListBenchmarkingTests
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
			15,
			20,
			30,
			40,
			50,
			60,
			70,
			80,
			90,
			100,
		];
		private static readonly int[] largeNumberOfItems =
		[
			99,		// To ensure all code is JIT compiled
			100,
			200,
			500,
			1000,
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

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Expression).Assembly,
				typeof(Graph).Assembly,
				typeof(Legend).Assembly);
		}

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
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			foreach (int N in NumberOfItems)
			{
				LinkedList = [];
				List = [];
				ChunkedList = [];

				using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
				{
					for (i = 0; i < N; i++)
						LinkedList.AddLast(i);
				}

				using (Benchmarking Test = Benchmarker.Start("List", N))
				{
					for (i = 0; i < N; i++)
						List.Add(i);
				}

				using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
				{
					for (i = 0; i < N; i++)
						ChunkedList.Add(i);
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name);
		}

		private static async Task OutputResults(Benchmarker2D Benchmarker, string FileNamePrefix)
		{
			StringBuilder Script = new();
			Script.Append("M:=");
			Script.Append(Benchmarker.GetResultScriptMilliseconds());
			Script.AppendLine(";");
			Script.AppendLine("M2:=M[,1..(M.Rows-1)];");
			Script.AppendLine("N:=M2[0,];");
			Script.AppendLine("t1:=M2[1,];");
			Script.AppendLine("t2:=M2[2,];");
			Script.AppendLine("t3:=M2[3,];");
			Script.AppendLine("G:=plot2dcurve(N,t1,\"Red\")+scatter2d(N,t1,\"Red\",5)+");
			Script.AppendLine("plot2dcurve(N,t2,\"Green\")+scatter2d(N,t2,\"Green\",5)+");
			Script.AppendLine("plot2dcurve(N,t3,\"Blue\")+scatter2d(N,t3,\"Blue\",5)+");
			Script.AppendLine("plot2dcurve(N,zeroes(count(N)),\"Black\");");
			Script.AppendLine("G.LabelX:=\"N\";");
			Script.AppendLine("G.LabelY:=\"Time (ms)\";");
			Script.AppendLine("G.Title:=\"Add()\";");
			Script.AppendLine("L:=legend([\"ChunkedList\",\"LinkedList\",\"List\"],[\"Red\",\"Green\",\"Blue\"],\"White\",1);");
			Script.AppendLine("r2:=100*t2./t1;");
			Script.AppendLine("r2:=r2>500?500:r2;");
			Script.AppendLine("r3:=100*t3./t1;");
			Script.AppendLine("GRel:=plot2dcurve(N,r2,\"Green\")+scatter2d(N,r2,\"Green\",5)+");
			Script.AppendLine("plot2dcurve(N,r3,\"Blue\")+scatter2d(N,r3,\"Blue\",5)+");
			Script.AppendLine("plot2dcurve(N,zeroes(count(N)),\"Black\")+");
			Script.AppendLine("plot2dcurve(N,100*ones(count(N)),\"Black\");");
			Script.AppendLine("GRel.LabelX:=\"N\";");
			Script.AppendLine("GRel.LabelY:=\"Performance Increase (%)\";");
			Script.AppendLine("GRel.Title:=\"Add()\";");
			Script.AppendLine("LRel:=legend([\"ChunkedList vs LinkedList\",\"ChunkedList vs List\"],[\"Green\",\"Blue\"],\"White\",1);");
			Script.AppendLine("GRel;");

			if (!Directory.Exists("Output"))
				Directory.CreateDirectory("Output");

			File.WriteAllText("Output\\" + FileNamePrefix + ".script", Script.ToString());

			Variables Variables = [];

			await Expression.EvalAsync(Script.ToString(), Variables);

			Graph G = (Graph)Variables["G"];
			Graph GRel = (Graph)Variables["GRel"];
			Graph Legend = (Graph)Variables["L"];
			Graph LegendRel = (Graph)Variables["LRel"];
			GraphSettings Settings = new()
			{
				Width = 1280,
				Height = 720
			};

			await File.WriteAllBytesAsync("Output\\" + FileNamePrefix + "_G.png",
				G.CreatePixels(Settings).EncodeAsPng());

			await File.WriteAllBytesAsync("Output\\" + FileNamePrefix + "_GRel.png",
				GRel.CreatePixels(Settings).EncodeAsPng());

			await File.WriteAllBytesAsync("Output\\" + FileNamePrefix + "_Legend.png",
				Legend.CreatePixels().EncodeAsPng());

			await File.WriteAllBytesAsync("Output\\" + FileNamePrefix + "_LegendRel.png",
				LegendRel.CreatePixels().EncodeAsPng());
		}
	}
}
