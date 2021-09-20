using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL.Groups;

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
			if (Variables is ObjectProperties Properties && Properties.Object is GroupObject GroupObject)
				return Expression.Encapsulate(GroupObject.Objects);
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
