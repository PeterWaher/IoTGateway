using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Represents a sequence of statements.
	/// </summary>
	public class Sequence : ScriptNode
	{
		private readonly ChunkedList<ScriptNode> statements;
		private bool isAsync;

		/// <summary>
		/// Represents a sequence of statements.
		/// </summary>
		/// <param name="Statements">Statements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sequence(ChunkedList<ScriptNode> Statements, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.statements = Statements;
			this.statements?.SetParent(this);

			this.CalcIsAsync();
		}

		private void CalcIsAsync()
		{
			this.isAsync = false;

			foreach (ScriptNode Statement in this.statements)
			{
				if (Statement?.IsAsynchronous ?? false)
				{
					this.isAsync = true;
					break;
				}
			}
		}


		/// <summary>
		/// Statements
		/// </summary>
		public ChunkedList<ScriptNode> Statements => this.statements;

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
			IElement Result = null;

			foreach (ScriptNode Node in this.statements)
				Result = Node.Evaluate(Variables);

			return Result;
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

			IElement Result = null;

			foreach (ScriptNode Node in this.statements)
				Result = await Node.EvaluateAsync(Variables);

			return Result;
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
			ChunkNode<ScriptNode> Loop;
			int i, c;

			if (Order == SearchMethod.DepthFirst)
			{
				Loop = this.statements?.FirstChunk;

				while (!(Loop is null))
				{
					for (i = Loop.Start, c = Loop.Pos; i < c; i++)
					{
						if (!(Loop[i]?.ForAllChildNodes(Callback, State, Order) ?? false))
							return false;
					}

					Loop = Loop.Next;
				}
			}

			bool RecalcIsAsync = false;

			if (!this.statements.Update((ref ScriptNode Node, out bool Keep) =>
			{
				Keep = true;

				if (!(Node is null))
				{
					bool Result = Callback(Node, out ScriptNode NewNode, State);
					if (!(NewNode is null))
					{
						NewNode.SetParent(this);
						Node = NewNode;

						RecalcIsAsync = true;
					}

					if (!Result || (Order == SearchMethod.TreeOrder && !Node.ForAllChildNodes(Callback, State, Order)))
					{
						if (RecalcIsAsync)
							this.CalcIsAsync();

						return false;
					}
				}

				return true;
			}))
			{
				return false;
			}

			if (RecalcIsAsync)
				this.CalcIsAsync();

			if (Order == SearchMethod.BreadthFirst)
			{
				Loop = this.statements?.FirstChunk;

				while (!(Loop is null))
				{
					for (i = Loop.Start, c = Loop.Pos; i < c; i++)
					{
						if (!(Loop[i]?.ForAllChildNodes(Callback, State, Order) ?? false))
							return false;
					}

					Loop = Loop.Next;
				}
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is Sequence O &&
				AreEqual(this.statements, O.statements) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.statements);
			return Result;
		}

	}
}
