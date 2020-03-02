using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Exceptions
{
	internal class Bucket
	{
		internal long Count = 0;

		internal Histogram<string>[] SubHistograms = null;

		internal void Inc(params string[] SubKeys)
		{
			int i, c;

			this.Count++;

			if ((c = SubKeys.Length) > 0)
			{
				if (this.SubHistograms is null)
				{
					this.SubHistograms = new Histogram<string>[c];
					i = 0;
				}
				else if ((i = this.SubHistograms.Length) < c)
					Array.Resize<Histogram<string>>(ref this.SubHistograms, c);

				while (i < c)
					this.SubHistograms[i++] = new Histogram<string>();

				for (i = 0; i < c; i++)
					this.SubHistograms[i].Inc(SubKeys[i]);
			}
		}

	}
}
