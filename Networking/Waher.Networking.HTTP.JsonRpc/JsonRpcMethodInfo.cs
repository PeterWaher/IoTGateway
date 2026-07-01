using System.Reflection;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a method published via a JSON-RPC web service.
	/// </summary>
	public class JsonRpcMethodInfo : ProtectedMethod
	{
		/// <summary>
		/// Information about a method published via a JSON-RPC web service.
		/// </summary>
		/// <param name="Method">Method information.</param>
		/// <param name="CaseSensitive">If names are case sensitive.</param>
		/// <param name="RequiredPrivileges">Required privileges</param>
		public JsonRpcMethodInfo(MethodInfo Method, 
			bool CaseSensitive, string[]? RequiredPrivileges)
			: base(Method, CaseSensitive, RequiredPrivileges)
		{
		}
	}
}
