using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace Waher.Content
{
	/// <summary>
	/// Static class managing loading of resources stored as embedded resources or in content files.
	/// </summary>
	public static class Resources
	{
		/// <summary>
		/// Loads an XML schema from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>XML Schema.</returns>
		/// <exception cref="XmlSchemaException">If a validation exception occurred.</exception>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XmlSchema LoadSchema(string ResourceName)
		{
			return LoadSchema(ResourceName, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Loads an XML schema from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>XML Schema.</returns>
		/// <exception cref="XmlSchemaException">If a validation exception occurred.</exception>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XmlSchema LoadSchema(string ResourceName, Assembly Assembly)
		{
			using (Stream f = Assembly.GetManifestResourceStream(ResourceName))
			{
				if (f == null)
					throw new ArgumentException("Resource not found: " + ResourceName, "ResourceName");

				XmlValidator Validator = new XmlValidator(ResourceName);
				XmlSchema Result = XmlSchema.Read(f, Validator.ValidationCallback);

				Validator.AssertNoError();

				return Result;
			}
		}

		/// <summary>
		/// Loads an XSL transformation from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>XSL tranformation.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XslCompiledTransform LoadTransform(string ResourceName)
		{
			return LoadTransform(ResourceName, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Loads an XSL transformation from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>XSL tranformation.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XslCompiledTransform LoadTransform(string ResourceName, Assembly Assembly)
		{
			using (Stream f = Assembly.GetManifestResourceStream(ResourceName))
			{
				if (f == null)
					throw new ArgumentException("Resource not found: " + ResourceName, "ResourceName");

				using (XmlReader r = XmlReader.Create(f))
				{
					XslCompiledTransform Xslt = new XslCompiledTransform();
					Xslt.Load(r);

					return Xslt;
				}
			}
		}

		/// <summary>
		/// Loads a resource from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Binary content.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static byte[] LoadResource(string ResourceName)
		{
			return LoadResource(ResourceName, Assembly.GetCallingAssembly());
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
					throw new ArgumentException("Resource not found: " + ResourceName, "ResourceName");

				if (f.Length > int.MaxValue)
					throw new ArgumentException("Resource size exceeds " + int.MaxValue.ToString() + " bytes.", "ResourceName");

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
			return (LoadCertificate(ResourceName, Assembly.GetCallingAssembly()));
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
			return LoadCertificate(ResourceName, Password, Assembly.GetCallingAssembly());
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
