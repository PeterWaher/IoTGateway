using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Security.Cryptography.X509Certificates;
using Waher.Runtime.Inventory;

namespace Waher.Content
{
	/// <summary>
	/// Static class managing loading of resources stored as embedded resources or in content files.
	/// </summary>
	public static class Resources
	{
		/// <summary>
		/// Gets the assembly corresponding to a given resource name.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <returns>Assembly.</returns>
		/// <exception cref="ArgumentException">If no assembly could be found.</exception>
		public static Assembly GetAssembly(string ResourceName)
		{
			string[] Parts = ResourceName.Split('.');
			string ParentNamespace;
			int i, c;
			Assembly A;

			if (!Types.IsRootNamespace(Parts[0]))
				A = null;
			else
			{
				ParentNamespace = Parts[0];
				i = 1;
				c = Parts.Length;

				while (i < c)
				{
					if (!Types.IsSubNamespace(ParentNamespace, Parts[i]))
						break;

					ParentNamespace += "." + Parts[i];
					i++;
				}

				A = Types.GetFirstAssemblyReferenceInNamespace(ParentNamespace);
			}

			if (A == null)
				throw new ArgumentException("Assembly not found for resource " + ResourceName + ".", nameof(ResourceName));

			return A;
		}

		/// <summary>
		/// Loads a resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Binary content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static byte[] LoadResource(string ResourceName)
		{
			return LoadResource(ResourceName, GetAssembly(ResourceName));
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
				if (f == null)
					throw new ArgumentException("Resource not found: " + ResourceName, nameof(ResourceName));

				if (f.Length > int.MaxValue)
					throw new ArgumentException("Resource size exceeds " + int.MaxValue.ToString() + " bytes.", nameof(ResourceName));

				int Len = (int)f.Length;
				byte[] Result = new byte[Len];
				f.Read(Result, 0, Len);
				return Result;
			}
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName)
		{
			return (LoadCertificate(ResourceName, GetAssembly(ResourceName)));
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName, Assembly Assembly)
		{
			return LoadCertificate(ResourceName, null, Assembly);
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Password">Optional password.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName, string Password)
		{
			return LoadCertificate(ResourceName, Password, GetAssembly(ResourceName));
		}

		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Password">Optional password.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName, string Password, Assembly Assembly)
		{
			byte[] Data = LoadResource(ResourceName, Assembly);
			if (Password == null)
				return new X509Certificate2(Data);
			else
				return new X509Certificate2(Data, Password);
		}

	}
}
