using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Language
{
	/// <summary>
	/// Contains a localized string.
	/// </summary>
	[CollectionName("LanguageStrings")]
	[Index("NamespaceId", "Id")]
	[TypeName(TypeNameSerialization.None)]
	public class LanguageString
	{
		private Guid objectId = Guid.Empty;
		private Guid namespaceId = Guid.Empty;
		private int id = 0;
		private string value = string.Empty;
		private bool untranslated = false;

		/// <summary>
		/// Contains information about a namespace in a language.
		/// </summary>
		public LanguageString()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Namespace ID.
		/// </summary>
		public Guid NamespaceId
		{
			get { return this.namespaceId; }
			set { this.namespaceId = value; }
		}

		/// <summary>
		/// String ID.
		/// </summary>
		[DefaultValue(0)]
		public int Id
		{
			get { return this.id; }
			set { this.id = value; }
		}

		/// <summary>
		/// Localized value.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// If the string is untranslated.
		/// </summary>
		[DefaultValue(false)]
		public bool Untranslated
		{
			get { return this.untranslated; }
			set { this.untranslated = value; }
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value;
		}

	}
}
