namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Abstract base class of blocks with inline elements.
	/// </summary>
	public abstract class InlineBlock : BlockElement
	{
		private HumanReadableElement[] elements;

		/// <summary>
		/// Inline elements
		/// </summary>
		public HumanReadableElement[] Elements
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

				foreach (HumanReadableElement E in this.elements)
				{
					if (E is null || !E.IsWellDefined)
						return false;
				}

				return true;
			}
		}
	}
}
