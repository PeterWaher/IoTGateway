using System.Collections;
using System.Threading.Tasks;

namespace Waher.Persistence
{
	/// <summary>
	/// Joins a set of enumerators into one, that enumerates the items of each
	/// consequitively.
	/// </summary>
	/// <typeparam name="T">Type of items being enumerated.</typeparam>
	public class UnionEnumerator<T> : IAsyncEnumerator<T>
		where T : class
	{
		private readonly IAsyncEnumerator<T>[] enumerators;
		private readonly int nrEnumerators;
		private int position = 0;

		/// <summary>
		/// Joins a set of enumerators into one, that enumerates the items of each
		/// consequitively.
		/// </summary>
		/// <param name="Enumerators">Enumerators to join in sequence.</param>
		public UnionEnumerator(params IAsyncEnumerator<T>[] Enumerators)
		{
			this.enumerators = Enumerators;
			this.nrEnumerators = this.enumerators.Length;
		}

		/// <summary>
		/// Current item.
		/// </summary>
		public T Current => this.enumerators[this.position].Current;

		/// <summary>
		/// Current item.
		/// </summary>
		object IEnumerator.Current => this.enumerators[this.position].Current;

		/// <summary>
		/// Disposes of the enumerator, and all embedded enumerators.
		/// </summary>
		public void Dispose()
		{
			int i;

			for (i = 0; i < this.nrEnumerators; i++)
				this.enumerators[i].Dispose();
		}

		/// <summary>
		/// Moves to the next item.
		/// </summary>
		/// <returns>If there's a next item.</returns>
		public bool MoveNext()
		{
			while (this.position < this.nrEnumerators)
			{
				if (this.enumerators[this.position].MoveNext())
					return true;

				this.position++;
			}

			return false;
		}

		/// <summary>
		/// Moves to the next item.
		/// </summary>
		/// <returns>If there's a next item.</returns>
		public async Task<bool> MoveNextAsync()
		{
			while (this.position < this.nrEnumerators)
			{
				if (await this.enumerators[this.position].MoveNextAsync())
					return true;

				this.position++;
			}

			return false;
		}

		/// <summary>
		/// Resets the enumerator, and corresponding embedded enumerators.
		/// </summary>
		public void Reset()
		{
			int i;

			for (i = 0; i <= this.position; i++)
				this.enumerators[i].Reset();

			this.position = 0;
		}
	}
}
