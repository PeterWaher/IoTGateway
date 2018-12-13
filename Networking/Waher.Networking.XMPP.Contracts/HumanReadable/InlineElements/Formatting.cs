using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Abstract base class for inline formatting elements.
	/// </summary>
	public abstract class Formatting : InlineElement
	{
		private InlineElement[] elements;

		/// <summary>
		/// Embedded elements
		/// </summary>
		public InlineElement[] Elements
		{
			get => this.elements;
			set => this.elements = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				foreach (InlineElement E in this.elements)
				{
					if (E == null || !E.IsWellDefined)
						return false;
				}

				return true;
			}
		}
	}
}
