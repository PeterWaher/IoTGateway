namespace MarkdownBenchmarkingInterface
{
	/// <summary>
	/// Interface for benchmarking tests.
	/// </summary>
	public interface IBenchmarkingTests : IDisposable
	{
		Task Initialize();
		Task Cleanup();
		Task Parse(string Markdown, string RootFolder);
		Task<string> RenderHTML(string Markdown, string RootFolder);
		Task<string> RenderText(string Markdown, string RootFolder);
		Task<string> RenderMarkdown(string Markdown, string RootFolder);
		Task<string> RenderJavscript(string Markdown, string RootFolder);
		Task<string> RenderWPF(string Markdown, string RootFolder);
		Task<string> RenderContract(string Markdown, string RootFolder);
		Task<string> RenderLatex(string Markdown, string RootFolder);
	}
}
