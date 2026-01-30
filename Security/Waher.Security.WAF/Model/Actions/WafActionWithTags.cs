using System.Xml;
using Waher.Runtime.Collections;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Abstract base class for WAF actions that has tags.
	/// </summary>
	public abstract class WafActionWithTags : WafActions
	{
		private readonly Tag[] tags;

		/// <summary>
		/// Abstract base class for WAF actions that has tags.
		/// </summary>
		public WafActionWithTags()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for WAF actions that has tags.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public WafActionWithTags(XmlElement Xml)
			: base(Xml)
		{
			ChunkedList<Tag> Tags = new ChunkedList<Tag>();

			foreach (WafAction Action in this.Actions)
			{
				if (Action is Tag Tag)
					Tags.Add(Tag);
			}

			this.tags = Tags.ToArray();
		}

		/// <summary>
		/// Tags
		/// </summary>
		public Tag[] Tags => this.tags;
	}
}
