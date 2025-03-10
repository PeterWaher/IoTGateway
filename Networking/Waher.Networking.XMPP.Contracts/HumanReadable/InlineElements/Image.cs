﻿using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Inline image
	/// </summary>
	public class Image : Formatting
	{
		/// <summary>
		/// 2048 pixels is maximum size (width or height) of images in smart contracts.
		/// </summary>
		public const int MaxSize = 2048;

		private byte[] data;
		private string contentType;
		private int width;
		private int height;

		/// <summary>
		/// Binary data of image.
		/// </summary>
		public byte[] Data
		{
			get => this.data;
			set => this.data = value;
		}

		/// <summary>
		/// Internet Content-Type of image.
		/// </summary>
		public string ContentType
		{
			get => this.contentType;
			set => this.contentType = value;
		}

		/// <summary>
		/// Width of image, in pixels.
		/// </summary>
		public int Width
		{
			get => this.width;
			set => this.width = value;
		}

		/// <summary>
		/// Height of image, in pixels.
		/// </summary>
		public int Height
		{
			get => this.height;
			set => this.height = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override async Task<HumanReadableElement> IsWellDefined()
		{
			if (this.data is null || this.data.Length == 0)
				return this;

			if (string.IsNullOrEmpty(this.contentType) || !this.contentType.StartsWith("image/"))
				return this;

			if (this.width <= 0 || this.width > MaxSize)
				return this;

			if (this.height <= 0 || this.height > MaxSize)
				return this;

			HumanReadableElement E = await base.IsWellDefined();
			if (!(E is null))
				return E;

			try
			{
				ContentResponse Content = await InternetContent.DecodeAsync(this.contentType, this.Data, null);
				if (Content.HasError)
					return this;

				object Obj = Content.Decoded;

				if (Obj is IDisposableAsync DisposableAsync)
					await DisposableAsync.DisposeAsync();
				else if (Obj is IDisposable Disposable)
					Disposable.Dispose();

				return null;
			}
			catch (Exception)
			{
				return this;
			}
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<imageInline contentType=\"");
			Xml.Append(XML.Encode(this.contentType));
			Xml.Append("\" height=\"");
			Xml.Append(this.height.ToString());
			Xml.Append("\" width=\"");
			Xml.Append(this.width.ToString());
			Xml.Append("\"><binary>");

			if (!(this.data is null))
				Xml.Append(Convert.ToBase64String(this.data));

			Xml.Append("</binary><caption>");

			Serialize(Xml, this.Elements);

			Xml.Append("</caption></imageInline>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override async Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			Markdown.Append("![");
			await base.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			Markdown.Append("](data:");
			Markdown.Append(this.contentType);
			Markdown.Append(";base64,");

			if (!(this.data is null))
				Markdown.Append(Convert.ToBase64String(this.data));

			Markdown.Append(' ');
			Markdown.Append(this.width.ToString());
			Markdown.Append(' ');
			Markdown.Append(this.height.ToString());
			Markdown.Append(')');
		}
	}
}
