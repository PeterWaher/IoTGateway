using System.Text;
using Waher.Content.Semantic;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Represents a possible solution during SPARQL evaluation.
	/// </summary>
	partial class Possibility
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
		/// <param name="Prev">Previous linked list of variables in possibility.</param>
		public Possibility(string VariableName, ISemanticElement Value, Possibility Prev)
		{
			this.VariableName = VariableName;
			this.Value = Value;
			this.Prev = Prev;
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
		public Possibility Prev { get; }

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

			Possibility Loop = this.Prev;
			while (!(Loop is null))
			{
				if (Loop.VariableName == VariableName)
					return Loop.Value;

				Loop = Loop.Prev;
			}

			return null;
		}

		/// <summary>
		/// Shows the collection of possible values as a string.
		/// </summary>
		/// <returns>String representation of possibilty.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.VariableName);
			sb.Append('=');
			sb.Append(Expression.ToString(this.Value));

			Possibility Loop = this.Prev;
			while (!(Loop is null))
			{
				sb.Append(", ");
				sb.Append(Loop.VariableName);
				sb.Append('=');
				sb.Append(Expression.ToString(Loop.Value));

				Loop = Loop.Prev;
			}

			return sb.ToString();
		}
	}
}
