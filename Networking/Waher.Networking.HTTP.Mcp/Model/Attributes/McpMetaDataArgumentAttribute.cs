using System;
using Waher.Networking.HTTP.JsonRpc;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Declares an argument as recipient for meta-data, if such is available in the
	/// request.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class McpMetaDataArgumentAttribute : JsonRpcMetaDataArgumentAttribute
	{
		/// <summary>
		/// Declares an argument as recipient for meta-data, if such is available in the
		/// request.
		/// </summary>
		public McpMetaDataArgumentAttribute()
		{
		}
	}
}
