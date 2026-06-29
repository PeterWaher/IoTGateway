using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Waher.Script;
using Waher.Security;
using Waher.Things.Http;

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
		/// <param name="RequiredPrivileges">Required privileges</param>
		public JsonRpcMethodInfo(MethodInfo Method, bool CaseSensitive,
			string[]? RequiredPrivileges)
		{
			ParameterInfo[] Arguments = Method.GetParameters();
			bool IsSpecialArgument;
			bool IsMetaDataArgument;
			this.Method = Method;
			this.NrArguments = Arguments.Length;
			this.NrSpecialArguments = 0;
			this.RequiredPrivileges = RequiredPrivileges ?? Array.Empty<string>();
			this.RequiresAuthentication = (RequiredPrivileges?.Length ?? 0) > 0;

			if (this.RequiresAuthentication)
				this.AuthenticationMechanisms = HttpModule.GetAuthenticationSchemes(RequiredPrivileges);
			else
				this.AuthenticationMechanisms = null;

			this.Arguments = new JsonRpcArgumentInfo[this.NrArguments];

			if (CaseSensitive)
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCulture);
			else
				this.NamedArguments = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

			foreach (ParameterInfo P in Arguments)
			{
				IsMetaDataArgument = false;

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
				else if (P.ParameterType == typeof(Dictionary<string, object?>) &&
					!(P.GetCustomAttribute<JsonRpcMetaDataArgumentAttribute>(true) is null))
				{
					if (this.MetaDataArgument is null)
					{
						this.MetaDataArgument = P.Position;
						this.NrSpecialArguments++;
						IsSpecialArgument = true;
						IsMetaDataArgument = true;
					}
					else
						throw new ArgumentException("Only one meta-data argument is allowed.", nameof(Method));
				}
				else
				{
					this.NamedArguments[P.Name] = P.Position;
					IsSpecialArgument = false;
				}

				this.Arguments[P.Position] = new JsonRpcArgumentInfo(P, IsSpecialArgument,
					P.HasDefaultValue, P.HasDefaultValue ? P.DefaultValue : null,
					IsMetaDataArgument);
			}
		}

		/// <summary>
		/// Authentication mechanisms available to authenticate users, if
		/// authentication is required.
		/// </summary>
		public HttpAuthenticationScheme[]? AuthenticationMechanisms { get; }

		/// <summary>
		/// Method information.
		/// </summary>
		public MethodInfo Method { get; }

		/// <summary>
		/// Number of arguments
		/// </summary>
		public int NrArguments { get; }

		/// <summary>
		/// Number of special arguments
		/// </summary>
		public int NrSpecialArguments { get; }

		/// <summary>
		/// Named arguments
		/// </summary>
		public Dictionary<string, int> NamedArguments { get; }

		/// <summary>
		/// Request argument index
		/// </summary>
		public int? RequestArgument { get; }

		/// <summary>
		/// Response argument index
		/// </summary>
		public int? ResponseArgument { get; }

		/// <summary>
		/// Meta-data argument index
		/// </summary>
		public int? MetaDataArgument { get; }

		/// <summary>
		/// Arguments
		/// </summary>
		public JsonRpcArgumentInfo[] Arguments { get; }

		/// <summary>
		/// If authentication of the user is required.
		/// </summary>
		public bool RequiresAuthentication { get; }

		/// <summary>
		/// Privileges required by the user that calls the method.
		/// </summary>
		public string[] RequiredPrivileges { get; }

		/// <summary>
		/// Checks if a user is authorized to call the method.
		/// </summary>
		/// <param name="User">User to check.</param>
		/// <returns>True if the user is authorized, false otherwise.</returns>
		public bool IsAuthorized(IUser? User)
		{
			return this.IsAuthorized(User, out _);
		}

		/// <summary>
		/// Checks if a user is authorized to call the method.
		/// </summary>
		/// <param name="User">User to check.</param>
		/// <param name="MissingPrivilege">Missing privilege, if any.</param>
		/// <returns>True if the user is authorized, false otherwise.</returns>
		public bool IsAuthorized(IUser? User, out string? MissingPrivilege)
		{
			MissingPrivilege = null;

			if (!this.RequiresAuthentication)
				return true;

			if (User is null)
				return false;

			foreach (string Privilege in this.RequiredPrivileges)
			{
				if (!User.HasPrivilege(Privilege))
				{
					MissingPrivilege = Privilege;
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Asserts user is authorized to call the method. If not, a 
		/// <see cref="ForbiddenException"/> is thrown.
		/// </summary>
		/// <param name="ObjectId">Object ID to use in log events.</param>
		/// <param name="User">User accessing method.</param>
		public void AssertAuthorized(string ObjectId, IUser? User)
		{
			if (!this.IsAuthorized(User, out string? MissingPrivilege))
			{
				throw ForbiddenException.AccessDenied(ObjectId,
					User?.UserName ?? string.Empty,
					MissingPrivilege ?? string.Empty);
			}
		}

		/// <summary>
		/// Tries to build a request for the method, based on the provided named parameters.
		/// </summary>
		/// <param name="Parameters">Named parameters.</param>
		/// <param name="MetaData">Additional Meta-Data available for the request.</param>
		/// <param name="Reason">Reason for not being able to create request.</param>
		/// <param name="Arguments">Ordered set of typed arguments, to be used in a
		/// call to the method.</param>
		/// <returns>If able to prepare a request to the method.</returns>
		public bool TryBuildRequest(Dictionary<string, object?> Parameters,
			Dictionary<string, object?>? MetaData,
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
					if (this.MetaDataArgument.HasValue &&
						P.Key == this.Arguments[this.MetaDataArgument.Value].Parameter.Name)
					{
						i = this.MetaDataArgument.Value;
					}
					else
					{
						Reason = "Invalid parameter name: " + P.Key;
						Arguments = null;
						return false;
					}
				}

				object? Value = P.Value;
				JsonRpcArgumentInfo ArgumentInfo = this.Arguments[i];
				Type ExpectedType = ArgumentInfo.Parameter.ParameterType;
				Type ParameterType = Value?.GetType() ?? typeof(object);

				if (ArgumentInfo.IsMetaDataArgument && !(MetaData is null))
				{
					Dictionary<string, object?>? MetaDataValue = Value as Dictionary<string, object?>;

					if (Value is null || !(MetaDataValue is null))
					{
						if (MetaDataValue is null)
							MetaDataValue = MetaData;
						else
						{
							foreach (KeyValuePair<string, object?> P2 in MetaData)
							{
								if (!MetaDataValue.ContainsKey(P2.Key))
									MetaDataValue[P2.Key] = P2.Value;
							}
						}

						Value = MetaDataValue;
					}
				}

				if (ParameterType == ExpectedType)
					Arguments[i] = Value;
				else if (Expression.TryConvert(Value, ExpectedType, true, out object Converted))
					Arguments[i] = Converted;
				else if (ArgumentInfo.HasDefaultValue &&
					Value is Dictionary<string, object?> Dictionary &&
					Dictionary.Count == 0)
				{
					Arguments[i] = ArgumentInfo.DefaultValue;
				}
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

			if (NrParametersSet < c - this.NrSpecialArguments)
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
