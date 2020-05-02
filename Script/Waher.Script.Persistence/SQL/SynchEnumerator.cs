using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.SQL
{
	internal class SynchEnumerator : IResultSetEnumerator
	{
		private readonly IEnumerator e;

		public SynchEnumerator(IEnumerator e)
		{
			this.e = e;
		}

		public object Current => this.e.Current;

		public bool MoveNext()
		{
			return this.e.MoveNext();
		}

		public Task<bool> MoveNextAsync()
		{
			return Task.FromResult<bool>(this.e.MoveNext());
		}

		public void Reset()
		{
			this.e.Reset();
		}
	}
}
