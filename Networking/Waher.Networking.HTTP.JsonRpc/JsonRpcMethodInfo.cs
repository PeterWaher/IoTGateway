using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Waher.Script;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a method published via a JSON-RPC web service.
	/// </summary>
	public class JsonRpcMethodInfo
	{
		/// <summary>
		/// Information about a method published via a JSON-RPC web service.
		/// </summary>
		/// <param name="Method">Method information.</param>
		/// <param name="CaseSensitive">If names are case sensitive.</param>
		public JsonRpcMethodInfo(MethodInfo Method, bool CaseSensitive)
		{
			ParameterInfo[] Arguments = Method.GetParameters();
			bool IsSpecialArgument;
			this.Method = Method;
			this.NrArguments = Arguments.Length;
			this.NrSpecialArguments = 0;

			this.Arguments = new JsonRpcArgumentInfo[this.NrArguments];

			if (CaseSensitive)
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCulture);
			else
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

			foreach (ParameterInfo P in Arguments)
			{
				if (P.ParameterType == typeof(HttpRequest))
				{
					if (this.RequestArgument is null)
					{
						this.RequestArgument = P.Position;
						this.NrSpecialArguments++;
						IsSpecialArgument = true;
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
						IsSpecialArgument = true;
					}
					else
						throw new ArgumentException("Only one argument of type HttpResponse is allowed.", nameof(Method));
				}
				else
				{
					this.NamedArguments[P.Name] = P.Position;
					IsSpecialArgument = false;
				}

				this.Arguments[P.Position] = new JsonRpcArgumentInfo(P, IsSpecialArgument,
					P.HasDefaultValue, P.HasDefaultValue ? P.DefaultValue : null);
			}
		}

		/// <summary>
		/// Method information.
		/// </summary>
		public MethodInfo Method { get; private set; }

		/// <summary>
		/// Number of arguments
		/// </summary>
		public int NrArguments { get; private set; }

		/// <summary>
		/// Number of special arguments
		/// </summary>
		public int NrSpecialArguments { get; private set; }

		/// <summary>
		/// Named arguments
		/// </summary>
		public Dictionary<string, int> NamedArguments { get; private set; }

		/// <summary>
		/// Request argument index
		/// </summary>
		public int? RequestArgument { get; private set; }

		/// <summary>
		/// Response argument index
		/// </summary>
		public int? ResponseArgument { get; private set; }

		/// <summary>
		/// Arguments
		/// </summary>
		public JsonRpcArgumentInfo[] Arguments { get; private set; }

		/// <summary>
		/// Tries to build a request for the method, based on the provided named parameters.
		/// </summary>
		/// <param name="Parameters">Named parameters.</param>
		/// <param name="Reason">Reason for not being able to create request.</param>
		/// <param name="Arguments">Ordered set of typed arguments, to be used in a
		/// call to the method.</param>
		/// <returns>If able to prepare a request to the method.</returns>
		public bool TryBuildRequest(Dictionary<string, object?> Parameters, 
			[NotNullWhen(false)] out string? Reason,
			[NotNullWhen(true)] out object?[]? Arguments)
		{
			int c = this.NrArguments;
			int NrParametersSet = 0;

			Arguments = new object?[c];

			foreach (KeyValuePair<string, object?> P in Parameters)
			{
				if (!this.NamedArguments.TryGetValue(P.Key, out int i))
				{
					Reason = "Invalid parameter name: " + P.Key;
					Arguments = null;
					return false;
				}
				else
				{
					Type ExpectedType = this.Arguments[i].Parameter.ParameterType;
					Type ParameterType = P.Value?.GetType() ?? typeof(object);

					if (ParameterType == ExpectedType)
						Arguments[i] = P.Value;
					else if (Expression.TryConvert(P.Value, ExpectedType, out object Converted))
						Arguments[i] = Converted;
					else
					{
						Arguments = null;
						Reason = "Parameter " + P.Key +
							" has incorrect type: " + ParameterType.FullName +
							", Expected: " + ExpectedType.FullName;
						return false;
					}

					NrParametersSet++;
				}
			}

			if (NrParametersSet != c - this.NrSpecialArguments)
			{
				foreach (JsonRpcArgumentInfo Argument in this.Arguments)
				{
					if (!Argument.IsSpecialArgument &&
						Argument.HasDefaultValue &&
						!Parameters.ContainsKey(Argument.Parameter.Name))
					{
						Arguments[Argument.Parameter.Position] = Argument.DefaultValue;
						NrParametersSet++;
					}
				}
			}

			if (NrParametersSet != c - this.NrSpecialArguments)
			{
				Reason = "Missing required parameters.";
				Arguments = null;
				return false;
			}

			Reason = null;

			return true;
		}
	}
}
