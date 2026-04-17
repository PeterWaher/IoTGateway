using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a method published via a JSON-RPC web service.
	/// </summary>
	internal class MethodInfo
	{
		public Delegate Method;
		public int NrArguments;
		public Dictionary<string, int> NamedArguments;
		public ParameterInfo[] Arguments;

		public MethodInfo(Delegate Method)
		{
			this.Method = Method;
			this.Arguments = Method.Method.GetParameters();
			this.NrArguments = this.Arguments.Length;
			this.NamedArguments = new Dictionary<string, int>();

			foreach (var P in this.Arguments)
				this.NamedArguments[P.Name] = P.Position;
		}
	}
}
