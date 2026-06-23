using System;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Declares an argument as recipient for meta-data, if such is available in the
	/// request.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class JsonRpcMetaDataArgumentAttribute : Attribute
	{
		/// <summary>
		/// Declares an argument as recipient for meta-data, if such is available in the
		/// request.
		/// </summary>
		public JsonRpcMetaDataArgumentAttribute()
		{
		}
	}
}
