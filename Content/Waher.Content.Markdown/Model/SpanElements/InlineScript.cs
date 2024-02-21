using System;
using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;
using Waher.Events;
using Waher.Script;
using Waher.Script.Model;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineScript : MarkdownElement, IContextVariables
	{
		private readonly Expression expression;
		private readonly Variables variables;
		private readonly int startPosition;
		private readonly int endPosition;
		private readonly bool aloneInParagraph;
		private bool authorized = false;

		/// <summary>
		/// Inline source code.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Variables">Collection of variables to use when executing the script.</param>
		/// <param name="AloneInParagraph">If construct stands alone in a paragraph.</param>
		/// <param name="StartPosition">Starting position of script.</param>
		/// <param name="EndPosition">Ending position of script.</param>
		public InlineScript(MarkdownDocument Document, Expression Expression, Variables Variables, bool AloneInParagraph,
			int StartPosition, int EndPosition)
			: base(Document)
		{
			this.expression = Expression;
			this.variables = Variables;
			this.aloneInParagraph = AloneInParagraph;
			this.startPosition = StartPosition;
			this.endPosition = EndPosition;
		}

		/// <summary>
		/// Expression
		/// </summary>
		public Expression Expression => this.expression;

		/// <summary>
		/// Variables.
		/// </summary>
		public Variables Variables => this.variables;

		/// <summary>
		/// If the element is alone in a paragraph.
		/// </summary>
		public bool AloneInParagraph => this.aloneInParagraph;

		/// <summary>
		/// Starting position of script in markdown document.
		/// </summary>
		public int StartPosition => this.startPosition;

		/// <summary>
		/// Ending position of script in markdown document.
		/// </summary>
		public int EndPosition => this.endPosition;

		/// <summary>
		/// Evaluates the script expression.
		/// </summary>
		/// <returns>Result</returns>
		public async Task<object> EvaluateExpression()
		{
			IContextVariables Bak = this.variables.ContextVariables;
			try
			{
				this.variables.ContextVariables = this;

				if (!this.authorized && !(this.Document.Settings.AuthorizeExpression is null))
				{
					ScriptNode Prohibited = await this.Document.Settings.AuthorizeExpression(this.expression);
					if (!(Prohibited is null))
						throw new UnauthorizedAccessException("Expression not permitted: " + Prohibited.SubExpression);
				}

				this.authorized = true;

				return await this.expression.EvaluateAsync(this.variables);
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				this.Document.CheckException(ex);

				return ex;
			}
			finally
			{
				this.variables.ContextVariables = Bak;
			}
		}

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => true;

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		public override bool OutsideParagraph => true;

		/// <summary>
		/// Baseline alignment
		/// </summary>
		public override BaselineAlignment BaselineAlignment => BaselineAlignment.Baseline;

		/// <summary>
		/// Tries to get a variable object, given its name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Variable">Variable, if found, or null otherwise.</param>
		/// <returns>If a variable with the corresponding name was found.</returns>
		public bool TryGetVariable(string Name, out Variable Variable)
		{
			switch (Name)
			{
				case "StartPosition":
					Variable = new Variable(Name, this.startPosition);
					return true;

				case "EndPosition":
					Variable = new Variable(Name, this.endPosition);
					return true;

				default:
					Variable = null;
					return false;
			}
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public bool ContainsVariable(string Name)
		{
			return Name == "StartPosition" || Name == "EndPosition";
		}

		/// <summary>
		/// Adds a variable to the collection.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Value">Associated variable object value.</param>
		public void Add(string Name, object Value)
		{
			throw new NotSupportedException("Variable collection is read-only.");
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is InlineScript x &&
				this.expression.Script == x.expression.Script &&
				this.aloneInParagraph == x.aloneInParagraph &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.expression?.Script?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.aloneInParagraph.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrInlineScript++;
		}

	}
}
