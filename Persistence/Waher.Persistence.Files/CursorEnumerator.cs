using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Cursor enumerator
	/// </summary>
	public class CursorEnumerator<T> : IEnumerator<T>, IEnumerator
	{
		private ICursor<T> cursor;

		/// <summary>
		/// Cursor enumerator
		/// </summary>
		/// <param name="Cursor">Cursor.</param>
		public CursorEnumerator(ICursor<T> Cursor)
		{
			this.cursor = Cursor;
		}

		public T Current
		{
			get
			{
				return this.cursor.Current;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return this.cursor.Current;
			}
		}

		public void Dispose()
		{
			this.cursor.Dispose();
		}

		public bool MoveNext()
		{
			Task<bool> Task = this.cursor.MoveNextAsync();
			Task.Wait();
			return Task.Result;
		}

		public void Reset()
		{
			throw new InvalidOperationException("Forward-only cursors cannot be reset.");
		}
	}
}
