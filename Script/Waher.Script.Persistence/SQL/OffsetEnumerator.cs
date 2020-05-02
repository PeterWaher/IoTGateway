using System;
using System.Collections;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that skips a given number of result records.
	/// </summary>
	public class OffsetEnumerator : IResultSetEnumerator
	{
		private readonly IResultSetEnumerator e;
		private readonly int offset0;
		private int offset;

		/// <summary>
		/// Enumerator that skips a given number of result records.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Offset">Number of records to skip.</param>
		public OffsetEnumerator(IResultSetEnumerator ItemEnumerator, int Offset)
		{
			this.e = ItemEnumerator;
			this.offset = this.offset0 = Offset;
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
			while (await this.e.MoveNextAsync())
			{
				if (this.offset > 0)
					this.offset--;
				else
					return true;
			}

			return false;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.offset = this.offset0;
			this.e.Reset();
		}
	}
}
