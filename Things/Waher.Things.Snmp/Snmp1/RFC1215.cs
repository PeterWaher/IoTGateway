using System;
using Waher.Content.Asn1;

namespace Waher.Things.Snmp.Snmp1.RFC1215
{
	/// <summary>
	/// TODO
	/// </summary>
	public static partial class Values
	{
		/// <summary>
		/// TODO
		/// </summary>
		public static readonly ObjectId snmp = new ObjectId(RFC1213_MIB.Values.mib_2, 11);

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Int64 coldStart = 0;

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Int64 warmStart = 1;

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Int64 linkDown = 2;

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Int64 linkUp = 3;

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Int64 authenticationFailure = 4;

		/// <summary>
		/// TODO
		/// </summary>
		public static readonly Int64 egpNeighborLoss = 5;
	}

}
