using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Object Content Block
	/// </summary>
	public class ObjectContent : ContentBlock 
	{
		/// <summary>
		/// Object Content Block
		/// </summary>
		public ObjectContent()
			: base()
		{
		}

		/// <summary>
		/// What types the content block encodes.
		/// </summary>
		public override Type[] Encodes => new Type[] 
		{ 
			typeof(object)
		};

		/// <summary>
		/// If the content block is encoded as structured content.
		/// </summary>
		public override bool IsStructuredContent => true;

		/// <summary>
		/// Encodes a content block, given the content to encode and available
		/// annotations and meta-data.
		/// </summary>
		/// <param name="Content">Content to encode.</param>
		/// <returns>MCP-encoded content block.</returns>
		public override async Task<Dictionary<string, object?>> Encode(object Content)
		{
			Dictionary<string, object?> Properties = new Dictionary<string, object?>();
			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "type", "object" },
				{ "properties", Properties }
			};

			this.Annotate(Result);

			Type T = Content?.GetType() ?? typeof(object);

			foreach (FieldInfo FI in T.GetFields(BindingFlags.Public | BindingFlags.Instance))
				Properties[FI.Name] = await ScriptNode.WaitPossibleTask(FI.GetValue(Content));

			foreach (PropertyInfo PI in T.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				Properties[PI.Name] = await ScriptNode.WaitPossibleTask(PI.GetValue(Content));

			return Result;
		}
	}
}
