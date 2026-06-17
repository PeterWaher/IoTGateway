using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model
{
	/// <summary>
	/// An optionally-sized icon that can be displayed in a user interface.
	/// </summary>
	public class Icon
	{
		/// <summary>
		/// An optionally-sized icon that can be displayed in a user interface.
		/// </summary>
		public Icon()
		{
		}

		/// <summary>
		/// An optionally-sized icon that can be displayed in a user interface.
		/// </summary>
		public Icon(Uri Source, string MimeType)
			: this(Source, MimeType, null, null)
		{
		}

		/// <summary>
		/// An optionally-sized icon that can be displayed in a user interface.
		/// </summary>
		public Icon(Uri Source, string MimeType, string[]? Sizes)
			: this(Source, MimeType, Sizes, null)
		{
		}

		/// <summary>
		/// An optionally-sized icon that can be displayed in a user interface.
		/// </summary>
		public Icon(Uri Source, string MimeType, string[]? Sizes, string? Theme)
		{
			this.Source = Source;
			this.MimeType = MimeType;
			this.Sizes = Sizes;
			this.Theme = Theme;
		}

		/// <summary>
		/// A standard URI pointing to an icon resource. May be an HTTP/HTTPS URL or a
		/// `data:` URI with Base64-encoded image data.
		/// 
		/// Consumers SHOULD takes steps to ensure URLs serving icons are from the
		/// same domain as the client/server or a trusted domain.
		/// 
		/// Consumers SHOULD take appropriate precautions when consuming SVGs as they 
		/// can contain executable JavaScript.
		/// </summary>
		public Uri? Source { get; internal set; }

		/// <summary>
		/// Optional MIME type override if the source MIME type is missing or generic.
		/// For example: `"image/png"`, `"image/jpeg"`, or `"image/svg+xml"`.
		/// </summary>
		public string? MimeType { get; internal set; }

		/// <summary>
		/// Optional array of strings that specify sizes at which the icon can be used.
		/// Each string should be in WxH format(e.g., `"48x48"`, `"96x96"`) or `"any"` 
		/// for scalable formats like SVG.
		/// 
		/// If not provided, the client should assume that the icon can be used at any size.
		/// </summary>
		public string[]? Sizes { get; internal set; }

		/// <summary>
		/// Optional specifier for the theme this icon is designed for. `light` indicates
		/// the icon is designed to be used with a light background, and `dark` indicates
		/// the icon is designed to be used with a dark background.
		/// 
		/// If not provided, the client should assume the icon can be used with any theme.
		/// </summary>
		public string? Theme { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out Icon Typed)
		{
			Icon Result = new Icon();

			if (Generic.TryGetValue("src", out object? Obj) &&
				Obj is string Src &&
				Uri.TryCreate(Src, UriKind.Absolute, out Uri Source))
			{
				Result.Source = Source;
			}

			if (Generic.TryGetValue("mimeType", out Obj))
				Result.MimeType = Obj as string;

			if (Generic.TryGetValue("sizes", out Obj) && Obj is Array Sizes)
			{
				int i, c = Sizes.Length;

				Result.Sizes = new string[c];
				for (i = 0; i < c; i++)
					Result.Sizes[i] = Sizes.GetValue(i)?.ToString() ?? string.Empty;
			}

			if (Generic.TryGetValue("theme", out Obj))
				Result.Theme = Obj as string;

			Typed = Result;
			return true;
		}

		/// <summary>
		/// Converts object to a generic representation.
		/// </summary>
		/// <returns>Generic representation.</returns>
		public Dictionary<string, object> ToJson()
		{
			Dictionary<string, object> Result = new Dictionary<string, object>();

			if (!(this.Source is null))
				Result["src"] = this.Source.ToString();

			if (!string.IsNullOrEmpty(this.MimeType))
				Result["mimeType"] = this.MimeType;

			if (!(this.Sizes is null))
				Result["sizes"] = this.Sizes;

			if (!string.IsNullOrEmpty(this.Theme))
				Result["theme"] = this.Theme;

			return Result;
		}
	}
}
