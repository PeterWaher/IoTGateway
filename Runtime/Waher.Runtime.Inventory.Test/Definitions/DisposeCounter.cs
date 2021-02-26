using System;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[Singleton]
	public class DisposeCounter : IDisposable
	{
		private int a;
		private int b;
		private int count;

		public DisposeCounter(int A, int B)
		{
			this.a = A;
			this.b = B;
			this.count = 0;
		}

		public int A => this.a;
		public int B => this.b;
		public int Count => this.count;

		public void Dispose()
		{
			this.count++;
		}
	}
}
