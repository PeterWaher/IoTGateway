using System;

namespace Waher.Networking.HTTP.JsonRpc.MetaData
{
	/// <summary>
	/// Adds documentation to a JSON-RPC method, parameter, property, field, event or 
	/// return value. This documentation will be available in the automatically generated
	/// documentation of the JSON-RPC web service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field |
		AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.Property |
		AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = true)]
	public class JsonRpcDocumentationAttribute : Attribute
	{
		/// <summary>
		/// Adds documentation to a JSON-RPC method, parameter, property, field, event or 
		/// return value. This documentation will be available in the automatically generated
		/// documentation of the JSON-RPC web service.
		/// </summary>
		/// <param name="Documentation">Documentation to include.</param>
		public JsonRpcDocumentationAttribute(string Documentation)
			: this(Documentation, false)
		{
		}

		/// <summary>
		/// Adds documentation to a JSON-RPC method, parameter, property, field, event or 
		/// return value. This documentation will be available in the automatically generated
		/// documentation of the JSON-RPC web service.
		/// </summary>
		/// <param name="Documentation">Documentation to include.</param>
		/// <param name="IsMarkdown">If the documentation is in Markdown format.</param>
		public JsonRpcDocumentationAttribute(string Documentation, bool IsMarkdown)
		{
			this.Documentation = Documentation;
			this.IsMarkdown = IsMarkdown;
		}

		/// <summary>
		/// Documentation to include.
		/// </summary>
		public string Documentation { get; }

		/// <summary>
		/// If the documentation is in Markdown format.
		/// </summary>
		public bool IsMarkdown { get; }
	}
}