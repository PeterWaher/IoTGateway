using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.MongoDB
{
	internal class KeyCollection : ICollection<string>
	{
		private readonly StringDictionary dictionary;

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
			IEnumerator<KeyValuePair<string, object>> e = this.dictionary.GetEnumerator();

			while (e.MoveNext())
				array[arrayIndex++] = e.Current.Key;
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
