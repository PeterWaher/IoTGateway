using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence
{
	/// <summary>
	/// Paginated object enumerator.
	/// </summary>
	/// <typeparam name="T">Type of object being enumerated.</typeparam>
	public class PaginatedEnumerator<T> : IEnumerator<T>, IAsyncEnumerator
		where T : class
	{
		private readonly IPage<T> firstPage;
		private IPage<T> currentPage;
		private IEnumerator<T> currentPageEnumerator;
		private IAsyncEnumerator currentPageEnumeratorAsync;

		/// <summary>
		/// Paginated object enumerator.
		/// </summary>
		/// <param name="FirstPage">First page being enumerated.</param>
		public PaginatedEnumerator(IPage<T> FirstPage)
		{
			this.currentPage = this.firstPage = FirstPage;
			this.currentPageEnumerator = this.currentPage.Items.GetEnumerator();
			this.currentPageEnumeratorAsync = this.currentPageEnumerator as IAsyncEnumerator;
		}

		/// <summary>
		/// Paginated object enumerator, enumerating a single page of enumerable items.
		/// </summary>
		/// <param name="FixedSetOfItems">Fixed set of items.</param>
		public PaginatedEnumerator(IEnumerable<T> FixedSetOfItems)
		{
			this.firstPage = null;
			this.currentPage = null;
			this.currentPageEnumerator = FixedSetOfItems.GetEnumerator();
			this.currentPageEnumeratorAsync = this.currentPageEnumerator as IAsyncEnumerator;
		}

		/// <summary>
		/// Current item.
		/// </summary>
		public T Current => this.currentPageEnumerator.Current;

		/// <summary>
		/// Current item.
		/// </summary>
		object IEnumerator.Current => this.currentPageEnumerator.Current;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.currentPage is IDisposable Disposable)
				Disposable.Dispose();

			this.currentPageEnumerator.Dispose();
		}

		/// <summary>
		/// Moves to next item.
		/// </summary>
		/// <returns>If there's a next item.</returns>
		public bool MoveNext()
		{
			while (true)
			{
				if (this.currentPageEnumerator.MoveNext())
					return true;

				if (this.currentPage is null)
					return false;

				if (!this.currentPage.More)
					return false;

				this.currentPage = Database.FindNext(this.currentPage).Result;
				this.currentPageEnumeratorAsync = this.currentPageEnumerator as IAsyncEnumerator;
			}
		}

		/// <summary>
		/// Moves to next item.
		/// </summary>
		/// <returns>If there's a next item.</returns>
		public async Task<bool> MoveNextAsync()
		{
			while (true)
			{
				if (await this.MoveNextOnCurrentPage())
					return true;

				if (this.currentPage is null)
					return false;

				if (!this.currentPage.More)
					return false;

				this.currentPage = await Database.FindNext(this.currentPage);
				this.currentPageEnumeratorAsync = this.currentPageEnumerator as IAsyncEnumerator;
			}
		}

		private Task<bool> MoveNextOnCurrentPage()
		{
			if (this.currentPageEnumeratorAsync is null)
				return Task.FromResult(this.currentPageEnumerator.MoveNext());
			else
				return this.currentPageEnumeratorAsync.MoveNextAsync();
		}

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public void Reset()
		{
			if (this.firstPage is null)
				this.currentPageEnumerator.Reset();
			else
			{
				this.currentPage = this.firstPage;
				this.currentPageEnumerator = this.currentPage.Items.GetEnumerator();
				this.currentPageEnumeratorAsync = this.currentPageEnumerator as IAsyncEnumerator;
			}
		}
	}
}
