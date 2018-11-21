using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Creates an object from nothing.
	/// </summary>
	public class ObjectExNihilo : ScriptNode
	{
		private readonly LinkedList<KeyValuePair<string, ScriptNode>> members;

		/// <summary>
		/// Creates an object from nothing.
		/// </summary>
		/// <param name="Members">Members</param>.
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ObjectExNihilo(LinkedList<KeyValuePair<string, ScriptNode>> Members, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.members = Members;
		}

		/// <summary>
		/// Members, in order of definition.
		/// </summary>
		public LinkedList<KeyValuePair<string, ScriptNode>> Members
		{
			get { return this.members; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            Dictionary<string, IElement> Result = new Dictionary<string, IElement>();

            foreach (KeyValuePair<string, ScriptNode> P in this.members)
                Result[P.Key] = P.Value.Evaluate(Variables);

            return new ObjectValue(Result);
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
			LinkedListNode<KeyValuePair<string, ScriptNode>> Loop;

			if (DepthFirst)
			{
				Loop = this.members.First;

				while (Loop != null)
				{
					if (!Loop.Value.Value.ForAllChildNodes(Callback, State, DepthFirst))
						return false;

					Loop = Loop.Next;
				}
			}

			Loop = this.members.First;

			while (Loop != null)
			{
				ScriptNode Node = Loop.Value.Value;
				bool Result = Callback(ref Node, State);
				if (Loop.Value.Value != Node)
					Loop.Value = new KeyValuePair<string, ScriptNode>(Loop.Value.Key, Node);

				if (!Result)
					return false;

				Loop = Loop.Next;
			}

			if (!DepthFirst)
			{
				Loop = this.members.First;

				while (Loop != null)
				{
					if (!Loop.Value.Value.ForAllChildNodes(Callback, State, DepthFirst))
						return false;

					Loop = Loop.Next;
				}
			}

			return true;
		}

	}
}
