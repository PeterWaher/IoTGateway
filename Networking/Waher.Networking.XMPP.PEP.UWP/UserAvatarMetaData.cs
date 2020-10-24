using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Security;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// User Avatar Metadata event, as defined in XEP-0084:
	/// https://xmpp.org/extensions/xep-0084.html
	/// </summary>
	public class UserAvatarMetaData : PersonalEvent
	{
		private UserAvatarReference[] references = null;
		private XmlElement[] pointers = null;

		/// <summary>
		/// User Avatar Metadata event, as defined in XEP-0084:
		/// https://xmpp.org/extensions/xep-0084.html
		/// </summary>
		public UserAvatarMetaData()
		{
		}

		/// <summary>
		/// User Avatar Metadata event, as defined in XEP-0084:
		/// https://xmpp.org/extensions/xep-0084.html
		/// </summary>
		/// <param name="References">User Avatar references.</param>
		public UserAvatarMetaData(UserAvatarReference[] References)
			: this(References, null)
		{
		}

		/// <summary>
		/// User Avatar Metadata event, as defined in XEP-0084:
		/// https://xmpp.org/extensions/xep-0084.html
		/// </summary>
		/// <param name="References">User Avatar references.</param>
		/// <param name="Pointers">User Avatar pointers.</param>
		public UserAvatarMetaData(UserAvatarReference[] References, XmlElement[] Pointers)
		{
			this.references = References;
			this.pointers = Pointers;
		}

		/// <summary>
		/// Local name of the event element.
		/// </summary>
		public override string LocalName => "metadata";

		/// <summary>
		/// Namespace of the event element.
		/// </summary>
		public override string Namespace => "urn:xmpp:avatar:metadata";

		/// <summary>
		/// XML representation of the event.
		/// </summary>
		public override string PayloadXml
		{
			get
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<metadata xmlns='");
				Xml.Append(this.Namespace);
				Xml.Append("'>");

				if (!(this.references is null))
				{
					foreach (UserAvatarReference Reference in this.references)
					{
						Xml.Append("<info bytes='");
						Xml.Append(Reference.Bytes.ToString());

						if (Reference.Height != 0)
						{
							Xml.Append("' height='");
							Xml.Append(Reference.Height.ToString());
						}

						Xml.Append("' id='");
						Xml.Append(XML.Encode(Reference.Id));
						Xml.Append("' type='");
						Xml.Append(XML.Encode(Reference.Type));

						if (Reference.Width != 0)
						{
							Xml.Append("' width='");
							Xml.Append(Reference.Width.ToString());
						}

						Xml.Append("'/>");
					}
				}

				if (!(this.pointers is null))
				{
					foreach (XmlElement Pointer in this.pointers)
					{
						Xml.Append("<pointer>");
						Xml.Append(Pointer.OuterXml);
						Xml.Append("</pointer>");
					}
				}

				Xml.Append("</metadata>");

				return Xml.ToString();
			}
		}

		/// <summary>
		/// Parses a personal event from its XML representation
		/// </summary>
		/// <param name="E">XML representation of personal event.</param>
		/// <returns>Personal event object.</returns>
		public override IPersonalEvent Parse(XmlElement E)
		{
			List<UserAvatarReference> References = new List<UserAvatarReference>();
			List<XmlElement> Pointers = new List<XmlElement>();

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2)
				{
					switch (E2.LocalName)
					{
						case "info":
							References.Add(new UserAvatarReference()
							{
								Bytes = XML.Attribute(E2, "bytes", 0),
								Height = XML.Attribute(E2, "height", 0),
								Id = XML.Attribute(E2, "id"),
								Type = XML.Attribute(E2, "type"),
								Width = XML.Attribute(E2, "width", 0)
							});
							break;

						case "pointer":
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3)
									Pointers.Add(E3);
							}
							break;
					}
				}
			}

			return new UserAvatarMetaData()
			{
				references = References.ToArray(),
				pointers = Pointers.ToArray()
			};
		}

		/// <summary>
		/// User Avatar references.
		/// </summary>
		public UserAvatarReference[] References => this.references;

		/// <summary>
		/// User Avatar pointers.
		/// </summary>
		public XmlElement[] Pointers => this.pointers;
	}
}
