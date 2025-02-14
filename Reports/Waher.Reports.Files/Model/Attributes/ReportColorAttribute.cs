using SkiaSharp;
using System;
using System.Xml;
using Waher.Script.Graphs;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report color attribute.
	/// </summary>
	public class ReportColorAttribute : ReportAttribute<SKColor>
	{
		/// <summary>
		/// Report color attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportColorAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">String representation</param>
		/// <returns>Parsed value.</returns>
		public override SKColor ParseValue(string s)
		{
			if (Graph.TryConvertToColor(s, out SKColor Result))
				return Result;
			else
				throw new ArgumentException("Invalid color value: " + s, nameof(s));
		}
	}
}
