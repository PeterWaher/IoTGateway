using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Dynamic function call operator
	/// </summary>
	public class DynamicFunctionCall : NullCheckUnaryScalarOperator
	{
		private readonly ScriptNode[] arguments;
		private readonly int nrArguments;

		/// <summary>
		/// Dynamic function call operator
		/// </summary>
		/// <param name="Function">Function</param>
		/// <param name="Arguments">Arguments</param>
		/// <param name="NullCheck">If null should be returned if operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DynamicFunctionCall(ScriptNode Function, ScriptNode[] Arguments, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Function, NullCheck, Start, Length, Expression)
		{
			this.arguments = Arguments;
			this.arguments?.SetParent(this);

			this.nrArguments = this.arguments.Length;

			this.CalcIsAsync();
		}

		private void CalcIsAsync()
		{
			this.isAsync = this.op?.IsAsynchronous ?? false;

			for (int i = 0; i < this.nrArguments; i++)
			{
				if (this.arguments[i]?.IsAsynchronous ?? false)
				{
					this.isAsync = true;
					break;
				}
			}
		}

		/// <summary>
		/// Arguments
		/// </summary>
		public ScriptNode[] Arguments => this.arguments;

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => this.isAsync || base.IsAsynchronous;

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement EvaluateScalar(IElement Operand, Variables Variables)
		{
			object Obj = Operand.AssociatedObjectValue;
			if (Obj is null && this.nullCheck)
				return ObjectValue.Null;

			if (!(Obj is ILambdaExpression Lambda))
				throw new ScriptRuntimeException("Expected a lambda expression.", this);

			if (this.nrArguments != Lambda.NrArguments)
				throw new ScriptRuntimeException("Expected " + Lambda.NrArguments.ToString() + " arguments.", this);

			IElement[] Arg = new IElement[this.nrArguments];
			ScriptNode Node;
			int i;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (Node is null)
					Arg[i] = ObjectValue.Null;
				else
					Arg[i] = Node.Evaluate(Variables);
			}

			return Lambda.Evaluate(Arg, Variables);
		}

		/// <summary>
		/// Evaluates the operator on scalar operands.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override async Task<IElement> EvaluateScalarAsync(IElement Operand, Variables Variables)
		{
			object Obj = Operand.AssociatedObjectValue;
			if (Obj is null && this.nullCheck)
				return ObjectValue.Null;

			if (!(Obj is ILambdaExpression Lambda))
				throw new ScriptRuntimeException("Expected a lambda expression.", this);

			if (this.nrArguments != Lambda.NrArguments)
				throw new ScriptRuntimeException("Expected " + Lambda.NrArguments.ToString() + " arguments.", this);

			IElement[] Arg = new IElement[this.nrArguments];
			ScriptNode Node;
			int i;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (Node is null)
					Arg[i] = ObjectValue.Null;
				else
					Arg[i] = await Node.EvaluateAsync(Variables);
			}

			return await Lambda.EvaluateAsync(Arg, Variables);
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
			int i;

			if (Order == SearchMethod.DepthFirst)
			{
				if (!this.arguments.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			ScriptNode Node;
			bool RecalcIsAsync = false;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (!(Node is null))
				{
					bool b = !Callback(Node, out ScriptNode NewNode, State);
					if (!(NewNode is null))
					{
						this.arguments[i] = Node = NewNode;
						NewNode.SetParent(this);

						RecalcIsAsync = true;
					}

					if (b || (Order == SearchMethod.TreeOrder && !Node.ForAllChildNodes(Callback, State, Order)))
					{
						if (RecalcIsAsync)
							this.CalcIsAsync();

						return false;
					}
				}
			}

			if (RecalcIsAsync)
				this.CalcIsAsync();

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.arguments.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is DynamicFunctionCall O &&
				AreEqual(this.arguments, O.arguments) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.arguments);
			return Result;
		}
	}
}
