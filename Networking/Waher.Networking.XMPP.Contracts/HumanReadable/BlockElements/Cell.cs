using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Horizontal alignment of contents in a cell.
	/// </summary>
	public enum CellAlignment
	{
		/// <summary>
		/// To the left.
		/// </summary>
		Left,

		/// <summary>
		/// To the right
		/// </summary>
		Right,

		/// <summary>
		/// In the center
		/// </summary>
		Center
	}

	/// <summary>
	/// A cell in a table row
	/// </summary>
	public class Cell : Item
	{
		/// <summary>
		/// If cell represents a header (true) or a normal cell (false).
		/// </summary>
		public bool Header { get; set; }

		/// <summary>
		/// How many columns the cell spans.
		/// </summary>
		public int ColumnSpan { get; set; }

		/// <summary>
		/// Horizontal alignment of contents in a cell.
		/// </summary>
		public CellAlignment Alignment { get; set; }

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override Task<bool> IsWellDefined()
		{
			if (this.ColumnSpan <= 0)
				return Task.FromResult(false);

			return base.IsWellDefined();
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<cell header='");
			Xml.Append(CommonTypes.Encode(this.Header));
			Xml.Append("' colSpan='");
			Xml.Append(this.ColumnSpan.ToString());
			Xml.Append("' alignment='");
			Xml.Append(this.Alignment.ToString());
			Xml.Append("'>");

			HumanReadableElement[] Elements = (HumanReadableElement[])this.InlineElements ?? this.BlockElements;

			if (!(Elements is null))
			{
				foreach (HumanReadableElement E in Elements)
					E.Serialize(Xml);
			}

			Xml.Append("</cell>");
		}

	}
}
