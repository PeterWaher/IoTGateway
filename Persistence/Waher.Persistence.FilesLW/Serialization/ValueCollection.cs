using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	internal class ValueCollection : ICollection<object>
	{
		private StringDictionary dictionary;

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
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.dictionary.GetEnumerator(true);
			FilesProvider.Wait(Task, this.dictionary.DictionaryFile.TimeoutMilliseconds);

			using (ObjectBTreeFileEnumerator<KeyValuePair<string, object>> e = Task.Result)
			{
				while (e.MoveNext())
					array[arrayIndex++] = e.Current.Value;
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
