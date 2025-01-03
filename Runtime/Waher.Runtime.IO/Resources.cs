using System.IO;
using System.Reflection;
using System;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.IO
{
	/// <summary>
	/// Static class managing loading of resources stored as embedded resources or in content files.
	/// </summary>
	public static class Resources
	{
		/// <summary>
		/// Loads a resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Binary content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static byte[] LoadResource(string ResourceName)
		{
			return LoadResource(ResourceName, Types.GetAssemblyForResource(ResourceName));
		}

		/// <summary>
		/// Loads a resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>Binary content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static byte[] LoadResource(string ResourceName, Assembly Assembly)
		{
			using (Stream f = Assembly.GetManifestResourceStream(ResourceName))
			{
				if (f is null)
					throw new ArgumentException("Resource not found: " + ResourceName, nameof(ResourceName));

				if (f.Length > int.MaxValue)
					throw new ArgumentException("Resource size exceeds " + int.MaxValue.ToString() + " bytes.", nameof(ResourceName));

				int Len = (int)f.Length;
				byte[] Result = new byte[Len];
				f.ReadAll(Result, 0, Len);
				return Result;
			}
		}
	}
}
