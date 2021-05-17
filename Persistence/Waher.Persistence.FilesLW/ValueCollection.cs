using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files
{
	internal class ValueCollection : ICollection<object>
	{
		private readonly StringDictionary dictionary;

		public ValueCollection(StringDictionary Dictionary)
		{
			this.dictionary = Dictionary;
		}

		public int Count
		{
			get
			{
				return this.dictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public void Add(object item)
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		public void Clear()
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		public bool Contains(object item)
		{
			throw new NotSupportedException("Dictionary only sorted on keys.");
		}

		public void CopyTo(object[] array, int arrayIndex)
		{
			Task Task = this.CopyToAsync(array, arrayIndex);
			FilesProvider.Wait(Task, this.dictionary.DictionaryFile.TimeoutMilliseconds);
		}

		/// <summary>
		/// Copies the values of the dicitionary to an array.
		/// </summary>
		/// <param name="array">Array</param>
		/// <param name="arrayIndex">Start index</param>
		public async Task CopyToAsync(object[] array, int arrayIndex)
		{
			await this.dictionary.DictionaryFile.BeginRead();
			try
			{
				ObjectBTreeFileCursor<KeyValuePair<string, object>> e = await this.dictionary.GetEnumeratorLocked();

				while (await e.MoveNextAsyncLocked())
					array[arrayIndex++] = e.Current.Value;
			}
			finally
			{
				await this.dictionary.DictionaryFile.EndRead();
			}
		}

		public IEnumerator<object> GetEnumerator()
		{
			return new ValueEnumeration(this.dictionary.GetEnumerator());
		}

		public bool Remove(object item)
		{
			throw new NotSupportedException("Dictionary only sorted on keys.");
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ValueEnumeration(this.dictionary.GetEnumerator());
		}
	}
}
