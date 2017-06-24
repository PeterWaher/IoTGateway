using System;
using System.IO;
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
		private MultimediaItem[] items;
		private bool aloneInParagraph;

		/// <summary>
		/// Multimedia
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Items">Multimedia items.</param>
		public Multimedia(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, bool AloneInParagraph, params MultimediaItem[] Items)
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
				if (this.handler == null)
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
			Grade CurrentGrade = Grade.NotAtAll;

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

				if (Best != null)
					break;
			}

			return Best;	// Will allways be != null, since Multimedia.LinkContent will be chosen by default if no better is found.
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
					if (handlers == null)
					{
						List<IMultimediaContent> Handlers = new List<IMultimediaContent>();
						IMultimediaContent Handler;

						foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IMultimediaContent)))
						{
							if (Type.GetTypeInfo().IsAbstract)
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
						Types.OnInvalidated += new EventHandler(Types_OnInvalidated);
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
		private static object synchObject = new object();

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			this.MultimediaHandler.GenerateXAML(Output, Settings, TextAlignment, this.items, this.Children, this.aloneInParagraph, this.Document);
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
	}
}
