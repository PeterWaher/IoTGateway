using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Events;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a header in a markdown document.
	/// </summary>
	public class Header : BlockElementChildren
	{
		private readonly TaskCompletionSource<bool> idCompletionSource;
		private bool idCalculated;
		private string id;
		private readonly string row;
		private readonly int level;
		private readonly bool prefix;

		/// <summary>
		/// Represents a header in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Level">Header level.</param>
		/// <param name="Prefix">If header was defined with a prefix (true) or with an underline (false).</param>
		/// <param name="Row">Header definition.</param>
		/// <param name="Children">Child elements.</param>
		public Header(MarkdownDocument Document, int Level, bool Prefix, string Row, ChunkedList<MarkdownElement> Children)
			: base(Document, Children)
		{
			this.level = Level;
			this.prefix = Prefix;
			this.row = Row;

			string s = null;

			foreach (MarkdownElement E in Children)
			{
				if (E is InlineText Text)
				{
					if (s is null)
						s = Text.Value;
					else
					{
						s = null;
						break;
					}
				}
			}

			if (s is null)
			{
				this.idCompletionSource = new TaskCompletionSource<bool>();
				this.idCalculated = false;
				this.CalcId();
			}
			else
			{
				this.id = this.GetId(s, new StringBuilder());
				this.idCalculated = true;
			}
		}

		private async void CalcId()
		{
			try
			{
				StringBuilder sb = new StringBuilder();
				string s;

				using (TextRenderer Renderer = new TextRenderer(sb))
				{
					await this.Render(Renderer);
					s = Renderer.ToString();
				}

				sb.Clear();

				this.id = this.GetId(s, sb);
				this.idCalculated = true;
				this.idCompletionSource.TrySetResult(true);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private string GetId(string s, StringBuilder sb)
		{
			bool FirstCharInWord = false;

			foreach (char ch in s)
			{
				if (!char.IsLetter(ch) && !char.IsDigit(ch))
				{
					FirstCharInWord = true;
					continue;
				}

				if (FirstCharInWord)
				{
					sb.Append(char.ToUpper(ch));
					FirstCharInWord = false;
				}
				else
					sb.Append(char.ToLower(ch));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Header level.
		/// </summary>
		public int Level => this.level;

		/// <summary>
		/// If header is defined using a prefix.
		/// </summary>
		public bool Prefix => this.prefix;

		/// <summary>
		/// Row definition
		/// </summary>
		public string Row => this.row;

		/// <summary>
		/// ID of header.
		/// </summary>
		public Task<string> Id => this.GetId();

		private async Task<string> GetId()
		{
			if (!this.idCalculated)
				await this.idCompletionSource.Task;

			return this.id;
		}

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => false;

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(ChunkedList<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new Header(Document, this.level, this.prefix, this.row, Children);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Header x &&
				x.level == this.level &&
				x.prefix == this.prefix &&
				x.row == this.row &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Header x &&
				this.level == x.level &&
				this.prefix == x.prefix &&
				this.row == x.row &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.level.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.prefix.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.row?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrHeaders++;
		}

	}
}
