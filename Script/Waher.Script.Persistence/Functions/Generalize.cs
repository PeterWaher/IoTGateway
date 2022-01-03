using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Persistence;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Creates a generalized representation of an object.
	/// </summary>
	public class Generalize : FunctionOneVariable
	{
		/// <summary>
		/// Saves an object to the object database.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Generalize(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Generalize";

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
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return this.EvaluateAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
		{
			return EvaluateAsync(Argument);
		}

		/// <summary>
		/// Generalizes the object in <paramref name="E"/>.
		/// </summary>
		/// <param name="E">Element</param>
		/// <returns>Generaized object.</returns>
		public static Task<IElement> EvaluateAsync(IElement E)
		{
			return EvaluateAsync(E.AssociatedObjectValue);
		}

		/// <summary>
		/// Generalizes the object in <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>Generaized object.</returns>
		public static async Task<IElement> EvaluateAsync(object Object)
		{
			if (Object is ICollection<KeyValuePair<string, IElement>> GenObj)
				return new ObjectValue(GenObj);

			if (!(Object is ICollection<KeyValuePair<string, object>> GenObj2))
				GenObj2 = await Database.Generalize(Object);

			return new ObjectValue(GenObj2);
		}

	}
}
