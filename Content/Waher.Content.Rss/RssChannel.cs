using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Runtime.Inventory;

namespace Waher.Content.Rss
{
	/// <summary>
	/// Contains information about an RSS Channel.
	/// </summary>
	public class RssChannel
	{
		/// <summary>
		/// Contains information about an RSS Channel.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="BaseUri">Base URI</param>
		public RssChannel(XmlElement Xml, Uri BaseUri)
		{
			if (Xml is null)
				throw new ArgumentNullException(nameof(Xml));

			List<RssWarning> Warnings = new List<RssWarning>();
			List<RssItem> Items = new List<RssItem>();
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

						case "language":
							this.Language = E.InnerText;
							break;

						case "copyright":
							this.Copyright = E.InnerText;
							break;

						case "managingEditor":
							this.ManagingEditor = E.InnerText;
							break;

						case "webMaster":
							this.WebMaster = E.InnerText;
							break;

						case "pubDate":
							if (CommonTypes.TryParseRfc822(E.InnerText, out DateTimeOffset PubDate))
								this.PublicationDate = PubDate;
							else
								Warnings.Add(new RssWarning(E, "Unable to parse publication date: " + E.InnerText));
							break;

						case "lastBuildDate":
							if (CommonTypes.TryParseRfc822(E.InnerText, out DateTimeOffset LastBuildDate))
								this.LastBuildDate = LastBuildDate;
							else
								Warnings.Add(new RssWarning(E, "Unable to parse last build date: " + E.InnerText));
							break;

						case "category":
							RssCategory Category = new RssCategory(E, BaseUri);
							Warnings.AddRange(Category.Warnings);
							Categories.Add(Category);
							break;

						case "generator":
							this.Generator = E.InnerText;
							break;

						case "docs":
							if (Uri.TryCreate(BaseUri, E.InnerText, out Uri Documentation))
								this.Documentation = Documentation;
							else
								Warnings.Add(new RssWarning(E, "Invalid URI: " + E.InnerText));
							break;

						case "ttl":
							if (int.TryParse(E.InnerText, out int Ttl))
								this.TimeToLive = TimeSpan.FromMinutes(Ttl);
							else
								Warnings.Add(new RssWarning(E, "Unable to parse TTL: " + E.InnerText));
							break;

						case "image":
							if (!(this.Image is null))
								Warnings.Add(new RssWarning(E, "Image already defined for channel."));
							else
							{
								this.Image = new RssImage(E, BaseUri);
								Warnings.AddRange(this.Image.Warnings);
							}
							break;

						case "item":
							RssItem Item = new RssItem(E, BaseUri);
							Warnings.AddRange(Item.Warnings);
							Items.Add(Item);
							break;

						case "cloud":
							if (!(this.Cloud is null))
								Warnings.Add(new RssWarning(E, "Cloud service already defined for channel."));
							else
								this.Cloud = new RssCloud(E, BaseUri);
							break;

						case "rating":      // TODO
						case "textInput":   // TODO
						case "skipHours":   // TODO
						case "skipDays":    // TODO
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

			this.Categories = Categories.ToArray();
			this.Items = Items.ToArray();
			this.Warnings = Warnings.ToArray();
			this.Extensions = Extensions.ToArray();
		}

		/// <summary>
		/// Any warning messages created during parsing.
		/// </summary>
		public RssWarning[] Warnings { get; } = null;

		/// <summary>
		/// The name of the channel. It's how people refer to your service. If you have an 
		/// HTML website that contains the same information as your RSS file, the title of 
		/// your channel should be the same as the title of your website.
		/// </summary>
		public string Title { get; } = null;

		/// <summary>
		/// The URL to the HTML website corresponding to the channel.
		/// </summary>
		public Uri Link { get; } = null;

		/// <summary>
		/// Phrase or sentence describing the channel.
		/// </summary>
		public string Description { get; } = null;

		/// <summary>
		/// The language the channel is written in. This allows aggregators to group all 
		/// Italian language sites, for example, on a single page. A list of allowable values 
		/// for this element, as provided by Netscape, is here. You may also use values defined 
		/// by the W3C.
		/// </summary>
		public string Language { get; } = null;

		/// <summary>
		/// Copyright notice for content in the channel.
		/// </summary>
		public string Copyright { get; } = null;

		/// <summary>
		/// Email address for person responsible for editorial content.
		/// </summary>
		public string ManagingEditor { get; } = null;

		/// <summary>
		/// Email address for person responsible for technical issues relating to channel.
		/// </summary>
		public string WebMaster { get; } = null;

		/// <summary>
		/// The publication date for the content in the channel. For example, the New York Times 
		/// publishes on a daily basis, the publication date flips once every 24 hours. That's 
		/// when the pubDate of the channel changes. All date-times in RSS conform to the Date 
		/// and Time Specification of RFC 822, with the exception that the year may be expressed 
		/// with two characters or four characters (four preferred).	
		/// </summary>
		public DateTimeOffset? PublicationDate { get; } = null;

		/// <summary>
		/// The last time the content of the channel changed.
		/// </summary>
		public DateTimeOffset? LastBuildDate { get; } = null;

		/// <summary>
		/// Specify one or more categories that the channel belongs to. Follows the same rules 
		/// as the &lt;item&gt;-level category element.
		/// </summary>
		public RssCategory[] Categories { get; } = null;

		/// <summary>
		/// A string indicating the program used to generate the channel.
		/// </summary>
		public string Generator { get; } = null;

		/// <summary>
		/// A URL that points to the documentation for the format used in the RSS file. 
		/// It's probably a pointer to this page. It's for people who might stumble across an 
		/// RSS file on a Web server 25 years from now and wonder what it is.
		/// </summary>
		public Uri Documentation { get; } = null;

		/// <summary>
		/// Reference to web service that supports the rssCloud interface.
		/// </summary>
		public RssCloud Cloud { get; } = null;

		/// <summary>
		/// ttl stands for time to live. It's a number of minutes that indicates how long a 
		/// channel can be cached before refreshing from the source.
		/// </summary>
		public TimeSpan TimeToLive { get; } = TimeSpan.FromMinutes(60);

		/// <summary>
		/// Channel image.
		/// </summary>
		public RssImage Image { get; } = null;

		/// <summary>
		/// Items.
		/// </summary>
		public RssItem[] Items { get; } = null;

		/// <summary>
		/// Extensions
		/// </summary>
		public IRssExtension[] Extensions { get; } = null;
	}
}
