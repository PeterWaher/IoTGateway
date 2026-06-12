using System.Collections.Generic;
using Waher.Networking.HTTP.JsonRpc;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// HTTP-based Model Context Protocol (MCP) resource.
	/// </summary>
	public class HttpMcpResource : JsonRpcWebService
	{
		private delegate void InitializeDelegate(
			Dictionary<string, object> Params,
			Dictionary<string, object> ClientInfo);

		public HttpMcpResource(string ResourceName)
			: base(ResourceName, true, false)
		{
			this.Register((InitializeDelegate)this.Initialize);
		}

		private void Initialize(
			Dictionary<string, object> Params,
			Dictionary<string, object> ClientInfo)
		{
		}
	}
}
