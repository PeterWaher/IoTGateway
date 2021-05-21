using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence.Files
{
	/// <summary>
	/// Delegate for reset methods.
	/// </summary>
	/// <typeparam name="T">Type argument</typeparam>
	/// <param name="ExistingCursor">Existing cursor</param>
	/// <returns>New cursor, or existing cursor reset to starting position.</returns>
	public delegate ICursor<T> GetNewCursorCallback<T>(ICursor<T> ExistingCursor);

	/// <summary>
	/// Cursor enumerator
	/// </summary>
	public class CursorEnumerator<T> : IEnumerator<T>, IEnumerator, IAsyncEnumerator
	{
		private readonly int timeoutMilliseconds;
		private readonly GetNewCursorCallback<T> resetFunction;
		private ICursor<T> cursor;

		/// <summary>
		/// Cursor enumerator
		/// </summary>
		/// <param name="Cursor">Cursor.</param>
		/// <param name="ResetFunction">Reset function.</param>
		/// <param name="TimeoutMilliseconds">Time to wait to get access to underlying database.</param>
		public CursorEnumerator(ICursor<T> Cursor, GetNewCursorCallback<T> ResetFunction, int TimeoutMilliseconds)
		{
			this.cursor = Cursor;
			this.timeoutMilliseconds = TimeoutMilliseconds;
			this.resetFunction = ResetFunction;
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
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		public bool MoveNext()
		{
			Task<bool> Task = this.cursor.MoveNextAsyncLocked();
			FilesProvider.Wait(Task, this.timeoutMilliseconds);
			return Task.Result;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public Task<bool> MoveNextAsync() => this.cursor.MoveNextAsync();

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		public void Reset()
		{
			this.cursor = this.resetFunction(this.cursor);
		}
	}
}
