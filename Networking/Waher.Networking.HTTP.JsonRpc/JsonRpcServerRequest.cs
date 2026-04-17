using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Information about a JSON-RPC request.
	/// </summary>
	internal class JsonRpcServerRequest : IDisposable
	{
		public Dictionary<string, object?> ResponseObj = new Dictionary<string, object?>();
		public Dictionary<string, object>? ParametersObj = null;
		public JsonRpcMethodInfo? MethodInfo = null;
		public Array? ParametersArray = null;
		public string JsonVersion = string.Empty;
		public object? Id = null;
		public int? ErrorCode = null;
		public int StatusCode = 204;   // No Content
		public string? ErrorMessage = null;
		public object? Result = null;

		public void SetError(int ErrorCode, string ErrorMessage, int StatusCode)
		{
			this.ErrorCode = ErrorCode;
			this.ErrorMessage = ErrorMessage;
			this.StatusCode = StatusCode;
		}

		public void Dispose()
		{
			if (this.Result is IDisposable Disposable)
				Disposable.Dispose();

			this.Result = null;
		}

		public async Task BuildResponse()
		{
			if (!string.IsNullOrEmpty(this.JsonVersion))
				this.ResponseObj["jsonrpc"] = this.JsonVersion;

			if (!(this.Id is null))
				this.ResponseObj["id"] = this.Id;

			if (this.MethodInfo is null && !this.ErrorCode.HasValue)
				this.SetError(-32600, "Missing method.", 400);

			if (!this.ErrorCode.HasValue)
			{
				try
				{
					int i, c = this.MethodInfo!.NrArguments;
					object?[] Parameters = new object?[c];

					if (!(this.ParametersObj is null))
					{
						if (this.ParametersObj.Count != c)
							this.SetError(-32602, "Invalid number of parameters.", 400);
						else
						{
							foreach (KeyValuePair<string, object> P in this.ParametersObj)
							{
								if (!this.MethodInfo.NamedArguments.TryGetValue(P.Key, out i))
								{
									this.SetError(-32602, "Invalid parameter name: " + P.Key, 400);
									break;
								}
								else
								{
									Type ExpectedType = this.MethodInfo.Arguments[i].ParameterType;
									Type ParameterType = P.Value?.GetType() ?? typeof(object);

									if (ParameterType == ExpectedType)
										Parameters[i] = P.Value;
									else if (Expression.TryConvert(P.Value, ExpectedType, out object Converted))
										Parameters[i] = Converted;
									else
									{
										this.SetError(-32602, "Parameter " + P.Key +
											" has incorrect type: " + ParameterType.FullName +
											", Expected: " + ExpectedType.FullName, 400);
										break;
									}
								}
							}

							if (!this.ErrorCode.HasValue)
							{
								this.Result = await ScriptNode.WaitPossibleTask(
									this.MethodInfo.Method.DynamicInvoke(Parameters));
							}
						}
					}
					else if (!(this.ParametersArray is null))
					{
						if (this.ParametersArray.Length != c)
							this.SetError(-32602, "Invalid number of parameters.", 400);
						else
						{
							for (i = 0; i < c; i++)
							{
								object? Value = this.ParametersArray.GetValue(i);
								Type ExpectedType = this.MethodInfo.Arguments[i].ParameterType;
								Type ParameterType = Value?.GetType() ?? typeof(object);

								if (ParameterType == ExpectedType)
									Parameters[i] = Value;
								else if (Expression.TryConvert(Value, ExpectedType, out object Converted))
									Parameters[i] = Converted;
								else
								{
									this.SetError(-32602, "Parameter " + (i + 1).ToString() +
										" has incorrect type: " + ParameterType.FullName +
										", Expected: " + ExpectedType.FullName, 400);
									break;
								}
							}

							this.Result = this.MethodInfo.Method.DynamicInvoke(Parameters);
						}
					}
					else
					{
						if (this.MethodInfo.NrArguments == 0)
							this.Result = this.MethodInfo.Method.DynamicInvoke(Parameters);
						else
							this.SetError(-32600, "Missing parameters.", 400);
					}
				}
				catch (Exception ex)
				{
					this.SetError(-32603, ex.Message, 500);
				}
			}

			if (this.ErrorCode.HasValue)
			{
				this.ResponseObj["error"] = new Dictionary<string, object>()
					{
						{ "code", this.ErrorCode.Value },
						{ "message", this.ErrorMessage ?? string.Empty }
					};
			}
			else
				this.ResponseObj["result"] = this.Result;
		}
	}
}
