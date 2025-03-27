using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Profiling;

namespace Waher.Runtime.Inventory.Test
{
	[TestClass]
	public class ChunkedListBenchmarkingTests
	{
		private static readonly int[] numberOfItems = new int[]
		{
			10,
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
		};

		[TestMethod]
		public void Test_01_Add()
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			foreach (int N in numberOfItems)
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

			PrintScript(Benchmarker);
		}

		private static void PrintScript(Benchmarker2D Benchmarker)
		{
			Benchmarker.Remove(10);

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
			Script.AppendLine("r3:=100*t3./t1;");
			Script.AppendLine("GRel:=plot2dcurve(N,r2,\"Green\")+scatter2d(N,r2,\"Green\",5)+");
			Script.AppendLine("plot2dcurve(N,r3,\"Blue\")+scatter2d(N,r3,\"Blue\",5)+");
			Script.AppendLine("plot2dcurve(N,zeroes(count(N)),\"Black\");");
			Script.AppendLine("GRel.LabelX:=\"N\";");
			Script.AppendLine("GRel.LabelY:=\"Performance Increase (%)\";");
			Script.AppendLine("GRel.Title:=\"Add()\";");
			Script.AppendLine("LRel:=legend([\"ChunkedList vs LinkedList\",\"ChunkedList vs List\"],[\"Green\",\"Blue\"],\"White\",1);");
			Script.AppendLine("GRel;");

			Console.Out.WriteLine(Script.ToString());
		}
	}
}
