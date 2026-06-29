using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Script;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.JsonRpc
{
	/// <summary>
	/// Server-side information about a JSON-RPC request.
	/// </summary>
	internal class JsonRpcServerRequest : IDisposable
	{
		public JsonRpcServerRequest?[]? BatchRequests = null;
		public Dictionary<string, object?>? ResponseObject = null;
		public Dictionary<string, object?>[]? ResponseArray = null;
		public object? Response = null;
		public Dictionary<string, object?>? ParametersObj = null;
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

			if (!(this.BatchRequests is null))
			{
				int i, c = this.BatchRequests.Length;

				for (i = 0; i < c; i++)
					this.BatchRequests[i]?.Dispose();

				this.BatchRequests = null;
			}

			this.Result = null;
		}

		public async Task BuildResponse(JsonRpcWebService WebService, HttpRequest HttpRequest,
			HttpResponse HttpResponse)
		{
			bool HasSniffer = HttpRequest.Server.HasSniffers;

			if (this.BatchRequests is null)
			{
				this.ResponseObject = new Dictionary<string, object?>();

				if (!string.IsNullOrEmpty(this.JsonVersion))
					this.ResponseObject["jsonrpc"] = this.JsonVersion;

				if (!(this.Id is null))
					this.ResponseObject["id"] = this.Id;

				if (this.MethodInfo is null && !this.ErrorCode.HasValue)
					this.SetError(-32600, "Missing method.", 400);

				if (!this.ErrorCode.HasValue)
				{
					try
					{
						int i, c = this.MethodInfo!.NrArguments;
						int NrParametersSet = 0;
						object?[]? Parameters = null;

						if (!(this.ParametersObj is null))
						{
							if (!this.MethodInfo.TryBuildRequest(this.ParametersObj, null,
								out string? Reason, out Parameters))
							{
								this.SetError(-32602, Reason, 400);
							}
						}
						else if (!(this.ParametersArray is null))
						{
							if (this.ParametersArray.Length != c - this.MethodInfo.NrSpecialArguments)
								this.SetError(-32602, "Invalid number of parameters.", 400);
							else
							{
								Parameters = new object?[c];

								for (i = 0; i < c; i++)
								{
									object? Value = this.ParametersArray.GetValue(i);
									JsonRpcArgumentInfo ArgumentInfo = this.MethodInfo.Arguments[i];
									Type ExpectedType = ArgumentInfo.Parameter.ParameterType;
									Type ParameterType = Value?.GetType() ?? typeof(object);

									if (ParameterType == ExpectedType)
										Parameters[i] = Value;
									else if (Expression.TryConvert(Value, ExpectedType, true, out object Converted))
										Parameters[i] = Converted;
									else if (ArgumentInfo.HasDefaultValue &&
										Value is Dictionary<string, object?> Dictionary &&
										Dictionary.Count == 0)
									{
										Parameters[i] = ArgumentInfo.DefaultValue;
									}
									else
									{
										this.SetError(-32602, "Parameter " + (i + 1).ToString() +
											" has incorrect type: " + ParameterType.FullName +
											", Expected: " + ExpectedType.FullName, 400);
										break;
									}
								}
							}
						}
						else
						{
							Parameters = new object?[c];

							if (NrParametersSet != c - this.MethodInfo.NrSpecialArguments)
							{
								foreach (JsonRpcArgumentInfo Argument in this.MethodInfo.Arguments)
								{
									if (!Argument.IsSpecialArgument &&
										Argument.HasDefaultValue)
									{
										Parameters[Argument.Parameter.Position] = Argument.DefaultValue;
										NrParametersSet++;
									}
								}
							}

							if (NrParametersSet != c - this.MethodInfo.NrSpecialArguments)
								this.SetError(-32600, "Missing required parameters.", 400);
						}

						if (!this.ErrorCode.HasValue)
						{
							Parameters ??= new object?[c];

							if (this.MethodInfo.NrSpecialArguments > 0)
							{
								if (this.MethodInfo.RequestArgument.HasValue)
									Parameters[this.MethodInfo.RequestArgument.Value] = HttpRequest;

								if (this.MethodInfo.ResponseArgument.HasValue)
									Parameters[this.MethodInfo.ResponseArgument.Value] = HttpResponse;
							}

							if (HasSniffer)
							{
								StringBuilder sb = new StringBuilder();
								bool First = true;

								sb.Append(this.MethodInfo.Method.Name);
								sb.Append('(');

								foreach (object? P in Parameters)
								{
									if (First)
										First = false;
									else
										sb.Append(',');

									sb.Append(Expression.ToExpressionString(P));
								}

								sb.Append(')');

								HttpRequest.Server.Information(sb.ToString());
							}

							this.Result = await ScriptNode.WaitPossibleTask(
								this.MethodInfo.Method.Invoke(WebService, Parameters));

							if (HasSniffer)
							{
								HttpRequest.Server.Information("Result: " + 
									Expression.ToExpressionString(this.Result));
							}
						}
					}
					catch (Exception ex)
					{
						if (HasSniffer)
							HttpRequest.Server.Exception(ex);

						this.SetError(-32603, ex.Message, 500);
					}
				}

				if (this.ErrorCode.HasValue)
				{
					this.ResponseObject["error"] = new Dictionary<string, object>()
					{
						{ "code", this.ErrorCode.Value },
						{ "message", this.ErrorMessage ?? string.Empty }
					};
				}
				else
					this.ResponseObject["result"] = this.Result;

				this.Response = this.ResponseObject;

				if (!(this.Id is null) && this.StatusCode == 204)
					this.StatusCode = 200;
			}
			else
			{
				int i, c = this.BatchRequests.Length;
				int j, d = 0;

				for (i = 0; i < c; i++)
				{
					if (!(this.BatchRequests[i]!.Id is null))
						d++;
				}

				this.ResponseArray = new Dictionary<string, object?>[d];

				for (i = j = 0; i < c; i++)
				{
					JsonRpcServerRequest Request = this.BatchRequests[i]!;

					await Request.BuildResponse(WebService, HttpRequest, HttpResponse);

					if (!(Request.Id is null))
						this.ResponseArray[j++] = Request.ResponseObject!;
				}

				this.Response = this.ResponseArray;

				if (this.StatusCode == 204)
					this.StatusCode = 200;
			}
		}
	}
}
