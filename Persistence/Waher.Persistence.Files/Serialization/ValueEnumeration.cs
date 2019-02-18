using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
{
	internal class ValueEnumeration : IEnumerator<object>
	{
		private IEnumerator<KeyValuePair<string, object>> e;

		public ValueEnumeration(IEnumerator<KeyValuePair<string, object>> Enumerator)
		{
			this.e = Enumerator;
		}

		public object Current
		{
			get
			{
				return this.e.Current.Value;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return this.e.Current.Value;
			}
		}

		public void Dispose()
		{
			this.e?.Dispose();
			this.e = null;
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
