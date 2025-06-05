using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Runtime.Cache;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// An image defined by a URL.
	/// </summary>
	public class ImageUrl : Image
	{
		private static readonly Cache<string, SKImage> imageCache = new Cache<string, SKImage>(int.MaxValue, TimeSpan.FromHours(8), TimeSpan.FromMinutes(15));

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
		/// URL
		/// </summary>
		public StringAttribute UrlAttribute
		{
			get => this.url;
			set => this.url = value;
		}

		/// <summary>
		/// Alternative
		/// </summary>
		public StringAttribute AlternativeAttribute
		{
			get => this.alt;
			set => this.alt = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.url = new StringAttribute(Input, "url", this.Document);
			this.alt = new StringAttribute(Input, "alt", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.url?.Export(Output);
			this.alt?.Export(Output);
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
				Dest.url = this.url?.CopyIfNotPreset(Destination.Document);
				Dest.alt = this.alt?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Loads the image defined by the element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Loaded image, or null if not possible to load image, or
		/// image loading is in process.</returns>
		protected override async Task<SKImage> LoadImage(DrawingState State)
		{
			EvaluationResult<string> URL = await this.url.TryEvaluate(State.Session);
			if (URL.Ok && !string.IsNullOrEmpty(URL.Result))
			{
				if (imageCache.TryGetValue(URL.Result, out SKImage Image))
				{
					this.image = Image;
					return Image;
				}

				if (!this.Document.SupportsAsynchronnousUpdates)
				{
					ContentResponse Result = await InternetContent.GetAsync(new Uri(URL.Result),
						new KeyValuePair<string, string>("Accept", "image/*"));

					if (!Result.HasError && Result.Decoded is SKImage Image2)
					{
						imageCache[URL.Result] = Image2;

						this.image = Image2;
						return Image2;
					}
				}

				this.StartLoad(URL.Result);
			}

			return null;
		}

		private async void StartLoad(string URL)
		{
			try
			{
				ContentResponse Result = await InternetContent.GetAsync(new Uri(URL),
					new KeyValuePair<string, string>("Accept", "image/*"));

				if (!Result.HasError && Result.Decoded is SKImage Image)
				{
					imageCache[URL] = Image;

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
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			EvaluationResult<string> RefId = await this.alt.TryEvaluate(State.Session);
			if (RefId.Ok && this.Document.TryGetElement(RefId.Result, out ILayoutElement Element))
				this.alternative = Element;

			await base.DoMeasureDimensions(State);

			if (!(this.alternative is null))
				await this.alternative.MeasureDimensions(State);
		}

		/// <summary>
		/// Gets an alternative representation, in case the image is not available.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Alternative representation, if available, null otherwise.</returns>
		protected override ILayoutElement GetAlternative(DrawingState State)
		{
			return this.alternative;
		}

		private ILayoutElement alternative = null;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (this.image is null && !(this.alternative is null))
				await this.alternative.DrawShape(State);

			await base.Draw(State);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.url?.ExportState(Output);
			this.alt?.ExportState(Output);
		}
	}
}
