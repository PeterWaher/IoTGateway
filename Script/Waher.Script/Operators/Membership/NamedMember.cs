using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// As operator
	/// </summary>
	public class NamedMember : UnaryOperator 
	{
		private string name;

		/// <summary>
		/// As operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public NamedMember(ScriptNode Operand, string Name, int Start, int Length)
			: base(Operand, Start, Length)
		{
			this.name = Name;
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}
	}
}
