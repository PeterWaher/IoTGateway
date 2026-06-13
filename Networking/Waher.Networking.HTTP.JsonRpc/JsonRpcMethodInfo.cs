using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a method published via a JSON-RPC web service.
	/// </summary>
	internal class JsonRpcMethodInfo
	{
		public Delegate Method;
		public int NrArguments;
		public Dictionary<string, int> NamedArguments;
		public ParameterInfo[] Arguments;

		public JsonRpcMethodInfo(Delegate Method, bool CaseSensitive)
		{
			this.Method = Method;
			this.Arguments = Method.Method.GetParameters();
			this.NrArguments = this.Arguments.Length;

			if (CaseSensitive)
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCulture);
			else
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

			foreach (var P in this.Arguments)
				this.NamedArguments[P.Name] = P.Position;
		}
	}
}
