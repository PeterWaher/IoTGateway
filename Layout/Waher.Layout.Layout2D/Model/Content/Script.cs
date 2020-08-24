using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Content;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Groups;
using Waher.Layout.Layout2D.Model.Images;
using Waher.Layout.Layout2D.Model.Transforms;
using Waher.Script;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Vectors;
using Waher.Script.Graphs;
using Waher.Script.Objects.Matrices;

namespace Waher.Layout.Layout2D.Model.Content
{
	/// <summary>
	/// Represents the result of executing script.
	/// </summary>
	public class Script : Point
	{
		private ExpressionAttribute expression;
		private EnumAttribute<HorizontalAlignment> halign;
		private EnumAttribute<VerticalAlignment> valign;

		/// <summary>
		/// Represents the result of executing script.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Script(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.evaluated?.Dispose();
			this.evaluated = null;
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Script";

		/// <summary>
		/// Expression
		/// </summary>
		public ExpressionAttribute ExpressionAttribute
		{
			get => this.expression;
			set => this.expression = value;
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
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.expression = new ExpressionAttribute(Input, "expression");
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign");
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.expression?.Export(Output);
			this.halign?.Export(Output);
			this.valign?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Script(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Script Dest)
			{
				Dest.expression = this.expression?.CopyIfNotPreset();
				Dest.halign = this.halign?.CopyIfNotPreset();
				Dest.valign = this.valign?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool DoMeasureDimensions(DrawingState State)
		{
			bool Relative = base.DoMeasureDimensions(State);

			if (this.evaluated is null)
			{
				if (!(this.expression is null) && this.expression.TryEvaluate(State.Session, out this.parsed))
				{
					object Result;

					try
					{
						Result = this.parsed.Evaluate(State.Session);
					}
					catch (ScriptReturnValueException ex)
					{
						Result = ex.ReturnValue;
					}
					catch (ScriptAbortedException)
					{
						State.Session.CancelAbort();
						Result = "Script execution aborted due to timeout.";
					}
					catch (Exception ex)
					{
						Result = ex;
					}

					if (!string.IsNullOrEmpty(this.cid))
					{
						this.Document.DisposeContent(this.cid);
						this.cid = null;
					}

					if (Result is Graph G)
					{
						GraphSettings Settings = new GraphSettings();
						Tuple<int, int> Size;

						if ((Size = G.RecommendedBitmapSize) != null)
						{
							Settings.Width = Size.Item1;
							Settings.Height = Size.Item2;

							Settings.MarginLeft = (int)Math.Round(15.0 * Settings.Width / 640);
							Settings.MarginRight = Settings.MarginLeft;

							Settings.MarginTop = (int)Math.Round(15.0 * Settings.Height / 480);
							Settings.MarginBottom = Settings.MarginTop;
							Settings.LabelFontSize = 12.0 * Settings.Height / 480;
						}
						else
						{
							if (State.Session.TryGetVariable("GraphWidth", out Variable v) && v.ValueObject is double w && w >= 1)
							{
								Settings.Width = (int)Math.Round(w);
								Settings.MarginLeft = (int)Math.Round(15 * w / 640);
								Settings.MarginRight = Settings.MarginLeft;
							}
							else if (!State.Session.ContainsVariable("GraphWidth"))
								State.Session["GraphWidth"] = (double)Settings.Width;

							if (State.Session.TryGetVariable("GraphHeight", out v) && v.ValueObject is double h && h >= 1)
							{
								Settings.Height = (int)Math.Round(h);
								Settings.MarginTop = (int)Math.Round(15 * h / 480);
								Settings.MarginBottom = Settings.MarginTop;
								Settings.LabelFontSize = 12 * h / 480;
							}
							else if (!State.Session.ContainsVariable("GraphHeight"))
								State.Session["GraphHeight"] = (double)Settings.Height;
						}

						SKImage Bmp = G.CreateBitmap(Settings);
						this.cid = this.Document.AddContent(Bmp);
						this.evaluated = new ImageInternal(this.Document, this)
						{
							CidAttribute = new StringAttribute("cid", this.cid)
						};
					}
					else if (Result is SKImage Img)
					{
						this.cid = this.Document.AddContent(Img);
						this.evaluated = new ImageInternal(this.Document, this)
						{
							CidAttribute = new StringAttribute("cid", this.cid)
						};
					}
					else if (Result is Exception ex)
					{
						Translate T = new Translate(this.Document, this)
						{
							TranslateXAttribute = this.XAttribute,
							TranslateYAttribute = this.YAttribute
						};
						Vertical Vertical = new Vertical(this.Document, T);
						List<ILayoutElement> Children = new List<ILayoutElement>()
						{
							new Label(this.Document, Vertical)
							{
								TextAttribute = new StringAttribute("text", ex.Message)
							},
							new Margins(this.Document, Vertical)
							{
								TopAttribute = new LengthAttribute("top", new Length(1, LengthUnit.Em))
							}
						};

						foreach (string Row in ex.StackTrace.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
						{
							Children.Add(new Label(this.Document, Vertical)
							{
								TextAttribute = new StringAttribute("text", Row)
							});
						}

						Vertical.Children = Children.ToArray();
						T.Children = new ILayoutElement[] { Vertical };

						this.evaluated = Vertical;
					}
					else if (Result is ObjectMatrix M && M.ColumnNames != null)
					{
						// TODO
					}
					else
					{
						this.evaluated = new Label(this.Document, this)
						{
							TextAttribute = new StringAttribute("text", Result.ToString()),
							XAttribute = this.XAttribute,
							YAttribute = this.YAttribute,
							HorizontalAlignmentAttribute = this.HorizontalAlignmentAttribute,
							VerticalAlignmentAttribute = this.VerticalAlignmentAttribute
						};
					}
				}
				else
					this.defined = false;
			}

			if (!(this.evaluated is null))
			{
				if (this.evaluated.MeasureDimensions(State))
					Relative = true;

				this.Width = this.evaluated.Width;
				this.Height = this.evaluated.Height;
			}

			return Relative;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			base.MeasurePositions(State);

			if (!(this.evaluated is null))
			{
				this.evaluated.MeasurePositions(State);

				this.Left = this.evaluated.Left;
				this.Top = this.evaluated.Top;

				if (!this.Width.HasValue)
					this.Right = this.evaluated.Right;

				if (!this.Height.HasValue)
					this.Bottom = this.evaluated.Bottom;
			}
		}

		private Expression parsed;
		private ILayoutElement evaluated = null;
		private string cid = null;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			this.evaluated?.Draw(State);
			base.Draw(State);
		}
	}
}
