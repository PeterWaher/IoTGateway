using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Represents a sequence of statements.
	/// </summary>
	public class Sequence : ScriptNode
	{
		LinkedList<ScriptNode> statements;

		/// <summary>
		/// Represents a sequence of statements.
		/// </summary>
		/// <param name="Statements">Statements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public Sequence(LinkedList<ScriptNode> Statements, int Start, int Length)
			: base(Start, Length)
		{
			this.statements = Statements;
		}

		/// <summary>
		/// Statements
		/// </summary>
		public LinkedList<ScriptNode> Statements
		{
			get { return this.statements; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Element Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}

	}
}
