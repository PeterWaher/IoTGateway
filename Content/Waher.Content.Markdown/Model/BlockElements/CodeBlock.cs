using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Rendering;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.BlockElements
{
	/// <summary>
	/// Represents a code block in a markdown document.
	/// </summary>
	public class CodeBlock : BlockElement
	{
		private ICodeContent handler;
		private Type handlerType = null;
		private readonly string[] rows;
		private readonly string indentString;
		private readonly string language;
		private readonly int start, end, indent;

		/// <summary>
		/// Represents a code block in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Rows">Rows</param>
		/// <param name="Start">Start index of code.</param>
		/// <param name="End">End index of code.</param>
		/// <param name="Indent">Additional indenting.</param>
		public CodeBlock(MarkdownDocument Document, string[] Rows, int Start, int End, int Indent)
			: this(Document, Rows, Start, End, Indent, null)
		{
		}

		/// <summary>
		/// Represents a code block in a markdown document.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Rows">Rows</param>
		/// <param name="Start">Start index of code.</param>
		/// <param name="End">End index of code.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Language">Language used.</param>
		public CodeBlock(MarkdownDocument Document, string[] Rows, int Start, int End, int Indent, string Language)
			: base(Document)
		{
			this.rows = Rows;
			this.start = Start;
			this.end = End;
			this.indent = Indent;
			this.indentString = this.indent <= 0 ? string.Empty : new string('\t', this.indent);
			this.language = Language;
		}

		/// <summary>
		/// Language
		/// </summary>
		public string Language => this.language;

		/// <summary>
		/// Rows in code block
		/// </summary>
		public string[] Rows => this.rows;

		/// <summary>
		/// Multimedia handler.
		/// </summary>
		public T CodeContentHandler<T>()
			where T : ICodeContentRenderer
		{
			if (this.handlerType is null || this.handlerType != typeof(T))
			{
				this.handler = GetCodeBlockHandler<T>(this.language);
				this.handlerType = typeof(T);
			}

			return (T)this.handler;
		}

		private static IXmlVisualizer[] xmlVisualizers = null;
		private readonly static Dictionary<Type, Dictionary<string, ICodeContentRenderer[]>> codeContentHandlers = new Dictionary<Type, Dictionary<string, ICodeContentRenderer[]>>();
		private readonly static Dictionary<string, IXmlVisualizer[]> xmlVisualizerHandlers = new Dictionary<string, IXmlVisualizer[]>(StringComparer.CurrentCultureIgnoreCase);

		static CodeBlock()
		{
			Init();
			Types.OnInvalidated += (sender, e) => Init();
		}

		private static void Init()
		{
			List<ICodeContentRenderer> CodeContents = new List<ICodeContentRenderer>();
			List<IXmlVisualizer> XmlVisualizers = new List<IXmlVisualizer>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(ICodeContentRenderer)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					ICodeContentRenderer CodeContent = (ICodeContentRenderer)CI.Invoke(Types.NoParameters);
					CodeContents.Add(CodeContent);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IXmlVisualizer)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					IXmlVisualizer XmlVisualizer = (IXmlVisualizer)CI.Invoke(Types.NoParameters);
					XmlVisualizers.Add(XmlVisualizer);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			lock (codeContentHandlers)
			{
				codeContentHandlers.Clear();
			}

			lock (xmlVisualizerHandlers)
			{
				xmlVisualizers = XmlVisualizers.ToArray();
				xmlVisualizerHandlers.Clear();
			}
		}

		internal static T GetCodeBlockHandler<T>(string Language)
			where T : ICodeContentRenderer
		{
			ICodeContentRenderer[] Handlers;

			if (string.IsNullOrEmpty(Language))
				return default;

			lock (codeContentHandlers)
			{
				if (!codeContentHandlers.TryGetValue(typeof(T), out Dictionary<string, ICodeContentRenderer[]> PerLanguage))
				{
					PerLanguage = new Dictionary<string, ICodeContentRenderer[]>();
					codeContentHandlers[typeof(T)] = PerLanguage;
				}

				if (!PerLanguage.TryGetValue(Language, out Handlers))
				{
					List<ICodeContentRenderer> List = new List<ICodeContentRenderer>();

					foreach (Type T2 in Types.GetTypesImplementingInterface(typeof(T)))
					{
						ConstructorInfo CI = Types.GetDefaultConstructor(T2);
						if (CI is null)
							continue;

						try
						{
							T CodeContent = (T)CI.Invoke(Types.NoParameters);
							if (CodeContent.Supports(Language) > Grade.NotAtAll)
								List.Add(CodeContent);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}

					if (List.Count > 0)
						Handlers = List.ToArray();
					else
						Handlers = null;

					PerLanguage[Language] = Handlers;
				}
			}

			if (Handlers is null)
				return default;

			T Best = default;
			Grade BestGrade = Grade.NotAtAll;
			Grade ContentGrade;

			foreach (ICodeContentRenderer Content in Handlers)
			{
				ContentGrade = Content.Supports(Language);
				if (ContentGrade > BestGrade && Content is T TypedContent)
				{
					BestGrade = ContentGrade;
					Best = TypedContent;
				}
			}

			return Best;
		}

		internal static IXmlVisualizer GetXmlVisualizerHandler(XmlDocument Xml)
		{
			if (Xml is null || Xml.DocumentElement is null)
				return null;

			IXmlVisualizer[] Handlers;
			string Key = Xml.DocumentElement.NamespaceURI + "#" + Xml.DocumentElement.LocalName;

			lock (xmlVisualizerHandlers)
			{
				if (!xmlVisualizerHandlers.TryGetValue(Key, out Handlers))
				{
					List<IXmlVisualizer> List = new List<IXmlVisualizer>();

					foreach (IXmlVisualizer Visualizer in xmlVisualizers)
					{
						if (Visualizer.Supports(Xml) > Grade.NotAtAll)
							List.Add(Visualizer);
					}

					if (List.Count > 0)
						Handlers = List.ToArray();
					else
						Handlers = null;

					xmlVisualizerHandlers[Key] = Handlers;
				}
			}

			if (Handlers is null)
				return null;

			IXmlVisualizer Best = null;
			Grade BestGrade = Grade.NotAtAll;
			Grade VisualizerGrade;

			foreach (IXmlVisualizer Visualizer in Handlers)
			{
				VisualizerGrade = Visualizer.Supports(Xml);
				if (VisualizerGrade > BestGrade)
				{
					BestGrade = VisualizerGrade;
					Best = Visualizer;
				}
			}

			return Best;
		}

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// Code block indentation.
		/// </summary>
		public int Indent => this.indent;

		/// <summary>
		/// String used for indentation.
		/// </summary>
		public string IndentString => this.indentString;

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => false;

		/// <summary>
		/// Start row index
		/// </summary>
		public int Start => this.start;

		/// <summary>
		/// End row index.
		/// </summary>
		public int End => this.end;

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is CodeBlock x &&
				this.indent == x.indent &&
				this.indentString == x.indentString &&
				this.language == x.language &&
				AreEqual(this.rows, x.rows) &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is CodeBlock x &&
				this.indent == x.indent &&
				this.indentString == x.indentString &&
				this.language == x.language &&
				AreEqual(this.rows, x.rows) &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.indent.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.indentString?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.language?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = GetHashCode(this.rows);

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			Statistics.NrCodeBlocks++;
		}

	}
}
