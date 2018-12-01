using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Extract the fields of a type or an object.
	/// </summary>
	public class Fields : FunctionOneVariable
	{
		/// <summary>
		/// Extract the fields of a type or an object.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Fields(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "fields"; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement E = this.Argument.Evaluate(Variables);
			object Obj = E.AssociatedObjectValue;
			List<IElement> Elements = new List<IElement>();

			if (Obj is Type T)
			{
				foreach (FieldInfo FI in T.GetTypeInfo().DeclaredFields)
					Elements.Add(new StringValue(FI.Name));

				return new ObjectVector(Elements);
			}
			else
			{
				T = Obj.GetType();

				foreach (FieldInfo FI in T.GetTypeInfo().DeclaredFields)
				{
					Elements.Add(new StringValue(FI.Name));
					Elements.Add(Expression.Encapsulate(FI.GetValue(Obj)));
				}

				ObjectMatrix M = new ObjectMatrix(Elements.Count / 2, 2, Elements)
				{
					ColumnNames = new string[] { "Name", "Value" }
				};

				return M;
			}
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return ObjectValue.Null;
		}
	}
}
