using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Runtime.Collections;
using Waher.Security;

namespace Waher.Networking.HTTP.Mcp.Model.Server
{
	/// <summary>
	/// Contains information about an MCP Server Prompt
	/// </summary>
	public class Prompt
	{
		private readonly JsonRpcMethodInfo methodInfo;
		private Icons? icons = null;

		/// <summary>
		/// Contains information about an MCP Server Prompt
		/// </summary>
		/// <param name="McpServer">MCP Server reference.</param>
		/// <param name="Method">Method to invoke when prompt is executed.</param>
		/// <param name="Title">A human-readable title for the prompt.</param>
		/// <param name="Description">A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the prompt. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="MetaData">Meta-data associated with prompt.</param>
		public Prompt(HttpMcpServerResource McpServer, MethodInfo Method, string Title, 
			string Description, string IconsMethod, 
			params KeyValuePair<string, object>[] MetaData)
		{
			this.Method = Method;
			this.Title = Title;
			this.Description = Description;
			this.IconsMethod = IconsMethod;
			this.MetaData = MetaData;
			this.HasReturnValue = Method.ReturnType != typeof(void);
			this.ReturnAttributes = Method.ReturnParameter?.GetCustomAttribute<McpParameterAttribute>();

			this.methodInfo = new JsonRpcMethodInfo(McpServer, Method, false, 
				JsonRpcWebService.GetRequiredPrivileges(Method));

			this.RequiresAuthentication = this.methodInfo.RequiresAuthentication;
			this.RequiredPrivileges = this.methodInfo.RequiredPrivileges;
		}

		/// <summary>
		/// Method to invoke when prompt is executed.
		/// </summary>
		public MethodInfo Method { get; }

		/// <summary>
		/// A human-readable title for the prompt.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// A human-readable description of the prompt.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available prompts. 
		/// It can be thought of like a "hint" to the model.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Name of method that returns an <see cref="Icon?"/>, an an <see cref="Icon[]?"/>
		/// or an <see cref="Icons?"/> resource representing the prompt. If null or empty, the 
		/// icon of the MCP server will be used.
		/// </summary>
		public string IconsMethod { get; }

		/// <summary>
		/// Meta-data associated with prompt.
		/// </summary>
		public KeyValuePair<string, object>[] MetaData { get; }

		/// <summary>
		/// If the prompt returns a value.
		/// </summary>
		public bool HasReturnValue { get; }

		/// <summary>
		/// Any MCP attributes declared for the return value.
		/// </summary>
		public McpParameterAttribute? ReturnAttributes { get; }

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
			return this.methodInfo.IsAuthorized(User);
		}

		/// <summary>
		/// Asserts user is authorized to call the method. If not, a 
		/// <see cref="ForbiddenException"/> is thrown.
		/// </summary>
		/// <param name="ObjectId">Object ID to use in log events.</param>
		/// <param name="User">User accessing method.</param>
		public void AssertAuthorized(string ObjectId, IUser? User)
		{
			this.methodInfo.AssertAuthorized(ObjectId, User);
		}

		/// <summary>
		/// Converts object to a generic representation.
		/// </summary>
		/// <param name="Resource">MCP Server reference.</param>
		/// <returns>Generic representation.</returns>
		public async Task<Dictionary<string, object>> ToJson(HttpMcpServerResource Resource)
		{
			this.icons ??= await Tool.GetIcons(Resource, this.IconsMethod);

			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "name", this.Method.Name }
			};

			PopulateArguments(this.Method, Result);

			if (!string.IsNullOrEmpty(this.Title))
				Result.Add("title", this.Title);

			if (!string.IsNullOrEmpty(this.Title))
				Result.Add("description", this.Description);

			if (!this.icons.Empty)
				Result.Add("icons", this.icons.ToJson());

			if ((this.MetaData?.Length ?? 0) > 0)
			{
				Dictionary<string, object> MetaData = new Dictionary<string, object>();

				foreach (KeyValuePair<string, object> P in this.MetaData!)
					MetaData[P.Key] = P.Value;

				Result["_meta"] = MetaData;
			}

			return Result;
		}

		private static void PopulateArguments(MethodInfo Method,
			Dictionary<string, object> Result)
		{
			ParameterInfo[] Parameters = Method.GetParameters();

			ChunkedList<Dictionary<string, object?>>? Arguments = null;

			foreach (ParameterInfo Parameter in Parameters)
			{
				Dictionary<string, object?> PromptArgument = new Dictionary<string, object?>()
				{
					{ "name", Parameter.Name },
					{ "required", !Parameter.HasDefaultValue }
				};

				McpParameterAttribute? Attribute = Parameter.GetCustomAttribute<McpParameterAttribute>();
				Attribute?.Annotate(PromptArgument);

				Arguments ??= new ChunkedList<Dictionary<string, object?>>();
				Arguments.Add(PromptArgument);
			}

			if (!(Arguments is null))
				Result["arguments"] = Arguments.ToArray();
		}

		/// <summary>
		/// Tries to build a request for the method, based on the provided named parameters.
		/// </summary>
		/// <param name="Parameters">Named parameters.</param>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="Response">HTTP Response object.</param>
		/// <param name="MetaData">Additional Meta-Data available for the request.</param>
		/// <param name="Reason">Reason for not being able to create request.</param>
		/// <param name="Arguments">Ordered set of typed arguments, to be used in a
		/// call to the method.</param>
		/// <returns>If able to prepare a request to the method.</returns>
		public bool TryBuildRequest(Dictionary<string, object?> Parameters,
			HttpRequest Request, HttpResponse Response,
			Dictionary<string, object?>? MetaData,
			[NotNullWhen(false)] out string? Reason,
			[NotNullWhen(true)] out object?[]? Arguments)
		{
			if (!this.methodInfo.TryBuildRequest(Parameters, MetaData, out Reason, out Arguments))
				return false;

			if (this.methodInfo.RequestArgument.HasValue)
				Arguments[this.methodInfo.RequestArgument.Value] = Request;

			if (this.methodInfo.ResponseArgument.HasValue)
				Arguments[this.methodInfo.ResponseArgument.Value] = Response;

			return true;
		}

	}
}
