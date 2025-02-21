using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.Test
{
	public class Converter3 : IContentConverter
	{
		public Converter3()
		{
		}

		public string[] FromContentTypes => ["text/x-test2"];
		public string[] ToContentTypes => ["text/x-test3"];
		public Grade ConversionGrade => Grade.Ok;

		public async Task<bool> ConvertAsync(ConversionState State)
		{
			byte[] Data = new byte[State.From.Length];
			await State.From.ReadAsync(Data, 0, (int)State.From.Length);
			string s = Encoding.UTF8.GetString(Data);

			s += "\r\nConverter 3 was here.";

			Data = Encoding.UTF8.GetBytes(s);
			await State.To.WriteAsync(Data, 0, Data.Length);

			return false;
		}
	}
}
