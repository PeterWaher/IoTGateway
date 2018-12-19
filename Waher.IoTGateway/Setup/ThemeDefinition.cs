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
		private readonly ThemeImage[] backgroundImages = null;
		private readonly ThemeImage[] bannerImages = null;
		private readonly ThemeImage thumbnail;
		private readonly string id;
		private readonly string title;
		private readonly string cssx;
		private readonly string fontFamily;
		private SKColor textColor;
		private SKColor backgroundColor;
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
											this.thumbnail = this.ParseThemeImage(E2);
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
											this.cssx = this.GetResourceName(E2.InnerText);
											break;

										case "TextColor":
											if (Color.TryParse(E2.InnerText, out SKColor cl))
												this.textColor = cl;
											break;

										case "BackgroundColor":
											if (Color.TryParse(E2.InnerText, out cl))
												this.backgroundColor = cl;
											break;

										case "HeaderColor":
											if (Color.TryParse(E2.InnerText, out cl))
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

										case "FontFamily":
											this.fontFamily = E2.InnerText;
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

						case "BackgroundImages":
							List<ThemeImage> Images = new List<ThemeImage>();

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "BackgroundImage")
									Images.Add(this.ParseThemeImage(E2));
							}

							this.backgroundImages = Images.ToArray();
							break;

						case "BannerImages":
							Images = new List<ThemeImage>();

							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "BannerImage")
									Images.Add(this.ParseThemeImage(E2));
							}

							this.bannerImages = Images.ToArray();
							break;
					}
				}
			}
		}

		private ThemeImage ParseThemeImage(XmlElement E)
		{
			string FileName = E.InnerText;
			int Width = XML.Attribute(E, "width", 0);
			int Height = XML.Attribute(E, "height", 0);

			return new ThemeImage(this.GetResourceName(FileName), Width, Height);
		}

		private string GetResourceName(string FileName)
		{
			return "/Themes/" + this.id + "/" + FileName;
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
		/// Thumbnail for the theme
		/// </summary>
		public ThemeImage Thumbnail => this.thumbnail;

		/// <summary>
		/// Resource of the CSSX file of the theme
		/// </summary>
		public string CSSX => this.cssx;

		/// <summary>
		/// Color for normal text.
		/// </summary>
		public SKColor TextColor => this.textColor;

		/// <summary>
		/// Background Color for normal text.
		/// </summary>
		public SKColor BackgroundColor => this.backgroundColor;

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
		/// CSS font-family value.
		/// </summary>
		public string FontFamily => this.fontFamily;

		/// <summary>
		/// Background images.
		/// </summary>
		public ThemeImage[] BackgroundImages => this.backgroundImages;

		/// <summary>
		/// Banner images.
		/// </summary>
		public ThemeImage[] BannerImages => this.bannerImages;

		/// <summary>
		/// Gets a background image best matching a given size.
		/// </summary>
		/// <param name="Width">Desired width</param>
		/// <param name="Height">Desired height</param>
		/// <returns>Image</returns>
		public ThemeImage GetBackgroundImage(int Width, int Height)
		{
			return this.GetImage(this.backgroundImages, Width, Height);
		}

		/// <summary>
		/// Gets a banner image best matching a given size.
		/// </summary>
		/// <param name="Width">Desired width</param>
		/// <param name="Height">Desired height</param>
		/// <returns>Image</returns>
		public ThemeImage GetBannerImage(int Width, int Height)
		{
			return this.GetImage(this.bannerImages, Width, Height);
		}

		private ThemeImage GetImage(ThemeImage[] Images, int Width, int Height)
		{
			ThemeImage Best = null;
			double BestSqrError = double.MaxValue;
			double SqrError, d;

			foreach (ThemeImage Img in Images)
			{
				d = Img.Width - Width;
				SqrError = d * d;

				d = Img.Height - Height;
				SqrError += d * d;

				if (Best is null || SqrError < BestSqrError)
				{
					Best = Img;
					BestSqrError = SqrError;
				}
			}

			return Best;
		}

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
