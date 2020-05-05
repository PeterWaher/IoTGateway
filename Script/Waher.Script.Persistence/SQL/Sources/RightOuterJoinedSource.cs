using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Data source formed through an RIGHT [OUTER] JOIN of two sources.
	/// </summary>
	public class RightOuterJoinedSource : LeftOuterJoinedSource 
	{
		/// <summary>
		/// Data source formed through an RIGHT [OUTER] JOIN of two sources.
		/// </summary>
		/// <param name="Left">Left source</param>
		/// <param name="Right">Right source</param>
		/// <param name="Conditions">Conditions for join.</param>
		public RightOuterJoinedSource(IDataSource Left, IDataSource Right, ScriptNode Conditions)
			: base(Right, Left, Conditions)
		{
		}

		/// <summary>
		/// If sources should be flipped in the <see cref="JoinedObject"/> instances created.
		/// </summary>
		protected override bool Flipped => true;
	}
}
