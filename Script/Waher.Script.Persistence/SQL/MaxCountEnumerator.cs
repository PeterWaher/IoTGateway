using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that limits the return set to a maximum number of records.
	/// </summary>
	public class MaxCountEnumerator : IEnumerator
	{
		private readonly IEnumerator e;
		private readonly int count0;
		private int count;

		/// <summary>
		/// Enumerator that limits the return set to a maximum number of records.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Count">Maximum number of records to enumerate.</param>
		public MaxCountEnumerator(IEnumerator ItemEnumerator, int Count)
		{
			this.e = ItemEnumerator;
			this.count = this.count0 = Count;
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.e.Current;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			if (this.count <= 0 || !this.e.MoveNext())
				return false;

			this.count--;
		
			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.count = this.count0;
			this.e.Reset();
		}
	}
}
