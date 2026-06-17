using System;
using System.Collections.Generic;
using Waher.Runtime.Collections;

namespace Waher.Networking.HTTP.Mcp.Model
{
	/// <summary>
	/// Base interface to add `icons` property.
	/// </summary>
	public class Icons
	{
		/// <summary>
		/// Base interface to add `icons` property.
		/// </summary>
		public Icons()
		{
		}

		/// <summary>
		/// Base interface to add `icons` property.
		/// </summary>
		/// <param name="Icons">Icons</param>
		public Icons(params Icon[] Icons)
		{
			this.IconArray = Icons;
		}

		/// <summary>
		/// Optional set of sized icons that the client can display in a user interface.
		/// 
		/// Clients that support rendering icons MUST support at least the following MIME types:
		/// - `image/png` - PNG images(safe, universal compatibility)
		/// - `image/jpeg` (and `image/jpg`) - JPEG images(safe, universal compatibility)
		/// 
		/// Clients that support rendering icons SHOULD also support:
		/// - `image/svg+xml` - SVG images(scalable but requires security precautions)
		/// - `image/webp` - WebP images(modern, efficient format)
		/// </summary>
		public Icon[]? IconArray { get; internal set; }

		/// <summary>
		/// If there are icons defined.
		/// </summary>
		public bool Empty => (this.IconArray?.Length ?? 0) == 0;

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out Icons Typed)
		{
			Icons Result = new Icons();

			if (Generic.TryGetValue("icons", out object? Obj) && Obj is Array Icons)
			{
				ChunkedList<Icon> IconList = new ChunkedList<Icon>();

				foreach (object Item in Icons)
				{
					if (Item is Dictionary<string, object> IconObj &&
						Icon.TryParse(IconObj, out Icon TypedIcon))
					{
						IconList.Add(TypedIcon);
					}
				}

				Result.IconArray = IconList.ToArray();
			}

			Typed = Result;
			return true;
		}

		/// <summary>
		/// Converts object to a generic representation.
		/// </summary>
		/// <returns>Generic representation.</returns>
		public Dictionary<string, object>[] ToJson()
		{
			if (this.IconArray is null)
				return Array.Empty<Dictionary<string, object>>();

			Dictionary<string, object>[] Result = new Dictionary<string, object>[this.IconArray.Length];
			for (int i = 0; i < this.IconArray.Length; i++)
				Result[i] = this.IconArray[i].ToJson();

			return Result;
		}
	}
}
