using System.IO;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// PreprocessMarkdown(Markdown)
	/// </summary>
	public class PreprocessMarkdown : FunctionOneScalarVariable
    {
		/// <summary>
		/// PreprocessMarkdown(Markdown)
		/// </summary>
		/// <param name="Markdown">Markdown.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PreprocessMarkdown(ScriptNode Markdown, int Start, int Length, Expression Expression)
            : base(Markdown, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "preprocessmarkdown"; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
			MarkdownSettings Settings = new MarkdownSettings()
			{
				Variables = Variables,
				ParseMetaData = false
			};

			string Markdown = MarkdownDocument.Preprocess(Argument, Settings);

			return new StringValue(Markdown);
        }

    }
}
