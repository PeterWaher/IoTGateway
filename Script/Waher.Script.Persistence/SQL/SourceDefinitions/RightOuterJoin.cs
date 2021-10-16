using System;
using System.Threading.Tasks;
using Waher.Script.Model;
using Waher.Script.Persistence.SQL.Sources;

namespace Waher.Script.Persistence.SQL.SourceDefinitions
{
	/// <summary>
	/// RIGHT [OUTER] JOIN of two source definitions.
	/// </summary>
	public class RightOuterJoin : Join
	{
		/// <summary>
		/// RIGHT [OUTER] JOIN of two source definitions.
		/// </summary>
		/// <param name="Left">Left source definition.</param>
		/// <param name="Right">Right source definition.</param>
		/// <param name="Conditions">Join conditions.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public RightOuterJoin(SourceDefinition Left, SourceDefinition Right, ScriptNode Conditions, int Start, int Length, Expression Expression)
			: base(Left, Right, Conditions, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Gets the actual data source, from its definition.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Data Source</returns>
		public override async Task<IDataSource> GetSource(Variables Variables)
		{
			return new RightOuterJoinedSource(
				await this.Left.GetSource(Variables), 
				await this.Right.GetSource(Variables), this.Conditions);
		}

	}
}
