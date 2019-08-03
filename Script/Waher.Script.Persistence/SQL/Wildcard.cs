using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Assignments;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Represents the Wildcard symbol *
	/// </summary>
	public class Wildcard : ScriptLeafNode
	{
		/// <summary>
		/// Represents the Wildcard symbol *
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Wildcard(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{

			if (Variables is ObjectProperties Properties &&
				Properties.Object is Dictionary<string, object> Result &&
				Result.TryGetValue(" First ", out object First))
			{
				return Expression.Encapsulate(First);
			}
			else
				throw new ScriptRuntimeException("Invalid context.", this);
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Wildcard;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return "*".GetHashCode();
		}

	}
}
