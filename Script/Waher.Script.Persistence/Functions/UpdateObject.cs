using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Persistence;
using System.Threading.Tasks;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Updates an object in the object database.
	/// </summary>
	public class UpdateObject : FunctionOneScalarVariable
	{
		/// <summary>
		/// Updates an object in the object database.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public UpdateObject(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "UpdateObject";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Object" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			await Database.Update(Argument.AssociatedObjectValue);
			return Argument;
		}

	}
}
