using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Rss
{
	/// <summary>
	/// A channel may contain any number of <item>s. An item may represent a "story" -- much like 
	/// a story in a newspaper or magazine; if so its description is a synopsis of the story, and 
	/// the link points to the full story. An item may also be complete in itself, if so, the 
	/// description contains the text (entity-encoded HTML is allowed; see examples), and the link 
	/// and title may be omitted. All elements of an item are optional, however at least one of 
	/// title or description must be present.
	/// </summary>
	public class RssItem
	{
		/// <summary>
		/// A channel may contain any number of <item>s. An item may represent a "story" -- much like 
		/// a story in a newspaper or magazine; if so its description is a synopsis of the story, and 
		/// the link points to the full story. An item may also be complete in itself, if so, the 
		/// description contains the text (entity-encoded HTML is allowed; see examples), and the link 
		/// and title may be omitted. All elements of an item are optional, however at least one of 
		/// title or description must be present.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssItem(XmlElement Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			List<RssWarning> Warnings = new List<RssWarning>();
			List<RssCategory> Categories = new List<RssCategory>();
			List<IRssExtension> Extensions = new List<IRssExtension>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (E.NamespaceURI == Xml.NamespaceURI)
				{
					switch (E.LocalName)
					{
						case "title":
							this.Title = E.InnerText;
							break;

						case "link":
							if (Uri.TryCreate(BaseUri, E.InnerText, out Uri Link))
								this.Link = Link;
							else
								Warnings.Add(new RssWarning(E, "Invalid URI: " + E.InnerText));
							break;

						case "description":
							this.Description = E.InnerText;
							break;

						case "author":
							this.Author = E.InnerText;
							break;

						case "category":
							RssCategory Category = new RssCategory(E, BaseUri);
							Warnings.AddRange(Category.Warnings);
							Categories.Add(Category);
							break;

						case "comments":
							if (Uri.TryCreate(BaseUri, E.InnerText, out Uri Comments))
								this.Comments = Comments;
							else
								Warnings.Add(new RssWarning(E, "Invalid URI: " + E.InnerText));
							break;

						case "enclosure":
							if (!(this.Enclosure is null))
								Warnings.Add(new RssWarning(E, "Enclosure already defined."));
							else
							{
								this.Enclosure = new RssEnclosure(E, BaseUri);
								Warnings.AddRange(this.Enclosure.Warnings);
							}
							break;

						case "guid":
							this.Guid = new RssGuid(E);
							break;

						case "pubDate":
							if (CommonTypes.TryParseRfc822(E.InnerText, out DateTimeOffset PubDate))
								this.PublicationDate = PubDate;
							else
								Warnings.Add(new RssWarning(E, "Unable to parse publication date: " + E.InnerText));
							break;

						case "source":
							if (!(this.Source is null))
								Warnings.Add(new RssWarning(E, "Source already defined."));
							else
							{
								this.Source = new RssSource(E, BaseUri);
								Warnings.AddRange(this.Source.Warnings);
							}
							break;

						default:
							Warnings.Add(new RssWarning(E));
							break;
					}
				}
				else
				{
					IRssExtension Extension = Types.FindBest<IRssExtension, XmlElement>(E);
					if (Extension is null)
						Warnings.Add(new RssWarning(E));
					else
						Extensions.Add(Extension.Create(E, BaseUri));
				}
			}

			this.Warnings = Warnings.ToArray();
			this.Categories = Categories.ToArray();
			this.Extensions = Extensions.ToArray();
		}

		/// <summary>
		/// Any warning messages created during parsing.
		/// </summary>
		public RssWarning[] Warnings { get; }

		/// <summary>
		/// The title of the item.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// The URL of the item.
		/// </summary>
		public Uri Link { get; } = null;

		/// <summary>
		/// The item synopsis.
		/// </summary>
		public string Description { get; } = null;

		/// <summary>
		/// Email address of the author of the item.
		/// </summary>
		public string Author { get; } = null;

		/// <summary>
		/// Includes the item in one or more categories.
		/// </summary>
		public RssCategory[] Categories { get; } = null;

		/// <summary>
		/// URL of a page for comments relating to the item.
		/// </summary>
		public Uri Comments { get; } = null;

		/// <summary>
		/// Describes a media object that is attached to the item.
		/// </summary>
		public RssEnclosure Enclosure { get; } = null;

		/// <summary>
		/// A string that uniquely identifies the item.
		/// </summary>
		public RssGuid Guid { get; } = null;

		/// <summary>
		/// Indicates when the item was published.
		/// </summary>
		public DateTimeOffset? PublicationDate { get; } = null;

		/// <summary>
		/// The RSS channel that the item came from.
		/// </summary>
		public RssSource Source { get; } = null;

		/// <summary>
		/// Extensions
		/// </summary>
		public IRssExtension[] Extensions { get; } = null;
	}
}
