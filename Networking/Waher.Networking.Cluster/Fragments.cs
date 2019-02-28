using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Waher.Networking.Cluster
{
	internal class Fragments
	{
		public SortedDictionary<int, byte[]> Parts = new SortedDictionary<int, byte[]>();
		public IPEndPoint Source;
		public long Timestamp;
		public int? NrParts = null;
		public bool Done = false;

		internal byte[] ToByteArray()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				foreach (byte[] Part in Parts.Values)
					ms.Write(Part, 0, Part.Length);

				return ms.ToArray();
			}
		}
	}
}
