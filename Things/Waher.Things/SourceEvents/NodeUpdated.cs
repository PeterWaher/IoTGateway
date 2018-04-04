using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node updated event.
	/// </summary>
	public class NodeUpdated : NodeParametersEvent
    {
		private string oldId = string.Empty;

		/// <summary>
		/// Node updated event.
		/// </summary>
		public NodeUpdated()
			: base()
		{
		}

		/// <summary>
		/// If renamed, this property contains the node identity before the node was renamed.
		/// </summary>
		[DefaultValueStringEmpty]
		public string OldId
		{
			get { return this.oldId; }
			set { this.oldId = value; }
		}
	}
}
