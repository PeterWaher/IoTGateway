namespace Waher.Runtime.Collections.Test
{
	internal sealed class DisposableEnumerable : IEnumerable<int>, IEnumerator<int>
	{
		private readonly int[] values;
		private int index = -1;

		public bool Disposed { get; private set; }

		public DisposableEnumerable(params int[] values)
		{
			this.values = values;
		}

		public int Current => this.values[this.index];

		object System.Collections.IEnumerator.Current => this.Current;

		public IEnumerator<int> GetEnumerator()
		{
			return this;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool MoveNext()
		{
			this.index++;
			return this.index < this.values.Length;
		}

		public void Reset()
		{
			this.index = -1;
		}

		public void Dispose()
		{
			this.Disposed = true;
		}
	}
}
