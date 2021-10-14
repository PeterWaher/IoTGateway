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
		private Dictionary<string, ScriptNode> quick = null;

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

				while (!(Loop is null))
				{
					if (!(Loop.Value.Value?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
						return false;

					Loop = Loop.Next;
				}
			}

			Loop = this.members.First;

			while (!(Loop is null))
			{
				ScriptNode Node = Loop.Value.Value;
				if (!(Node is null))
				{
					bool Result = Callback(ref Node, State);
					if (Loop.Value.Value != Node)
						Loop.Value = new KeyValuePair<string, ScriptNode>(Loop.Value.Key, Node);

					if (!Result)
						return false;
				}

				Loop = Loop.Next;
			}

			if (!DepthFirst)
			{
				Loop = this.members.First;

				while (!(Loop is null))
				{
					if (!(Loop.Value.Value?.ForAllChildNodes(Callback, State, DepthFirst) ?? true))
						return false;

					Loop = Loop.Next;
				}
			}

			return true;
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is ObjectExNihilo O) || !base.Equals(obj))
				return false;

			LinkedList<KeyValuePair<string, ScriptNode>>.Enumerator e1 = this.members.GetEnumerator();
			LinkedList<KeyValuePair<string, ScriptNode>>.Enumerator e2 = O.members.GetEnumerator();

			while (true)
			{
				bool b1 = e1.MoveNext();
				bool b2 = e2.MoveNext();

				if (b1 ^ b2)
					return false;

				if (!b1)
					return true;

				KeyValuePair<string, ScriptNode> Item1 = e1.Current;
				KeyValuePair<string, ScriptNode> Item2 = e2.Current;

				if (!Item1.Key.Equals(Item2.Key) ||
					!Item1.Value.Equals(Item2.Value))
				{
					return false;
				}
			}
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();

			foreach (KeyValuePair<string, ScriptNode> P in this.members)
			{
				Result ^= Result << 5 ^ P.Key.GetHashCode();
				Result ^= Result << 5 ^ P.Value.GetHashCode();
			}

			return Result;
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (!(CheckAgainst is ObjectValue Obj))
				return PatternMatchResult.NoMatch;
			
			if (this.quick is null)
			{
				Dictionary<string, ScriptNode> Quick = new Dictionary<string, ScriptNode>();

				foreach (KeyValuePair<string, ScriptNode> N in this.members)
					Quick[N.Key] = N.Value;

				this.quick = Quick;
			}

			PatternMatchResult Result;

			if (Obj.AssociatedObjectValue is IDictionary<string, IElement> Object)
			{
				foreach (KeyValuePair<string, IElement> P in Object)
				{
					if (!(this.quick.ContainsKey(P.Key)))
						return PatternMatchResult.NoMatch;
				}

				foreach (KeyValuePair<string, ScriptNode> P in this.members)
				{
					if (Object.TryGetValue(P.Key, out IElement E))
						Result = P.Value.PatternMatch(E, AlreadyFound);
					else
						Result = P.Value.PatternMatch(ObjectValue.Null, AlreadyFound);

					if (Result != PatternMatchResult.Match)
						return Result;
				}
			}
			else if (Obj.AssociatedObjectValue is IDictionary<string, object> Object2)
			{
				foreach (KeyValuePair<string, object> P in Object2)
				{
					if (!(this.quick.ContainsKey(P.Key)))
						return PatternMatchResult.NoMatch;
				}

				foreach (KeyValuePair<string, ScriptNode> P in this.members)
				{
					if (Object2.TryGetValue(P.Key, out object E))
						Result = P.Value.PatternMatch(Expression.Encapsulate(E), AlreadyFound);
					else
						Result = P.Value.PatternMatch(ObjectValue.Null, AlreadyFound);

					if (Result != PatternMatchResult.Match)
						return Result;
				}
			}

			return PatternMatchResult.Match;
		}
	}
}
