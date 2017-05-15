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
		private int timeoutMilliseconds;

		/// <summary>
		/// Cursor enumerator
		/// </summary>
		/// <param name="Cursor">Cursor.</param>
		/// <param name="TimeoutMilliseconds">Time to wait to get access to underlying database.</param>
		public CursorEnumerator(ICursor<T> Cursor, int TimeoutMilliseconds)
		{
			this.cursor = Cursor;
			this.timeoutMilliseconds = TimeoutMilliseconds;
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
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		public void Reset()
		{
			throw new InvalidOperationException("Forward-only cursors cannot be reset.");
		}
	}
}
