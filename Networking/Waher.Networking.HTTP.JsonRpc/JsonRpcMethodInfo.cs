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
		public MethodInfo Method;
		public int NrArguments;
		public int NrSpecialArguments;
		public Dictionary<string, int> NamedArguments;
		public int? RequestArgument;
		public int? ResponseArgument;
		public ParameterInfo[] Arguments;

		public JsonRpcMethodInfo(MethodInfo Method, bool CaseSensitive)
		{
			this.Method = Method;
			this.Arguments = Method.GetParameters();
			this.NrArguments = this.Arguments.Length;
			this.NrSpecialArguments = 0;

			if (CaseSensitive)
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCulture);
			else
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

			foreach (ParameterInfo P in this.Arguments)
			{
				if (P.ParameterType == typeof(HttpRequest))
				{
					if (this.RequestArgument is null)
					{
						this.RequestArgument = P.Position;
						this.NrSpecialArguments++;
					}
					else
						throw new ArgumentException("Only one argument of type HttpRequest is allowed.", nameof(Method));
				}
				else if (P.ParameterType == typeof(HttpResponse))
				{
					if (this.ResponseArgument is null)
					{
						this.ResponseArgument = P.Position;
						this.NrSpecialArguments++;
					}
					else
						throw new ArgumentException("Only one argument of type HttpResponse is allowed.", nameof(Method));
				}
				else
					this.NamedArguments[P.Name] = P.Position;
			}
		}
	}
}
