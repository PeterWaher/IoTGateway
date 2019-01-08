using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// The "all" mechanism is a test that always matches.  It is used as the
	/// rightmost mechanism in a record to provide an explicit default.
	/// </summary>
	public class All : Mechanism
	{
		/// <summary>
		/// The "all" mechanism is a test that always matches.  It is used as the
		/// rightmost mechanism in a record to provide an explicit default.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public All(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public override Task<SpfResult> Matches()
		{
			return Task.FromResult<SpfResult>(SpfResult.Pass);
		}
	}
}
