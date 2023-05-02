using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.QR
{
	/// <summary>
	/// QR Code correction level.
	/// </summary>
	public enum CorrectionLevel
	{
		/// <summary>
		/// Low (7%)
		/// </summary>
		L = 0,

		/// <summary>
		/// Medium (15%)
		/// </summary>
		M = 1,

		/// <summary>
		/// Quartile (25%)
		/// </summary>
		Q = 2,

		/// <summary>
		/// High (30%)
		/// </summary>
		H = 3
	}

	/// <summary>
	/// QR Code encoding mode
	/// </summary>
	public enum EncodingMode
	{
		/// <summary>
		/// Numeric (0-9)
		/// </summary>
		Numeric = 0b0001,

		/// <summary>
		/// Alphanumeric (0-9, A-Z, space, $, %, *, +, -, ., /, and :
		/// </summary>
		Alphanumeric = 0b0010,

		/// <summary>
		/// Bytes, by default, from the ISO-8859-1 character set. (Some decoders can read UTF-8)
		/// </summary>
		Byte = 0b0100,

		/// <summary>
		/// Shift JIS character set
		/// </summary>
		Kanji = 0b1000,

		/// <summary>
		/// Extended Channel Interpretation, allows specification of character set
		/// </summary>
		Eci = 0b0111
	}

	/// <summary>
	/// Type of dot in code.
	/// </summary>
	public enum DotType
	{
		/// <summary>
		/// Code background color
		/// </summary>
		CodeBackground = 0,

		/// <summary>
		/// Code foreground color
		/// </summary>
		CodeForeground = 1,

		/// <summary>
		/// Finder Marker background color
		/// </summary>
		FinderMarkerBackground = 2,

		/// <summary>
		/// Finder Marker foreground color
		/// </summary>
		FinderMarkerForeground = 3,

		/// <summary>
		/// Alignment Marker background color
		/// </summary>
		AlignmentMarkerBackground = 4,

		/// <summary>
		/// Alignment Marker foreground color
		/// </summary>
		AlignmentMarkerForeground = 5
	}
}
