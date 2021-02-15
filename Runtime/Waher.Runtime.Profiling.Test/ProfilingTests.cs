using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Runtime.Profiling.Test
{
	[TestClass]
	public class ProfilingTests
	{
		private readonly Random rnd = new Random();

		[TestMethod]
		public async Task Test_01_States()
		{
			Profiler Profiler = new Profiler();
			Profiler.Start();
			await Task.Delay(rnd.Next(10, 1000));
			Profiler.NewState("A");
			await Task.Delay(rnd.Next(10, 1000));
			Profiler.NewState("B");
			await Task.Delay(rnd.Next(10, 1000));
			Profiler.NewState("C");
			await Task.Delay(rnd.Next(10, 1000));
			Profiler.NewState("D");
			await Task.Delay(rnd.Next(10, 1000));
			Profiler.Stop();
			this.ExportResults(Profiler, "Test_01_States");
		}

		private void ExportResults(Profiler Profiler, string MethodName)
		{
			string Folder = Path.Combine("Results", "XML");

			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			File.WriteAllText(Path.Combine(Folder, MethodName + ".xml"),
				Profiler.ExportXml(TimeUnit.DynamicPerProfiling));

			Folder = Path.Combine("Results", "PlantUML");

			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			File.WriteAllText(Path.Combine(Folder, MethodName + ".uml"),
				Profiler.ExportPlantUml(TimeUnit.DynamicPerProfiling));
		}

		[TestMethod]
		public async Task Test_02_Levels()
		{
			Profiler Profiler = new Profiler(ProfilerThreadType.StateMachine);
			int i;

			Profiler.Start();
			await Task.Delay(rnd.Next(10, 1000));

			for (i = 0; i < 100; i++)
			{
				Profiler.NewState(new string((char)(rnd.Next(0, 10) + 'A'), 1));
				await Task.Delay(rnd.Next(1, 100));
			}

			Profiler.Stop();
			this.ExportResults(Profiler, "Test_02_Levels");
		}

		[TestMethod]
		public async Task Test_03_Events()
		{
			Profiler Profiler = new Profiler();
			Profiler.Start();

			Task T1 = Task.Run(async () =>
			{
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("A");
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("B");
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("C");
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("D");
				await Task.Delay(rnd.Next(10, 1000));
			});

			Task T2 = Task.Run(async () =>
			{
				int i;

				for (i = 0; i < 100; i++)
				{
					Profiler.Event(new string((char)(rnd.Next(0, 10) + 'A'), 1));
					await Task.Delay(rnd.Next(1, 100));
				}
			});

			await Task.WhenAll(T1, T2);

			Profiler.Stop();
			this.ExportResults(Profiler, "Test_03_Events");
		}

		[TestMethod]
		public async Task Test_04_Exceptions()
		{
			Profiler Profiler = new Profiler();
			Profiler.Start();

			Task T1 = Task.Run(async () =>
			{
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("A");
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("B");
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("C");
				await Task.Delay(rnd.Next(10, 1000));
				Profiler.NewState("D");
				await Task.Delay(rnd.Next(10, 1000));
			});

			Task T2 = Task.Run(async () =>
			{
				int i;

				for (i = 0; i < 100; i++)
				{
					Profiler.Exception(new InvalidOperationException("Invalid operation."));
					await Task.Delay(rnd.Next(1, 100));
				}
			});

			await Task.WhenAll(T1, T2);

			Profiler.Stop();
			this.ExportResults(Profiler, "Test_04_Exceptions");
		}

		[TestMethod]
		public async Task Test_05_SubThreads()
		{
			Profiler Profiler = new Profiler();
			Profiler.Start();

			Task[] Tasks = new Task[5];
			int i;

			for (i = 1; i <= 5; i++)
				Tasks[i - 1] = this.Subtask(Profiler.MainThread.CreateSubThread("Thread " + i.ToString(), ProfilerThreadType.Sequential));

			await Task.WhenAll(Tasks);

			Profiler.Stop();
			this.ExportResults(Profiler, "Test_05_SubThreads");
		}

		private async Task Subtask(ProfilerThread Thread)
		{
			await Task.Delay(rnd.Next(10, 1000));
			Thread.Start();
			Thread.NewState("A");
			await Task.Delay(rnd.Next(10, 1000));
			Thread.NewState("B");
			await Task.Delay(rnd.Next(10, 1000));
			Thread.NewState("C");
			await Task.Delay(rnd.Next(10, 1000));
			Thread.NewState("D");
			await Task.Delay(rnd.Next(10, 1000));
			Thread.Stop();
		}

		[TestMethod]
		public async Task Test_06_MultipleThreads()
		{
			Profiler Profiler = new Profiler();
			Profiler.Start();

			Task[] Tasks = new Task[5];
			int i;

			for (i = 1; i <= 5; i++)
				Tasks[i - 1] = this.Subtask(Profiler.CreateThread("Thread " + i.ToString(), ProfilerThreadType.Sequential));

			await Task.WhenAll(Tasks);

			Profiler.Stop();
			this.ExportResults(Profiler, "Test_06_MultipleThreads");
		}

	}
}
