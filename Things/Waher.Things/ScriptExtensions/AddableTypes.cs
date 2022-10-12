using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Things.ScriptExtensions
{
	/// <summary>
	/// Gets an array of types of nodes that can be added to an existing node.
	/// </summary>
	public class AddableTypes : FunctionOneVariable
	{
		/// <summary>
		/// Gets an array of types of nodes that can be added to an existing node.
		/// </summary>
		/// <param name="Node">Node.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public AddableTypes(ScriptNode Node, int Start, int Length, Expression Expression)
			: base(Node, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(AddableTypes);

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
		public override async Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
		{
			if (!(Argument.AssociatedObjectValue is INode Node))
				throw new ScriptRuntimeException("Expected a node as argument.", this);

			return new ObjectVector(await GetAddableTypes(Node));
		}

		/// <summary>
		/// Gets an array of type names of nodes that can be added to <paramref name="Node"/>.
		/// </summary>
		/// <param name="Node">Reference node.</param>
		/// <returns>Array of names of types of nodes that can be added to <paramref name="Node"/>.</returns>
		public static async Task<Type[]> GetAddableTypes(INode Node)
		{
			List<Type> Result = new List<Type>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(INode)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					INode PresumptiveChild = (INode)CI.Invoke(Types.NoParameters);

					if (await Node.AcceptsChildAsync(PresumptiveChild) && await PresumptiveChild.AcceptsParentAsync(Node))
						Result.Add(T);
				}
				catch (Exception)
				{
					continue;
				}
			}

			return Result.ToArray();
		}

	}
}
