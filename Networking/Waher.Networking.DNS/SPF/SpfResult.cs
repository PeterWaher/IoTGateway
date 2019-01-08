using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.SPF
{
	/// <summary>
	/// Result of a SPF (Sender Policy Framework) evaluation.
	/// </summary>
	public enum SpfResult
	{
		/// <summary>
		/// A result of "none" means either (a) no syntactically valid DNS domain
		/// name was extracted from the SMTP session that could be used as the
		/// one to be authorized, or (b) no SPF records were retrieved from
		/// the DNS.
		/// </summary>
		None,

		/// <summary>
		/// A "neutral" result means the ADMD has explicitly stated that it is
		/// not asserting whether the IP address is authorized.
		/// </summary>
		Neutral,

		/// <summary>
		///  A "pass" result is an explicit statement that the client is
		///  authorized to inject mail with the given identity.
		/// </summary>
		Pass,

		/// <summary>
		/// A "fail" result is an explicit statement that the client is not
		/// authorized to use the domain in the given identity.
		/// </summary>
		Fail,

		/// <summary>
		/// A "softfail" result is a weak statement by the publishing ADMD that
		/// the host is probably not authorized.  It has not published a
		/// stronger, more definitive policy that results in a "fail".
		/// </summary>
		SoftFail,

		/// <summary>
		/// A "temperror" result means the SPF verifier encountered a transient
		/// (generally DNS) error while performing the check.  A later retry may
		/// succeed without further DNS operator action.
		/// </summary>
		TemporaryError,

		/// <summary>
		/// A "permerror" result means the domain's published records could not
		/// be correctly interpreted.  This signals an error condition that
		/// definitely requires DNS operator intervention to be resolved.
		/// </summary>
		PermanentError
	}
}
