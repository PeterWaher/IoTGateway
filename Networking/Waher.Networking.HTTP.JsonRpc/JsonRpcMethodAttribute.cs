using System;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Declares the method as a JSON-RPC method, to be published by the JSON-RPC web 
	/// service in which it is defined.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class JsonRpcMethodAttribute : Attribute
	{
		/// <summary>
		/// Declares the method as a JSON-RPC method, to be published by the JSON-RPC web 
		/// service in which it is defined.
		/// </summary>
		public JsonRpcMethodAttribute()
		{
		}
	}
}
