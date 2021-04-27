using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
				long Token = obj.Token;

				Assert.AreEqual(0, obj.NrReaders);

				await obj.BeginRead();
				Assert.AreEqual(1, obj.NrReaders);
				Assert.AreEqual(Token + 1, obj.Token);

				await obj.EndRead();
				Assert.AreEqual(0, obj.NrReaders);
				Assert.AreEqual(Token + 2, obj.Token);
			}
		}

		[TestMethod]
		public async Task Test_02_Write()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				long Token = obj.Token;

				Assert.IsFalse(obj.IsWriting);

				await obj.BeginWrite();
				Assert.IsTrue(obj.IsWriting);
				Assert.AreEqual(Token + 1, obj.Token);

				await obj.EndWrite();
				Assert.IsFalse(obj.IsWriting);
				Assert.AreEqual(Token + 2, obj.Token);
			}
		}

		[TestMethod]
		public async Task Test_03_MultiRead()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				long Token = obj.Token;

				Assert.AreEqual(0, obj.NrReaders);

				await obj.BeginRead();
				Assert.AreEqual(1, obj.NrReaders);
				Assert.AreEqual(Token + 1, obj.Token);

				await obj.BeginRead();
				Assert.AreEqual(2, obj.NrReaders);
				Assert.AreEqual(Token + 1, obj.Token);

				await obj.EndRead();
				Assert.AreEqual(1, obj.NrReaders);
				Assert.AreEqual(Token + 1, obj.Token);

				await obj.EndRead();
				Assert.AreEqual(0, obj.NrReaders);
				Assert.AreEqual(Token + 2, obj.Token);
			}
		}

		[TestMethod]
		public async Task Test_04_ReadWrite()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();
				long Token = obj.Token;

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.AreEqual(0, obj.NrReaders);

						await obj.BeginRead();
						Assert.AreEqual(1, obj.NrReaders);
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(Token + 1, obj.Token);

						Thread.Sleep(2000);
						Assert.AreEqual(1, obj.NrReaders);
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(Token + 1, obj.Token);

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
						Assert.IsFalse(obj.IsWriting);

						await obj.BeginWrite();
						Assert.AreEqual(0, obj.NrReaders);
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(Token + 3, obj.Token);

						Thread.Sleep(2000);
						Assert.AreEqual(0, obj.NrReaders);
						Assert.IsTrue(obj.IsWriting);

						await obj.EndWrite();
						Assert.IsFalse(obj.IsWriting);

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
				Assert.AreEqual(Token + 4, obj.Token);
			}
		}

		[TestMethod]
		public async Task Test_05_WriteRead()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();
				long Token = obj.Token;

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.IsFalse(obj.IsWriting);

						await obj.BeginWrite();
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(0, obj.NrReaders);
						Assert.AreEqual(Token + 1, obj.Token);

						Thread.Sleep(2000);
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(0, obj.NrReaders);

						await obj.EndWrite();
						Assert.IsFalse(obj.IsWriting);

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
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(0, obj.NrReaders);

						await obj.BeginRead();
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(1, obj.NrReaders);
						Assert.AreEqual(Token + 3, obj.Token);

						Thread.Sleep(2000);
						Assert.IsFalse(obj.IsWriting);
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
				Assert.AreEqual(Token + 4, obj.Token);
			}
		}

		[TestMethod]
		public async Task Test_06_WriteWrite()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();
				long Token = obj.Token;

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.IsFalse(obj.IsWriting);

						await obj.BeginWrite();
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(0, obj.NrReaders);
						Assert.AreEqual(Token + 1, obj.Token);

						Thread.Sleep(2000);
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(0, obj.NrReaders);

						await obj.EndWrite();
						Assert.IsFalse(obj.IsWriting);
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
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(0, obj.NrReaders);

						await obj.BeginWrite();
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(0, obj.NrReaders);
						Assert.AreEqual(Token + 3, obj.Token);

						await obj.EndWrite();
						Assert.IsFalse(obj.IsWriting);
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
				Assert.AreEqual(Token + 4, obj.Token);
			}
		}

		[TestMethod]
		public async Task Test_07_ReadReadWrite()
		{
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				TaskCompletionSource<bool> Done1 = new TaskCompletionSource<bool>();
				TaskCompletionSource<bool> Done2 = new TaskCompletionSource<bool>();
				long Token = obj.Token;

				Thread T1 = new Thread(async () =>
				{
					try
					{
						Assert.AreEqual(0, obj.NrReaders);

						await obj.BeginRead();
						Assert.AreEqual(1, obj.NrReaders);
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(Token + 1, obj.Token);

						await obj.BeginRead();
						Assert.AreEqual(2, obj.NrReaders);
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(Token + 1, obj.Token);

						Thread.Sleep(2000);
						Assert.AreEqual(2, obj.NrReaders);
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(Token + 1, obj.Token);

						await obj.EndRead();
						Assert.AreEqual(1, obj.NrReaders);
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(Token + 1, obj.Token);

						Thread.Sleep(2000);
						Assert.AreEqual(1, obj.NrReaders);
						Assert.IsFalse(obj.IsWriting);
						Assert.AreEqual(Token + 1, obj.Token);

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
						Assert.IsFalse(obj.IsWriting);

						await obj.BeginWrite();
						Assert.AreEqual(0, obj.NrReaders);
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(Token + 3, obj.Token);

						Thread.Sleep(2000);
						Assert.AreEqual(0, obj.NrReaders);
						Assert.IsTrue(obj.IsWriting);
						Assert.AreEqual(Token + 3, obj.Token);

						await obj.EndWrite();
						Assert.IsFalse(obj.IsWriting);
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
				Assert.AreEqual(Token + 4, obj.Token);
			}
		}

		private class Counter
		{
			private readonly object synchObj = new object();
			private long token;

			public Counter(long StartValue)
			{
				this.token = StartValue;
			}

			public long Token
			{
				get
				{
					lock (this.synchObj)
					{
						return this.token;
					}
				}
			}

			public long PreInc()
			{
				lock (this.synchObj)
				{
					return ++this.token;
				}
			}

			public long PostInc()
			{
				lock (this.synchObj)
				{
					return this.token++;
				}
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
				Counter Counter = new Counter(obj.Token);

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
									Assert.IsTrue(obj.IsWriting);
									Assert.AreEqual(Counter.PreInc(), obj.Token);
									NrWrites++;

									Thread.Sleep(10);
									Assert.AreEqual(0, obj.NrReaders);
									Assert.IsTrue(obj.IsWriting);
									Assert.AreEqual(Counter.PostInc(), obj.Token);

									await obj.EndWrite();

									Thread.Sleep(j - 100);
								}
								else
								{
									bool Recursive = false;

									int NrReaders = await obj.BeginRead();
									Assert.IsFalse(obj.IsWriting);

									switch (NrReaders)
									{
										case 0:
											Assert.Fail();
											break;

										case 1:
											Assert.AreEqual(Counter.PreInc(), obj.Token);
											break;

										default:
											Assert.AreEqual(Counter.Token, obj.Token);
											Recursive = true;
											break;
									}
									NrReads++;

									Thread.Sleep(10);
									Assert.IsFalse(obj.IsWriting);
									Assert.IsTrue(obj.NrReaders > 0);

									Assert.AreEqual(Recursive ? Counter.Token : Counter.PostInc(), obj.Token);

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
				Assert.AreEqual(Counter.Token, obj.Token);
			}
		}

	}
}
