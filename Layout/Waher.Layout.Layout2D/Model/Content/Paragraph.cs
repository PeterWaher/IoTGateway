using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Content.FlowingText;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Layout.Layout2D.Model.Groups;
using Waher.Script.Graphs.Functions.Canvas;

namespace Waher.Layout.Layout2D.Model.Content
{
	/// <summary>
	/// Represents a paragraph of flowing text.
	/// </summary>
	public class Paragraph : Point
	{
		private IFlowingText[] text;
		private StringAttribute font;
		private EnumAttribute<HorizontalAlignment> halign;
		private EnumAttribute<VerticalAlignment> valign;

		/// <summary>
		/// Represents a paragraph of flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Paragraph(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Paragraph";

		/// <summary>
		/// Font
		/// </summary>
		public StringAttribute FontAttribute
		{
			get => this.font;
			set => this.font = value;
		}

		/// <summary>
		/// Degrees
		/// </summary>
		public EnumAttribute<HorizontalAlignment> HorizontalAlignmentAttribute
		{
			get => this.halign;
			set => this.halign = value;
		}

		/// <summary>
		/// Degrees
		/// </summary>
		public EnumAttribute<VerticalAlignment> VerticalAlignmentAttribute
		{
			get => this.valign;
			set => this.valign = value;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.text is null))
			{
				foreach (ILayoutElement E in this.text)
					E.Dispose();
			}
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override async Task FromXml(XmlElement Input)
		{
			await base.FromXml(Input);

			this.font = new StringAttribute(Input, "font");
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign");
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign");

			List<IFlowingText> Children = new List<IFlowingText>();

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					ILayoutElement Child = await this.Document.CreateElement(E, this);

					if (Child is IFlowingText Text)
						Children.Add(Text);
					else
						throw new LayoutSyntaxException("Not flowing text: " + E.NamespaceURI + "#" + E.LocalName);
				}
			}

			this.text = Children.ToArray();
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.font?.Export(Output);
			this.halign?.Export(Output);
			this.valign?.Export(Output);
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			if (!(this.text is null))
			{
				foreach (ILayoutElement Child in this.text)
					Child.ToXml(Output);
			}
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Paragraph(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Paragraph Dest)
			{
				Dest.font = this.font?.CopyIfNotPreset();
				Dest.halign = this.halign?.CopyIfNotPreset();
				Dest.valign = this.valign?.CopyIfNotPreset();

				if (!(this.text is null))
				{
					int i, c = this.text.Length;

					IFlowingText[] Children = new IFlowingText[c];

					for (i = 0; i < c; i++)
						Children[i] = this.text[i].Copy(Dest) as IFlowingText;

					Dest.text = Children;
				}
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			this.halignment = await this.halign.Evaluate(State.Session, HorizontalAlignment.Left);
			this.valignment = await this.valign.Evaluate(State.Session, VerticalAlignment.Top);

			if (!(this.font is null) && this.fontRef is null)
			{
				EvaluationResult<string> FontId = await this.font.TryEvaluate(State.Session);

				if (FontId.Ok && this.Document.TryGetElement(FontId.Result, out ILayoutElement E) && E is Font Font)
					this.fontRef = Font;
			}

			List<Segment> Segments = new List<Segment>();
			SKFont FontBak = State.Font;
			SKPaint TextBak = State.Text;
			float LineHeight = 0f;
			float LineHeightRel = Font.DefaultLineHeightRelative;
			bool LineHeightExplicit = false;

			if (!(this.fontRef is null))
			{
				State.Font = this.fontRef.FontDef;
				State.Text = this.fontRef.Text;
				LineHeight = this.fontRef.LineHeight;
				LineHeightRel = LineHeight / State.Font.Size;
				LineHeightExplicit = this.fontRef.LineHeightExplicit;
			}

			if (!(this.text is null))
			{
				foreach (IFlowingText TextItem in this.text)
					await TextItem.MeasureSegments(Segments, State);
			}

			if (!(this.fontRef is null))
			{
				State.Font = FontBak;
				State.Text = TextBak;
			}

			if (!(this.segments is null))
			{
				foreach (Segment Segment in this.segments)
					Segment.Dispose();

				this.segments = null;
			}

			this.segments = Segments.ToArray();

			List<Row> Rows = new List<Row>();
			Row Row;
			float RowWidth = 0f;
			float RowTop = float.MaxValue;
			float RowBottom = float.MinValue;
			float RowDeltaY = 0f;
			float LastSpaceWidth = 0f;
			float AreaWidth;
			float ParagraphWidth = 0f;
			bool EmptyRow = true;
			bool HasRows = false;

			if (this.Width.HasValue)
				AreaWidth = this.Width.Value;
			else if (this.Parent.PotentialWidth.HasValue)
			{
				AreaWidth = this.Parent.PotentialWidth.Value;

				if (this.MaxWidth.HasValue && this.MaxWidth.Value < AreaWidth)
					AreaWidth = this.MaxWidth.Value;
				else if (this.MinWidth.HasValue && this.MinWidth.Value > AreaWidth)
					AreaWidth = this.MinWidth.Value;
			}
			else if (this.MaxWidth.HasValue)
				AreaWidth = this.MaxWidth.Value;
			else if (this.MinWidth.HasValue)
				AreaWidth = this.MinWidth.Value;
			else
				AreaWidth = float.MaxValue;

			Segments.Clear();

			foreach (Segment Segment in this.segments)
			{
				if (EmptyRow && string.IsNullOrEmpty(Segment.Text))
					continue;

				if (!EmptyRow && RowWidth + LastSpaceWidth + Segment.Width > AreaWidth)
				{
					Rows.Add(Row = new Row()
					{
						Segments = Segments.ToArray(),
						Bounds = new SKRect(0, RowTop + RowDeltaY, RowWidth, RowBottom + RowDeltaY)
					});

					foreach (Segment Segment2 in Row.Segments)
						Segment2.TranslateY(RowTop - Segment2.Top);

					HasRows = true;
					if (RowWidth > ParagraphWidth)
						ParagraphWidth = RowWidth;

					if (LineHeightExplicit && LineHeight > 0)
						RowDeltaY += LineHeight;
					else
						RowDeltaY += Row.Height * LineHeightRel;

					Segments.Clear();
					RowWidth = 0f;
					LastSpaceWidth = 0f;
					RowTop = float.MaxValue;
					RowBottom = float.MinValue;
				}

				Segments.Add(Segment);
				EmptyRow = false;
				RowWidth += Segment.Width + LastSpaceWidth;
				LastSpaceWidth = Segment.SpaceWidth;

				if (Segment.Top < RowTop)
					RowTop = Segment.Top;

				if (Segment.Bottom > RowBottom)
					RowBottom = Segment.Bottom;
			}

			if (!EmptyRow)
			{
				Rows.Add(Row = new Row()
				{
					Segments = Segments.ToArray(),
					Bounds = new SKRect(0, RowTop + RowDeltaY, RowWidth, RowBottom + RowDeltaY)
				});

				foreach (Segment Segment2 in Row.Segments)
					Segment2.TranslateY(RowTop - Segment2.Top);

				HasRows = true;
				if (RowWidth > ParagraphWidth)
					ParagraphWidth = RowWidth;
			}

			if (HasRows)
				this.bounds = new SKRect(0, Rows[0].Top, ParagraphWidth, Rows[Rows.Count - 1].Bottom);
			else
				this.bounds = new SKRect(0, 0, 0, 0);

			if (!this.Width.HasValue)
				this.Width = this.bounds.Width;

			if (!this.Height.HasValue)
				this.Height = this.bounds.Height;

			if (!(this.rows is null))
			{
				foreach (Row Row2 in this.rows)
					Row2.Dispose();

				this.rows = null;
			}

			this.rows = Rows.ToArray();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			base.MeasurePositions(State);

			if (this.defined)
			{
				switch (this.halignment)
				{
					case HorizontalAlignment.Left:
					default:
						this.Left = this.xCoordinate + this.bounds.Left;
						break;

					case HorizontalAlignment.Center:
						this.xCoordinate -= this.bounds.Width / 2;
						this.Left = this.xCoordinate;
						break;

					case HorizontalAlignment.Right:
						this.xCoordinate -= this.bounds.Width;
						this.Left = this.xCoordinate;
						break;
				}

				switch (this.valignment)
				{
					case VerticalAlignment.Top:
					default:
						this.yCoordinate -= this.bounds.Top;
						this.Top = this.yCoordinate;
						break;

					case VerticalAlignment.Center:
						this.yCoordinate -= this.bounds.Top;
						this.yCoordinate -= this.bounds.Height / 2;
						this.Top = this.yCoordinate + this.bounds.Top + this.bounds.Height / 2;
						break;

					case VerticalAlignment.BaseLine:
						this.Top = this.yCoordinate + this.bounds.Top;
						break;

					case VerticalAlignment.Bottom:
						this.yCoordinate -= this.bounds.Top;
						this.yCoordinate -= this.bounds.Height;
						this.Top = this.yCoordinate + this.bounds.Top;
						break;
				}
			}
		}

		private HorizontalAlignment halignment;
		private VerticalAlignment valignment;
		private Font fontRef = null;
		private Segment[] segments = null;
		private Row[] rows = null;
		private SKRect bounds = new SKRect();

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (this.defined)
			{
				SKPaint HorizontalLine = null;

				foreach (Row Row in this.rows)
				{
					float dx = 0f;
					float LastX = 0f;
					float x, y;
					float? LastLinePos = null;

					foreach (Segment Segment in Row.Segments)
					{
						x = this.xCoordinate + dx;
						y = this.yCoordinate + Row.Top - Segment.Top + Segment.DeltaY;

						State.Canvas.DrawText(Segment.Text, x, y, Segment.Font, Segment.Paint);

						float x1 = x;
						x += Segment.Width;

						if (Segment.LinePos.HasValue)
						{
							float y1 = Segment.LinePos.Value;

							if (LastLinePos.HasValue && LastLinePos.Value == y1)
								x1 = LastX;

							LastLinePos = y1;
							y1 += y;

							if (HorizontalLine is null)
							{
								HorizontalLine = State.Text.Clone();
								HorizontalLine.StrokeWidth = State.Font.Size / 16;
							}

							State.Canvas.DrawLine(x1, y1, x, y1, HorizontalLine);
						}
						else
							LastLinePos = null;

						LastX = x;
						dx += Segment.Width + Segment.SpaceWidth;
					}
				}
			}

			await base.Draw(State);
		}

	}
}
