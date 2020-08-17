using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Content;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Figures;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// An image defined by a URL.
	/// </summary>
	public class ImageUrl : Image
	{
		private StringAttribute url;
		private StringAttribute alt;

		/// <summary>
		/// An image defined by a URL.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public ImageUrl(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "ImageUrl";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.url = new StringAttribute(Input, "url");
			this.alt = new StringAttribute(Input, "alt");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.url.Export(Output);
			this.alt.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new ImageUrl(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is ImageUrl Dest)
			{
				Dest.url = this.url.CopyIfNotPreset();
				Dest.alt = this.alt.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected override SKImage LoadImage(DrawingState State)
		{
			if (this.url.TryEvaluate(State.Session, out string URL))
				this.StartLoad(URL);

			return null;
		}

		private async void StartLoad(string URL)
		{
			try
			{
				object Result = await InternetContent.GetAsync(new Uri(URL),
					new KeyValuePair<string, string>("Accept", "image/*"));

				if (Result is SKImage Image)
				{
					this.image = Image;
					this.Document.RaiseUpdated(this);
				}
			}
			catch (Exception)
			{
				// Ignore
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			if (this.alt.TryEvaluate(State.Session, out string RefId) &&
				State.TryGetElement(RefId, out ILayoutElement Element))
			{
				this.alternative = Element;
			}

			base.Measure(State);
		}

		private ILayoutElement alternative = null;

		/// <summary>
		/// Default width of element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Width</returns>
		public override float GetDefaultWidth(DrawingState State)
		{
			if (!(this.image is null) || this.alternative is null)
				return base.GetDefaultWidth(State);
			else if (this.alternative is FigurePoint2 P2)
				return P2.GetDefaultWidth(State);
			else if (this.alternative is LayoutArea A)
				return A.Width;
			else
				return base.GetDefaultWidth(State);
		}

		/// <summary>
		/// Default height of element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Height</returns>
		public override float GetDefaultHeight(DrawingState State)
		{
			if (!(this.image is null) || this.alternative is null)
				return base.GetDefaultHeight(State);
			else if (this.alternative is FigurePoint2 P2)
				return P2.GetDefaultHeight(State);
			else if (this.alternative is LayoutArea A)
				return A.Height;
			else
				return base.GetDefaultHeight(State);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			base.Draw(State);

			if (this.image is null && !(this.alternative is null))
				this.alternative.DrawShape(State);
		}
	}
}
