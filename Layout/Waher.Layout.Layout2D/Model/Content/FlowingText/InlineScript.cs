using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Runtime.Collections;
using Waher.Script;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents output from inline script in flowing text.
	/// </summary>
	public class InlineScript : LayoutElement, IFlowingText
	{
		private ExpressionAttribute expression;

		/// <summary>
		/// Represents output from inline script in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public InlineScript(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "InlineScript";

		/// <summary>
		/// Expression
		/// </summary>
		public ExpressionAttribute ExpressionAttribute
		{
			get => this.expression;
			set => this.expression = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.expression = new ExpressionAttribute(Input, "expression", this.Document);
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
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new InlineScript(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is InlineScript Dest)
				Dest.expression = this.expression?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public async Task MeasureSegments(ChunkedList<Segment> Segments, DrawingState State)
		{
			EvaluationResult<Expression> Parsed = await this.expression.TryEvaluate(State.Session);
			if (Parsed.Ok)
			{
				object Result;

				try
				{
					Result = await Parsed.Result.EvaluateAsync(State.Session);
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
					Result = FirstRow(ex.Message);
				}

				string s = Result?.ToString();

				if (!string.IsNullOrEmpty(s))
					Text.AddSegments(Segments, s, State);
			}
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.expression?.ExportState(Output);
		}
	}
}
