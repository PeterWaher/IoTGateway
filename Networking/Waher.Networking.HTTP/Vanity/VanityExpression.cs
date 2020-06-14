using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Waher.Networking.HTTP.Vanity
{
	internal class VanityExpression
	{
		public string Pattern;
		public Regex Expression;
		public string MapSeed;
		public KeyValuePair<int, string>[] Parameters;
	}
}
