using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// FULL [OUTER] JOIN of two source definitions.
	/// </summary>
	public class FullOuterJoin : Join
	{
		/// <summary>
		/// FULL [OUTER] JOIN of two source definitions.
		/// </summary>
		/// <param name="Left">Left source definition.</param>
		/// <param name="Right">Right source definition.</param>
		/// <param name="Conditions">Join conditions.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FullOuterJoin(SourceDefinition Left, SourceDefinition Right, ScriptNode Conditions, int Start, int Length, Expression Expression)
			: base(Left, Right, Conditions, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Finds objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public override Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			throw new NotImplementedException();
		}

	}
}
