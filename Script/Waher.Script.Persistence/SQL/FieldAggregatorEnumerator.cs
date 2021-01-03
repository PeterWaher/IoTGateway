using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL.Groups;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that adds fields to enumerated items.
	/// </summary>
	public class FieldAggregatorEnumerator : IResultSetEnumerator
	{
		private readonly KeyValuePair<string, ScriptNode>[] additionalFields;
		private readonly IResultSetEnumerator e;
		private readonly Variables variables;
		private ObjectProperties objectVariables = null;
		private object current = null;

		/// <summary>
		/// Enumerator that adds fields to enumerated items.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables</param>
		/// <param name="AdditionalFields">Fields to add to enumerated items.</param>
		public FieldAggregatorEnumerator(IResultSetEnumerator ItemEnumerator, Variables Variables, KeyValuePair<string, ScriptNode>[] AdditionalFields)
		{
			this.e = ItemEnumerator;
			this.variables = Variables;
			this.additionalFields = AdditionalFields;
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.current;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			return this.MoveNextAsync().Result;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			if (!await this.e.MoveNextAsync())
				return false;

			this.current = this.e.Current;

			if (this.objectVariables is null)
				this.objectVariables = new ObjectProperties(this.current, this.variables);
			else
				this.objectVariables.Object = e.Current;

			if (this.current is GenericObject GenObj)
			{
				foreach (KeyValuePair<string, ScriptNode> P in this.additionalFields)
					GenObj[P.Key] = P.Value.Evaluate(this.objectVariables);
			}
			else if (this.current is GroupObject GroupObj)
			{
				foreach (KeyValuePair<string, ScriptNode> P in this.additionalFields)
					GroupObj[P.Key] = P.Value.Evaluate(this.objectVariables);
			}
			else
			{
				GroupObject Obj = new GroupObject(new object[0], new object[0], new ScriptNode[0], this.variables);

				foreach (KeyValuePair<string, ScriptNode> P in this.additionalFields)
					Obj[P.Key] = P.Value.Evaluate(this.objectVariables);

				this.current = Obj;
			}

			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.e.Reset();
			this.current = null;
		}
	}
}
