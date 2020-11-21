using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Graphs.Functions
{
	/// <summary>
	/// Makes a graph use the same scale for all axes.
	/// </summary>
	public class SameScale : FunctionOneScalarVariable
	{
		/// <summary>
		/// Makes a graph use the same scale for all axes.
		/// </summary>
		/// <param name="Graph">Graph.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SameScale(ScriptNode Graph, int Start, int Length, Expression Expression)
			: base(Graph, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Graph" };
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get
			{
				return "SameScale";
			}
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			if (Argument is Graph G)
			{
				G.SameScale = true;
				return G;
			}
			else
				throw new ScriptRuntimeException("Graph expected.", this);
		}
	}
}
