using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Runtime.Profiling.Test
{
	[TestClass]
	public class ProfilingTests
	{
		private readonly Random rnd = new Random();

		[TestMethod]
		public void Test_01_States()
		{
			Profiler Profiler = new Profiler();
			Profiler.Start();
			this.DelayRandom();
			Profiler.NewState("A");
			this.DelayRandom();
			Profiler.NewState("B");
			this.DelayRandom();
			Profiler.NewState("C");
			this.DelayRandom();
			Profiler.NewState("D");
			this.DelayRandom();
			Profiler.Stop();


		}

		private void DelayRandom()
		{
			while (rnd.NextDouble() > 0.0001)
				;
		}

		private void ExportResults(Profiler Profiler)
		{
			StackTrace StackTrace = new StackTrace();
			StackFrame StackFrame = StackTrace.GetFrame(1);
			string MethodName = StackFrame.GetMethod().Name;

			File.WriteAllText(Path.Combine("Results", "XML", MethodName + ".xml"),
				Profiler.ExportXml());
		}
	}
}
