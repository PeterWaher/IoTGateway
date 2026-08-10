using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.HTTP.JsonRpc.Transports;
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
		public int StatusCode = 204;
		public string StatusMessage = "No Content";
		public string? ErrorMessage = null;
		public object? Result = null;
		public bool IsResult = false;
		public bool IsError = false;

		public void SetError(int ErrorCode, string ErrorMessage, int StatusCode,
			string StatusMessage)
		{
			this.ErrorCode = ErrorCode;
			this.ErrorMessage = ErrorMessage;
			this.StatusCode = StatusCode;
			this.StatusMessage = StatusMessage;
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

		/// <summary>
		/// Prepares a response to the request.
		/// </summary>
		/// <param name="WebService">JSON-RPC web service object reference.</param>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <returns>If a response has been returned</returns>
		public async Task<bool> BuildResponse(JsonRpcWebService WebService,
			IJsonRpcCall Call)
		{
			bool HasSniffer = Call.Server.HasSniffers;

			if (this.IsResult)
			{
				if (this.Id is null)
				{
					this.SetError(-32600, "Missing id attribute.",
						BadRequestException.Code, BadRequestException.StatusMessage);
					return false;
				}

				IJsonRpcClientRequest? Request = WebService.PopClientRequest(this.Id.ToString());
				if (Request is null)
				{
					this.SetError(-32600, "Id attribute not recognized or obsolete.",
						NotFoundException.Code, NotFoundException.StatusMessage);
					return false;
				}

				await Request.ReportResult(this.Result);
				return false;
			}
			else if (this.IsError && !(this.Id is null))
			{
				IJsonRpcClientRequest? Request = WebService.PopClientRequest(this.Id.ToString());
				if (Request is null)
				{
					this.SetError(-32600, "Id attribute not recognized or obsolete.",
						NotFoundException.Code, NotFoundException.StatusMessage);
					return false;
				}

				await Request.ReportError(this.ErrorCode, this.ErrorMessage ?? "Unable to perform request.");
				return false;
			}

			if (this.BatchRequests is null)
			{
				this.ResponseObject = new Dictionary<string, object?>();

				if (!string.IsNullOrEmpty(this.JsonVersion))
					this.ResponseObject["jsonrpc"] = this.JsonVersion;

				if (!(this.Id is null))
					this.ResponseObject["id"] = this.Id;

				if (this.MethodInfo is null && !this.ErrorCode.HasValue)
				{
					this.SetError(-32600, "Missing method.",
						BadRequestException.Code, BadRequestException.StatusMessage);
				}

				if (!this.ErrorCode.HasValue)
				{
					try
					{
						int i, c = this.MethodInfo!.NrArguments;
						int NrParametersSet = 0;
						object?[]? Parameters = null;

						if (!(this.ParametersObj is null))
						{
							if (!this.MethodInfo.TryBuildRequest(
								this.Id?.ToString() ?? string.Empty,
								this.ParametersObj, null, out string? Reason,
								out Parameters))
							{
								this.SetError(-32602, Reason,
									BadRequestException.Code, BadRequestException.StatusMessage);
							}
						}
						else if (!(this.ParametersArray is null))
						{
							if (this.ParametersArray.Length != c - this.MethodInfo.NrSpecialArguments)
							{
								this.SetError(-32602, "Invalid number of parameters.",
									BadRequestException.Code, BadRequestException.StatusMessage);
							}
							else
							{
								Parameters = new object?[c];

								for (i = 0; i < c; i++)
								{
									object? Value = this.ParametersArray.GetValue(i);
									ProtectedMethodArgumentInfo ArgumentInfo = this.MethodInfo.Arguments[i];
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
											", Expected: " + ExpectedType.FullName,
											BadRequestException.Code, BadRequestException.StatusMessage);
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
								foreach (ProtectedMethodArgumentInfo Argument in this.MethodInfo.Arguments)
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
							{
								this.SetError(-32600, "Missing required parameters.",
									BadRequestException.Code, BadRequestException.StatusMessage);
							}
						}

						if (!this.ErrorCode.HasValue)
						{
							Parameters ??= new object?[c];

							if (this.MethodInfo.NrSpecialArguments > 0)
							{
								if (this.MethodInfo.CallArgument.HasValue)
									Parameters[this.MethodInfo.CallArgument.Value] = Call;

								if (this.MethodInfo.IdArgument.HasValue)
									Parameters[this.MethodInfo.IdArgument.Value] = this.Id;
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

								Call.Server.Information(sb.ToString());
							}

							IJsonRpcSession? Session = await WebService.TryGetSession(Call, false);

							await Call.CheckAuthentication(Session,
								this.MethodInfo.RequiresAuthentication,
								this.MethodInfo.AuthenticationMechanisms,
								this.MethodInfo.RequiredPrivileges);

							if (Call.ResponseSent)
								return true;

							this.Result = await ScriptNode.WaitPossibleTask(
								this.MethodInfo.Method.Invoke(WebService, Parameters));

							if (HasSniffer)
							{
								if (this.Result is null)
									Call.Server.Information("Result: null");
								else if (Expression.IsVoid(this.Result.GetType()))
									Call.Server.Information("Result: void");
								else
								{
									Call.Server.Information("Result: " +
										Expression.ToExpressionString(this.Result));
								}
							}

							if (Call.ResponseSent)
								return true;
						}
					}
					catch (Exception ex)
					{
						if (HasSniffer)
							Call.Server.Exception(ex);

						this.SetError(-32603, Log.UnnestException(ex).Message,
							InternalServerErrorException.Code, InternalServerErrorException.StatusMessage);
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
				else if (Expression.IsNullOrVoid(this.Result))
					this.ResponseObject["result"] = new Dictionary<string, object>();
				else
					this.ResponseObject["result"] = this.Result;

				this.Response = this.ResponseObject;

				if (!(this.Id is null) && this.StatusCode == 204)
				{
					this.StatusCode = 200;
					this.StatusMessage = "OK";
				}
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

					if (await Request.BuildResponse(WebService, Call))
						return true;

					if (!(Request.Id is null))
						this.ResponseArray[j++] = Request.ResponseObject!;
				}

				this.Response = this.ResponseArray;

				if (this.StatusCode == 204)
				{
					this.StatusCode = 200;
					this.StatusMessage = "OK";
				}
			}

			return false;
		}
	}
}
