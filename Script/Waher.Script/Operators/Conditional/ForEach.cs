using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators.Conditional
{
	/// <summary>
	/// FOR-EACH operator.
	/// </summary>
	public class ForEach : BinaryOperator
	{
		private readonly string variableName;

		/// <summary>
		/// FOR-EACH operator.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Collection">Collection.</param>
		/// <param name="Statement">Statement to execute.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ForEach(string VariableName, ScriptNode Collection, ScriptNode Statement, int Start, int Length, Expression Expression)
			: base(Collection, Statement, Start, Length, Expression)
		{
			this.variableName = VariableName;
		}

		/// <summary>
		/// Variable Name.
		/// </summary>
		public string VariableName
		{
			get { return this.variableName; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement S = this.left.Evaluate(Variables);
			
			if (!(S is ICollection<IElement> Elements))
			{
				if (S is IVector Vector)
					Elements = Vector.VectorElements;
				else if (!S.IsScalar)
					Elements = S.ChildElements;
				else if (S.AssociatedObjectValue is IEnumerable Enumerable)
				{
					IEnumerator e = Enumerable.GetEnumerator();

					while (e.MoveNext())
					{
						Variables[this.variableName] = e.Current;
						S = this.right.Evaluate(Variables);
					}

					return S;
				}
				else
					Elements = new IElement[] { S };
			}

			foreach (IElement Element in Elements)
			{
				Variables[this.variableName] = Element;
				S = this.right.Evaluate(Variables);
			}

			return S;
		}
	}
}
