using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all unary operators.
	/// </summary>
	public abstract class UnaryOperator : ScriptNode
	{
		/// <summary>
		/// Operand.
		/// </summary>
		protected ScriptNode op;

		/// <summary>
		/// If subtree is asynchroneous.
		/// </summary>
		protected bool isAsync;

		/// <summary>
		/// Base class for all unary operators.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public UnaryOperator(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.op = Operand;
			this.op?.SetParent(this);

			this.isAsync = Operand?.IsAsynchronous ?? false;
		}

		/// <summary>
		/// Operand.
		/// </summary>
		public ScriptNode Operand => this.op;

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public virtual string DefaultVariableName
		{
			get
			{
				if (this.op is IDifferentiable Differentiable)
					return Differentiable.DefaultVariableName;
				else
					return null;
			}
		}

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
			IElement Operand = this.op.Evaluate(Variables);

			return this.Evaluate(Operand, Variables);
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

			IElement Operand = await this.op.EvaluateAsync(Variables);
			return await this.EvaluateAsync(Operand, Variables);
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public abstract IElement Evaluate(IElement Operand, Variables Variables);

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public virtual Task<IElement> EvaluateAsync(IElement Operand, Variables Variables)
		{
			return Task.FromResult<IElement>(this.Evaluate(Operand, Variables));
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
				if (!(this.op?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			if (!(this.op is null))
			{
				bool b = !Callback(this.op, out ScriptNode NewNode, State);
				if (!(NewNode is null))
				{
					this.op = NewNode;
					this.op.SetParent(this);
				
					this.isAsync = NewNode.IsAsynchronous;
				}

				if (b)
					return false;
			}

			if (!DepthFirst)
			{
				if (!(this.op?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is UnaryOperator O &&
				this.op.Equals(O.op) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.op.GetHashCode();
			return Result;
		}

	}
}
