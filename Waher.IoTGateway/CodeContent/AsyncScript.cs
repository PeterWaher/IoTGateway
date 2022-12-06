using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security;

namespace Waher.IoTGateway.CodeContent
{
	/// <summary>
	/// Class managing asynchronous script execution in Markdown documents.
	/// </summary>
	public class AsyncScript : ICodeContent
	{
		private MarkdownDocument document;

		/// <summary>
		/// Class managing 2D XML Layout integration into Markdown documents.
		/// </summary>
		public AsyncScript()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			int i = Language.IndexOf(':');
			if (i > 0)
				Language = Language.Substring(0, i).TrimEnd();

			if (Language.ToLower() == "async")
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
			this.document = Document;

			if (!Document.TryGetMetaData("JAVASCRIPT", out KeyValuePair<string, bool>[] Values) ||
				!this.Contains(Values, "/Events.js"))
			{
				Document.AddMetaData("JAVASCRIPT", "/Events.js");
			}
		}

		private bool Contains(KeyValuePair<string, bool>[] Values, string Value)
		{
			foreach (KeyValuePair<string, bool> P in Values)
			{
				if (string.Compare(P.Key, Value, true) == 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// If HTML is handled.
		/// </summary>
		public bool HandlesHTML => true;

		/// <summary>
		/// If Plain Text is handled.
		/// </summary>
		public bool HandlesPlainText => true;

		/// <summary>
		/// If XAML is handled.
		/// </summary>
		public bool HandlesXAML => true;

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public Task<bool> GenerateHTML(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string LoadingText;
			int i = Language.IndexOf(':');
			if (i > 0)
				LoadingText = Language.Substring(i + 1).Trim();
			else
				LoadingText = "&#8987;";

			string Id = Hashes.BinaryToString(Gateway.NextBytes(32));

			Output.Append("<div id=\"id");
			Output.Append(Id);
			Output.Append("\">");
			Output.Append(LoadingText);
			Output.AppendLine("</div>");
			Output.Append("<script type=\"text/javascript\">LoadContent(\"");
			Output.Append(Id);
			Output.AppendLine("\");</script>");

			Expression Script = this.BuildExpression(Rows);
			Variables Variables = new Variables();
			Document.Settings.Variables.CopyTo(Variables);

			Document.QueueAsyncTask(() => this.Evaluate(Script, Variables, Id));

			return Task.FromResult<bool>(true);
		}

		private Expression BuildExpression(string[] Rows)
		{
			return new Expression(MarkdownDocument.AppendRows(Rows), this.document?.FileName);
		}

		private async Task<object> Evaluate(Expression Script, Variables Variables)
		{
			try
			{
				return await Script.EvaluateAsync(Variables);
			}
			catch (Exception ex)
			{
				return Log.UnnestException(ex);
			}
		}

		private async Task Evaluate(Expression Script, Variables Variables, string Id)
		{
			async void Preview(object sender, PreviewEventArgs e)
			{
				try
				{
					StringBuilder Html2 = new StringBuilder();
					await InlineScript.GenerateHTML(e.Preview.AssociatedObjectValue, Html2, true, Variables);

					await ClientEvents.ReportAsynchronousResult(Id, "text/html; charset=utf-8", Encoding.UTF8.GetBytes(Html2.ToString()), true);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			};

			StringBuilder Html = new StringBuilder();

			Variables.OnPreview += Preview;
			try
			{
				object Result = await this.Evaluate(Script, Variables);

				await InlineScript.GenerateHTML(Result, Html, true, Variables);
			}
			catch (Exception ex)
			{
				Html.Clear();
				await InlineScript.GenerateHTML(ex, Html, true, Variables);
			}
			finally
			{
				Variables.OnPreview -= Preview;
			}

			await ClientEvents.ReportAsynchronousResult(Id, "text/html; charset=utf-8", Encoding.UTF8.GetBytes(Html.ToString()), false);
		}

		/// <summary>
		/// Generates Plain Text for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GeneratePlainText(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables;
			object Result = await this.Evaluate(Script, Variables);

			await InlineScript .GeneratePlainText(Result, Output, true);

			return true;
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables;
			object Result = await this.Evaluate(Script, Variables);

			await InlineScript.GenerateXAML(Result, Output, TextAlignment, true, Variables, Document.Settings.XamlSettings);

			return true;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables;
			object Result = await this.Evaluate(Script, Variables);

			await InlineScript.GenerateXamarinForms(Result, Output, State, true, Variables, Document.Settings.XamlSettings);

			return true;
		}
	}
}
