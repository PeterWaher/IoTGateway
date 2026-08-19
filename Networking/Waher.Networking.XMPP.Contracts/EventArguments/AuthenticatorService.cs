using System.Collections.Generic;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Identity application authenticator service reference.
	/// </summary>
	public class AuthenticatorService : IdentityApplicationService
	{
		/// <summary>
		/// Identity application authenticator service reference.
		/// </summary>
		/// <param name="Id">ID of service</param>
		/// <param name="Name">Name of service</param>
		/// <param name="FullName">Fully qualified name of service</param>
		/// <param name="IconUrl">URL of service icon</param>
		/// <param name="IconWidth">Width of service icon</param>
		/// <param name="IconHeight">Height of service icon</param>
		/// <param name="Properties">Properties reviewed by service, and if they are
		/// required (true) or optional (false)</param>
		/// <param name="Attachments">Attachments reviewed by service, and if they are
		/// required (true) or optional (false)</param>
		public AuthenticatorService(string Id, string Name, string FullName,
			string IconUrl, int IconWidth, int IconHeight,
			Dictionary<string, bool> Properties, Dictionary<string, bool> Attachments)
			: base(Id, Name, FullName, IconUrl, IconWidth, IconHeight, Properties,
				  Attachments)
		{
		}
	}
}
