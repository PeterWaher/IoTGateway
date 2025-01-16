using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.IO
{
	/// <summary>
	/// Static class managing certificate resources.
	/// </summary>
	public static class Certificates
	{
		/// <summary>
		/// Loads a certificate from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>Certificate.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static X509Certificate2 LoadCertificate(string ResourceName)
		{
			return LoadCertificate(ResourceName, Types.GetAssemblyForResource(ResourceName));
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
			return LoadCertificate(ResourceName, Password, Types.GetAssemblyForResource(ResourceName));
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
			byte[] Data = Runtime.IO.Resources.LoadResource(ResourceName, Assembly);
			if (Password is null)
				return new X509Certificate2(Data);
			else
				return new X509Certificate2(Data, Password);
		}
	}
}
