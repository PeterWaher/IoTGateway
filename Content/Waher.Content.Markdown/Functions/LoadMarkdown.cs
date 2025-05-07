using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.IO;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// LoadMarkdown(FileName[,Headers])
	/// </summary>
	public class LoadMarkdown : FunctionMultiVariate
    {
		/// <summary>
		/// LoadMarkdown(FileName[,Headers])
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadMarkdown(ScriptNode FileName, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { FileName }, argumentTypes1Scalar, Start, Length, Expression)
        {
        }

		/// <summary>
		/// LoadMarkdown(FileName[,Headers])
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Headers">If Markdown headers should be included.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadMarkdown(ScriptNode FileName, ScriptNode Headers, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { FileName, Headers }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(LoadMarkdown);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "FileName", "Headers" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0].AssociatedObjectValue is string FileName))
				throw new ScriptRuntimeException("Expected first argument to be a string file name.", this);

			bool IncludeHeaders;

			if (Arguments.Length > 1)
			{
				if (!(Arguments[1].AssociatedObjectValue is bool b))
					throw new ScriptRuntimeException("Expected second argument to be a Boolean value.", this);

				IncludeHeaders = b;
			}
			else
				IncludeHeaders = false;

			string Markdown = await Files.ReadAllTextAsync(FileName);
			MarkdownSettings Settings = new MarkdownSettings()
			{
				Variables = Variables,
				ParseMetaData = true
			};

			if (Variables.TryGetVariable(MarkdownDocument.MarkdownSettingsVariableName, out Variable v) &&
				v.ValueObject is MarkdownSettings ParentSettings)
			{
				Settings.AllowScriptTag = ParentSettings.AllowScriptTag;
				Settings.AudioAutoplay = ParentSettings.AudioAutoplay;
				Settings.AudioControls = ParentSettings.AudioControls;
				Settings.EmbedEmojis = ParentSettings.EmbedEmojis;
				Settings.EmojiSource = ParentSettings.EmojiSource;
				Settings.HttpxProxy = ParentSettings.HttpxProxy;
				Settings.LocalHttpxResourcePath = ParentSettings.LocalHttpxResourcePath;
				Settings.RootFolder = ParentSettings.RootFolder;
				Settings.VideoAutoplay = ParentSettings.VideoAutoplay;
				Settings.VideoControls = ParentSettings.VideoControls;
			}

			KeyValuePair<string, bool> P = await MarkdownDocument.Preprocess(Markdown, Settings, FileName);
			Markdown = P.Key;

			if (!IncludeHeaders)
			{
				int? Pos = MarkdownDocument.HeaderEndPosition(Markdown);

				if (Pos.HasValue)
					Markdown = Markdown.Substring(Pos.Value).TrimStart();
			}

			return new StringValue(Markdown);
        }
    }
}
