using System;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Extract the names of an enumeration type or an enumeration object.
	/// </summary>
	public class Names : FunctionOneVariable
	{
		/// <summary>
		/// Extract the names of an enumeration type or an enumeration object.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Names(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Names);

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			Type T;

			if (Argument is TypeValue TypeValue)
				T = TypeValue.Value;
			else
				T = Argument.AssociatedObjectValue?.GetType() ?? typeof(object);

			if (!T.IsEnum)
				throw new ScriptRuntimeException("Argument is not an enumeration type of value.", this);

			return new ObjectVector(Enum.GetNames(T));
		}
	}
}
