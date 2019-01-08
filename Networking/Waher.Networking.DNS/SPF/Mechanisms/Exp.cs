using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// If check_host() results in a "fail" due to a mechanism match (such as
	/// "-all"), and the "exp" modifier is present, then the explanation
	/// string returned is computed as described below.If no "exp" modifier
	/// is present, then either a default explanation string or an empty
	/// explanation string MUST be returned to the calling application.
	/// </summary>
	public class Exp : MechanismDomainSpec
	{
		/// <summary>
		/// If check_host() results in a "fail" due to a mechanism match (such as
		/// "-all"), and the "exp" modifier is present, then the explanation
		/// string returned is computed as described below.If no "exp" modifier
		/// is present, then either a default explanation string or an empty
		/// explanation string MUST be returned to the calling application.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Exp(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}

		/// <summary>
		/// Mechanism separator
		/// </summary>
		public override char Separator => '=';
	}
}
