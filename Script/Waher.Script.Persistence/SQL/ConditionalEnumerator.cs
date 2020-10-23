using System;
using System.Collections;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that only returns elements matching a set of conditions.
	/// </summary>
	public class ConditionalEnumerator : IResultSetEnumerator
	{
		private readonly ScriptNode conditions;
		private readonly Variables variables;
		private readonly IResultSetEnumerator e;
		private ObjectProperties properties = null;

		/// <summary>
		/// Enumerator that only returns elements matching a set of conditions.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Conditions">Set of conditions that must be fulfilled.</param>
		public ConditionalEnumerator(IResultSetEnumerator ItemEnumerator, Variables Variables, ScriptNode Conditions)
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
			while (this.e.MoveNext())
			{
				if (this.MatchesCondition())
					return true;
			}

			return false;
		}

		/// <summary>
		/// <see cref="IAsyncEnumerator.MoveNextAsync"/>
		/// </summary>
		public async Task<bool> MoveNextAsync()
		{
			while (await this.e.MoveNextAsync())
			{
				if (this.MatchesCondition())
					return true;
			}

			return false;
		}

		private bool MatchesCondition()
		{
			try
			{
				if (this.properties is null)
					this.properties = new ObjectProperties(this.e.Current, this.variables);
				else
					this.properties.Object = this.e.Current;

				IElement E = this.conditions.Evaluate(this.properties);
				if (!(E is BooleanValue B) || !B.Value)
					return false;

				return true;
			}
			catch (Exception)
			{
				return false;
			}
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
