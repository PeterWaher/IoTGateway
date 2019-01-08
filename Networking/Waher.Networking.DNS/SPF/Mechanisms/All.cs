using System;
using System.Collections.Generic;
using System.Text;

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
	}
}
