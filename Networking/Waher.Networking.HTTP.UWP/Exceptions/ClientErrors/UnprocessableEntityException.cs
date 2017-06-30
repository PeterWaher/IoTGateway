using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// The request was well-formed but was unable to be followed due to semantic errors.
	/// </summary>
	public class UnprocessableEntityException : HttpException
	{
		/// <summary>
		/// The request was well-formed but was unable to be followed due to semantic errors.
		/// </summary>
		public UnprocessableEntityException()
			: base(422, "Unprocessable Entity")
		{
		}
	}
}
