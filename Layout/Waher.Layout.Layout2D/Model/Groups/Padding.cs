using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Provides padding for a layout element in a group contruct.
	/// </summary>
	public class Padding
	{
		private readonly ILayoutElement element;
		private readonly Cell cell;
		private readonly bool isCell;
		private float offsetX;
		private float offsetY;

		/// <summary>
		/// Provides padding for a cell in a group contruct.
		/// </summary>
		/// <param name="Element">Embedded element.</param>
		/// <param name="OffsetX">X-offset</param>
		/// <param name="OffsetY">Y-offset</param>
		public Padding(ILayoutElement Element, float OffsetX, float OffsetY)
		{
			this.element = Element;
			this.cell = Element as Cell;
			this.isCell = !(this.cell is null);
			this.offsetX = OffsetX;
			this.offsetY = OffsetY;
		}

		/// <summary>
		/// Embedded element
		/// </summary>
		public ILayoutElement Element => this.element;

		/// <summary>
		/// Embedded cell
		/// </summary>
		public Cell Cell => this.cell;

		/// <summary>
		/// If the embedded element is a <see cref="Cell"/>.
		/// </summary>
		public bool IsCell => this.isCell;

		/// <summary>
		/// X-offset
		/// </summary>
		public float OffsetX
		{
			get => this.offsetX;
			set => this.offsetX = value;
		}

		/// <summary>
		/// Y-offset
		/// </summary>
		public float OffsetY
		{
			get => this.offsetY;
			set => this.offsetY = value;
		}

		/// <summary>
		/// Aligns a measured cell
		/// </summary>
		/// <param name="MaxWidth">Maximum width of area assigned to the cell</param>
		/// <param name="MaxHeight">Maximum height of area assigned to the cell</param>
		/// <param name="Session">Current session.</param>
		public void AlignedMeasuredCell(float? MaxWidth, float? MaxHeight, Variables Session)
		{
			if (this.isCell)
				this.cell.AlignedMeasuredCell(MaxWidth, MaxHeight, Session);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return "(" + this.offsetX.ToString() + ", " + this.offsetY.ToString() + "): " + this.element.ToString();
		}
	}
}
