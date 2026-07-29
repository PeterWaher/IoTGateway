using System;

namespace Waher.Networking.HTTP.JsonRpc.MetaData
{
	/// <summary>
	/// Marks a parameter as the JSON-RPC ID attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class JsonRpcIdAttribute : Attribute
	{
		/// <summary>
		/// Marks a parameter as the JSON-RPC ID attribute.
		/// </summary>
		public JsonRpcIdAttribute()
		{
		}
	}
}