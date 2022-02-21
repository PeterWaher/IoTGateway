using System.Threading.Tasks;
using Waher.Runtime.Threading;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Threading.Functions
{
	/// <summary>
	/// Protects script by using a semaphore to make sure that only one thread evaluates a particular script.
	/// </summary>
	public class Semaphore : FunctionTwoVariables
	{
		/// <summary>
		/// Protects script by using a semaphore to make sure that only one thread evaluates a particular script.
		/// </summary>
		/// <param name="Name">Semaphore name.</param>
		/// <param name="Node">Node to evaluate when semaphore allows.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Semaphore(ScriptNode Name, ScriptNode Node, int Start, int Length, Expression Expression)
			: base(Name, Node, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Semaphore";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Name", "Script" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
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
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return Argument2;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement Argument1 = await this.Argument1.EvaluateAsync(Variables);
			string Name = Argument1.AssociatedObjectValue.ToString();
			IElement Argument2;

			await Semaphores.BeginWrite(Name);
			try
			{
				Argument2 = await this.Argument2.EvaluateAsync(Variables);
			}
			finally
			{
				await Semaphores.EndWrite(Name);
			}

			return Argument2;
		}
	}
}
