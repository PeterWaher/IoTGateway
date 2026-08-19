using System.Collections.Generic;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Identity application service reference.
	/// </summary>
	public abstract class IdentityApplicationService
	{
		private readonly string id;
		private readonly string name;
		private readonly string fullName;
		private readonly string iconUrl;
		private readonly int iconWidth;
		private readonly int iconHeight;
		private readonly Dictionary<string, bool> properties;
		private readonly Dictionary<string, bool> attachments;

		/// <summary>
		/// Identity application service reference.
		/// </summary>
		/// <param name="Id">ID of service</param>
		/// <param name="Name">Name of service</param>
		/// <param name="FullName">Fully qualified name of service</param>
		/// <param name="IconUrl">URL of service icon</param>
		/// <param name="IconWidth">Width of service icon</param>
		/// <param name="IconHeight">Height of service icon</param>
		/// <param name="Properties">Properties reviewed by service, and if they are
		/// required (true) or optional (false)</param>
		/// <param name="Attachments">Attachments reviewed by service, and if they are
		/// required (true) or optional (false)</param>
		public IdentityApplicationService(string Id, string Name, string FullName,
			string IconUrl, int IconWidth, int IconHeight,
			Dictionary<string, bool> Properties, Dictionary<string, bool> Attachments)
		{
			this.id = Id;
			this.name = Name;
			this.fullName = FullName;
			this.iconUrl = IconUrl;
			this.iconWidth = IconWidth;
			this.iconHeight = IconHeight;
			this.properties = Properties;
			this.attachments = Attachments;
		}

		/// <summary>
		/// ID of service
		/// </summary>
		public string Id => this.id;

		/// <summary>
		/// Name of service
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Fully qualified name of service
		/// </summary>
		public string FullName => this.fullName;

		/// <summary>
		/// URL of service icon
		/// </summary>
		public string IconUrl => this.iconUrl;

		/// <summary>
		/// Width of service icon
		/// </summary>
		public int IconWidth => this.iconWidth;

		/// <summary>
		/// Height of service icon
		/// </summary>
		public int IconHeight => this.iconHeight;

		/// <summary>
		/// Properties reviewed by service, and if they are required (true) or 
		/// optional (false)
		/// </summary>
		public Dictionary<string, bool> Properties => this.properties;

		/// <summary>
		/// Attachments reviewed by service, and if they are required (true) or
		/// optional (false)
		/// </summary>
		public Dictionary<string, bool> Attachments => this.attachments;

		/// <summary>
		/// Required properties.
		/// </summary>
		public IEnumerable<string> RequiredProperties
		{
			get
			{
				foreach (KeyValuePair<string, bool> P in this.properties)
				{
					if (P.Value)
						yield return P.Key;
				}
			}
		}

		/// <summary>
		/// Optional properties.
		/// </summary>
		public IEnumerable<string> OptionalProperties
		{
			get
			{
				foreach (KeyValuePair<string, bool> P in this.properties)
				{
					if (!P.Value)
						yield return P.Key;
				}
			}
		}

		/// <summary>
		/// Required attachments.
		/// </summary>
		public IEnumerable<string> RequiredAttachments
		{
			get
			{
				foreach (KeyValuePair<string, bool> P in this.attachments)
				{
					if (P.Value)
						yield return P.Key;
				}
			}
		}

		/// <summary>
		/// Optional attachments.
		/// </summary>
		public IEnumerable<string> OptionalAttachments
		{
			get
			{
				foreach (KeyValuePair<string, bool> P in this.attachments)
				{
					if (!P.Value)
						yield return P.Key;
				}
			}
		}
	}
}
