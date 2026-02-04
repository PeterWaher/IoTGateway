using System.Collections.Generic;
using System.Threading.Tasks;
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
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActionWithTags(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
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

		/// <summary>
		/// Evaluates available tags.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Array of evaluated tags.</returns>
		public async Task<KeyValuePair<string, object>[]> EvaluateTags(ProcessingState State)
		{
			ChunkedList<KeyValuePair<string, object>> Result = new ChunkedList<KeyValuePair<string, object>>();

			foreach (Tag Tag in this.tags)
				Result.Add(await Tag.EvaluateTag(State));

			return Result.ToArray();
		}
	}
}
