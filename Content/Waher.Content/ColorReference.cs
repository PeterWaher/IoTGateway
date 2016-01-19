using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content
{
	/// <summary>
	/// Color reference. Separate class to avoid reference to Windows Forms or WPF libraries.
	/// </summary>
	public class ColorReference
	{
		private byte red;
		private byte green;
		private byte blue;
		private byte alpha;
		private bool hasAlpha;

		/// <summary>
		/// Color reference. Separate class to avoid reference to Windows Forms or WPF libraries.
		/// </summary>
		/// <param name="Red">Red component.</param>
		/// <param name="Green">Green component.</param>
		/// <param name="Blue">Blue component.</param>
		public ColorReference(byte Red, byte Green, byte Blue)
		{
			this.red = Red;
			this.green = Green;
			this.blue = Blue;
			this.alpha = 0xff;
			this.hasAlpha = false;
		}

		/// <summary>
		/// Color reference. Separate class to avoid reference to Windows Forms or WPF libraries.
		/// </summary>
		/// <param name="Red">Red component.</param>
		/// <param name="Green">Green component.</param>
		/// <param name="Blue">Blue component.</param>
		/// <param name="Alpha">Alpha component.</param>
		public ColorReference(byte Red, byte Green, byte Blue, byte Alpha)
		{
			this.red = Red;
			this.green = Green;
			this.blue = Blue;
			this.alpha = Alpha;
			this.hasAlpha = true;
		}

		/// <summary>
		/// Red component.
		/// </summary>
		public byte Red { get { return this.red; } }

		/// <summary>
		/// Green component.
		/// </summary>
		public byte Green { get { return this.green; } }
		
		/// <summary>
		/// Blue component.
		/// </summary>
		public byte Blue { get { return this.blue; } }
		
		/// <summary>
		/// Alpha component.
		/// </summary>
		public byte Alpha { get { return this.alpha; } }

		/// <summary>
		/// If the <see cref="Alpha"/> component was explicitly specified.
		/// </summary>
		public bool HasAlpha { get { return this.hasAlpha; } }
	}
}
