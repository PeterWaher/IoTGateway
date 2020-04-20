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
	/// Enumerator that skips a given number of result records.
	/// </summary>
	public class OffsetEnumerator : IEnumerator
	{
		private readonly IEnumerator e;
		private readonly int offset0;
		private int offset;

		/// <summary>
		/// Enumerator that skips a given number of result records.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Offset">Number of records to skip.</param>
		public OffsetEnumerator(IEnumerator ItemEnumerator, int Offset)
		{
			this.e = ItemEnumerator;
			this.offset = this.offset0 = Offset;
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
			while (this.e.MoveNext())
			{
				if (this.offset > 0)
					this.offset--;
				else
					return true;
			}

			return false;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.offset = this.offset0;
			this.e.Reset();
		}
	}
}
