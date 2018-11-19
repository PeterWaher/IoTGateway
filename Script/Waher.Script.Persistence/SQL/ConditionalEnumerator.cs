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
	/// Enumerator that only returns elements matching a set of conditions.
	/// </summary>
	public class ConditionalEnumerator : IEnumerator
	{
		private readonly ScriptNode conditions;
		private readonly Variables variables;
		private readonly IEnumerator e;

		/// <summary>
		/// Enumerator that only returns elements matching a set of conditions.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Conditions">Set of conditions that must be fulfilled.</param>
		public ConditionalEnumerator(IEnumerator ItemEnumerator, Variables Variables, ScriptNode Conditions)
		{
			this.e = ItemEnumerator;
			this.variables = Variables;
			this.conditions = Conditions;
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
			while (e.MoveNext())
			{
				try
				{
					ObjectProperties Properties = new ObjectProperties(e.Current, this.variables);

					IElement E = this.conditions.Evaluate(Properties);
					if (!(E is BooleanValue B) || !B.Value)
						continue;

					return true;
				}
				catch (Exception)
				{
					continue;
				}
			}

			return false;
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
