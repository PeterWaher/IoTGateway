using System;

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
				if (this.elements is null)
					return false;

				foreach (InlineElement E in this.elements)
				{
					if (E is null || !E.IsWellDefined)
						return false;
				}

				return true;
			}
		}
	}
}
