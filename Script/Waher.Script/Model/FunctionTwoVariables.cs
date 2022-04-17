using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for funcions of one variable.
	/// </summary>
	public abstract class FunctionTwoVariables : Function
	{
		private ScriptNode argument1;
		private ScriptNode argument2;
		private bool isAsync;

		/// <summary>
		/// Base class for funcions of one variable.
		/// </summary>
		/// <param name="Argument1">Argument 1.</param>
		/// <param name="Argument2">Argument 2.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FunctionTwoVariables(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.argument1 = Argument1;
			this.argument1?.SetParent(this);

			this.argument2 = Argument2;
			this.argument2?.SetParent(this);

			this.CalcIsAsync();
		}

		private void CalcIsAsync()
		{
			this.isAsync =
				(this.argument1?.IsAsynchronous ?? false) ||
				(this.argument2?.IsAsynchronous ?? false);
		}

		/// <summary>
		/// Function argument 1.
		/// </summary>
		public ScriptNode Argument1 => this.argument1;

		/// <summary>
		/// Function argument 2.
		/// </summary>
		public ScriptNode Argument2 => this.argument2;

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "x", "y" };

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
			IElement Arg1 = this.argument1.Evaluate(Variables);
			IElement Arg2 = this.argument2.Evaluate(Variables);

			return this.Evaluate(Arg1, Arg2, Variables);
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

			IElement Arg1 = await this.argument1.EvaluateAsync(Variables);
			IElement Arg2 = await this.argument2.EvaluateAsync(Variables);

			return await this.EvaluateAsync(Arg1, Arg2, Variables);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public abstract IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables);

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public virtual Task<IElement> EvaluateAsync(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return Task.FromResult<IElement>(this.Evaluate(Argument1, Argument2, Variables));
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (Order == SearchMethod.DepthFirst)
			{
				if (!(this.argument1?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.argument2?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			ScriptNode NewNode;
			bool RecalcIsAsync = false;
			bool b;

			if (!(this.argument1 is null))
			{
				b = !Callback(this.argument1, out NewNode, State);
				if (!(NewNode is null))
				{
					this.argument1 = NewNode;
					this.argument1.SetParent(this);

					RecalcIsAsync = true;
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.argument1.ForAllChildNodes(Callback, State, Order)))
				{
					if (RecalcIsAsync)
						this.CalcIsAsync();

					return false;
				}
			}

			if (!(this.argument2 is null))
			{
				b = !Callback(this.argument2, out NewNode, State);
				if (!(NewNode is null))
				{
					this.argument2 = NewNode;
					this.argument2.SetParent(this);

					RecalcIsAsync = true;
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.argument2.ForAllChildNodes(Callback, State, Order)))
				{
					if (RecalcIsAsync)
						this.CalcIsAsync();

					return false;
				}
			}

			if (RecalcIsAsync)
				this.CalcIsAsync();

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.argument1?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.argument2?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is FunctionTwoVariables O &&
				this.argument1.Equals(O.argument1) &&
				this.argument2.Equals(O.argument2) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.argument1.GetHashCode();
			Result ^= Result << 5 ^ this.argument2.GetHashCode();
			return Result;
		}

	}
}
