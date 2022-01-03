using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for funcions of one variable.
	/// </summary>
	public abstract class FunctionOneVariable : Function
	{
		private ScriptNode argument;
		private bool isAsync;

		/// <summary>
		/// Base class for funcions of one variable.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FunctionOneVariable(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.argument = Argument;
			this.isAsync = Argument?.IsAsynchronous ?? false;
		}

		/// <summary>
		/// Function argument.
		/// </summary>
		public ScriptNode Argument => this.argument;

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "x" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => this.isAsync;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Arg = this.argument.Evaluate(Variables);
			return this.Evaluate(Arg, Variables);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement Arg = await this.argument.EvaluateAsync(Variables);
			return await this.EvaluateAsync(Arg, Variables);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public abstract IElement Evaluate(IElement Argument, Variables Variables);

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public virtual Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult<IElement>(this.Evaluate(Argument, Variables));
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			if (DepthFirst)
			{
				if (!(this.argument?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			if (!(this.argument is null))
			{
				bool b = !Callback(this.argument, out ScriptNode NewNode, State);
				if (!(NewNode is null))
				{
					this.argument = NewNode;
					this.isAsync = NewNode.IsAsynchronous;
				}

				if (b)
					return false;
			}

			if (!DepthFirst)
			{
				if (!(this.argument?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is FunctionOneVariable O &&
				this.argument.Equals(O.argument) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.argument.GetHashCode();
			return Result;
		}

	}
}
