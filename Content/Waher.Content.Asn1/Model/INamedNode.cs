using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Named ASN.1 node.
	/// </summary>
	public interface INamedNode
	{
		/// <summary>
		/// Name of node.
		/// </summary>
		string Name
		{
			get;
		}
	}
}
