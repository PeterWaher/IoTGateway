using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Represents an item in a web menu.
	/// </summary>
	public class WebMenuItem
	{
		private readonly string title;
		private readonly string url;

		/// <summary>
		/// Represents an item in a web menu.
		/// </summary>
		/// <param name="Title">Visible menu item title.</param>
		/// <param name="Url">URL to navigate to if item is selected.</param>
		public WebMenuItem(string Title, string Url)
		{
			this.title = Title;
			this.url = Url;
		}

		/// <summary>
		/// Displayable title.
		/// </summary>
		public string Title => this.title;

		/// <summary>
		/// URL to navigate to when item has been selected.
		/// </summary>
		public string Url => this.url;
	}
}
