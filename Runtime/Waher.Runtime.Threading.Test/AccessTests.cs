using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Waher.Runtime.Threading.Test
{
	[TestClass]
	public class AccessTests
	{
		[TestMethod]
		public async Task Test_01_Read()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				Assert.AreEqual(0, obj.NrReaders);
				await obj.BeginRead();
				Assert.AreEqual(1, obj.NrReaders);
				await obj.EndRead();
				Assert.AreEqual(0, obj.NrReaders);
			}
		}

		[TestMethod]
		public async Task Test_02_Write()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				Assert.AreEqual(0, obj.NrWriters);
				await obj.BeginWrite();
				Assert.AreEqual(1, obj.NrWriters);
				await obj.EndWrite();
				Assert.AreEqual(0, obj.NrWriters);
			}
		}

		[TestMethod]
		public async Task Test_03_MultiRead()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				Assert.AreEqual(0, obj.NrReaders);
				await obj.BeginRead();
				Assert.AreEqual(1, obj.NrReaders);
				await obj.BeginRead();
				Assert.AreEqual(2, obj.NrReaders);
				await obj.EndRead();
				Assert.AreEqual(1, obj.NrReaders);
				await obj.EndRead();
				Assert.AreEqual(0, obj.NrReaders);
			}
		}

		[TestMethod]
		public async Task Test_04_ReadWrite()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.AreEqual(0, obj.NrReaders);
						await obj.BeginRead();
						Assert.AreEqual(1, obj.NrReaders);
						Thread.Sleep(2000);
						Assert.AreEqual(1, obj.NrReaders);
						await obj.EndRead();
						Assert.AreEqual(0, obj.NrReaders);
						Done1.SetResult(true);
					}
					catch (Exception ex)
					{
						Done1.SetException(ex);
					}
				});

				Thread T2 = new Thread(async () =>
				{
					try
					{
						Thread.Sleep(1000);
						Assert.AreEqual(1, obj.NrReaders);
						Assert.AreEqual(0, obj.NrWriters);
						await obj.BeginWrite();
						Assert.AreEqual(0, obj.NrReaders);
						Assert.AreEqual(1, obj.NrWriters);
						await obj.EndWrite();
						Assert.AreEqual(0, obj.NrWriters);
						Done2.SetResult(true);
					}
					catch (Exception ex)
					{
						Done2.SetException(ex);
					}
				});

				T1.Start();
				T2.Start();

				Assert.IsTrue(await Done1.Task);
				Assert.IsTrue(await Done2.Task);
			}
		}

		[TestMethod]
		public async Task Test_05_WriteRead()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.AreEqual(0, obj.NrWriters);
						await obj.BeginWrite();
						Assert.AreEqual(1, obj.NrWriters);
						Thread.Sleep(2000);
						Assert.AreEqual(1, obj.NrWriters);
						await obj.EndWrite();
						Assert.AreEqual(0, obj.NrWriters);
						Done1.SetResult(true);
					}
					catch (Exception ex)
					{
						Done1.SetException(ex);
					}
				});

				Thread T2 = new Thread(async () =>
				{
					try
					{
						Thread.Sleep(1000);
						Assert.AreEqual(1, obj.NrWriters);
						Assert.AreEqual(0, obj.NrReaders);
						await obj.BeginRead();
						Assert.AreEqual(0, obj.NrWriters);
						Assert.AreEqual(1, obj.NrReaders);
						await obj.EndRead();
						Assert.AreEqual(0, obj.NrReaders);
						Done2.SetResult(true);
					}
					catch (Exception ex)
					{
						Done2.SetException(ex);
					}
				});

				T1.Start();
				T2.Start();

				Assert.IsTrue(await Done1.Task);
				Assert.IsTrue(await Done2.Task);
			}
		}

		[TestMethod]
		public async Task Test_06_WriteWrite()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.AreEqual(0, obj.NrWriters);
						await obj.BeginWrite();
						Assert.AreEqual(1, obj.NrWriters);
						Thread.Sleep(2000);
						Assert.AreEqual(1, obj.NrWriters);
						await obj.EndWrite();
						Assert.AreEqual(0, obj.NrWriters);
						Done1.SetResult(true);
					}
					catch (Exception ex)
					{
						Done1.SetException(ex);
					}
				});

				Thread T2 = new Thread(async () =>
				{
					try
					{
						Thread.Sleep(1000);
						Assert.AreEqual(1, obj.NrWriters);
						await obj.BeginWrite();
						Assert.AreEqual(1, obj.NrWriters);
						await obj.EndWrite();
						Assert.AreEqual(0, obj.NrWriters);
						Done2.SetResult(true);
					}
					catch (Exception ex)
					{
						Done2.SetException(ex);
					}
				});

				T1.Start();
				T2.Start();

				Assert.IsTrue(await Done1.Task);
				Assert.IsTrue(await Done2.Task);
			}
		}

		[TestMethod]
		public async Task Test_07_ReadReadWrite()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.AreEqual(0, obj.NrReaders);
						await obj.BeginRead();
						Assert.AreEqual(1, obj.NrReaders);
						await obj.BeginRead();
						Assert.AreEqual(2, obj.NrReaders);
						Thread.Sleep(2000);
						Assert.AreEqual(2, obj.NrReaders);
						await obj.EndRead();
						Assert.AreEqual(1, obj.NrReaders);
						Thread.Sleep(2000);
						Assert.AreEqual(1, obj.NrReaders);
						await obj.EndRead();
						Assert.AreEqual(0, obj.NrReaders);
						Done1.SetResult(true);
					}
					catch (Exception ex)
					{
						Done1.SetException(ex);
					}
				});

				Thread T2 = new Thread(async () =>
				{
					try
					{
						Thread.Sleep(1000);
						Assert.AreEqual(2, obj.NrReaders);
						Assert.AreEqual(0, obj.NrWriters);
						await obj.BeginWrite();
						Assert.AreEqual(0, obj.NrReaders);
						Assert.AreEqual(1, obj.NrWriters);
						await obj.EndWrite();
						Assert.AreEqual(0, obj.NrWriters);
						Done2.SetResult(true);
					}
					catch (Exception ex)
					{
						Done2.SetException(ex);
					}
				});

				T1.Start();
				T2.Start();

				Assert.IsTrue(await Done1.Task);
				Assert.IsTrue(await Done2.Task);
			}
		}

		[TestMethod]
		public async Task Test_08_RandomLoad()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				LinkedList<TaskCompletionSource<bool>> Tasks = new LinkedList<TaskCompletionSource<bool>>();
				Random rnd = new Random();
				int i;
				int NrReads = 0;
				int NrWrites = 0;

				for (i = 0; i < 100; i++)
				{
					TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
					Thread T = new Thread(async (P) =>
					{
						try
						{
							DateTime Start = DateTime.Now;
							int j;

							while ((DateTime.Now - Start).TotalSeconds < 30)
							{
								lock (rnd)
								{
									j = rnd.Next(1, 200);
								}

								if (j > 100)
								{
									await obj.BeginWrite();
									Assert.AreEqual(0, obj.NrReaders);
									Assert.AreEqual(1, obj.NrWriters);
									NrWrites++;
									Thread.Sleep(10);
									await obj.EndWrite();
									Thread.Sleep(j - 100);
								}
								else
								{
									await obj.BeginRead();
									Assert.AreEqual(0, obj.NrWriters);
									Assert.IsTrue(obj.NrReaders > 0);
									NrReads++;
									Thread.Sleep(10);
									await obj.EndRead();
									Thread.Sleep(j);
								}
							}

							((TaskCompletionSource<bool>)P).SetResult(true);
						}
						catch (Exception ex)
						{
							((TaskCompletionSource<bool>)P).SetException(ex);
						}
					});

					Tasks.AddLast(Result);
					T.Start(Result);
				}

				foreach (TaskCompletionSource<bool> Task in Tasks)
					Assert.IsTrue(await Task.Task);

				Console.Out.WriteLine("Nr reads: " + NrReads.ToString());
				Console.Out.WriteLine("Nr writes: " + NrWrites.ToString());
			}
		}

	}
}
