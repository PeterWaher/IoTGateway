using System;
using System.Collections;
using System.Collections.Generic;
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

		/// <summary>
		/// The current element in the collection.
		/// </summary>
		public T Current
		{
			get
			{
				return this.cursor.Current;
			}
		}

		/// <summary>
		/// The current element in the collection.
		/// </summary>
		object IEnumerator.Current
		{
			get
			{
				return this.cursor.Current;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.cursor.Dispose();
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		public bool MoveNext()
		{
			Task<bool> Task = this.cursor.MoveNextAsync();
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		public void Reset()
		{
			throw new InvalidOperationException("Forward-only cursors cannot be reset.");
		}
	}
}
