using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects.Sets;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Defines a vector, by implicitly limiting its members to members of an optional vector, matching given conditions.
	/// </summary>
	public class ImplicitVectorDefinition : BinaryOperator
	{
		private readonly In[] setConditions;
		private readonly ScriptNode[] otherConditions;

		/// <summary>
		/// Defines a vector, by implicitly limiting its members to members of an optional vector, matching given conditions.
		/// </summary>
		/// <param name="Pattern">Pattern defining elements in the set.</param>
		/// <param name="Vector">Optional vector, if defining vector from members of a previous vector.</param>
		/// <param name="Conditions">Conditions of elements in the set.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ImplicitVectorDefinition(ScriptNode Pattern, ScriptNode Vector, ScriptNode[] Conditions,
			int Start, int Length, Expression Expression)
			: base(Pattern, Vector, Start, Length, Expression)
		{
			Conditions?.SetParent(this);

			Sets.ImplicitSetDefinition.SeparateConditions(Conditions, out this.setConditions, out this.otherConditions);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IEnumerable<IElement> VectorElements;
			bool CanEncapsulateAsMatrix;

			if (this.right is null)
			{
				VectorElements = null;
				CanEncapsulateAsMatrix = true;
			}
			else
			{
				IElement E = this.right.Evaluate(Variables);
				CanEncapsulateAsMatrix = (E is IMatrix);
				VectorElements = ImplicitSet.GetSetMembers(E);

				if (VectorElements is null)
					throw new ScriptRuntimeException("Unable to evaluate vector elements.", this.right);
			}

			IEnumerable<IElement> Elements = ImplicitSet.CalculateElements(this.left, VectorElements,
				this.setConditions, this.otherConditions, Variables);

			if (!(Elements is ICollection<IElement> Elements2))
			{
				Elements2 = new List<IElement>();

				foreach (IElement E in Elements)
					Elements2.Add(E);
			}

			return VectorDefinition.Encapsulate(Elements2, CanEncapsulateAsMatrix, this);
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

			IEnumerable<IElement> VectorElements;
			bool CanEncapsulateAsMatrix;

			if (this.right is null)
			{
				VectorElements = null;
				CanEncapsulateAsMatrix = true;
			}
			else
			{
				IElement E = await this.right.EvaluateAsync(Variables);
				CanEncapsulateAsMatrix = (E is IMatrix);
				VectorElements = ImplicitSet.GetSetMembers(E);

				if (VectorElements is null)
					throw new ScriptRuntimeException("Unable to evaluate vector elements.", this.right);
			}

			IEnumerable<IElement> Elements = await ImplicitSet.CalculateElementsAsync(this.left, VectorElements,
				this.setConditions, this.otherConditions, Variables);

			if (!(Elements is ICollection<IElement> Elements2))
			{
				Elements2 = new List<IElement>();

				foreach (IElement E in Elements)
					Elements2.Add(E);
			}

			return VectorDefinition.Encapsulate(Elements2, CanEncapsulateAsMatrix, this);
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
				if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.setConditions?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.otherConditions?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			ScriptNode Node;
			ScriptNode NewNode;
			int i, c;
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

			if (!(this.setConditions is null))
			{
				for (i = 0, c = this.setConditions.Length; i < c; i++)
				{
					Node = this.setConditions[i];
					if (!(Node is null))
					{
						b = !Callback(Node, out NewNode, State);
						if (!(NewNode is null) && NewNode is In NewIn)
						{
							this.setConditions[i] = NewIn;
							NewIn.SetParent(this);
							Node = NewNode;

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
			}

			if (!(this.otherConditions is null))
			{
				for (i = 0, c = this.otherConditions.Length; i < c; i++)
				{
					Node = this.otherConditions[i];
					if (!(Node is null))
					{
						b = !Callback(Node, out NewNode, State);
						if (!(NewNode is null))
						{
							this.otherConditions[i] = NewNode;
							NewNode.SetParent(this);
							Node = NewNode;

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
			}

			if (RecalcIsAsync)
				this.CalcIsAsync();

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.setConditions?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!(this.otherConditions?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is ImplicitVectorDefinition O &&
				AreEqual(this.setConditions, O.setConditions) &&
				AreEqual(this.otherConditions, O.otherConditions) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.setConditions);
			Result ^= Result << 5 ^ GetHashCode(this.otherConditions);
			return Result;
		}
	}
}
