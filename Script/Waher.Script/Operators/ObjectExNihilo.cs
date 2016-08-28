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
		private LinkedList<KeyValuePair<string, ScriptNode>> members;

		/// <summary>
		/// Creates an object from nothing.
		/// </summary>
		/// <param name="Members">Members</param>.
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
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

    }
}
