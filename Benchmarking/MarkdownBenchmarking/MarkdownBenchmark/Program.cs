using MarkdownBenchmarkingInterface;
using MarkdownDevelopmentVersion;
using MarkdownNugetVersion;
using System.Text;
using Waher.Runtime.Profiling;
using Waher.Script;

internal class Program
{
	private static async Task Main(string[] _)
	{
		try
		{
			using DevelopmentVersion Dev = new();
			using NugetVersion Nuget = new();

			Console.Out.WriteLine("Initializing Development Version...");
			await Dev.Initialize();

			Console.Out.WriteLine("Initializing Nuget Version...");
			await Nuget.Initialize();

			Console.Out.WriteLine("Loading Markdown Reference text...");
			string MarkdownFileName = Path.GetFullPath("..\\..\\..\\..\\..\\..\\Waher.IoTGateway.Resources\\Root\\Markdown.md");
			string MarkdownReference = File.ReadAllText(MarkdownFileName);

			Console.Out.WriteLine("Loading Script Reference text...");
			string ScriptFileName = Path.GetFullPath("..\\..\\..\\..\\..\\..\\Waher.IoTGateway.Resources\\Root\\Script.md");
			string ScriptReference = File.ReadAllText(ScriptFileName);

			string RootFolder = Path.GetDirectoryName(MarkdownFileName)!;

			Console.Out.WriteLine("JIT pass");

			await RunTests(Dev, MarkdownReference, RootFolder);
			await RunTests(Nuget, MarkdownReference, RootFolder);

			await RunTests(Dev, ScriptReference, RootFolder);
			await RunTests(Nuget, ScriptReference, RootFolder);

			Console.Out.WriteLine("Benchmarking pass");

			StringBuilder sb = new();

			sb.Append("DevMarkdown:=");
			sb.Append(await RunTests(Dev, MarkdownReference, RootFolder));
			sb.AppendLine(";");
			sb.AppendLine();

			sb.Append("NugetMarkdown:=");
			sb.Append(await RunTests(Nuget, MarkdownReference, RootFolder));
			sb.AppendLine(";");
			sb.AppendLine();

			sb.Append("DevScript:=");
			sb.Append(await RunTests(Dev, ScriptReference, RootFolder));
			sb.AppendLine(";");
			sb.AppendLine();

			sb.Append("NugetScript:=");
			sb.Append(await RunTests(Nuget, ScriptReference, RootFolder));
			sb.AppendLine(";");
			sb.AppendLine();

			sb.AppendLine("DevMarkdownMs:=DevMarkdown[1,];");
			sb.AppendLine("NugetMarkdownMs:=NugetMarkdown[1,];");
			sb.AppendLine("DevScriptMs:=DevScript[1,];");
			sb.AppendLine("NugetScriptMs:=NugetScript[1,];");
			sb.AppendLine("Labels:=DevMarkdown[0,];");
			sb.AppendLine("");
			sb.AppendLine("MarkdownChange:=100*DevMarkdownMs/NugetMarkdownMs;");
			sb.AppendLine("ScriptChange:=100*DevScriptMs/NugetScriptMs;");
			sb.AppendLine("");
			sb.AppendLine("MarkdownGraph:=HorizontalBars(Labels,MarkdownChange,'Red');");
			sb.AppendLine("MarkdownGraph.Title:='Development relative to published nuget(Markdown.md)';");
			sb.AppendLine("MarkdownGraph.LabelX:='%';");
			sb.AppendLine("MarkdownGraph.LabelY:='Operation';");
			sb.AppendLine("");
			sb.AppendLine("ScriptGraph:=HorizontalBars(Labels,ScriptChange,'Blue');");
			sb.AppendLine("ScriptGraph.Title:='Development relative to published nuget(Script.md)';");
			sb.AppendLine("ScriptGraph.LabelX:='%';");
			sb.AppendLine("ScriptGraph.LabelY:='Operation'; ");
			sb.AppendLine();
			sb.AppendLine("GraphWidth:=1280;");
			sb.AppendLine("GraphHeight:=720;");
			sb.AppendLine("SaveFile(MarkdownGraph,'MarkdownGraph.png');");
			sb.AppendLine("SaveFile(ScriptGraph,'ScriptGraph.png');");

			Console.Out.WriteLine("Writing results to file.");

			string Script = sb.ToString();
			File.WriteAllText("Result.script", Script);

			Variables Variables = [];
			await Expression.EvalAsync(Script, Variables);


		}
		catch (Exception ex)
		{
			Console.Out.WriteLine("EXCEPTION: " + ex.Message);
			Console.Out.WriteLine();
			Console.Out.WriteLine(ex.StackTrace);
		}
	}

	private static async Task<string> RunTests(IBenchmarkingTests Tests, string Markdown,
		string RootFolder)
	{
		Benchmarker1D Benchmarker = new();

		using (Benchmarking Test = Benchmarker.Start("Parse"))
		{
			await Tests.Parse(Markdown, RootFolder);
		}

		using (Benchmarking Test = Benchmarker.Start("HTML"))
		{
			await Tests.RenderHTML(Markdown, RootFolder);
		}

		using (Benchmarking Test = Benchmarker.Start("Text"))
		{
			await Tests.RenderText(Markdown, RootFolder);
		}

		using (Benchmarking Test = Benchmarker.Start("Markdown"))
		{
			await Tests.RenderMarkdown(Markdown, RootFolder);
		}

		using (Benchmarking Test = Benchmarker.Start("JavaScript"))
		{
			await Tests.RenderJavscript(Markdown, RootFolder);
		}

		using (Benchmarking Test = Benchmarker.Start("WPF"))
		{
			await Tests.RenderWPF(Markdown, RootFolder);
		}

		using (Benchmarking Test = Benchmarker.Start("Contract"))
		{
			await Tests.RenderContract(Markdown, RootFolder);
		}

		using (Benchmarking Test = Benchmarker.Start("Latex"))
		{
			await Tests.RenderLatex(Markdown, RootFolder);
		}

		return Benchmarker.GetResultScriptMilliseconds();
	}
}