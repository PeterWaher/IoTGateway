using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	internal class KeyCollection : ICollection<string>
	{
		private StringDictionary dictionary;

		public KeyCollection(StringDictionary Dictionary)
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

		public void Add(string item)
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		public void Clear()
		{
			throw new NotSupportedException("Collection is read-only.");
		}

		public bool Contains(string item)
		{
			return this.dictionary.ContainsKey(item);
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			Task<ObjectBTreeFileEnumerator<KeyValuePair<string, object>>> Task = this.dictionary.GetEnumerator(true);
			FilesProvider.Wait(Task, this.dictionary.DictionaryFile.TimeoutMilliseconds);

			using (ObjectBTreeFileEnumerator<KeyValuePair<string, object>> e = Task.Result)
			{
				while (e.MoveNext())
					array[arrayIndex++] = e.Current.Key;
			}
		}

		public IEnumerator<string> GetEnumerator()
		{
			return new KeyEnumeration(this.dictionary.GetEnumerator());
		}

		public bool Remove(string item)
		{
			return this.dictionary.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new KeyEnumeration(this.dictionary.GetEnumerator());
		}
	}
}
