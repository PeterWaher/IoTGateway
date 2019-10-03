using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Represents a sequence of statements.
	/// </summary>
	public class Sequence : ScriptNode
	{
		private readonly LinkedList<ScriptNode> statements;

		/// <summary>
		/// Represents a sequence of statements.
		/// </summary>
		/// <param name="Statements">Statements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sequence(LinkedList<ScriptNode> Statements, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.statements = Statements;
		}

		/// <summary>
		/// Statements
		/// </summary>
		public LinkedList<ScriptNode> Statements
		{
			get { return this.statements; }
		}

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
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			LinkedListNode<ScriptNode> Loop;

			if (DepthFirst)
			{
				Loop = this.statements.First;

				while (!(Loop is null))
				{
					if (!(Loop.Value?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
						return false;

					Loop = Loop.Next;
				}
			}

			Loop = this.statements.First;

			while (!(Loop is null))
			{
				ScriptNode Node = Loop.Value;
				if (!(Node is null))
				{
					bool Result = Callback(ref Node, State);
					Loop.Value = Node;

					if (!Result)
						return false;
				}

				Loop = Loop.Next;
			}

			if (!DepthFirst)
			{
				Loop = this.statements.First;

				while (!(Loop is null))
				{
					if (!(Loop.Value?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
						return false;

					Loop = Loop.Next;
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Sequence O &&
				AreEqual(this.statements, O.statements) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.statements);
			return Result;
		}

	}
}
