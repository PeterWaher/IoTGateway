using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines the Content-Type of a multi-row property. Acts as a hint to clients, on how the text can be edited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class ContentTypeAttribute : Attribute
	{
		private readonly string contentType;

		/// <summary>
		/// Defines the Content-Type of a multi-row property. Acts as a hint to clients, on how the text can be edited.
		/// </summary>
		/// <param name="ContentType">Internet Content-Type.</param>
		public ContentTypeAttribute(string ContentType)
		{
			this.contentType = ContentType;
		}

		/// <summary>
		/// Internet Content-Type.
		/// </summary>
		public string ContentType => this.contentType;
	}
}
