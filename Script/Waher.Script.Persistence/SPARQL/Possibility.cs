using System.Collections;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model.Literals;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Represents a possible solution during SPARQL evaluation.
	/// </summary>
	public class Possibility : ISparqlResultRecord, ISparqlResultItem
	{
		/// <summary>
		/// Represents a possible solution during SPARQL evaluation.
		/// </summary>
		/// <param name="VariableName">Variable name</param>
		/// <param name="Value">Variable value.</param>
		public Possibility(string VariableName, ISemanticElement Value)
			: this(VariableName, Value, null)
		{
		}

		/// <summary>
		/// Represents a possible solution during SPARQL evaluation.
		/// </summary>
		/// <param name="VariableName">Variable name</param>
		/// <param name="Value">Variable value.</param>
		/// <param name="NextVariable">Previous linked list of variables in possibility.</param>
		public Possibility(string VariableName, ISemanticElement Value, Possibility NextVariable)
		{
			this.Name = VariableName;
			this.Value = Value ?? new NullValue();
			this.NextVariable = NextVariable;
		}

		/// <summary>
		/// Name of variable.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Variable value.
		/// </summary>
		public ISemanticElement Value { get; set; }

		/// <summary>
		/// Previous possiblity
		/// </summary>
		public Possibility NextVariable { get; private set; }

		/// <summary>
		/// Access to possible variable values, given a variable name.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <returns>Value of variable, if found, null otherwise.</returns>
		public ISemanticElement this[string VariableName]
		{
			get => this.GetValue(VariableName);
			set => this.SetValue(VariableName, value);
		}

		/// <summary>
		/// Access to possible variable values, given a variable name.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <returns>Value of variable, if found, null otherwise.</returns>
		public ISemanticElement GetValue(string VariableName)
		{
			if (this.Name == VariableName)
				return this.Value;

			Possibility Loop = this.NextVariable;
			while (!(Loop is null))
			{
				if (Loop.Name == VariableName)
					return Loop.Value;

				Loop = Loop.NextVariable;
			}

			return null;
		}

		/// <summary>
		/// Sets a variable value, given its variable name.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Value">Value</param>
		public void SetValue(string VariableName, ISemanticElement Value)
		{
			if (this.Name == VariableName)
				this.Value = Value;
			else
			{
				Possibility Loop = this;

				while (!(Loop.NextVariable is null) && Loop.NextVariable.Name != VariableName)
					Loop = Loop.NextVariable;

				if (Loop.NextVariable is null)
					Loop.NextVariable = new Possibility(VariableName, Value);
				else
					Loop.NextVariable.Value = Value;
			}
		}

		/// <summary>
		/// Shows the collection of possible values as a string.
		/// </summary>
		/// <returns>String representation of possibilty.</returns>
		public override string ToString()
		{
			return GetSortedDescription(this);
		}

		/// <summary>
		/// Gets a text description of the sorted set of possibilities, starting with
		/// a given possibility <paramref name="P"/>.
		/// </summary>
		/// <param name="P">First possibility.</param>
		/// <returns>Sorted description.</returns>
		public static string GetSortedDescription(Possibility P)
		{
			SortedDictionary<string, ISemanticElement> Sorted = new SortedDictionary<string, ISemanticElement>();

			while (!(P is null))
			{
				Sorted[P.Name] = P.Value;
				P = P.NextVariable;
			}

			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (KeyValuePair<string, ISemanticElement> P2 in Sorted)
			{
				if (First)
					First = false;
				else
					sb.Append(", ");

				sb.Append(P2.Key);
				sb.Append(':');
				sb.Append(P2.Value.ToString());
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets an enumerator for all items in the possibility.
		/// </summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<ISparqlResultItem> GetEnumerator()
		{
			return new PossibilityEnumerator(this);
		}

		/// <summary>
		/// Gets an enumerator for all items in the possibility.
		/// </summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new PossibilityEnumerator(this);
		}

		private class PossibilityEnumerator : IEnumerator<ISparqlResultItem>
		{
			private readonly Possibility first;
			private Possibility current;

			public PossibilityEnumerator(Possibility First)
			{
				this.first = First;
				this.current = null;
			}

			public ISparqlResultItem Current => this.current;
			object IEnumerator.Current => this.current;

			public void Dispose() { }

			public bool MoveNext()
			{
				if (this.current is null)
					this.current = this.first;
				else
					this.current = this.current.NextVariable;

				return !(this.current is null);
			}

			public void Reset()
			{
				this.current = null;
			}
		}
	}
}
