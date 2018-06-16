using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Content.Xml;
using Waher.Script.Graphs.Functions.Colors;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Contains properties for a theme.
	/// </summary>
	public class ThemeDefinition
	{
		private readonly Dictionary<string, string> customProperties = new Dictionary<string, string>();
		private readonly string id;
		private readonly string title;
		private readonly string thumbnail;
		private readonly int thumbnailWidth;
		private readonly int thumbnailHeight;
		private readonly string cssx;
		private SKColor headerColor;
		private SKColor headerTextColor;
		private SKColor buttonColor;
		private SKColor buttonTextColor;
		private SKColor menuTextColor;
		private SKColor editColor;
		private SKColor linkColorUnvisited;
		private SKColor linkColorVisited;
		private SKColor linkColorHot;

		/// <summary>
		/// Contains properties for a theme.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		internal ThemeDefinition(XmlDocument Xml)
		{
			this.id = XML.Attribute(Xml.DocumentElement, "id");

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "Presentation":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2)
								{
									switch (E2.LocalName)
									{
										case "Title":
											this.title = E2.InnerText;
											break;

										case "Thumbnail":
											this.thumbnailWidth = int.Parse(XML.Attribute(E2, "width"));
											this.thumbnailHeight = int.Parse(XML.Attribute(E2, "height"));
											this.thumbnail = "/Themes/" + this.id + "/" + E2.InnerText;
											break;
									}
								}
							}
							break;

						case "BasicProperties":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2)
								{
									switch (E2.LocalName)
									{
										case "CSSX":
											this.cssx = "/Themes/" + this.id + "/" + E2.InnerText;
											break;

										case "HeaderColor":
											if (Color.TryParse(E2.InnerText, out SKColor cl))
												this.headerColor = cl;
											break;

										case "HeaderTextColor":
											if (Color.TryParse(E2.InnerText, out cl))
												this.headerTextColor = cl;
											break;

										case "ButtonColor":
											if (Color.TryParse(E2.InnerText, out cl))
												this.buttonColor = cl;
											break;

										case "ButtonTextColor":
											if (Color.TryParse(E2.InnerText, out cl))
												this.buttonTextColor = cl;
											break;

										case "MenuTextColor":
											if (Color.TryParse(E2.InnerText, out cl))
												this.menuTextColor = cl;
											break;

										case "EditColor":
											if (Color.TryParse(E2.InnerText, out cl))
												this.editColor = cl;
											break;

										case "LinkColorUnvisited":
											if (Color.TryParse(E2.InnerText, out cl))
												this.linkColorUnvisited = cl;
											break;

										case "LinkColorVisited":
											if (Color.TryParse(E2.InnerText, out cl))
												this.linkColorVisited = cl;
											break;

										case "LinkColorHot":
											if (Color.TryParse(E2.InnerText, out cl))
												this.linkColorHot = cl;
											break;
									}
								}
							}
							break;

						case "CustomProperties":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "Property")
								{
									string Name = XML.Attribute(E2, "name");
									string Value = XML.Attribute(E2, "value");

									this.customProperties[Name] = Value;
								}
							}
							break;
					}
				}
			}
		}

		/// <summary>
		/// Access to custom properties
		/// </summary>
		/// <param name="key">Key Name</param>
		/// <returns>Key value, if found.</returns>
		public string this[string key] => this.customProperties[key];

		/// <summary>
		/// ID of the theme.
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// A human readable title for the theme.
		/// </summary>
		public string Title => this.title;

		/// <summary>
		/// Resource of thumbnail image for the theme
		/// </summary>
		public string Thumbnail => this.thumbnail;

		/// <summary>
		/// Width of the thumbnail image for the theme
		/// </summary>
		public int ThumbnailWidth => this.thumbnailWidth;

		/// <summary>
		/// Height of the thumbnail image for the theme
		/// </summary>
		public int ThumbnailHeight => this.thumbnailHeight;

		/// <summary>
		/// Resource of the CSSX file of the theme
		/// </summary>
		public string CSSX => this.cssx;

		/// <summary>
		/// Color for text headers.
		/// </summary>
		public SKColor HeaderColor => this.headerColor;

		/// <summary>
		/// Text color for text headers, if used with backgound <see cref="HeaderColor"/>.
		/// </summary>
		public SKColor HeaderTextColor => this.headerTextColor;

		/// <summary>
		/// Background color for controls.
		/// </summary>
		public SKColor ButtonColor => this.buttonColor;

		/// <summary>
		/// Text color for controls.
		/// </summary>
		public SKColor ButtonTextColor => this.buttonTextColor;

		/// <summary>
		/// Text color for links in the menu.
		/// </summary>
		public SKColor MenuTextColor => this.menuTextColor;

		/// <summary>
		/// Backgound color for edited text.
		/// </summary>
		public SKColor EditColor => this.editColor;

		/// <summary>
		/// Color of unvisited links.
		/// </summary>
		public SKColor LinkColorUnvisited => this.linkColorUnvisited;

		/// <summary>
		/// Color of visited links.
		/// </summary>
		public SKColor LinkColorVisited => this.linkColorVisited;

		/// <summary>
		/// Color of hot links.
		/// </summary>
		public SKColor LinkColorHot => this.linkColorHot;

		/// <summary>
		/// Gets defined custom properties.
		/// </summary>
		/// <returns>Custom propeties.</returns>
		public KeyValuePair<string, string>[] GetCustomProperties()
		{
			KeyValuePair<string, string>[] Result = new KeyValuePair<string, string>[this.customProperties.Count];
			int i = 0;

			foreach (KeyValuePair<string, string> P in this.customProperties)
				Result[i++] = P;

			return Result;
		}
	}
}
