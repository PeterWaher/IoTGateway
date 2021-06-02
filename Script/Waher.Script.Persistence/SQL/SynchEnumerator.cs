using System;
using System.Collections;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Synchronous enumerator
	/// </summary>
	public class SynchEnumerator : IResultSetEnumerator
	{
		private readonly IEnumerator e;

		/// <summary>
		/// Synchronous enumerator
		/// </summary>
		/// <param name="e">Underlying enumerator.</param>
		public SynchEnumerator(IEnumerator e)
		{
			this.e = e;
		}

		/// <summary>
		/// Current item.
		/// </summary>
		public object Current => this.e.Current;

		/// <summary>
		/// Tries to move to next item.
		/// </summary>
		/// <returns>If successful</returns>
		public bool MoveNext()
		{
			return this.e.MoveNext();
		}

		/// <summary>
		/// Tries to move to next item.
		/// </summary>
		/// <returns>If successful</returns>
		public Task<bool> MoveNextAsync()
		{
			return Task.FromResult<bool>(this.e.MoveNext());
		}

		/// <summary>
		/// Resets the enumerator
		/// </summary>
		public void Reset()
		{
			this.e.Reset();
		}
	}
}
