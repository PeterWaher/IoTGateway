using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.Mcp.Model.Server
{
	/// <summary>
	/// Contains information about an MCP Server Tool
	/// </summary>
	public class Tool
	{
		private Icons? icons = null;

		/// <summary>
		/// Contains information about an MCP Server Tool
		/// </summary>
		/// <param name="Method">Method to invoke when tool is executed.</param>
		/// <param name="Title">A human-readable title for the tool.</param>
		/// <param name="Description">A human-readable description of the tool.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available tools. 
		/// It can be thought of like a "hint" to the model.</param>
		/// <param name="IconsMethod">Name of method that returns an <see cref="Icon?"/>, 
		/// an an <see cref="Icon[]?"/> or an <see cref="Icons?"/> resource representing 
		/// the tool. If null or empty, the icon of the MCP server will be used.</param>
		/// <param name="CanModifyEnvironment">If the tool can modify the environment. If false, 
		/// the tool is expected to be read-only and not cause any side effects.</param>
		/// <param name="CanDestroyEnvironment">If true, the tool may perform destructive 
		/// updates to its environment. If false, the tool performs only additive updates.</param>
		/// <param name="Idempotent">If true, calling the tool repeatedly with the same 
		/// arguments will have no additional effect on its environment.</param>
		/// <param name="OpenWorldAccess">If true, this tool may interact with an 
		/// "open world" of external entities. If false, the tool's domain of interaction 
		/// is closed.</param>
		/// <param name="MetaData">Meta-data associated with tool.</param>
		public Tool(MethodInfo Method, string Title, string Description, string IconsMethod,
			bool CanModifyEnvironment, bool CanDestroyEnvironment, bool Idempotent,
			bool OpenWorldAccess, params KeyValuePair<string, object>[] MetaData)
		{
			this.Method = Method;
			this.Title = Title;
			this.Description = Description;
			this.IconsMethod = IconsMethod;
			this.CanModifyEnvironment = CanModifyEnvironment;
			this.CanDestroyEnvironment = CanDestroyEnvironment;
			this.Idempotent = Idempotent;
			this.OpenWorldAccess = OpenWorldAccess;
			this.MetaData = MetaData;
		}

		/// <summary>
		/// Method to invoke when tool is executed.
		/// </summary>
		public MethodInfo Method { get; private set; }

		/// <summary>
		/// A human-readable title for the tool.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// A human-readable description of the tool.
		/// 
		/// This can be used by clients to improve the LLM's understanding of available tools. 
		/// It can be thought of like a "hint" to the model.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Name of method that returns an <see cref="Icon?"/>, an an <see cref="Icon[]?"/>
		/// or an <see cref="Icons?"/> resource representing the tool. If null or empty, the 
		/// icon of the MCP server will be used.
		/// </summary>
		public string IconsMethod { get; private set; }

		/// <summary>
		/// If the tool can modify the environment. If false, the tool is expected to be 
		/// read-only and not cause any side effects.
		/// </summary>
		public bool CanModifyEnvironment { get; private set; }

		/// <summary>
		/// If true, the tool may perform destructive updates to its environment.
		/// If false, the tool performs only additive updates.
		/// </summary>
		/// <remarks>
		/// This property is meaningful only when <see cref="CanModifyEnvironment"/> is true.
		/// </remarks>
		public bool CanDestroyEnvironment { get; private set; }

		/// <summary>
		/// If true, calling the tool repeatedly with the same arguments
		/// will have no additional effect on its environment.
		/// </summary>
		/// <remarks>
		/// This property is meaningful only when <see cref="CanModifyEnvironment"/> is true.
		/// </remarks>
		public bool Idempotent { get; private set; }

		/// <summary>
		/// If true, this tool may interact with an "open world" of external
		/// entities.If false, the tool's domain of interaction is closed.
		/// </summary>
		public bool OpenWorldAccess { get; private set; }

		/// <summary>
		/// Meta-data associated with tool.
		/// </summary>
		public KeyValuePair<string, object>[] MetaData { get; private set; }

		/// <summary>
		/// Converts object to a generic representation.
		/// </summary>
		/// <param name="Resource">MCP Server reference.</param>
		/// <returns>Generic representation.</returns>
		public async Task<Dictionary<string, object>> ToJson(HttpMcpServerResource Resource)
		{
			if (this.icons is null)
			{
				if (string.IsNullOrEmpty(this.IconsMethod))
					this.icons = new Icons();
				else
				{
					MethodInfo? MI = Resource.GetType().GetMethod(this.IconsMethod, 
						BindingFlags.Static | BindingFlags.Instance | 
						BindingFlags.Public | BindingFlags.NonPublic);

					if (MI is null)
						this.icons = new Icons();
					else
					{
						object? Obj = await ScriptNode.WaitPossibleTask(MI.Invoke(Resource, null));

						if (Obj is Icons Typed)
							this.icons = Typed;
						else if (Obj is Icon[] IconArray)
							this.icons = new Icons(IconArray);
						else if (Obj is Icon SingleIcon)
							this.icons = new Icons(SingleIcon);
						else if (Obj is null)
							this.icons = new Icons();
						else
						{
							throw new ArgumentException("Method " + this.IconsMethod +
								"returned an invalid type: " + Obj.GetType().FullName,
								nameof(this.IconsMethod));
						}
					}
				}

				if (this.icons.Empty)
					this.icons = Resource.Icons;
			}

			Dictionary<string, object> Annotations = new Dictionary<string, object>()
			{
				{ "readOnlyHint", this.CanModifyEnvironment },
				{ "destructiveHint", this.CanDestroyEnvironment },
				{ "idempotentHint", this.Idempotent },
				{ "openWorldHint", this.OpenWorldAccess }
			};
			Dictionary<string, object> Result = new Dictionary<string, object>()
			{
				{ "name", this.Method.Name },
				{ "execution", new Dictionary<string,object>()
					{
						{ "taskSupport", "optional" }
					}
				},
				{ "annotations", Annotations }
			};

			if (!string.IsNullOrEmpty(this.Title))
				Annotations.Add("title", this.Title);

			if (!string.IsNullOrEmpty(this.Title))
				Result.Add("description", this.Description);

			if (!this.icons.Empty)
				Result.Add("icons", this.icons.ToJson());

			// TODO: inputSchema
			// TODO: outputSchema

			if ((this.MetaData?.Length ?? 0) > 0)
			{
				Dictionary<string, object> MetaData = new Dictionary<string, object>();

				foreach (KeyValuePair<string, object> P in this.MetaData!)
					MetaData[P.Key] = P.Value;

				Result["_meta"] = MetaData;
			}

			return Result;
		}
}
}
