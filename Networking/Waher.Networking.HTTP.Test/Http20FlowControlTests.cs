using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Waher.Networking.HTTP.HTTP2;

namespace Waher.Networking.HTTP.Test
{
	/// <summary>
	/// Test flow control implementation for HTTP/2
	/// </summary>
	[TestClass]
	public class Http20FlowControlTests
	{
		[TestMethod]
		public async Task Test_01_BasicFlowControl()
		{
			ConnectionSettings Settings = new();
			using FlowControl FlowControl = new(Settings);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));
		}

		[TestMethod]
		public async Task Test_02_SiblingStreams()
		{
			ConnectionSettings Settings = new();
			using FlowControl FlowControl = new(Settings);

			FlowControl.AddStreamForTest(1);
			FlowControl.AddStreamForTest(3);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(2768, await FlowControl.RequestResources(1, 10000));
			
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(3, 10000));
			Assert.AreEqual(2767, await FlowControl.RequestResources(3, 10000));
		}

		[TestMethod]
		public async Task Test_03_ChildDependencies()
		{
			ConnectionSettings Settings = new();
			using FlowControl FlowControl = new(Settings);

			FlowControl.AddStreamForTest(1);
			FlowControl.AddStreamForTest(3);
			FlowControl.AddStreamForTest(5, 16, 3, false);
			FlowControl.AddStreamForTest(7, 16, 3, false);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(2768, await FlowControl.RequestResources(1, 10000));

			Assert.AreEqual(10000, await FlowControl.RequestResources(5, 10000));
			Assert.AreEqual(6384, await FlowControl.RequestResources(5, 10000));

			Assert.AreEqual(10000, await FlowControl.RequestResources(7, 10000));
			Assert.AreEqual(6383, await FlowControl.RequestResources(7, 10000));
		}

		[TestMethod]
		public async Task Test_04_Cancellation()
		{
			ConnectionSettings Settings = new();
			using FlowControl FlowControl = new(Settings);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			DateTime Start = DateTime.Now;
			CancellationTokenSource Cancel = new();
			Cancel.CancelAfter(1000);
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			{
				await FlowControl.RequestResources(1, 10000, Cancel.Token);
			});

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);
		}

		[TestMethod]
		public async Task Test_05_ReleaseStreamResources()
		{
			ConnectionSettings Settings = new();
			using FlowControl FlowControl = new(Settings);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Assert.IsTrue(FlowControl.ReleaseStreamResources(1, 10000));

			DateTime Start = DateTime.Now;
			CancellationTokenSource Cancel = new();
			Cancel.CancelAfter(1000);
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			{
				await FlowControl.RequestResources(1, 10000, Cancel.Token);
			});

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);
		}

		[TestMethod]
		public async Task Test_06_ReleaseConnectionResources()
		{
			ConnectionSettings Settings = new();
			using FlowControl FlowControl = new(Settings);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Assert.IsTrue(FlowControl.ReleaseConnectionResources(10000));

			DateTime Start = DateTime.Now;
			CancellationTokenSource Cancel = new();
			Cancel.CancelAfter(1000);
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
			{
				await FlowControl.RequestResources(1, 10000, Cancel.Token);
			});

			Assert.IsTrue(DateTime.Now.Subtract(Start).TotalSeconds >= 1);
		}


		[TestMethod]
		public async Task Test_07_ReleaseResources()
		{
			ConnectionSettings Settings = new();
			using FlowControl FlowControl = new(Settings);

			FlowControl.AddStreamForTest(1);

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
			Assert.AreEqual(5535, await FlowControl.RequestResources(1, 10000));

			Assert.IsTrue(FlowControl.ReleaseStreamResources(1, 10000));
			Assert.IsTrue(FlowControl.ReleaseConnectionResources(10000));

			Assert.AreEqual(10000, await FlowControl.RequestResources(1, 10000));
		}


		/* ReleaseConnectionResources
		 * ReleaseStreamResources
		 * RemoveStream
		 * TryGetPriorityNode
		 * TryGetStream
		 * UpdatePriority
		 * UpdateSettings
		 * Exclusive reorder
		 * asymetric weights
		 * Changing priority to exclusive
		 * changing dependency and solve circular references
		 * max frame size
		 */
	}
}
