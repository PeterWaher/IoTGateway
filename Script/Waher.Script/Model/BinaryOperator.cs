using Waher.Script.Abstraction.Elements;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all binary operators.
	/// </summary>
	public abstract class BinaryOperator : ScriptNode
	{
		/// <summary>
		/// Left operand.
		/// </summary>
		protected ScriptNode left;

		/// <summary>
		/// Right operand.
		/// </summary>
		protected ScriptNode right;

		/// <summary>
		/// If subtree is asynchroneous.
		/// </summary>
		protected bool isAsync;

		/// <summary>
		/// Base class for all binary operators.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public BinaryOperator(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.left = Left;
			this.left?.SetParent(this);

			this.right = Right;
			this.right?.SetParent(this);

			this.CalcIsAsync();
		}

		/// <summary>
		/// Recalculates if operator is asynchronous or not.
		/// </summary>
		protected virtual void CalcIsAsync()
		{
			this.isAsync =
				(this.left?.IsAsynchronous ?? false) ||
				(this.right?.IsAsynchronous ?? false);
		}

		/// <summary>
		/// Left operand.
		/// </summary>
		public ScriptNode LeftOperand => this.left;

		/// <summary>
		/// Right operand.
		/// </summary>
		public ScriptNode RightOperand => this.right;

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public virtual string DefaultVariableName
		{
			get
			{
				if (this.left is IDifferentiable Left &&
					this.right is IDifferentiable Right)
				{
					string s = Left.DefaultVariableName;
					if (s is null)
						return null;
					else if (s == Right.DefaultVariableName)
						return s;
					else
						return null;
				}
				else
					return null;
			}
		}

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => this.isAsync;

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
				if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			ScriptNode NewNode;
			bool RecalcIsAsync = false;
			bool b;

			if (!(this.left is null))
			{
				b = !Callback(this.left, out NewNode, State);
				if (!(NewNode is null))
				{
					this.left = NewNode;
					this.left.SetParent(this);

					RecalcIsAsync = true;
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.left.ForAllChildNodes(Callback, State, Order)))
				{
					if (RecalcIsAsync)
						this.CalcIsAsync();

					return false;
				}
			}

			if (!(this.right is null))
			{
				b = !Callback(this.right, out NewNode, State);
				if (!(NewNode is null))
				{
					this.right = NewNode;
					this.right.SetParent(this);

					RecalcIsAsync = true;
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.right.ForAllChildNodes(Callback, State, Order)))
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
				if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is BinaryOperator O &&
				AreEqual(this.left, O.left) &&
				AreEqual(this.right, O.right) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.left);
			Result ^= Result << 5 ^ GetHashCode(this.right);
			return Result;
		}

		/// <summary>
		/// Evaluates a named operator available in code-behind.
		/// </summary>
		/// <param name="Name">Name of operator.
		/// 
		/// Reference: https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/operator-overloads</param>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operad.</param>
		/// <param name="Node">Node performing the evaluation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateNamedOperator(string Name, IElement Left, IElement Right, ScriptNode Node)
		{
			ScriptNode[] ArgumentNodes = new ScriptNode[] { Node, Node };
			NamedMethodCall OperatorMethod = new NamedMethodCall(Node, Name, ArgumentNodes, 
				false, Node?.Start ?? 0, Node?.Length ?? 0, Node?.Expression);

			IElement[] Arguments = new IElement[] { Left, Right };

			IElement Result = OperatorMethod.EvaluateAsync(
				Left.AssociatedObjectValue?.GetType() ?? typeof(object),
				null, Arguments, null).Result;

			if (!(Result is null))
				return Result;

			Result = OperatorMethod.EvaluateAsync(
				Right.AssociatedObjectValue?.GetType() ?? typeof(object),
				null, Arguments, null).Result;

			return Result;
		}
	}
}
