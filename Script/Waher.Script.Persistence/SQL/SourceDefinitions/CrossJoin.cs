using System;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Persistence.SQL.Sources;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// CROSS JOIN of two source definitions.
	/// </summary>
	public class CrossJoin : Join
	{
		/// <summary>
		/// CROSS JOIN of two source definitions.
		/// </summary>
		/// <param name="Left">Left source definition.</param>
		/// <param name="Right">Right source definition.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CrossJoin(SourceDefinition Left, SourceDefinition Right, int Start, int Length, Expression Expression)
			: base(Left, Right, null, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets the actual data source, from its definition.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Data Source</returns>
		public override IDataSource GetSource(Variables Variables)
		{
			throw new NotImplementedException();
		}

	}
}
