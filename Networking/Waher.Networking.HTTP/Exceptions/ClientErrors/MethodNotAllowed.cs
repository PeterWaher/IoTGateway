using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
	/// </summary>
	public class MethodNotAllowed : HttpException
	{
		/// <summary>
		/// The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.
		/// </summary>
		/// <param name="AllowedMethods">Allowed methods.</param>
		public MethodNotAllowed(string[] AllowedMethods)
			: base(405, "Method Not Allowed", new KeyValuePair<string, string>("Allow", Join(AllowedMethods)))
		{
		}

		private static string Join(string[] Methods)
		{
			StringBuilder Output = null;

			foreach (string Method in Methods)
			{
				if (Output == null)
					Output = new StringBuilder();
				else
					Output.Append(", ");

				Output.Append(Method);
			}

			if (Output == null)
				return string.Empty;
			else
				return Output.ToString();
		}
	}
}
