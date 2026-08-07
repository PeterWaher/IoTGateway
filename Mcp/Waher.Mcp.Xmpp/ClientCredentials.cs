using System;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Security.CallStack;

namespace Waher.Mcp.Xmpp
{
	/// <summary>
	/// Contains credentials for an MCP client.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	[CollectionName("McpXmppClientCredentials")]
	[Index("McpUserName")]
	public class ClientCredentials : IEncryptedProperties
	{
		private static ICallStackCheck[]? approvedSources = null;

		private string? passwordHash;

		/// <summary>
		/// Contains credentials for an MCP client.
		/// </summary>
		public ClientCredentials()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string? ObjectID { get; set; }

		/// <summary>
		/// MCP User name
		/// </summary>
		public string? McpUserName { get; set; }

		/// <summary>
		/// XMPP Account name
		/// </summary>
		public string? XmppAccountName { get; set; }

		/// <summary>
		/// XMPP domain or host.
		/// </summary>
		public string? Domain { get; set; }

		/// <summary>
		/// If the server certificate should be trusted by default.
		/// </summary>
		public bool TrustServer { get; set; }

		/// <summary>
		/// If insecure authentication mechanisms are allowed when connecting to 
		/// XMPP server.
		/// </summary>
		public bool AllowInsecureMechanisms { get; set; }

		/// <summary>
		/// Password
		/// </summary>
		[Encrypted(32)]
		public string? PasswordHash
		{
			get
			{
				AssertAllowed();
				return this.passwordHash;
			}

			set
			{
				if (!string.IsNullOrEmpty(this.passwordHash))
					AssertAllowed();

				this.passwordHash = value;
			}
		}

		/// <summary>
		/// Password
		/// </summary>
		[Encrypted(32)]
		public string? PasswordHashType { get; set; }

		/// <summary>
		/// Array of properties that are encrypted.
		/// </summary>
		public string[] EncryptedProperties => new string[]
		{
			nameof(this.PasswordHash)
		};

		/// <summary>
		/// If access to sensitive methods is only accessible from a set of approved sources.
		/// </summary>
		/// <param name="ApprovedSources">Approved sources.</param>
		/// <exception cref="NotSupportedException">If trying to change previously set sources.</exception>
		public static void SetAllowedSources(ICallStackCheck[] ApprovedSources)
		{
			if (!(approvedSources is null))
				throw new NotSupportedException("Changing approved sources not permitted.");

			approvedSources = ApprovedSources;
		}

		private static void AssertAllowed()
		{
			if (!(approvedSources is null))
				Assert.CallFromSource(approvedSources);
		}
	}
}