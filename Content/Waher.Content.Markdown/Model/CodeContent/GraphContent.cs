using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Content.Markdown.Model.CodeContent
{
	/// <summary>
	/// Script graph content.
	/// </summary>
	public class GraphContent : IImageCodeContent
	{
		/// <summary>
		/// Script graph content.
		/// </summary>
		public GraphContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			return string.Compare(Language, "graph", true) == 0 ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
		}

		/// <summary>
		/// If (transportable) Markdown is handled.
		/// </summary>
		public bool HandlesMarkdown => false;

		/// <summary>
		/// If HTML is handled.
		/// </summary>
		public bool HandlesHTML => true;

		/// <summary>
		/// If Plain Text is handled.
		/// </summary>
		public bool HandlesPlainText => false;

		/// <summary>
		/// If XAML is handled.
		/// </summary>
		public bool HandlesXAML => true;

		/// <summary>
		/// If LaTeX is handled.
		/// </summary>
		public bool HandlesLaTeX => true;

		/// <summary>
		/// Generates (transportanle) Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public Task<bool> GenerateMarkdown(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateHTML(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			try
			{
				Graph G = await GetGraph(Rows);
				await SpanElements.InlineScript.GenerateHTML(G, Output, true, Document.Settings.Variables ?? new Variables());
				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Gets a graph object from its XML Code Block representation.
		/// </summary>
		/// <param name="Rows">Rows</param>
		/// <returns>Graph object</returns>
		public static async Task<Graph> GetGraph(string[] Rows)
		{
			XmlDocument Xml = new XmlDocument();
			Xml.LoadXml(MarkdownDocument.AppendRows(Rows));

			if (Xml.DocumentElement is null ||
				Xml.DocumentElement.LocalName != Graph.GraphLocalName ||
				Xml.DocumentElement.NamespaceURI != Graph.GraphNamespace)
			{
				throw new Exception("Invalid Graph XML");
			}

			string TypeName = XML.Attribute(Xml.DocumentElement, "type");
			Type T = Types.GetType(TypeName)
				?? throw new Exception("Type not recognized: " + TypeName);

			Graph G = (Graph)Activator.CreateInstance(T);
			G.SameScale = XML.Attribute(Xml.DocumentElement, "sameScale", false);

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				if (N is XmlElement E)
				{
					await G.ImportGraphAsync(E);
					break;
				}
			}

			return G;
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
			try
			{
				Graph G = await GetGraph(Rows);
				await SpanElements.InlineScript.GeneratePlainText(G, Output, true);
				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
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
		public async Task<bool> GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, string[] Rows, string Language, int Indent,
			MarkdownDocument Document)
		{
			try
			{
				Graph G = await GetGraph(Rows);
				await SpanElements.InlineScript.GenerateXAML(G, Output, TextAlignment, true, Document.Settings.Variables ?? new Variables(), 
					Document.Settings.XamlSettings);
				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
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
		public async Task<bool> GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, string[] Rows, string Language, int Indent,
			MarkdownDocument Document)
		{
			try
			{
				Graph G = await GetGraph(Rows);

				await SpanElements.InlineScript.GenerateXamarinForms(G, Output, State, true, 
					Document.Settings.Variables ?? new Variables(), Document.Settings.XamlSettings);
				
				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Generates LaTeX text for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateLaTeX(StringBuilder Output, string[] Rows, string Language, int Indent,
			MarkdownDocument Document)
		{
			try
			{
				Graph G = await GetGraph(Rows);
				await SpanElements.InlineScript.GenerateLaTeX(G, Output, true, Document.Settings.Variables ?? new Variables());
				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Generates an image of the contents.
		/// </summary>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>Image, if successful, null otherwise.</returns>
		public async Task<PixelInformation> GenerateImage(string[] Rows, string Language, MarkdownDocument Document)
		{
			Graph G = await GetGraph(Rows);
			return G.CreatePixels(Document.Settings.Variables ?? new Variables());
		}

	}
}
