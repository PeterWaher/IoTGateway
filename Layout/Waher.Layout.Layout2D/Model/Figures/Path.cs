using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Figures.SegmentNodes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A path
	/// </summary>
	public class Path : Figure
	{
		private ISegment[] segments;

		/// <summary>
		/// A path
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Path(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Path";

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.segments is null))
			{
				foreach (ILayoutElement E in this.segments)
					E.Dispose();
			}
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			List<ISegment> Segments = null;

			foreach (ILayoutElement Child in this.Children)
			{
				if (Child is ISegment Segment)
				{
					if (Segments is null)
						Segments = new List<ISegment>();

					Segments.Add(Segment);
				}
				else
					throw new LayoutSyntaxException("Not a segment type: " + Child.Namespace + "#" + Child.LocalName);
			}

			this.segments = Segments?.ToArray();
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Path(Document, Parent);
		}

	}
}
