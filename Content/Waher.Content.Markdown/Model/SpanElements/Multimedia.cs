using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Script;
using Waher.Events;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Multimedia
	/// </summary>
	public class Multimedia : Link
	{
		private IMultimediaContent handler = null;
		private int? width;
		private int? height;
		private bool aloneInParagraph;

		/// <summary>
		/// Multimedia
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="Url">URL</param>
		/// <param name="Title">Optional title.</param>
		/// <param name="Width">Optional width.</param>
		/// <param name="Height">Optional title.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		public Multimedia(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Url, string Title, int? Width, int? Height,
			bool AloneInParagraph)
			: base(Document, ChildElements, Url, Title)
		{
			this.width = Width;
			this.height = Height;
			this.aloneInParagraph = AloneInParagraph;
		}

		/// <summary>
		/// Optional width.
		/// </summary>
		public int? Width
		{
			get { return this.width; }
		}

		/// <summary>
		/// Optional height.
		/// </summary>
		public int? Height
		{
			get { return this.height; }
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
			this.MultimediaHandler.GenerateHTML(Output, this.Url, this.Title, this.width, this.height, this.Children, this.aloneInParagraph);
		}

		public IMultimediaContent MultimediaHandler
		{
			get
			{
				if (this.handler == null)
				{
					IMultimediaContent Best = null;
					Grade BestGrade = Grade.NotAtAll;
					Grade CurrentGrade = Grade.NotAtAll;

					foreach (IMultimediaContent Handler in Handlers)
					{
						CurrentGrade = Handler.Supports(this.Url);
						if (CurrentGrade > BestGrade)
						{
							Best = Handler;
							BestGrade = CurrentGrade;
						}
					}

					this.handler = Best;	// Will allways be != null, since Multimedia.LinkContent will be chosen by default if no better is found.
				}

				return this.handler;
			}
		}

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
						ConstructorInfo CI;

						foreach (Type Type in Types.GetTypesImplementingInterface(typeof(IMultimediaContent)))
						{
							CI = Type.GetConstructor(Types.NoTypes);
							if (CI == null)
								continue;

							try
							{
								Handler = (IMultimediaContent)CI.Invoke(Types.NoParameters);
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

	}
}
