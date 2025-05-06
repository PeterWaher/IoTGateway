using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Contracts;
using Waher.Content.Markdown.Latex;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Markdown.Wpf;
using Waher.Content.Markdown.Xamarin;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.IoTGateway.CodeContent
{
	/// <summary>
	/// Class managing asynchronous script execution in Markdown documents.
	/// </summary>
	public class AsyncScript : ICodeContent, ICodeContentHtmlRenderer, ICodeContentTextRenderer, ICodeContentMarkdownRenderer,
		ICodeContentContractsRenderer, ICodeContentLatexRenderer, ICodeContentWpfXamlRenderer, ICodeContentXamarinFormsXamlRenderer
	{
		private static readonly AsyncMarkdownHtmlContent asyncHtmlOutput = new AsyncMarkdownHtmlContent();

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

			if (string.Compare(Language, "async", true) == 0)
				return Grade.Excellent;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// If script is evaluated for this type of code block.
		/// </summary>
		public bool EvaluatesScript => true;

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
			this.document = Document;

			if (!Document.TryGetMetaData("JAVASCRIPT", out KeyValuePair<string, bool>[] Values) ||
				!Contains(Values, "/Events.js"))
			{
				Document.AddMetaData("JAVASCRIPT", "/Events.js");
			}
		}

		private static bool Contains(KeyValuePair<string, bool>[] Values, string Value)
		{
			foreach (KeyValuePair<string, bool> P in Values)
			{
				if (string.Compare(P.Key, Value, true) == 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Generates HTML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderHtml(HtmlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string Title;
			int i = Language.IndexOf(':');
			if (i > 0)
				Title = Language.Substring(i + 1).Trim();
			else
				Title = null;

			AsyncState State = new AsyncState()
			{
				Id = await asyncHtmlOutput.GenerateStub(MarkdownOutputType.Html, Renderer.Output, Title, Document),
				Script = this.BuildExpression(Rows),
				Variables = HttpServer.CreateSessionVariables(),
				ImplicitPrint = new StringBuilder()
			};

			State.Variables.ConsoleOut = new StringWriter(State.ImplicitPrint);
			Document.Settings.Variables?.CopyTo(State.Variables);

			Document.QueueAsyncTask(this.ExecuteScript, State);

			return true;
		}

		private class AsyncState
		{
			public string Id;
			public Expression Script;
			public Variables Variables;
			public StringBuilder ImplicitPrint;
		}

		private Task ExecuteScript(object State)
		{
			AsyncState AsyncState = (AsyncState)State;
			return this.Evaluate(AsyncState.Script, AsyncState.Variables, AsyncState.ImplicitPrint, AsyncState.Id);
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

		private async Task Evaluate(Expression Script, Variables Variables, StringBuilder ImplicitPrint, string Id)
		{
			async Task Preview(object Sender, PreviewEventArgs e)
			{
				try
				{
					using (HtmlRenderer Renderer2 = new HtmlRenderer(new HtmlSettings()
					{
						XmlEntitiesOnly = true
					}))
					{
						await Renderer2.RenderObject(e.Preview.AssociatedObjectValue, true, Variables);
						await asyncHtmlOutput.ReportResult(MarkdownOutputType.Html, Id, Renderer2.ToString(), true);
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			};

			using (HtmlRenderer Renderer = new HtmlRenderer(new HtmlSettings()
			{
				XmlEntitiesOnly = true
			}))
			{
				Variables.OnPreview += Preview;
				try
				{
					object Result = await this.Evaluate(Script, Variables);

					string Printed = ImplicitPrint.ToString();
					if (!string.IsNullOrEmpty(Printed))
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine("BodyOnly: 1");

						if (this.document?.Settings.AllowScriptTag ?? false)
							sb.AppendLine("AllowScriptTag: 1");

						sb.AppendLine();
						sb.Append(Printed);

						MarkdownDocument Doc;

						if (!(this.document is null))
							Doc = await MarkdownDocument.CreateAsync(sb.ToString(), this.document.Settings);
						else if (Variables.TryGetVariable(MarkdownDocument.MarkdownSettingsVariableName, out Variable v) &&
							v.ValueObject is MarkdownSettings Settings)
						{
							Doc = await MarkdownDocument.CreateAsync(sb.ToString(), Settings);
						}
						else
							Doc = await MarkdownDocument.CreateAsync(sb.ToString());

						await Doc.RenderDocument(Renderer);
					}

					await Renderer.RenderObject(Result, true, Variables);
				}
				catch (Exception ex)
				{
					Renderer.Clear();
					await Renderer.RenderObject(ex, true, Variables);
				}
				finally
				{
					Variables.OnPreview -= Preview;
				}

				await asyncHtmlOutput.ReportResult(MarkdownOutputType.Html, Id, Renderer.ToString(), false);
			}
		}

		/// <summary>
		/// Generates Markdown for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderMarkdown(MarkdownRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables ?? HttpServer.CreateSessionVariables();
			object Result = await this.Evaluate(Script, Variables);

			await Renderer.RenderObject(Result, true, Variables);

			return true;
		}

		/// <summary>
		/// Generates plain text for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderText(TextRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables ?? HttpServer.CreateSessionVariables();
			object Result = await this.Evaluate(Script, Variables);

			await Renderer.RenderObject(Result, true);

			return true;
		}

		/// <summary>
		/// Generates WPF XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderWpfXaml(WpfXamlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables ?? HttpServer.CreateSessionVariables();
			object Result = await this.Evaluate(Script, Variables);

			await Renderer.RenderObject(Result, true, Variables);

			return true;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables ?? HttpServer.CreateSessionVariables();
			object Result = await this.Evaluate(Script, Variables);

			await Renderer.RenderObject(Result, true, Variables);

			return true;
		}

		/// <summary>
		/// Generates LaTeX for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderLatex(LatexRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables ?? HttpServer.CreateSessionVariables();
			object Result = await this.Evaluate(Script, Variables);

			await Renderer.RenderObject(Result, true, Variables);

			return true;
		}

		/// <summary>
		/// Generates smart contract XML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderContractXml(ContractsRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			Expression Script = this.BuildExpression(Rows);
			Variables Variables = Document.Settings.Variables ?? HttpServer.CreateSessionVariables();
			object Result = await this.Evaluate(Script, Variables);

			await Renderer.RenderObject(Result, true, Variables);

			return true;
		}

	}
}
