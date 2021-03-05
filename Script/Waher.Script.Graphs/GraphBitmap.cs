using System;
using System.Runtime.InteropServices;
using System.Xml;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Handles bitmap-based graphs.
	/// </summary>
	public class GraphBitmap : Graph, IDisposable
	{
		private PixelInformation pixels;

		/// <summary>
		/// Handles bitmap-based graphs.
		/// </summary>
		public GraphBitmap()
			: base()
		{
		}

		/// <summary>
		/// Handles bitmap-based graphs.
		/// </summary>
		/// <param name="Pixels">Pixels</param>
		public GraphBitmap(PixelInformation Pixels)
			: base()
		{
			this.pixels = Pixels;
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddLeft(ISemiGroupElement Element)
		{
			if (!(Element is Graph G))
				return null;

			GraphSettings Settings = new GraphSettings();
			if (!(this.pixels is null))
			{
				Settings.Width = this.pixels.Width;
				Settings.Height = this.pixels.Height;
			}

			PixelInformation Pixels = G.CreatePixels(Settings);

			using (SKSurface Surface = SKSurface.Create(new SKImageInfo(Math.Max(Pixels.Width, this.pixels?.Width ?? 0),
				Math.Max(Pixels.Height, this.pixels?.Height ?? 0), SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
			{
				SKCanvas Canvas = Surface.Canvas;

				using (SKImage Bmp = Pixels.CreateBitmap())
				{
					Canvas.DrawImage(Bmp, 0, 0);
				}

				if (!(this.pixels is null))
				{
					using (SKImage Image = this.pixels.CreateBitmap())
					{
						Canvas.DrawImage(Image, 0, 0);
					}
				}

				using (SKImage Result = Surface.Snapshot())
				{
					return new GraphBitmap(PixelInformation.FromImage(Result));
				}
			}
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddRight(ISemiGroupElement Element)
		{
			if (!(Element is Graph G))
				return null;

			GraphSettings Settings = new GraphSettings();
			if (!(this.pixels is null))
			{
				Settings.Width = this.pixels.Width;
				Settings.Height = this.pixels.Height;
			}

			PixelInformation Pixels = G.CreatePixels(Settings);

			using (SKSurface Surface = SKSurface.Create(new SKImageInfo(Math.Max(Pixels.Width, this.pixels?.Width ?? 0),
				Math.Max(Pixels.Height, this.pixels?.Height ?? 0), SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
			{
				SKCanvas Canvas = Surface.Canvas;

				if (!(this.pixels is null))
				{
					using (SKImage Image = this.pixels.CreateBitmap())
					{
						Canvas.DrawImage(Image, 0, 0);
					}
				}

				using (SKImage Bmp = Pixels.CreateBitmap())
				{
					Canvas.DrawImage(Bmp, 0, 0);
				}

				using (SKImage Result = Surface.Snapshot())
				{
					return new GraphBitmap(PixelInformation.FromImage(Result));
				}
			}
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State object(s) that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public override PixelInformation CreatePixels(GraphSettings Settings, out object[] States)
		{
			States = new object[0];
			return this.pixels;
		}

		/// <summary>
		/// The recommended bitmap size of the graph, if such is available.
		/// </summary>
		public override Tuple<int, int> RecommendedBitmapSize
		{
			get
			{
				return new Tuple<int, int>(this.pixels?.Width ?? 0, this.pixels?.Height ?? 0);
			}
		}

		/// <summary>
		/// Gets script corresponding to a point in a generated bitmap representation of the graph.
		/// </summary>
		/// <param name="X">X-Coordinate.</param>
		/// <param name="Y">Y-Coordinate.</param>
		/// <param name="States">State objects for the generated bitmap.</param>
		/// <returns>Script.</returns>
		public override string GetBitmapClickScript(double X, double Y, object[] States)
		{
			return "[" + Expression.ToString(X) + "," + Expression.ToString(Y) + "]";
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is GraphBitmap B && (this.pixels?.Equals(B.pixels) ?? (B.pixels is null)));
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.pixels?.GetHashCode() ?? 0;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Exports graph specifics to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportGraph(XmlWriter Output)
		{
			if (!(this.pixels is null))
			{
				Output.WriteStartElement("GraphBitmap");
				Output.WriteAttributeString("width", this.pixels.Width.ToString());
				Output.WriteAttributeString("height", this.pixels.Height.ToString());

				Output.WriteElementString("Png", Convert.ToBase64String(this.pixels.EncodeAsPng()));

				Output.WriteEndElement();
			}
		}

		/// <summary>
		/// Imports graph specifics from XML.
		/// </summary>
		/// <param name="Xml">XML input.</param>
		public override void ImportGraph(XmlElement Xml)
		{
			int Width = int.Parse(Xml.GetAttribute("width"));
			int Height = int.Parse(Xml.GetAttribute("height"));

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Png")
				{
					byte[] Data = Convert.FromBase64String(E.InnerText);
					this.pixels = new PixelInformationPng(Data, Width, Height);
					break;
				}
			}
		}

		/// <summary>
		/// If graph uses default color
		/// </summary>
		public override bool UsesDefaultColor => false;

		/// <summary>
		/// Tries to set the default color.
		/// </summary>
		/// <param name="Color">Default color.</param>
		/// <returns>If possible to set.</returns>
		public override bool TrySetDefaultColor(SKColor Color)
		{
			return false;
		}

	}
}
