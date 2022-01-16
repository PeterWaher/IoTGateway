using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Language
{
	/// <summary>
	/// Translation level.
	/// </summary>
	public enum TranslationLevel
	{
		/// <summary>
		/// String is untranslated.
		/// </summary>
		Untranslated = 0,

		/// <summary>
		/// String is machine translated.
		/// </summary>
		MachineTranslated = 1,

		/// <summary>
		/// String is humanly translated, or verified by human.
		/// </summary>
		HumanTranslated = 2
	}

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
		private string id = string.Empty;
		private string value = string.Empty;
		private TranslationLevel level = TranslationLevel.HumanTranslated;

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
		public string Id
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
		[DefaultValue(TranslationLevel.HumanTranslated)]
		public TranslationLevel Level
		{
			get { return this.level; }
			set { this.level = value; }
		}

		/// <summary>
		/// Lega
		/// </summary>
		[Obsolete("Use Level instead.")]
		[IgnoreMember]
		public bool Untranslated
		{
			get => this.level == TranslationLevel.Untranslated;
			set => this.level = value ? TranslationLevel.Untranslated : TranslationLevel.HumanTranslated;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.value;
		}

	}
}
