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

		/// <summary>
		/// Multimedia
		/// </summary>
		public Multimedia(MarkdownDocument Document, LinkedList<MarkdownElement> ChildElements, string Url, string Title, int? Width, int? Height)
			: base(Document, ChildElements, Url, Title)
		{
			this.width = Width;
			this.height = Height;
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
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			this.MultimediaHandler.GenerateHTML(Output, this.Url, this.Title, this.width, this.height, this.Children);
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
