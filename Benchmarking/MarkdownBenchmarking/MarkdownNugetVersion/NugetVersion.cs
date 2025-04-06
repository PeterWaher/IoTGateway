using MarkdownBenchmarkingInterface;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Contracts;
using Waher.Content.Markdown.JavaScript;
using Waher.Content.Markdown.Latex;
using Waher.Content.Markdown.Wpf;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Content.Functions.InputOutput;
using Waher.Script.Graphs;
using Waher.Script.Graphs3D;

namespace MarkdownNugetVersion
{
	public class NugetVersion : IBenchmarkingTests
	{
		public async Task Initialize()
		{
			Types.Initialize(
				typeof(MarkdownDocument).Assembly,
				typeof(ContractsRenderer).Assembly,
				typeof(JavaScriptRenderer).Assembly,
				typeof(LatexRenderer).Assembly,
				typeof(WpfXamlRenderer).Assembly,
				typeof(Expression).Assembly,
				typeof(Graph).Assembly,
				typeof(Graph3D).Assembly,
				typeof(SaveFile).Assembly);

			await Types.StartAllModules(60000);
		}

		public async Task Cleanup()
		{
			await Types.StopAllModules();
		}

		public void Dispose()
		{
			this.Cleanup().Wait();
		}

		public async Task Parse(string Markdown, string RootFolder)
		{
			await ParseInternal(Markdown, RootFolder);
		}

		private static async Task<MarkdownDocument> ParseInternal(string Markdown, string RootFolder)
		{
			MarkdownSettings Settings = new(null, true, [])
			{
				RootFolder = RootFolder
			};
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings,
				RootFolder + "\\File.md", null, null);
			return Doc;
		}

		public async Task<string> RenderHTML(string Markdown, string RootFolder)
		{
			MarkdownDocument Doc = await ParseInternal(Markdown, RootFolder);
			return await Doc.GenerateHTML();
		}

		public async Task<string> RenderText(string Markdown, string RootFolder)
		{
			MarkdownDocument Doc = await ParseInternal(Markdown, RootFolder);
			return await Doc.GeneratePlainText();
		}

		public async Task<string> RenderMarkdown(string Markdown, string RootFolder)
		{
			MarkdownDocument Doc = await ParseInternal(Markdown, RootFolder);
			return await Doc.GenerateMarkdown();
		}

		public async Task<string> RenderJavscript(string Markdown, string RootFolder)
		{
			MarkdownDocument Doc = await ParseInternal(Markdown, RootFolder);
			return await Doc.GenerateJavaScript();
		}

		public async Task<string> RenderWPF(string Markdown, string RootFolder)
		{
			MarkdownDocument Doc = await ParseInternal(Markdown, RootFolder);
			return await Doc.GenerateXAML();
		}

		public async Task<string> RenderContract(string Markdown, string RootFolder)
		{
			MarkdownDocument Doc = await ParseInternal(Markdown, RootFolder);
			return await Doc.GenerateSmartContractXml();
		}

		public async Task<string> RenderLatex(string Markdown, string RootFolder)
		{
			MarkdownDocument Doc = await ParseInternal(Markdown, RootFolder);
			return await Doc.GenerateLaTeX();
		}
	}
}
