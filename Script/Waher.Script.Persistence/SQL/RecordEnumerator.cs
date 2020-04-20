using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Events;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that limits the return set to a maximum number of records.
	/// </summary>
	public class RecordEnumerator : IEnumerator
	{
		private readonly IEnumerator e;
		private readonly ScriptNode[] columns;
		private readonly Variables variables;
		private readonly int count;
		private IElement[] record;

		/// <summary>
		/// Enumerator that limits the return set to a maximum number of records.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Columns">Column definitions. Might be null if objects are to be returned.</param>
		/// <param name="Variables">Current set of variables.</param>
		public RecordEnumerator(IEnumerator ItemEnumerator, ScriptNode[] Columns, Variables Variables)
		{
			this.e = ItemEnumerator;
			this.columns = Columns;
			this.variables = Variables;
			this.count = this.columns?.Length ?? 0;
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.e.Current;

		/// <summary>
		/// Current record
		/// </summary>
		public IElement[] CurrentRecord => this.record;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public virtual bool MoveNext()
		{
			if (!this.e.MoveNext())
				return false;

			int i;
			object Item = e.Current;
			ObjectProperties Properties = new ObjectProperties(Item, this.variables);
			
			if (this.columns is null)
				this.record = new IElement[1] { Expression.Encapsulate(Item) };
			else
			{
				this.record = new IElement[this.count];

				for (i = 0; i < this.count; i++)
				{
					try
					{
						this.record[i] = this.columns[i].Evaluate(Properties);
					}
					catch (Exception ex)
					{
						ex = Log.UnnestException(ex);
						this.record[i] = Expression.Encapsulate(ex);
					}
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public virtual void Reset()
		{
			this.e.Reset();
		}

	}
}
