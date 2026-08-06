using Waher.Networking.HTTP.Mcp.Model.Attributes;

namespace Waher.Mcp.Xmpp.UserInput
{
	/// <summary>
	/// Class containing user input parameters for XMPP credentials.
	/// </summary>
	internal class XmppCredentialsInput
	{
		/// <summary>
		/// Domain of XMPP account.
		/// </summary>
		[McpStringParameter("Domain", "Domain of XMPP server.", 4, 128)]
		public string Domain = string.Empty;

		/// <summary>
		/// User name (account name) of XMPP account.
		/// </summary>
		[McpStringParameter("User Name", "User name (account name) of XMPP account.", 1, 128)]
		public string UserName = string.Empty;

		/// <summary>
		/// Password of XMPP account.
		/// </summary>
		[McpPasswordParameter("Password", "Password of XMPP account.", 10, 128)]
		public string Password = string.Empty;

		/// <summary>
		/// If the server certificate should be trusted by default.
		/// </summary>
		[McpParameter("Trust Server", "If the server certificate should be trusted by default.")]
		public bool TrustServer = false;

		/// <summary>
		/// If insecure authentication mechanisms are allowed when connecting to 
		/// XMPP server.
		/// </summary>
		[McpParameter("Allow Insecure Mechanisms", "If insecure authentication mechanisms are allowed when connecting to XMPP server.")]
		public bool AllowInsecureMechanisms = false;

		/// <summary>
		/// If a new account is to be created, rather than using an existing account.
		/// </summary>
		[McpParameter("Create Account", "If a new account is to be created, rather than using an existing account.")]
		public bool CreateAccount = false;

		/// <summary>
		/// API Key to use when creating an account.
		/// </summary>
		[McpStringParameter("API Key", "API Key to use when creating an account.")]
		public string ApiKey = string.Empty;

		/// <summary>
		/// API Secret to use when creating an account.
		/// </summary>
		[McpStringParameter("API Secret", "API Secret to use when creating an account.")]
		public string ApiSecret = string.Empty;
	}
}
