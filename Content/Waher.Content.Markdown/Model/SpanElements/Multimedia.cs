using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Events;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Multimedia
	/// </summary>
	public class Multimedia : MarkdownElementChildren
	{
		private IMultimediaContent handler = null;
		private readonly MultimediaItem[] items;
		private readonly bool aloneInParagraph;

		/// <summary>
		/// Multimedia
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Items">Multimedia items.</param>
		public Multimedia(MarkdownDocument Document, IEnumerable<MarkdownElement> ChildElements, bool AloneInParagraph, params MultimediaItem[] Items)
			: base(Document, ChildElements)
		{
			this.items = Items;
			this.aloneInParagraph = AloneInParagraph;
		}

		/// <summary>
		/// Multimedia items.
		/// </summary>
		public MultimediaItem[] Items
		{
			get { return this.items; }
		}

		/// <summary>
		/// If the element is alone in a paragraph.
		/// </summary>
		public bool AloneInParagraph
		{
			get { return this.aloneInParagraph; }
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override void GenerateMarkdown(StringBuilder Output)
		{
			bool First = true;

			Output.Append("![");
			base.GenerateMarkdown(Output);
			Output.Append(']');

			foreach (MultimediaItem Item in this.items)
			{
				if (First)
					First = false;
				else if (this.aloneInParagraph)
				{
					Output.AppendLine();
					Output.Append('\t');
				}

				Output.Append('(');
				Output.Append(Item.Url);

				if (!string.IsNullOrEmpty(Item.Title))
				{
					Output.Append(" \"");
					Output.Append(Item.Title.Replace("\"", "\\\""));
					Output.Append('"');
				}

				if (Item.Width.HasValue)
				{
					Output.Append(' ');
					Output.Append(Item.Width.Value.ToString());

					if (Item.Height.HasValue)
					{
						Output.Append(' ');
						Output.Append(Item.Height.Value.ToString());
					}
				}

				Output.Append(')');
			}

		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			this.MultimediaHandler.GenerateHTML(Output, this.items, this.Children, this.aloneInParagraph, this.Document);
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override void GeneratePlainText(StringBuilder Output)
		{
			this.MultimediaHandler.GeneratePlainText(Output, this.items, this.Children, this.aloneInParagraph, this.Document);
		}

		/// <summary>
		/// Multimedia handler.
		/// </summary>
		public IMultimediaContent MultimediaHandler
		{
			get
			{
				if (this.handler is null)
					this.handler = GetMultimediaHandler(this.items);

				return this.handler;
			}
		}

		/// <summary>
		/// Gets the best multimedia handler for a set of URLs or file names.
		/// </summary>
		/// <param name="URLs">Set of URLs, or file names.</param>
		/// <returns>Best multimedia handler.</returns>
		public static IMultimediaContent GetMultimediaHandler(params string[] URLs)
		{
			int i, c = URLs.Length;
			MultimediaItem[] Items = new MultimediaItem[c];

			for (i = 0; i < c; i++)
				Items[i] = new MultimediaItem(null, URLs[i], string.Empty, null, null);

			return GetMultimediaHandler(Items);
		}

		/// <summary>
		/// Gets the best multimedia handler for a set of multimedia items.
		/// </summary>
		/// <param name="Items">Set of multimedia items.</param>
		/// <returns>Best multimedia handler.</returns>
		public static IMultimediaContent GetMultimediaHandler(params MultimediaItem[] Items)
		{
			IMultimediaContent Best = null;
			Grade BestGrade = Grade.NotAtAll;
			Grade CurrentGrade;

			foreach (MultimediaItem Item in Items)
			{
				foreach (IMultimediaContent Handler in Handlers)
				{
					CurrentGrade = Handler.Supports(Item);
					if (CurrentGrade > BestGrade)
					{
						Best = Handler;
						BestGrade = CurrentGrade;
					}
				}

				if (!(Best is null))
					break;
			}

			return Best;    // Will allways be != null, since Multimedia.LinkContent will be chosen by default if no better is found.
		}

		/// <summary>
		/// Multimedia handlers.
		/// </summary>
		public static IMultimediaContent[] Handlers
		{
			get
			{
				lock (synchObject)
				{
					if (handlers is null)
					{
						List<IMultimediaContent> Handlers = new List<IMultimediaContent>();
						IMultimediaContent Handler;
						TypeInfo TI;

						foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IMultimediaContent)))
						{
							TI = Type.GetTypeInfo();
							if (TI.IsAbstract || TI.IsGenericTypeDefinition)
								continue;

							try
							{
								Handler = (IMultimediaContent)Activator.CreateInstance(Type);
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
								continue;
							}

							Handlers.Add(Handler);
						}

						handlers = Handlers.ToArray();
						Types.OnInvalidated += Types_OnInvalidated;
					}
				}
				return handlers;
			}
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			lock (synchObject)
			{
				handlers = null;
			}
		}

		private static IMultimediaContent[] handlers = null;
		private readonly static object synchObject = new object();

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			this.MultimediaHandler.GenerateXAML(Output, TextAlignment, this.items, this.Children, this.aloneInParagraph, this.Document);
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment)
		{
			this.MultimediaHandler.GenerateXamarinForms(Output, TextAlignment, this.items, this.Children, this.aloneInParagraph, this.Document);
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal override bool OutsideParagraph
		{
			get { return true; }
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("Multimedia");
			Output.WriteAttributeString("aloneInParagraph", CommonTypes.Encode(this.aloneInParagraph));
			this.ExportChildren(Output);

			foreach (MultimediaItem Item in this.items)
				Item.Export(Output);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(IEnumerable<MarkdownElement> Children, MarkdownDocument Document)
		{
			return new Multimedia(Document, Children, this.aloneInParagraph, this.items);
		}

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public override bool SameMetaData(MarkdownElement E)
		{
			return E is Multimedia x &&
				x.aloneInParagraph == this.aloneInParagraph &&
				AreEqual(x.items, this.items) &&
				base.SameMetaData(E);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is Multimedia x &&
				this.aloneInParagraph == x.aloneInParagraph &&
				AreEqual(x.items, this.items) &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.aloneInParagraph.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = GetHashCode(this.items);
			
			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
