using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html.JavaScript;
using Waher.Content.Markdown;
using Waher.Content.Markdown.JavaScript;
using Waher.Networking.HTTP;
using Waher.Script;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// A resource that returns as a single JavaScript file, the following four files:
	/// 
	/// * /Master.js
	/// * /AlertPopup.md.js
	/// * /ConfirmPopup.md.js
	/// * /PromptPopup.md.js
	/// </summary>
	public class MasterJavascript : HttpSynchronousResource, IHttpGetMethod
	{
		private IResourceMap resourceMap;
		private string masterJsFileName = null;
		private string alertPopupMdFileName = null;
		private string confirmPopupMdFileName = null;
		private string promptPopupMdFileName = null;
		private string masterJs = null;
		private string alertPopupJs = null;
		private string confirmPopupJs = null;
		private string promptPopupJs = null;
		private DateTime masterJsLastModified = DateTime.MinValue;
		private DateTime alertPopupJsLastModified = DateTime.MinValue;
		private DateTime confirmPopupJsLastModified = DateTime.MinValue;
		private DateTime promptPopupJsLastModified = DateTime.MinValue;
		private DateTime lastModified = DateTime.MinValue;
		private byte[] masterJsData = null;
		private string contentType = null;

		/// <summary>
		/// A resource that returns as a single JavaScript file, the following four files:
		/// 
		/// * /Master.js
		/// * /AlertPopup.md.js
		/// * /ConfirmPopup.md.js
		/// * /PromptPopup.md.js
		/// </summary>
		public MasterJavascript(IResourceMap ResourceMap)
			: base("/Master.js")
		{
			this.resourceMap = ResourceMap;
		}

		/// <summary>
		/// How resources are mapped on the server.
		/// </summary>
		public IResourceMap ResourceMap
		{
			get => this.resourceMap;
			internal set => this.resourceMap = value;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			DateTime TP;
			bool Modified = false;

			if (string.IsNullOrEmpty(this.masterJsFileName))
			{
				if (!this.resourceMap.TryGetFileName("/Master.js", out this.masterJsFileName))
					this.masterJsFileName = Path.Combine(Gateway.RootFolder, "Master.js");
			}

			if (string.IsNullOrEmpty(this.alertPopupMdFileName))
			{
				if (!this.resourceMap.TryGetFileName("/AlertPopup.md", out this.alertPopupMdFileName))
					this.alertPopupMdFileName = Path.Combine(Gateway.RootFolder, "AlertPopup.md");
			}

			if (string.IsNullOrEmpty(this.confirmPopupMdFileName))
			{
				if (!this.resourceMap.TryGetFileName("/ConfirmPopup.md", out this.confirmPopupMdFileName))
					this.confirmPopupMdFileName = Path.Combine(Gateway.RootFolder, "ConfirmPopup.md");
			}

			if (string.IsNullOrEmpty(this.promptPopupMdFileName))
			{
				if (!this.resourceMap.TryGetFileName("/PromptPopup.md", out this.promptPopupMdFileName))
					this.promptPopupMdFileName = Path.Combine(Gateway.RootFolder, "PromptPopup.md");
			}

			if ((TP = File.GetLastWriteTimeUtc(this.masterJsFileName)) > this.masterJsLastModified ||
				this.masterJs is null)
			{
				this.masterJs = await File.ReadAllTextAsync(this.masterJsFileName);
				this.masterJsLastModified = TP;
				Modified = true;

				if (TP > this.lastModified)
					this.lastModified = TP;
			}

			if ((TP = File.GetLastWriteTimeUtc(this.alertPopupMdFileName)) > this.alertPopupJsLastModified ||
				this.alertPopupJs is null)
			{
				this.alertPopupJs = await MarkdownToJavaScript(this.alertPopupMdFileName);
				this.alertPopupJsLastModified = TP;
				Modified = true;

				if (TP > this.lastModified)
					this.lastModified = TP;
			}

			if ((TP = File.GetLastWriteTimeUtc(this.confirmPopupMdFileName)) > this.confirmPopupJsLastModified ||
				this.confirmPopupJs is null)
			{
				this.confirmPopupJs = await MarkdownToJavaScript(this.confirmPopupMdFileName);
				this.confirmPopupJsLastModified = TP;
				Modified = true;

				if (TP > this.lastModified)
					this.lastModified = TP;
			}

			if ((TP = File.GetLastWriteTimeUtc(this.promptPopupMdFileName)) > this.promptPopupJsLastModified ||
				this.promptPopupJs is null)
			{
				this.promptPopupJs = await MarkdownToJavaScript(this.promptPopupMdFileName);
				this.promptPopupJsLastModified = TP;
				Modified = true;

				if (TP > this.lastModified)
					this.lastModified = TP;
			}

			if (Modified || this.masterJsData is null)
			{
				StringBuilder sb = new StringBuilder();

				sb.AppendLine(this.masterJs);
				sb.AppendLine();
				sb.AppendLine(this.alertPopupJs);
				sb.AppendLine();
				sb.AppendLine(this.confirmPopupJs);
				sb.AppendLine();
				sb.AppendLine(this.promptPopupJs);

				this.contentType = JavaScriptCodec.DefaultContentType + "; charset=utf-8";
				this.masterJsData = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
			}

			Response.ContentType = this.contentType;
			await Response.Write(true, this.masterJsData);
		}

		private static async Task<string> MarkdownToJavaScript(string FileName)
		{
			string Markdown = await File.ReadAllTextAsync(FileName);
			MarkdownSettings Settings = new MarkdownSettings()
			{
				ParseMetaData = false,
				AllowInlineScript = true,
				Variables = new Variables()
			};
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings, FileName, null, null);
			string JavaScript = await Doc.GenerateJavaScript();

			return JavaScript;
		}

	}
}
