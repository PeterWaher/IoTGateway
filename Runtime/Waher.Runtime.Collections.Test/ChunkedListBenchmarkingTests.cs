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
			return Test_Add("Test_01_Add_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_02_Add_Large()
		{
			return Test_Add("Test_02_Add_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_Add(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			lock (syncObj)
			{
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
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Add()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_03_Enumerate_Small()
		{
			return Test_Enumerate("Test_03_Enumerate_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_04_Enumerate_Large()
		{
			return Test_Enumerate("Test_04_Enumerate_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_Enumerate(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						foreach (double _ in LinkedList)
							;
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						foreach (double _ in List)
							;
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						foreach (double _ in ChunkedList)
							;
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Enumerate()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_05_ForEach_Small()
		{
			return Test_ForEach("Test_05_ForEach_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_06_ForEach_Large()
		{
			return Test_ForEach("Test_06_ForEach_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_ForEach(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						foreach (double _ in LinkedList)
							;
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						List.ForEach((_) =>
						{
						});
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						ChunkedList.ForEach((_) => true);
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "ForEach()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_07_ForEachChunk_Small()
		{
			return Test_ForEachChunk("Test_07_ForEachChunk_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_08_ForEachChunk_Large()
		{
			return Test_ForEachChunk("Test_08_ForEachChunk_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_ForEachChunk(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						foreach (double _ in LinkedList)
							;
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						List.ForEach((_) =>
						{
						});
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						ChunkedList.ForEachChunk((Items, i, c) =>
						{
							while (c-- > 0)
							{
								double _ = Items[i++];
							}

							return true;
						});
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "ForEachChunk()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_09_Contains_Small()
		{
			return Test_Contains("Test_09_Contains_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_10_Contains_Large()
		{
			return Test_Contains("Test_10_Contains_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_Contains(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			double[] Items;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					Items = RandomOrder(N, 500);
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						foreach (double Item in Items)
						{
							if (!LinkedList.Contains(Item))
								throw new Exception("Item not found in LinkedList");
						}
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						foreach (double Item in Items)
						{
							if (!List.Contains(Item))
								throw new Exception("Item not found in List");
						}
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						foreach (double Item in Items)
						{
							if (!ChunkedList.Contains(Item))
								throw new Exception("Item not found in ChunkedList");
						}
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Contains()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_11_Remove_Small()
		{
			return Test_Remove("Test_11_Remove_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_12_Remove_Large()
		{
			return Test_Remove("Test_12_Remove_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_Remove(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			double[] Items;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					Items = RandomOrder(N, 500);
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						foreach (double Item in Items)
						{
							if (!LinkedList.Remove(Item))
								throw new Exception("Item not found in LinkedList");
						}
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						foreach (double Item in Items)
						{
							if (!List.Remove(Item))
								throw new Exception("Item not found in List");
						}
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						foreach (double Item in Items)
						{
							if (!ChunkedList.Remove(Item))
								throw new Exception("Item not found in ChunkedList");
						}
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "Remove()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_13_RemoveFirst_Small()
		{
			return Test_RemoveFirst("Test_13_RemoveFirst_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_14_RemoveFirst_Large()
		{
			return Test_RemoveFirst("Test_14_RemoveFirst_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_RemoveFirst(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						while (LinkedList.First is not null)
							LinkedList.RemoveFirst();
					}

					if (N <= 100000)
					{
						using Benchmarking Test = Benchmarker.Start("List", N);

						while (List.Count > 0)
							List.RemoveAt(0);
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						while (ChunkedList.HasFirstItem)
							ChunkedList.RemoveFirst();
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "RemoveFirst()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_15_RemoveLast_Small()
		{
			return Test_RemoveLast("Test_15_RemoveLast_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_16_RemoveLast_Large()
		{
			return Test_RemoveLast("Test_16_RemoveLast_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_RemoveLast(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						while (LinkedList.Last is not null)
							LinkedList.RemoveLast();
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						while ((i = List.Count) > 0)
							List.RemoveAt(i - 1);
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						while (ChunkedList.HasLastItem)
							ChunkedList.RemoveLast();
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "RemoveLast()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_17_AddFirst_Small()
		{
			return Test_AddFirst("Test_17_AddFirst_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_18_AddFirst_Large()
		{
			return Test_AddFirst("Test_18_AddFirst_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_AddFirst(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					LinkedList = [];
					List = [];
					ChunkedList = [];

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						for (i = 0; i < N; i++)
							LinkedList.AddFirst(i);
					}

					if (N <= 100000)
					{
						using Benchmarking Test = Benchmarker.Start("List", N);

						for (i = 0; i < N; i++)
							List.Insert(0, i);
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						for (i = 0; i < N; i++)
							ChunkedList.AddFirstItem(i);
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "AddFirst()", Limit, true, true);
		}

		[TestMethod]
		public Task Test_19_Index_Small()
		{
			return Test_Index("Test_19_Index_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_20_Index_Large()
		{
			return Test_Index("Test_20_Index_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_Index(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			double[] Items;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					Items = RandomOrder(N, 500);
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						for (i = 0; i < N; i++)
							List[i] = List[i];
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						for (i = 0; i < N; i++)
							List[i] = List[i];
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "this[Index]", Limit, false, true);
		}

		[TestMethod]
		public Task Test_21_IndexOf_Small()
		{
			return Test_IndexOf("Test_21_IndexOf_Small", smallNumberOfItems, 500);
		}

		[TestMethod]
		public Task Test_22_IndexOf_Large()
		{
			return Test_IndexOf("Test_22_IndexOf_Large", largeNumberOfItems, 2000);
		}

		private static async Task Test_IndexOf(string Name, int[] NumberOfItems, int Limit)
		{
			Benchmarker2D Benchmarker = new();
			LinkedList<double> LinkedList;
			List<double> List;
			ChunkedList<double> ChunkedList;
			double[] Items;
			int i;

			lock (syncObj)
			{
				foreach (int N in NumberOfItems)
				{
					Items = RandomOrder(N, 500);
					LinkedList = [];
					List = [];
					ChunkedList = [];

					for (i = 0; i < N; i++)
					{
						LinkedList.AddLast(i);
						List.Add(i);
						ChunkedList.Add(i);
					}

					using (Benchmarking Test = Benchmarker.Start("LinkedList", N))
					{
						foreach (double Item in Items)
						{
							if (LinkedList.Find(Item) is null)
								throw new Exception("Item not found in LinkedList");
						}
					}

					using (Benchmarking Test = Benchmarker.Start("List", N))
					{
						foreach (double Item in Items)
						{
							if (List.IndexOf(Item) < 0)
								throw new Exception("Item not found in List");
						}
					}

					using (Benchmarking Test = Benchmarker.Start("ChunkedList", N))
					{
						foreach (double Item in Items)
						{
							if (ChunkedList.IndexOf(Item) < 0)
								throw new Exception("Item not found in ChunkedList");
						}
					}
				}
			}

			Benchmarker.Remove(NumberOfItems[0]);   // May be affected by JIT compilation.

			await OutputResults(Benchmarker, Name, "IndexOf()", Limit, true, true);
		}

		private static double[] RandomOrder(int N, int MaxItems)
		{
			Random rnd = new();
			int c = Math.Min(N, MaxItems);
			double[] Result = new double[c];
			List<int> Left = [];
			int i, j, k;

			for (i = 0; i < N; i++)
				Left.Add(i);

			for (i = 0, j = N; i < c; i++, j--)
			{
				k = rnd.Next(j);
				Result[i] = Left[k];
				Left.RemoveAt(k);
			}

			return Result;
		}

		private static async Task OutputResults(Benchmarker2D Benchmarker, string BaseFileName,
			string Title, int Limit, bool IncludeLinkedList, bool IncludeList)
		{
			StringBuilder Script = new();

			Script.Append("M:=");
			Script.Append(Benchmarker.GetResultScriptMilliseconds());
			Script.AppendLine(";");
			Script.AppendLine("M2:=M[,1..(M.Rows-1)];");
			Script.AppendLine("c:=0;");
			Script.AppendLine("N:=M2[c++,];");
			Script.AppendLine("t1:=M2[c++,];");

			if (IncludeLinkedList)
				Script.AppendLine("t2:=M2[c++,];");

			if (IncludeList)
				Script.AppendLine("t3:=M2[c++,];");

			Script.AppendLine("G:=plot2dcurve(N,t1,\"Red\")+scatter2d(N,t1,\"Red\",5)+");

			if (IncludeLinkedList)
				Script.AppendLine("plot2dcurve(N,t2,\"Green\")+scatter2d(N,t2,\"Green\",5)+");

			if (IncludeList)
				Script.AppendLine("plot2dcurve(N,t3,\"Blue\")+scatter2d(N,t3,\"Blue\",5)+");

			Script.AppendLine("plot2dcurve(N,zeroes(count(N)),\"Black\");");
			Script.AppendLine("G.LabelX:=\"N\";");
			Script.AppendLine("G.LabelY:=\"Time (ms)\";");
			Script.Append("G.Title:=\"");
			Script.Append(Title);
			Script.AppendLine("\";");
			Script.Append("L:=legend([\"ChunkedList\"");
			if (IncludeLinkedList)
				Script.Append(",\"LinkedList\"");
			if (IncludeList)
				Script.Append(",\"List\"");
			Script.Append("],[\"Red\"");
			if (IncludeLinkedList)
				Script.Append(",\"Green\"");
			if (IncludeList)
				Script.Append(",\"Blue\"");
			Script.AppendLine("],\"White\",1);");

			if (IncludeLinkedList)
				Script.AppendLine("r2:=100*t2./t1;");

			if (IncludeList)
				Script.AppendLine("r3:=[foreach i in 0..(count(t3)-1): exists(t3[i]) ? 100*t3[i]/t1[i] : null];");

			Script.Append("GRel:=");

			if (IncludeLinkedList)
				Script.AppendLine("plot2dcurve(N,r2,\"Green\")+scatter2d(N,r2,\"Green\",5)+");

			if (IncludeList)
				Script.AppendLine("plot2dcurve(N,r3,\"Blue\")+scatter2d(N,r3,\"Blue\",5)+");

			Script.AppendLine("plot2dcurve(N,zeroes(count(N)),\"Black\")+");
			Script.AppendLine("plot2dcurve(N,100*ones(count(N)),\"Black\");");

			if (IncludeLinkedList && IncludeList)
				Script.Append("if Max(Max(r2),Max(r3))>");
			else if (IncludeLinkedList)
				Script.Append("if Max(r2)>");
			else
				Script.Append("if Max(r3)>");
			Script.Append(Limit);
			Script.AppendLine(" then GRel.MaxY:=");
			Script.Append(Limit);
			Script.AppendLine(";");
			Script.AppendLine("GRel.LabelX:=\"N\";");
			Script.AppendLine("GRel.LabelY:=\"Performance Increase (%)\";");
			Script.Append("GRel.Title:=\"");
			Script.Append(Title);
			Script.AppendLine("\";");
			Script.AppendLine("LRel:=legend([\"ChunkedList vs LinkedList\",\"ChunkedList vs List\"],[\"Green\",\"Blue\"],\"White\",1);");
			Script.AppendLine("GRel;");

			if (!Directory.Exists("Output"))
				Directory.CreateDirectory("Output");

			File.WriteAllText("Output\\Script_" + BaseFileName + ".script", Script.ToString());

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

			await File.WriteAllBytesAsync("Output\\G_" + BaseFileName + ".png",
				G.CreatePixels(Settings).EncodeAsPng());

			await File.WriteAllBytesAsync("Output\\GRel_" + BaseFileName + ".png",
				GRel.CreatePixels(Settings).EncodeAsPng());

			if (!File.Exists("Output\\Legend.png"))
			{
				await File.WriteAllBytesAsync("Output\\Legend.png",
					Legend.CreatePixels().EncodeAsPng());
			}

			if (!File.Exists("Output\\LegendRel.png"))
			{
				await File.WriteAllBytesAsync("Output\\LegendRel.png",
					LegendRel.CreatePixels().EncodeAsPng());
			}
		}
	}
}
