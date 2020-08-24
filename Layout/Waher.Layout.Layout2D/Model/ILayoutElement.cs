using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Base interface for all layout elements.
	/// </summary>
	public interface ILayoutElement : IDisposable
	{
		/// <summary>
		/// Layout document.
		/// </summary>
		Layout2DDocument Document
		{
			get;
		}

		/// <summary>
		/// Parent element.
		/// </summary>
		ILayoutElement Parent
		{
			get;
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		string LocalName
		{
			get;
		}

		/// <summary>
		/// Namespace of type of element.
		/// </summary>
		string Namespace
		{
			get;
		}

		/// <summary>
		/// Left coordinate of bounding box, after measurement.
		/// </summary>
		float? Left
		{
			get;
			set;
		}

		/// <summary>
		/// Right coordinate of bounding box, after measurement.
		/// </summary>
		float? Right
		{
			get;
			set;
		}

		/// <summary>
		/// Top coordinate of bounding box, after measurement.
		/// </summary>
		float? Top
		{
			get;
			set;
		}

		/// <summary>
		/// Bottom coordinate of bounding box, after measurement.
		/// </summary>
		float? Bottom
		{
			get;
			set;
		}

		/// <summary>
		/// Width of element
		/// </summary>
		float? Width
		{
			get;
			set;
		}

		/// <summary>
		/// Height of element
		/// </summary>
		float? Height
		{
			get;
			set;
		}

		/// <summary>
		/// Inner Width of element
		/// </summary>
		float? InnerWidth
		{
			get;
		}

		/// <summary>
		/// Inner Height of element
		/// </summary>
		float? InnerHeight
		{
			get;
		}

		/// <summary>
		/// Explicit set width of element, if defined.
		/// </summary>
		float? ExplicitWidth
		{
			get;
			set;
		}

		/// <summary>
		/// Explicit set height of element, if defined.
		/// </summary>
		float? ExplicitHeight
		{
			get;
			set;
		}

		/// <summary>
		/// If the element is visible or not.
		/// </summary>
		bool IsVisible
		{
			get;
		}

		/// <summary>
		/// ID Attribute
		/// </summary>
		StringAttribute IdAttribute
		{
			get;
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Containing document.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent);

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		void FromXml(XmlElement Input);

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		void ToXml(XmlWriter Output);

		/// <summary>
		/// Creates a copy of the layout element.
		/// </summary>
		/// <param name="Parent">Parent of the new element.</param>
		/// <returns>Copy of layout element.</returns>
		ILayoutElement Copy(ILayoutElement Parent);

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		void CopyContents(ILayoutElement Destination);

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// Calls, in order: <see cref="BeforeMeasureDimensions(DrawingState)"/>,
		/// <see cref="DoMeasureDimensions(DrawingState)"/> and
		/// <see cref="AfterMeasureDimensions(DrawingState, ref bool)"/>.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		bool MeasureDimensions(DrawingState State);

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		bool DoMeasureDimensions(DrawingState State);

		/// <summary>
		/// Called before dimensions are measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		void BeforeMeasureDimensions(DrawingState State);

		/// <summary>
		/// Called when dimensions have been measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="Relative">If layout contains relative sizes and dimensions should be recalculated.</param>
		void AfterMeasureDimensions(DrawingState State, ref bool Relative);

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		void MeasurePositions(DrawingState State);

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		void Draw(DrawingState State);

		/// <summary>
		/// Draw the shape represented by the layout element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		void DrawShape(DrawingState State);
	}
}
