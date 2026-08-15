using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Binary;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// If the camera should be the user-facing camera or the environment-facing camera.
	/// </summary>
	public enum CameraCapture
	{
		/// <summary>
		/// User-facing camera.
		/// </summary>
		User,

		/// <summary>
		/// Environment-facing camera.
		/// </summary>
		Environment
	}

	/// <summary>
	/// Provides meta-data about a file upload parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpFileUploadParameterAttribute : McpParameterAttribute
	{
		/// <summary>
		/// Regular expression pattern for a BASE64-encoded string.
		/// </summary>
		public const string Base64Pattern = @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";

		/// <summary>
		/// Provides meta-data about a file upload parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="Accept">Accepted file Content-Types.</param>
		public McpFileUploadParameterAttribute(string? Title, string? Description,
			string Accept)
			: base(Title, Description)
		{
			this.Accept = Accept;
			this.Capture = null;
		}

		/// <summary>
		/// Provides meta-data about a file upload parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="Accept">Accepted file Content-Types.</param>
		/// <param name="Capture">If camera capture is requested, and if so, which 
		/// camera to use.</param>
		public McpFileUploadParameterAttribute(string? Title, string? Description,
			string Accept, CameraCapture Capture)
			: base(Title, Description)
		{
			this.Accept = Accept;
			this.Capture = Capture;
		}

		/// <summary>
		/// Accepted file Content-Types.
		/// </summary>
		public string? Accept { get; }

		/// <summary>
		/// If camera capture is requested, and if so, which camera to use.
		/// </summary>
		public CameraCapture? Capture { get; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			if (Schema.TryGetValue("title", out object? Obj) && Obj is string Title)
				Schema["title"] = Title + " (BASE64)";

			if (Schema.TryGetValue("description", out Obj) && Obj is string Description)
				Schema["description"] = Description + " (As a BASE64-encoded string.)";

			Schema["type"] = "string";
			Schema["pattern"] = Base64Pattern;
		}

		/// <summary>
		/// Gets HTML input attributes for the parameter, if any.
		/// </summary>
		/// <param name="Attributes">Set of attributes.</param>
		public override void GetHtmlInputAttributes(Dictionary<string, string> Attributes)
		{
			base.GetHtmlInputAttributes(Attributes);
			Attributes["type"] = "file";

			if (!string.IsNullOrEmpty(this.Accept))
				Attributes["accept"] = this.Accept;

			if (this.Capture.HasValue)
				Attributes["capture"] = this.Capture.Value.ToString().ToLower();
		}

		/// <summary>
		/// Gets the HTML attribute value for a parameter, if any.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>HTML attribute value</returns>
		public override string GetHtmlAttributeValue(object Value)
		{
			return string.Empty;
		}

		/// <summary>
		/// Checks if a value is valid for the parameter.
		/// </summary>
		/// <param name="Value">Value to check.</param>
		/// <returns>If the value is valid according to validation rules for the parameter.</returns>
		public override async Task<bool> IsValid(object Value)
		{
			if (Value is CustomEncoding || Value is byte[])
				return true;

			if (!(Value is string s))
				return false;

			try
			{
				Convert.FromBase64String(s);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
