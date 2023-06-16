using System.Collections.Generic;
using System.Text;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model.Literals;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Represents a possible solution during SPARQL evaluation.
	/// </summary>
	public class Possibility
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
			this.VariableName = VariableName;
			this.Value = Value ?? new NullValue();
			this.NextVariable = NextVariable;
		}

		/// <summary>
		/// Name of variable.
		/// </summary>
		public string VariableName { get; }

		/// <summary>
		/// Variable value.
		/// </summary>
		public ISemanticElement Value { get; }

		/// <summary>
		/// Previous possiblity
		/// </summary>
		public Possibility NextVariable { get; }

		/// <summary>
		/// Access to possible variable values, given a variable name.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <returns>Value of variable, if found, null otherwise.</returns>
		public ISemanticElement this[string VariableName]
		{
			get => this.GetValue(VariableName);
		}

		/// <summary>
		/// Access to possible variable values, given a variable name.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <returns>Value of variable, if found, null otherwise.</returns>
		public ISemanticElement GetValue(string VariableName)
		{
			if (this.VariableName == VariableName)
				return this.Value;

			Possibility Loop = this.NextVariable;
			while (!(Loop is null))
			{
				if (Loop.VariableName == VariableName)
					return Loop.Value;

				Loop = Loop.NextVariable;
			}

			return null;
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
				Sorted[P.VariableName] = P.Value;
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

	}
}
