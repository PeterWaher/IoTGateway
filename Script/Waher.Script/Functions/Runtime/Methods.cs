using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Extract the methods of a type or an object.
	/// </summary>
	public class Methods : FunctionOneVariable
	{
		/// <summary>
		/// Extract the methods of a type or an object.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Methods(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "methods"; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement E = this.Argument.Evaluate(Variables);
			return this.Evaluate(E, Variables);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement E = await this.Argument.EvaluateAsync(Variables);
			return this.Evaluate(E, Variables);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			object Obj = Argument.AssociatedObjectValue;
			List<IElement> Elements = new List<IElement>();

			if (Obj is Type T)
			{
				foreach (MethodInfo MI in T.GetRuntimeMethods())
				{
					if (MI.IsPublic)
						Elements.Add(new StringValue(ToString(MI)));
				}

				return new ObjectVector(Elements);
			}
			else
			{
				T = Obj.GetType();

				foreach (MethodInfo MI in T.GetRuntimeMethods())
				{
					if (MI.IsPublic)
					{
						Elements.Add(new StringValue(MI.Name));
						Elements.Add(new ObjectValue(new MethodLambda(Obj, MI)));
					}
				}

				ObjectMatrix M = new ObjectMatrix(Elements.Count / 2, 2, Elements)
				{
					ColumnNames = new string[] { "Name", "Lambda" }
				};

				return M;
			}
		}

		private static string ToString(MethodInfo MI)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append(MI.Name);
			sb.Append('(');

			foreach (ParameterInfo PI in MI.GetParameters())
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append(PI.Name);
			}

			sb.Append(')');

			return sb.ToString();
		}
	}
}
