using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;

namespace Waher.Networking.XMPP.PubSub
{
	/// <summary>
	/// Contains options for a node subscription
	/// </summary>
	public class SubscriptionOptions
	{
		private SubscriptionType? type = null;
		private SubscriptonDepth? depth = null;
		private bool? deliver = null;
		private bool? digest = null;
		private bool? includeBody = null;
		private int? digestFrequencyMilliseconds = null;
		private DateTime? expires = null;

		/// <summary>
		/// Contains options for a node subscription
		/// </summary>
		public SubscriptionOptions()
		{
		}

		/// <summary>
		/// Contains options for a node subscription
		/// </summary>
		public SubscriptionOptions(DataForm Form)
		{
			foreach (Field F in Form.Fields)
			{
				switch (F.Var)
				{
					case "pubsub#deliver":
						if (CommonTypes.TryParse(F.ValueString, out bool b))
							this.deliver = b;
						break;

					case "pubsub#digest":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.digest = b;
						break;

					case "pubsub#include_body":
						if (CommonTypes.TryParse(F.ValueString, out b))
							this.includeBody = b;
						break;

					case "pubsub#digest_frequency":
						if (int.TryParse(F.ValueString, out int i))
							this.digestFrequencyMilliseconds = i;
						break;

					case "pubsub#expire":
						if (DateTime.TryParse(F.ValueString, out DateTime TP))
							this.expires = TP;
						break;

					case "pubsub#subscription_type":
						if (Enum.TryParse(F.ValueString, out SubscriptionType SubscriptionType))
							this.type = SubscriptionType;
						break;

					case "pubsub#subscription_depth":
						if (F.ValueString == "1")
							this.depth = SubscriptonDepth.one;
						else if (Enum.TryParse(F.ValueString, out SubscriptonDepth SubscriptonDepth))
							this.depth = SubscriptonDepth;
						break;
				}
			}
		}

		/// <summary>
		/// Type of subscription
		/// </summary>
		public SubscriptionType? Type
		{
			get { return this.type; }
			set { this.type = value; }
		}

		/// <summary>
		/// Subscription depth
		/// </summary>
		public SubscriptonDepth? Depth
		{
			get { return this.depth; }
			set { this.depth = value; }
		}

		/// <summary>
		/// Whether an entity wants to receive or disable notifications
		/// </summary>
		public bool? Deliver
		{
			get { return this.deliver; }
			set { this.deliver = value; }
		}

		/// <summary>
		/// Whether an entity wants to receive digests (aggregations) of notifications or all notifications individually
		/// </summary>
		public bool? Digest
		{
			get { return this.digest; }
			set { this.digest = value; }
		}

		/// <summary>
		/// Whether an entity wants to receive an XMPP message body in addition to the payload format
		/// </summary>
		public bool? IncludeBody
		{
			get { return this.includeBody; }
			set { this.includeBody = value; }
		}

		/// <summary>
		/// The minimum number of milliseconds between sending any two notification digests
		/// </summary>
		public int? DigestFrequencyMilliseconds
		{
			get { return this.digestFrequencyMilliseconds; }
			set { this.digestFrequencyMilliseconds = value; }
		}

		/// <summary>
		/// The DateTime at which a leased subscription will end or has ended
		/// </summary>
		public DateTime? Expires
		{
			get { return this.expires; }
			set { this.expires = value; }
		}

		/// <summary>
		/// Creates a data form for subsmission.
		/// </summary>
		/// <param name="Client">XMPP Publish/Subscribe Client</param>
		/// <returns>Data form.</returns>
		public DataForm ToForm(PubSubClient Client)
		{
			List<Field> Fields = new List<Field>()
			{
				new HiddenField("FORM_TYPE", "http://jabber.org/protocol/pubsub#subscribe_options")
			};

			if (this.deliver.HasValue)
				Fields.Add(new BooleanField("pubsub#deliver", this.deliver.Value));

			if (this.digest.HasValue)
				Fields.Add(new BooleanField("pubsub#digest", this.digest.Value));

			if (this.includeBody.HasValue)
				Fields.Add(new BooleanField("pubsub#include_body", this.includeBody.Value));

			if (this.digestFrequencyMilliseconds.HasValue)
				Fields.Add(new TextSingleField("pubsub#digest_frequency", this.digestFrequencyMilliseconds.Value.ToString()));

			if (this.expires.HasValue)
				Fields.Add(new TextSingleField("pubsub#expire", XML.Encode(this.expires.Value)));

			if (this.type.HasValue)
				Fields.Add(new TextSingleField("pubsub#subscription_type", this.type.Value.ToString()));

			if (this.depth.HasValue)
				Fields.Add(new TextSingleField("pubsub#subscription_depth", this.depth.Value == SubscriptonDepth.one ? "1" : this.depth.Value.ToString()));

			return new DataForm(Client.Client, FormType.Form, Client.Client.FullJID, 
				Client.ComponentAddress, Fields.ToArray());
		}

	}
}
