using System.Collections.Generic;
using Waher.Networking.HTTP.JsonRpc;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// HTTP-based Model Context Protocol (MCP) server resource.
	/// </summary>
	public class HttpMcpServerResource : JsonRpcWebService
	{
		private delegate void InitializeDelegate(
			string ProtocolVersion,
			Dictionary<string, object> Capabilities,
			Dictionary<string, object> ClientInfo);

		/// <summary>
		/// HTTP-based Model Context Protocol (MCP) resource.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public HttpMcpServerResource(string ResourceName)
			: base(ResourceName, true, false)
		{
			this.Register((InitializeDelegate)this.Initialize);
		}

		/// <summary>
		/// Protocol version of client, if available.
		/// </summary>
		public string? ClientProtocolVersion { get; private set; }

		/// <summary>
		/// Client capabilities, if available.
		/// </summary>
		public ClientCapabilities? ClientCapabilities { get; private set; }

		/// <summary>
		/// Information about client, if available.
		/// </summary>
		public Implementation? ClientInformation { get; private set; }

		private void Initialize(
			string ProtocolVersion,
			Dictionary<string, object> Capabilities,
			Dictionary<string, object> ClientInfo)
		{
			this.ClientProtocolVersion = ProtocolVersion;

			if (ClientCapabilities.TryParse(Capabilities, out ClientCapabilities CapabilitiesParsed))
				this.ClientCapabilities = CapabilitiesParsed;

			if (Implementation.TryParse(ClientInfo, out Implementation ClientInfoParsed))
				this.ClientInformation = ClientInfoParsed;
		}
	}
}
