using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Events;
using Waher.Content.Markdown.Rendering;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Multimedia
	/// </summary>
	public class Multimedia : MarkdownElementChildren
	{
		private static readonly Dictionary<Type, IMultimediaRenderer[]> renderersPerType = new Dictionary<Type, IMultimediaRenderer[]>();

		private IMultimediaContent handler = null;
		private Type handlerType = null;
		private readonly MultimediaItem[] items;
		private readonly bool aloneInParagraph;

		/// <summary>
		/// Multimedia
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="ChildElements">Child elements.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Items">Multimedia items.</param>
		public Multimedia(MarkdownDocument Document, ChunkedList<MarkdownElement> ChildElements, bool AloneInParagraph, params MultimediaItem[] Items)
			: base(Document, ChildElements)
		{
			this.items = Items;
			this.aloneInParagraph = AloneInParagraph;
		}

		/// <summary>
		/// Multimedia items.
		/// </summary>
		public MultimediaItem[] Items => this.items;

		/// <summary>
		/// If the element is alone in a paragraph.
		/// </summary>
		public bool AloneInParagraph => this.aloneInParagraph;

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public override Task Render(IRenderer Output) => Output.Render(this);

		/// <summary>
		/// Multimedia handler.
		/// </summary>
		public T MultimediaHandler<T>()
			where T : IMultimediaRenderer
		{
			if (this.handlerType is null || this.handlerType != typeof(T))
			{
				this.handler = GetMultimediaHandler<T>(this.items);
				this.handlerType = typeof(T);
			}

			return (T)this.handler;
		}

		/// <summary>
		/// Gets the best multimedia handler for a set of URLs or file names.
		/// </summary>
		/// <param name="URLs">Set of URLs, or file names.</param>
		/// <returns>Best multimedia handler.</returns>
		public static IMultimediaContent GetMultimediaHandler<T>(params string[] URLs)
			where T : IMultimediaRenderer
		{
			int i, c = URLs.Length;
			MultimediaItem[] Items = new MultimediaItem[c];

			for (i = 0; i < c; i++)
				Items[i] = new MultimediaItem(null, URLs[i], string.Empty, null, null);

			return GetMultimediaHandler<T>(Items);
		}

		/// <summary>
		/// Gets the best multimedia handler for a set of multimedia items.
		/// </summary>
		/// <param name="Items">Set of multimedia items.</param>
		/// <returns>Best multimedia handler.</returns>
		public static T GetMultimediaHandler<T>(params MultimediaItem[] Items)
			where T : IMultimediaRenderer
		{
			T Best = default;
			Grade BestGrade = Grade.NotAtAll;
			Grade CurrentGrade;
			bool HasBest = false;

			foreach (MultimediaItem Item in Items)
			{
				foreach (IMultimediaContent Handler in GetRenderers<T>())
				{
					CurrentGrade = Handler.Supports(Item);
					if (CurrentGrade > BestGrade && Handler is T TypedHandler)
					{
						Best = TypedHandler;
						BestGrade = CurrentGrade;
						HasBest = true;
					}
				}

				if (HasBest)
					break;
			}

			return Best;
		}

		/// <summary>
		/// Multimedia handlers.
		/// </summary>
		public static IMultimediaRenderer[] GetRenderers<T>()
			where T : IMultimediaRenderer
		{
			lock (renderersPerType)
			{
				if (!renderersPerType.TryGetValue(typeof(T), out IMultimediaRenderer[] Renderers))
				{
					ChunkedList<IMultimediaRenderer> Handlers = new ChunkedList<IMultimediaRenderer>();
					IMultimediaRenderer Handler;

					foreach (Type Type in Types.GetTypesImplementingInterface(typeof(T)))
					{
						ConstructorInfo CI = Types.GetDefaultConstructor(Type);
						if (CI is null)
							continue;

						try
						{
							Handler = (IMultimediaRenderer)CI.Invoke(Types.NoParameters);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
							continue;
						}

						Handlers.Add(Handler);
					}

					Renderers = Handlers.ToArray();
					renderersPerType[typeof(T)] = Renderers;
				}

				return Renderers;
			}
		}

		static Multimedia()
		{
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			lock (renderersPerType)
			{
				renderersPerType.Clear();
			}
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		public override bool OutsideParagraph => true;

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public override bool InlineSpanElement => true;

		/// <summary>
		/// Creates an object of the same type, and meta-data, as the current object,
		/// but with content defined by <paramref name="Children"/>.
		/// </summary>
		/// <param name="Children">New content.</param>
		/// <param name="Document">Document that will contain the element.</param>
		/// <returns>Object of same type and meta-data, but with new content.</returns>
		public override MarkdownElementChildren Create(ChunkedList<MarkdownElement> Children, MarkdownDocument Document)
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

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public override void IncrementStatistics(MarkdownStatistics Statistics)
		{
			IncrementStatistics(Statistics, this.items);
		}

		internal static void IncrementStatistics(MarkdownStatistics Statistics, MultimediaItem[] Items)
		{
			if (!(Items is null))
			{
				Statistics.NrMultimedia++;

				if (Items.Length > 1)
					Statistics.NrMultiformatMultimedia++;

				foreach (MultimediaItem Item in Items)
				{
					if (Statistics.IntMultimediaPerContentType is null)
					{
						Statistics.IntMultimediaPerContentType = new Dictionary<string, ChunkedList<string>>();
						Statistics.IntMultimediaPerContentCategory = new Dictionary<string, ChunkedList<string>>();
						Statistics.IntMultimediaPerExtension = new Dictionary<string, ChunkedList<string>>();
					}

					IncItem(Item.ContentType, Item.Url, Statistics.IntMultimediaPerContentType);
					IncItem(Item.Extension, Item.Url, Statistics.IntMultimediaPerExtension);

					string s = Item.ContentType;
					int i = s.IndexOf('/');
					if (i > 0)
						s = s.Substring(0, i);

					IncItem(s, Item.Url, Statistics.IntMultimediaPerContentCategory);
				}
			}
		}

		private static void IncItem(string Key, string Url, Dictionary<string, ChunkedList<string>> Dictionary)
		{
			if (!Dictionary.TryGetValue(Key, out ChunkedList<string> List))
			{
				List = new ChunkedList<string>();
				Dictionary[Key] = List;
			}

			List.Add(Url);
		}

	}
}
