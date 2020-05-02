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
	public class DistinctEnumerator : RecordEnumerator
	{
		private readonly Dictionary<Record, bool> reported = new Dictionary<Record, bool>();

		/// <summary>
		/// Enumerator that limits the return set to a maximum number of records.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Columns">Column definitions. Might be null if objects are to be returned.</param>
		/// <param name="Variables">Current set of variables.</param>
		public DistinctEnumerator(IResultSetEnumerator ItemEnumerator, ScriptNode[] Columns, Variables Variables)
			: base(ItemEnumerator, Columns, Variables)
		{
		}

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public override bool MoveNext()
		{
			while (base.MoveNext())
			{
				Record Rec = new Record(this.CurrentRecord);
				if (!this.reported.ContainsKey(Rec))
				{
					this.reported[Rec] = true;
					return true;
				}
			}
		
			return false;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public override void Reset()
		{
			base.Reset();
			this.reported.Clear();
		}

	}
}
