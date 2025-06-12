﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Layout.Layout2D.Events;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Backgrounds;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.Layout.Layout2D
{
	/// <summary>
	/// Contains a 2D layout document.
	/// </summary>
	public class Layout2DDocument : IDisposableAsync
	{
		/// <summary>
		/// Layout2D
		/// </summary>
		public const string LocalName = "Layout2D";

		/// <summary>
		/// http://waher.se/Schema/Layout2D.xsd
		/// </summary>
		public const string Namespace = "http://waher.se/Schema/Layout2D.xsd";

		/// <summary>
		/// Schema resource name.
		/// </summary>
		public static readonly string SchemaResourceName = typeof(Layout2DDocument).Namespace + ".Schema.Layout2D.xsd";

		private static Dictionary<string, ILayoutElement> elementTypes = new Dictionary<string, ILayoutElement>();
		private static bool initialized = false;

		private readonly Dictionary<string, ILayoutElement> elementsById = new Dictionary<string, ILayoutElement>();
		private readonly Dictionary<string, object> attachments = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
		private readonly Variables session;
		private ILayoutElement root;

		#region Construction

		/// <summary>
		/// Contains a 2D layout document.
		/// </summary>
		/// <param name="Session">Session variables</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		private Layout2DDocument(Variables Session, params KeyValuePair<string, object>[] Attachments)
		{
			this.session = Session;

			if (!(Attachments is null))
			{
				foreach (KeyValuePair<string, object> P in Attachments)
					this.attachments[P.Key] = P.Value;
			}
		}

		internal async Task<ILayoutElement> CreateElement(XmlElement Xml, ILayoutElement Parent)
		{
			string Key = Xml.NamespaceURI + "#" + Xml.LocalName;

			if (!elementTypes.TryGetValue(Key, out ILayoutElement E))
				throw new LayoutSyntaxException("Layout element not recognized: " + Key);

			ILayoutElement Result = E.Create(this, Parent);
			await Result.FromXml(Xml);

			EvaluationResult<string> Id = await Result.IdAttribute.TryEvaluate(this.session);
			if (Id.Ok && !string.IsNullOrEmpty(Id.Result))
				this.AddElementId(Id.Result, Result);

			return Result;
		}

		/// <summary>
		/// Loads a 2D layout document from a file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromFile(string FileName, params KeyValuePair<string, object>[] Attachments)
		{
			return FromFile(FileName, true, Attachments);
		}

		/// <summary>
		/// Loads a 2D layout document from a file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromFile(string FileName, bool Preprocess, params KeyValuePair<string, object>[] Attachments)
		{
			return FromFile(FileName, Preprocess, new Variables(), Attachments);
		}

		/// <summary>
		/// Loads a 2D layout document from a file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Session">Session variables.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static async Task<Layout2DDocument> FromFile(string FileName, bool Preprocess, Variables Session, params KeyValuePair<string, object>[] Attachments)
		{
			string Xml = await Files.ReadAllTextAsync(FileName);
			return await FromXml(Xml, Preprocess, Session, Attachments);
		}

		/// <summary>
		/// Loads a 2D layout document from a stream.
		/// </summary>
		/// <param name="Input">Stream input.</param>
		/// <param name="DefaultEncoding">Default text encoding to use, if not deduced from a Byte-Order-Mark (BOM) of the file.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromStream(Stream Input, Encoding DefaultEncoding, params KeyValuePair<string, object>[] Attachments)
		{
			return FromStream(Input, DefaultEncoding, true, Attachments);
		}

		/// <summary>
		/// Loads a 2D layout document from a stream.
		/// </summary>
		/// <param name="Input">Stream input.</param>
		/// <param name="DefaultEncoding">Default text encoding to use, if not deduced from a Byte-Order-Mark (BOM) of the file.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromStream(Stream Input, Encoding DefaultEncoding, bool Preprocess, params KeyValuePair<string, object>[] Attachments)
		{
			return FromStream(Input, DefaultEncoding, Preprocess, new Variables(), Attachments);
		}

		/// <summary>
		/// Loads a 2D layout document from a stream.
		/// </summary>
		/// <param name="Input">Stream input.</param>
		/// <param name="DefaultEncoding">Default text encoding to use, if not deduced from a Byte-Order-Mark (BOM) of the file.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Session">Session variables.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromStream(Stream Input, Encoding DefaultEncoding, bool Preprocess, Variables Session, params KeyValuePair<string, object>[] Attachments)
		{
			long c = Input.Length - Input.Position;
			if (c > int.MaxValue)
				throw new OutOfMemoryException("Input too large");

			int c2 = (int)c;
			byte[] Bin = new byte[c2];
			Input.ReadAll(Bin, 0, c2);

			string Xml = Strings.GetString(Bin, DefaultEncoding);

			return FromXml(Xml, Preprocess, Session, Attachments);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromXml(string Xml, params KeyValuePair<string, object>[] Attachments)
		{
			return FromXml(Xml, true, Attachments);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromXml(string Xml, bool Preprocess, params KeyValuePair<string, object>[] Attachments)
		{
			return FromXml(Xml, Preprocess, new Variables(), Attachments);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Session">Session variables.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static async Task<Layout2DDocument> FromXml(string Xml, bool Preprocess, Variables Session, params KeyValuePair<string, object>[] Attachments)
		{
			if (Preprocess)
				Xml = await Expression.TransformAsync(Xml, "{{", "}}", Session);

			XmlDocument Doc = new XmlDocument();

			try
			{
				Doc.LoadXml(Xml);
			}
			catch (XmlException ex)
			{
				throw XML.AnnotateException(ex, Xml);
			}

			return await FromXml(Doc, Session, Attachments);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromXml(XmlDocument Xml, params KeyValuePair<string, object>[] Attachments)
		{
			return FromXml(Xml.DocumentElement, new Variables(), Attachments);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Session">Session variables.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromXml(XmlDocument Xml, Variables Session, params KeyValuePair<string, object>[] Attachments)
		{
			return FromXml(Xml.DocumentElement, Session, Attachments);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static Task<Layout2DDocument> FromXml(XmlElement Xml, params KeyValuePair<string, object>[] Attachments)
		{
			return FromXml(Xml, new Variables(), Attachments);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Session">Session variables.</param>
		/// <param name="Attachments">Any attachments referenced from the layout.</param>
		public static async Task<Layout2DDocument> FromXml(XmlElement Xml, Variables Session, params KeyValuePair<string, object>[] Attachments)
		{
			if (Xml.LocalName != LocalName || Xml.NamespaceURI != Namespace)
				throw new ArgumentException("XML does not represent a 2D layout document.", nameof(Xml));

			lock (elementTypes)
			{
				if (!initialized)
				{
					Type[] LayoutElementTypes = Types.GetTypesImplementingInterface(typeof(ILayoutElement));
					Dictionary<string, ILayoutElement> TypesPerKey = new Dictionary<string, ILayoutElement>();
					Layout2DDocument Temp = new Layout2DDocument(new Variables());

					foreach (Type T in LayoutElementTypes)
					{
						TypeInfo TI = T.GetTypeInfo();
						if (TI.IsAbstract || TI.IsInterface || TI.IsGenericTypeDefinition)
							continue;

						try
						{
							ILayoutElement E = (ILayoutElement)Types.Instantiate(T, Temp, null);
							string Key = E.Namespace + "#" + E.LocalName;

							if (TypesPerKey.ContainsKey(Key))
								Log.Error("Layout element type already defined: " + Key);
							else
								TypesPerKey[Key] = E;
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}

					elementTypes = TypesPerKey;
					initialized = true;

					Types.OnInvalidated += (Sender, e) => initialized = false;
				}
			}

			Layout2DDocument Result = new Layout2DDocument(Session, Attachments);
			Result.root = await Result.CreateElement(Xml, null);

			return Result;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync() instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync"/>
		/// </summary>
		public async Task DisposeAsync()
		{
			this.root?.Dispose();

			foreach (object Attachment in this.attachments.Values)
			{
				if (Attachment is IDisposableAsync DisposableAsync)
					await DisposableAsync.DisposeAsync();
				else if (Attachment is IDisposable Disposable)
					Disposable.Dispose();
			}
		}

		#endregion

		#region Rendering

		/// <summary>
		/// Renders the layout to an image
		/// </summary>
		/// <param name="Settings">Rendering settings.</param>
		/// <returns>Image and generated maps</returns>
		public async Task<KeyValuePair<SKImage, Map[]>> Render(RenderSettings Settings)
		{
			Map[] Maps = null;    // TODO: Generate maps.

			int Width;
			int Height;

			switch (Settings.ImageSize)
			{
				case RenderedImageSize.ResizeImage:
					Width = Height = 10;
					break;

				case RenderedImageSize.ScaleToFit:
				default:
					Width = Settings.Width;
					Height = Settings.Height;
					break;
			}

			SKSurface Surface = SKSurface.Create(new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
			DrawingState State = null;
			try
			{
				SKCanvas Canvas = Surface.Canvas;
				State = new DrawingState(Canvas, Settings, this.session);

				if (Settings.BackgroundColor != SKColor.Empty)
					Canvas.Clear(Settings.BackgroundColor);

				int Limit = 100;

				while (!(this.root is null))
				{
					State.ClearRelativeMeasurement(--Limit <= 0);
					await this.root.MeasureDimensions(State);

					this.RaiseMeasuringDimensions(State);

					if (!State.MeasureRelative)
						break;

					if (Limit <= 0)
					{
						string ShortestBranch = State.GetShortestRelativeMeasurementStateXml();
						throw new InvalidOperationException("Layout positions not well defined. Dimensions diverge:\r\n\r\n" + ShortestBranch);
					}
				}

				this.root?.MeasurePositions(State);
				this.RaiseMeasuringPositions(State);

				switch (Settings.ImageSize)
				{
					case RenderedImageSize.ResizeImage:
						Surface.Dispose();
						Surface = null;

						if (!(this.root is null))
						{
							Width = (int)this.root.Right - (int)this.root.Left + 1;
							Height = (int)this.root.Bottom - (int)this.root.Top - 1;
						}

						Surface = SKSurface.Create(new SKImageInfo(Width, Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
						if (Surface is null)
							throw new InvalidOperationException("Unable to render layout.");

						Canvas = Surface.Canvas;
						State.Canvas = Canvas;

						if (Settings.BackgroundColor != SKColor.Empty)
							Canvas.Clear(Settings.BackgroundColor);

						if (!(this.root is null) && this.root.Left.HasValue && this.root.Top.HasValue)
							State.Canvas.Translate(-this.root.Left.Value, -this.root.Top.Value);
						break;

					case RenderedImageSize.ScaleToFit:
						if (!(this.root is null))
						{
							if (this.root.Width.HasValue && this.root.Height.HasValue)
							{
								float Width2 = this.root.Width.Value + 1;
								float Height2 = this.root.Height.Value + 1;
								float ScaleX = Width / Width2;
								float ScaleY = Height / Height2;

								if (ScaleX < ScaleY)
								{
									State.Canvas.Translate(0, (Height - Height2 * ScaleX) / 2);
									State.Canvas.Scale(ScaleX);
								}
								else if (ScaleY < ScaleX)
								{
									State.Canvas.Translate((Width - Width2 * ScaleY) / 2, 0);
									State.Canvas.Scale(ScaleY);
								}
							}

							if (this.root.Left.HasValue && this.root.Top.HasValue)
								State.Canvas.Translate(-this.root.Left.Value, -this.root.Top.Value);
						}
						break;
				}

				if (!(this.root is null))
					await this.root.Draw(State);

				return new KeyValuePair<SKImage, Map[]>(Surface.Snapshot(), Maps);
			}
			finally
			{
				State?.Dispose();
				Surface?.Dispose();
			}
		}

		/// <summary>
		/// Event raised when the layout dimensions are being measured. Event can be raised multiple times
		/// during the rendering process.
		/// </summary>
		public event EventHandler<DrawingEventArgs> OnMeasuringDimensions = null;

		/// <summary>
		/// Raises the <see cref="OnMeasuringDimensions"/> event.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public void RaiseMeasuringDimensions(DrawingState State)
		{
			this.OnMeasuringDimensions?.Raise(this, new DrawingEventArgs(this, State));
		}

		/// <summary>
		/// Event raised when the layout positions are being measured. Event is eaised once after dimensions have been measured.
		/// </summary>
		public event EventHandler<DrawingEventArgs> OnMeasuringPositions = null;

		/// <summary>
		/// Raises the <see cref="OnMeasuringPositions"/> event.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public void RaiseMeasuringPositions(DrawingState State)
		{
			this.OnMeasuringPositions?.Raise(this, new DrawingEventArgs(this, State));
		}

		#endregion

		/// <summary>
		/// Event raised when the internal state of the layout has been updated.
		/// </summary>
		public event EventHandler<UpdatedEventArgs> OnUpdated = null;

		/// <summary>
		/// If asynchronous updates are supported.
		/// </summary>
		public bool SupportsAsynchronnousUpdates => !(this.OnUpdated is null);

		/// <summary>
		/// Raises the <see cref="OnUpdated"/> event.
		/// </summary>
		/// <param name="Element">Element being updated.</param>
		public void RaiseUpdated(ILayoutElement Element)
		{
			this.OnUpdated?.Raise(this, new UpdatedEventArgs(this, Element));
		}

		/// <summary>
		/// Tries to get content from attached content.
		/// </summary>
		/// <param name="ContentId">Content ID</param>
		/// <param name="Content">Content, if found, null otherwise.</param>
		/// <returns>If attached content was found with the given content ID.</returns>
		public bool TryGetContent(string ContentId, out object Content)
		{
			return this.attachments.TryGetValue(ContentId, out Content);
		}

		/// <summary>
		/// Disposes of attached content, given its ID.
		/// </summary>
		/// <param name="ContentId">Content ID</param>
		/// <returns>If content with the corresponding ID was found and disposed.</returns>
		public async Task<bool> DisposeContent(string ContentId)
		{
			if (this.attachments.TryGetValue(ContentId, out object Obj))
			{
				this.attachments.Remove(ContentId);

				if (Obj is IDisposableAsync DisposableAsync)
					await DisposableAsync.DisposeAsync();
				else if (Obj is IDisposable Disposable)
					Disposable.Dispose();

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Adds content to the layout.
		/// </summary>
		/// <param name="Content">Content</param>
		/// <returns>Content ID of newly added content</returns>
		public string AddContent(object Content)
		{
			string ContentId;

			do
			{
				ContentId = Guid.NewGuid().ToString();
			}
			while (this.attachments.ContainsKey(ContentId));

			this.attachments[ContentId] = Content;

			return ContentId;
		}

		/// <summary>
		/// Adds content to the layout.
		/// </summary>
		/// <param name="ContentId">Content ID</param>
		/// <param name="Content">Content</param>
		public async Task AddContent(string ContentId, object Content)
		{
			await this.DisposeContent(ContentId);
			this.attachments[ContentId] = Content;
		}

		/// <summary>
		/// Adds an element with an ID
		/// </summary>
		/// <param name="Id">Element ID</param>
		/// <param name="Element">Element</param>
		public void AddElementId(string Id, ILayoutElement Element)
		{
			this.elementsById[Id] = Element;
		}

		/// <summary>
		/// Tries to get a layout element, given an ID reference
		/// </summary>
		/// <param name="Id">Layout ID</param>
		/// <param name="Element">Element retrieved, if found.</param>
		/// <returns>If an element with the corresponding ID was found.</returns>
		public bool TryGetElement(string Id, out ILayoutElement Element)
		{
			return this.elementsById.TryGetValue(Id, out Element);
		}

		/// <summary>
		/// Clears registered elements with IDs.
		/// </summary>
		public void ClearElementIds()
		{
			this.elementsById.Clear();
		}

		/// <summary>
		/// Creates a render settings object.
		/// </summary>
		/// <param name="Session">Session variables.</param>
		/// <returns>Render settings.</returns>
		public async Task<RenderSettings> GetRenderSettings(Variables Session)
		{
			RenderSettings Result = new RenderSettings()
			{
				ImageSize = RenderedImageSize.ResizeImage   // TODO: Theme colors, font, etc.
			};

			if (this.root is Model.Backgrounds.Layout2D Layout2D)
			{
				if (Layout2D.WidthAttribute.Defined || Layout2D.HeightAttribute.Defined)
				{
					DrawingState State = new DrawingState(null, Result, Session);

					EvaluationResult<Length> Width = await Attribute<Length>.TryEvaluate(Layout2D.WidthAttribute, Session);
					if (Width.Ok)
					{
						float w = Result.Width;
						State.CalcDrawingSize(Width.Result, ref w, true, this.root);
						Result.Width = (int)(w + 0.5f);
					}

					EvaluationResult<Length> Height = await Attribute<Length>.TryEvaluate(Layout2D.HeightAttribute, Session);
					if (Height.Ok)
					{
						float h = Result.Height;
						State.CalcDrawingSize(Height.Result, ref h, false, this.root);
						Result.Height = (int)(h + 0.5f);
					}
				}

				EvaluationResult<string> BackgroundId = await Attribute<string>.TryEvaluate(Layout2D.BackgroundColorAttribute, Session);
				if (BackgroundId.Ok &&
					this.TryGetElement(BackgroundId.Result, out ILayoutElement Element) &&
					Element is SolidBackground SolidBackground)
				{
					EvaluationResult<SKColor> Color = await Attribute<SKColor>.TryEvaluate(SolidBackground.ColorAttribute, Session);
					if (Color.Ok)
						Result.BackgroundColor = Color.Result;
				}
			}

			return Result;
		}

		/// <summary>
		/// Makes a copy of the layout document.
		/// </summary>
		/// <param name="Session">Session variables.</param>
		/// <returns>Copy of layout document.</returns>
		public Layout2DDocument Copy(Variables Session)
		{
			KeyValuePair<string, object>[] Attachments = new KeyValuePair<string, object>[this.attachments.Count];
			int i = 0;

			foreach (KeyValuePair<string, object> P in this.attachments)
				Attachments[i++] = P;

			Layout2DDocument Result = new Layout2DDocument(Session, Attachments)
			{
				root = this.root.Copy(null),
				Dynamic = this.Dynamic
			};

			Result.root.RegisterIDs(Session);

			return Result;
		}

		/// <summary>
		/// If the layout is dynamic (i.e. contains script).
		/// </summary>
		public bool Dynamic
		{
			get;
			internal set;
		}

		/// <summary>
		/// Exports the internal state of the layout.
		/// </summary>
		/// <returns>XML output</returns>
		public string ExportState()
		{
			return this.ExportState(XML.WriterSettings(true, true));
		}

		/// <summary>
		/// Exports the internal state of the layout.
		/// </summary>
		/// <param name="Settings">XML Writer settings.</param>
		/// <returns>XML output</returns>
		public string ExportState(XmlWriterSettings Settings)
		{
			StringBuilder Output = new StringBuilder();
			this.ExportState(Output, Settings);
			return Output.ToString();
		}

		/// <summary>
		/// Exports the internal state of the layout.
		/// </summary>
		/// <param name="Output">XML output</param>
		/// <param name="Settings">XML Writer settings.</param>
		public void ExportState(StringBuilder Output, XmlWriterSettings Settings)
		{
			using (XmlWriter w = XmlWriter.Create(Output, Settings))
			{
				this.ExportState(w);
				w.Flush();
			}
		}

		/// <summary>
		/// Exports the internal state of the layout.
		/// </summary>
		/// <param name="Output">XML output</param>
		public void ExportState(XmlWriter Output)
		{
			Output.WriteStartElement("Layout2DState", "http://waher.se/Schema/Layout2DState.xsd");
			this.root?.ExportState(Output);
			Output.WriteEndElement();
		}

	}

	/* TODO:
	 * 
     * Tree layout
     * Radix/circular
     * Directed graphs
     * Smart Art/Graphs/Layout
     * Mindmap (Example: https://bhavkaran.com/reconspider/mindmap.html)
     * 
     * Blur when too small, and dont continue rendering
     * Clip optimization
     * 
	 */

}
