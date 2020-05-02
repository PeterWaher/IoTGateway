using System;
using System.Collections;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that limits the return set to a maximum number of records.
	/// </summary>
	public class MaxCountEnumerator : IResultSetEnumerator
	{
		private readonly IResultSetEnumerator e;
		private readonly int count0;
		private int count;

		/// <summary>
		/// Enumerator that limits the return set to a maximum number of records.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Count">Maximum number of records to enumerate.</param>
		public MaxCountEnumerator(IResultSetEnumerator ItemEnumerator, int Count)
		{
			this.e = ItemEnumerator;
			this.count = this.count0 = Count;
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.e.Current;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			return this.MoveNextAsync().Result;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			if (this.count <= 0 || !await this.e.MoveNextAsync())
				return false;

			this.count--;
		
			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.count = this.count0;
			this.e.Reset();
		}
	}
}
