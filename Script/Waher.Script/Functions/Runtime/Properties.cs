using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Functions.Runtime.PropertyEnumerators;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Extract the properties of a type or an object.
	/// </summary>
	public class Properties : FunctionOneVariable
	{
		/// <summary>
		/// Extract the properties of a type or an object.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Properties(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Properties);

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement E = await this.Argument.EvaluateAsync(Variables);
			object Obj = E.AssociatedObjectValue;
			if (Obj is null)
				return ObjectValue.Null;

			IPropertyEnumerator Enumerator = GetEnumerator(Obj.GetType());

			if (Enumerator is null)
				return ObjectValue.Null;
			else
				return await Enumerator.EnumerateProperties(Obj);
		}

		private static readonly Dictionary<Type, IPropertyEnumerator> enumerators = new Dictionary<Type, IPropertyEnumerator>();

		private static IPropertyEnumerator GetEnumerator(Type T)
		{
			lock (enumerators)
			{
				if (enumerators.TryGetValue(T, out IPropertyEnumerator Enumerator))
					return Enumerator;
			}

			IPropertyEnumerator Best = Types.FindBest<IPropertyEnumerator, Type>(T);
			
			lock (enumerators)
			{
				enumerators[T] = Best;
			}

			return Best;
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
