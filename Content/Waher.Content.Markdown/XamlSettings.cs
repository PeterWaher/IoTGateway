using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Contains settings that the XAML export uses to customize XAML output.
	/// </summary>
	public class XamlSettings
	{
		private string paragraphMargins = "0,5,0,5";
		private int paragraphMarginLeft = 0;
		private int paragraphMarginTop = 5;
		private int paragraphMarginRight = 0;
		private int paragraphMarginBottom = 5;

		private string blockQuoteBorderColor = "Black";
		private int blockQuoteBorderThickness = 5;
		private int blockQuoteMargin = 20;
		private int blockQuotePadding = 10;

		private int[] headerFontSize = new int[] { 28, 24, 22, 20, 18, 16, 15, 14, 13, 12 };
		private string[] headerForegroundColor = new string[] { "Navy", "Navy", "Navy", "Navy", "Navy", "Navy", "Navy", "Navy", "Navy", "Navy" };

		private int listContentMargin = 5;

		private string tableCellPadding = "5,2,5,2";
		private string tableCellBorderColor = "Gray";
		private string tableCellRowBackgroundColor1 = string.Empty;
		private string tableCellRowBackgroundColor2 = string.Empty;
		private int tableCellPaddingLeft = 5;
		private int tableCellPaddingTop = 2;
		private int tableCellPaddingRight = 5;
		private int tableCellPaddingBottom = 2;
		private double tableCellBorderThickness = 0.5;

		private int definitionSeparator = 10;
		private int definitionMargin = 20;

		private double superscriptScale = 0.75;
		private int superscriptOffset = -5;

		private int footnoteSeparator = 2;

		private int defaultGraphWidth = 480;
		private int defaultGraphHeight = 360;

		/// <summary>
		/// Contains settings that the XAML export uses to customize XAML output.
		/// </summary>
		public XamlSettings()
		{
		}

		private void Parse(string s, out int Left, out int Top, out int Right, out int Bottom)
		{
			string[] Parts = s.Split(',');
			if (Parts.Length != 4)
				throw new ArgumentException("Invalid Margins.", "s");

			int Left2 = int.Parse(Parts[0]);
			int Top2 = int.Parse(Parts[1]);
			int Right2 = int.Parse(Parts[2]);
			int Bottom2 = int.Parse(Parts[3]);

			Left = Left2;
			Top = Top2;
			Right = Right2;
			Bottom = Bottom2;
		}

		/// <summary>
		/// Paragraph margins.
		/// </summary>
		public string ParagraphMargins
		{
			get { return this.paragraphMargins; }
			set
			{
				this.Parse(value, out this.paragraphMarginLeft, out this.paragraphMarginTop, out this.paragraphMarginRight, out this.paragraphMarginBottom);
				this.paragraphMargins = value;
			}
		}

		private void UpdateParagraphMargins()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.paragraphMarginLeft);
			sb.Append(',');
			sb.Append(this.paragraphMarginTop);
			sb.Append(',');
			sb.Append(this.paragraphMarginRight);
			sb.Append(',');
			sb.Append(this.paragraphMarginBottom);

			this.paragraphMargins = sb.ToString();
		}

		/// <summary>
		/// Left margin for paragraphs.
		/// </summary>
		public int ParagraphMarginLeft
		{
			get { return this.paragraphMarginLeft; }
			set
			{
				this.paragraphMarginLeft = value;
				this.UpdateParagraphMargins();
			}
		}

		/// <summary>
		/// Top margin for paragraphs.
		/// </summary>
		public int ParagraphMarginTop
		{
			get { return this.paragraphMarginTop; }
			set
			{
				this.paragraphMarginTop = value;
				this.UpdateParagraphMargins();
			}
		}

		/// <summary>
		/// Right margin for paragraphs.
		/// </summary>
		public int ParagraphMarginRight
		{
			get { return this.paragraphMarginRight; }
			set
			{
				this.paragraphMarginRight = value;
				this.UpdateParagraphMargins();
			}
		}

		/// <summary>
		/// Bottom margin for paragraphs.
		/// </summary>
		public int ParagraphMarginBottom
		{
			get { return this.paragraphMarginBottom; }
			set
			{
				this.paragraphMarginBottom = value;
				this.UpdateParagraphMargins();
			}
		}

		/// <summary>
		/// Block Quote border color.
		/// 
		/// NOTE: Property is a string, to allow generation of XAML where access to WPF libraries is not available.
		/// </summary>
		public string BlockQuoteBorderColor
		{
			get { return this.blockQuoteBorderColor; }
			set { this.blockQuoteBorderColor = value; }
		}

		/// <summary>
		/// Block Quote border thickness
		/// </summary>
		public int BlockQuoteBorderThickness
		{
			get { return this.blockQuoteBorderThickness; }
			set { this.blockQuoteBorderThickness = value; }
		}

		/// <summary>
		/// Block Quote margin (from outside left margin to border)
		/// </summary>
		public int BlockQuoteMargin
		{
			get { return this.blockQuoteMargin; }
			set { this.blockQuoteMargin = value; }
		}

		/// <summary>
		/// Block Quote padding (from border to inside left margin)
		/// </summary>
		public int BlockQuotePadding
		{
			get { return this.blockQuotePadding; }
			set { this.blockQuotePadding = value; }
		}

		/// <summary>
		/// Header font sizes for different levels. Index corresponds to header level - 1.
		/// </summary>
		public int[] HeaderFontSize
		{
			get { return this.headerFontSize; }
		}

		/// <summary>
		/// Header foreground colors for different levels. Index corresponds to header level - 1.
		/// 
		/// NOTE: Property is an array of strings, to allow generation of XAML where access to WPF libraries is not available.
		/// </summary>
		public string[] HeaderForegroundColor
		{
			get { return this.headerForegroundColor; }
		}

		/// <summary>
		/// Margin between list item bullet and list item content.
		/// </summary>
		public int ListContentMargin
		{
			get { return this.listContentMargin; }
			set { this.listContentMargin = value; }
		}

		/// <summary>
		/// TableCell padding.
		/// </summary>
		public string TableCellPadding
		{
			get { return this.tableCellPadding; }
			set
			{
				this.Parse(value, out this.tableCellPaddingLeft, out this.tableCellPaddingTop, out this.tableCellPaddingRight, out this.tableCellPaddingBottom);
				this.tableCellPadding = value;
			}
		}

		private void UpdateTableCellPadding()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.tableCellPaddingLeft);
			sb.Append(',');
			sb.Append(this.tableCellPaddingTop);
			sb.Append(',');
			sb.Append(this.tableCellPaddingRight);
			sb.Append(',');
			sb.Append(this.tableCellPaddingBottom);

			this.tableCellPadding = sb.ToString();
		}

		/// <summary>
		/// Left padding for table cells.
		/// </summary>
		public int TableCellPaddingLeft
		{
			get { return this.tableCellPaddingLeft; }
			set
			{
				this.tableCellPaddingLeft = value;
				this.UpdateTableCellPadding();
			}
		}

		/// <summary>
		/// Top padding for table cells.
		/// </summary>
		public int TableCellPaddingTop
		{
			get { return this.tableCellPaddingTop; }
			set
			{
				this.tableCellPaddingTop = value;
				this.UpdateTableCellPadding();
			}
		}

		/// <summary>
		/// Right padding for table cells.
		/// </summary>
		public int TableCellPaddingRight
		{
			get { return this.tableCellPaddingRight; }
			set
			{
				this.tableCellPaddingRight = value;
				this.UpdateTableCellPadding();
			}
		}

		/// <summary>
		/// Bottom padding for table cells.
		/// </summary>
		public int TableCellPaddingBottom
		{
			get { return this.tableCellPaddingBottom; }
			set
			{
				this.tableCellPaddingBottom = value;
				this.UpdateTableCellPadding();
			}
		}

		/// <summary>
		/// Table cell border color.
		/// </summary>
		public string TableCellBorderColor
		{
			get { return this.tableCellBorderColor; }
			set { this.tableCellBorderColor = value; }
		}

		/// <summary>
		/// Table cell border thickness.
		/// </summary>
		public double TableCellBorderThickness
		{
			get { return this.tableCellBorderThickness; }
			set { this.tableCellBorderThickness = value; }
		}

		/// <summary>
		/// Optional background color for tables, odd row numbers.
		/// </summary>
		public string TableCellRowBackgroundColor1
		{
			get { return this.tableCellRowBackgroundColor1; }
			set { this.tableCellRowBackgroundColor1 = value; }
		}

		/// <summary>
		/// Optional background color for tables, even row numbers.
		/// </summary>
		public string TableCellRowBackgroundColor2
		{
			get { return this.tableCellRowBackgroundColor2; }
			set { this.tableCellRowBackgroundColor2 = value; }
		}

		/// <summary>
		/// Distance between definitions.
		/// </summary>
		public int DefinitionSeparator
		{
			get { return this.definitionSeparator; }
			set { this.definitionSeparator = value; }
		}

		/// <summary>
		/// Left margin for definitions.
		/// </summary>
		public int DefinitionMargin
		{
			get { return this.definitionMargin; }
			set { this.definitionMargin = value; }
		}

		/// <summary>
		/// Superscript scaling, compared to the normal font size.
		/// </summary>
		public double SuperscriptScale
		{
			get { return this.superscriptScale; }
			set { this.superscriptScale = value; }
		}

		/// <summary>
		/// Superscript vertical offset.
		/// </summary>
		public int SuperscriptOffset
		{
			get { return this.superscriptOffset; }
			set { this.superscriptOffset = value; }
		}

		/// <summary>
		/// Space between footnote and text in the footnote section.
		/// </summary>
		public int FootnoteSeparator
		{
			get { return this.footnoteSeparator; }
			set { this.footnoteSeparator = value; }
		}

		/// <summary>
		/// Default graph width
		/// </summary>
		public int DefaultGraphWidth
		{
			get { return this.defaultGraphWidth; }
			set { this.defaultGraphWidth = value; }
		}

		/// <summary>
		/// Default graph height
		/// </summary>
		public int DefaultGraphHeight
		{
			get { return this.defaultGraphHeight; }
			set { this.defaultGraphHeight = value; }
		}
	}
}
