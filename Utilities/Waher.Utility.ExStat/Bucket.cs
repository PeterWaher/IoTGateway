using System;
using System.Collections.Generic;

namespace Waher.Utility.ExStat
{
	public class Bucket
	{
		public long Count = 0;
		public bool Touched = false;

		public Histogram<string>[] SubHistograms = null;

		public void Inc(params string[] SubKeys)
		{
			int i, c;

			this.Count++;
			this.Touched = true;

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

		public void Join(int Count, bool First, params string[] SubKeys)
		{
			int i, c;

			this.Count += Count;
			this.Touched = true;

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
				{
					string SubKey = SubKeys[i];
					if (!(SubKey is null))
						this.SubHistograms[i].Join(SubKey, Count, First);
				}
			}
		}

		public void RemoveUntouched()
		{
			if (!(this.SubHistograms is null))
			{
				foreach (Histogram<string> H in this.SubHistograms)
					H.RemoveUntouched();
			}
		}

	}
}
