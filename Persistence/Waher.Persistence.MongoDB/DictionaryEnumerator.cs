using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Waher.Persistence.MongoDB
{
	/// <summary>
	/// Dictionary enumerator
	/// </summary>
	public class DictionaryEnumerator : IEnumerator<KeyValuePair<string, object>>
	{
		private readonly IEnumerable<DictionaryEntry> entries;
		private readonly IEnumerator<DictionaryEntry> e;

		internal DictionaryEnumerator(IEnumerable<DictionaryEntry> Entries)
		{
			this.entries = Entries;
			this.e = this.entries.GetEnumerator();
		}

		/// <summary>
		/// <see cref="IEnumerator{T}.Current"/>
		/// </summary>
		public KeyValuePair<string, object> Current => new KeyValuePair<string, object>(this.e.Current.Key, this.e.Current.Value);

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		object IEnumerator.Current => new KeyValuePair<string, object>(this.e.Current.Key, this.e.Current.Value);

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.e.Dispose();
		}

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			return this.e.MoveNext();
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.e.Reset();
		}
	}
}
