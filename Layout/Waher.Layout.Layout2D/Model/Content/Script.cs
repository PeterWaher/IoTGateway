using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Content;
using Waher.Events;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Layout.Layout2D.Model.Groups;
using Waher.Layout.Layout2D.Model.Images;
using Waher.Layout.Layout2D.Model.Transforms;
using Waher.Script;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Objects;
using Waher.Script.Operators.Matrices;

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
		private StringAttribute font;

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
		/// Font
		/// </summary>
		public StringAttribute FontAttribute
		{
			get => this.font;
			set => this.font = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.Document.Dynamic = true;

			this.expression = new ExpressionAttribute(Input, "expression", this.Document);
			this.halign = new EnumAttribute<HorizontalAlignment>(Input, "halign", this.Document);
			this.valign = new EnumAttribute<VerticalAlignment>(Input, "valign", this.Document);
			this.font = new StringAttribute(Input, "font", this.Document);

			return base.FromXml(Input);
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
			this.font?.Export(Output);
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
				Dest.expression = this.expression?.CopyIfNotPreset(Destination.Document);
				Dest.halign = this.halign?.CopyIfNotPreset(Destination.Document);
				Dest.valign = this.valign?.CopyIfNotPreset(Destination.Document);
				Dest.font = this.font?.CopyIfNotPreset(Destination.Document);
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

			if (this.evaluated is null)
			{
				EvaluationResult<Expression> Parsed = await this.expression.TryEvaluate(State.Session);
				if (Parsed.Ok)
				{
					object Result;

					this.parsed = Parsed.Result;

					try
					{
						Result = await this.parsed.EvaluateAsync(State.Session);
					}
					catch (ScriptReturnValueException ex)
					{
						Result = ex.ReturnValue;
						//ScriptReturnValueException.Reuse(ex);
					}
					catch (ScriptBreakLoopException ex)
					{
						Result = ex.LoopValue ?? ObjectValue.Null;
						//ScriptBreakLoopException.Reuse(ex);
					}
					catch (ScriptContinueLoopException ex)
					{
						Result = ex.LoopValue ?? ObjectValue.Null;
						//ScriptContinueLoopException.Reuse(ex);
					}
					catch (ScriptAbortedException)
					{
						State.Session.CancelAbort();
						Result = "Script execution aborted due to timeout.";
					}
					catch (Exception ex)
					{
						ex = Log.UnnestException(ex);
						Result = ex.Message;
					}

					if (!string.IsNullOrEmpty(this.cid))
					{
						await this.Document.DisposeContent(this.cid);
						this.cid = null;
					}

					if (Result is IToMatrix ToMatrix)
						Result = ToMatrix.ToMatrix();

					if (Result is Graph G)
					{
						PixelInformation Pixels = G.CreatePixels();
						this.cid = this.Document.AddContent(Pixels.CreateBitmap());
						this.Width = Pixels.Width;
						this.Height = Pixels.Height;
						this.evaluated = new ImageInternal(this.Document, this)
						{
							CidAttribute = new StringAttribute("cid", this.cid, this.Document),
							XAttribute = this.XAttribute,
							YAttribute = this.YAttribute,
							Width = Pixels.Width,
							Height = Pixels.Height
						};
					}
					else if (Result is SKImage Img)
					{
						this.cid = this.Document.AddContent(Img);
						this.Width = Img.Width;
						this.Height = Img.Height;
						this.evaluated = new ImageInternal(this.Document, this)
						{
							CidAttribute = new StringAttribute("cid", this.cid, this.Document),
							XAttribute = this.XAttribute,
							YAttribute = this.YAttribute,
							Width = Img.Width,
							Height = Img.Height
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
								TextAttribute = new StringAttribute("text", ex.Message, this.Document),
								FontAttribute = this.FontAttribute
							},
							new Margins(this.Document, Vertical)
							{
								TopAttribute = new LengthAttribute("top", new Length(1, LengthUnit.Em), this.Document)
							}
						};

						foreach (string Row in ex.StackTrace.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
						{
							Children.Add(new Label(this.Document, Vertical)
							{
								TextAttribute = new StringAttribute("text", Row, this.Document),
								FontAttribute = this.FontAttribute
							});
						}

						Vertical.Children = Children.ToArray();
						T.Children = new ILayoutElement[] { Vertical };

						this.evaluated = Vertical;
					}
					else
					{
						this.evaluated = new Label(this.Document, this)
						{
							TextAttribute = new StringAttribute("text", Result.ToString(), this.Document),
							XAttribute = this.XAttribute,
							YAttribute = this.YAttribute,
							HorizontalAlignmentAttribute = this.HorizontalAlignmentAttribute,
							VerticalAlignmentAttribute = this.VerticalAlignmentAttribute,
							FontAttribute = this.FontAttribute
						};
					}
				}
				else
					this.defined = false;
			}

			if (!(this.font is null) && this.fontRef is null)
			{
				EvaluationResult<string> FontId = await this.font.TryEvaluate(State.Session);

				if (FontId.Ok &&
					this.Document.TryGetElement(FontId.Result, out ILayoutElement E) &&
					E is Font Font)
				{
					this.fontRef = Font;
				}
			}

			if (!(this.evaluated is null))
			{
				FontState Bak = State.Push(this.fontRef);

				await this.evaluated.MeasureDimensions(State);

				State.Restore(Bak);

				this.Width = this.evaluated.Width;
				this.Height = this.evaluated.Height;
			}
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
				FontState Bak = State.Push(this.fontRef);
				this.evaluated.MeasurePositions(State);
				State.Restore(Bak);

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
		private Font fontRef = null;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			FontState Bak = State.Push(this.fontRef);

			if (!(this.evaluated is null))
				await this.evaluated.Draw(State);

			State.Restore(Bak);

			await base.Draw(State);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.expression?.ExportState(Output);
			this.halign?.ExportState(Output);
			this.valign?.ExportState(Output);
			this.font?.ExportState(Output);
		}
	}
}
