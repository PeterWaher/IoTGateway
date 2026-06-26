using System;
using System.Collections;
using System.Collections.Generic;

namespace Waher.Persistence.MongoDB
{
	internal class ValueCollection : ICollection<object>
	{
		private readonly StringDictionary dictionary;

		public ValueCollection(StringDictionary Dictionary)
		{
			this.dictionary = Dictionary;
		}

		public int Count => this.dictionary.Count;
		public bool IsReadOnly => true;

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
			foreach (object Value in this.dictionary.Values)
				array[arrayIndex++] = Value;
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

		public object[] GetAllValues()
		{
			List<object> Result = [];

			foreach (object Value in this.dictionary.Values)
				Result.Add(Value);

			return [.. Result];
		}
	}
}
