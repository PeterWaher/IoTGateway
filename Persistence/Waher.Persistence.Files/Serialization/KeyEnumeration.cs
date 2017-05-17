using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	internal class KeyEnumeration : IEnumerator<string>
	{
		private IEnumerator<KeyValuePair<string, object>> e;

		public KeyEnumeration(IEnumerator<KeyValuePair<string, object>> Enumerator)
		{
			this.e = Enumerator;
		}

		public string Current
		{
			get
			{
				return this.e.Current.Key;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return this.e.Current.Key;
			}
		}

		public void Dispose()
		{
			if (this.e != null)
			{
				this.e.Dispose();
				this.e = null;
			}
		}

		public bool MoveNext()
		{
			return this.e.MoveNext();
		}

		public void Reset()
		{
			this.e.Reset();
		}
	}
}
