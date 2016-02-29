using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Script;

namespace Waher.Networking.HTTP.Test
{
	public class Converter1 : IContentConverter
	{
		public Converter1()
		{
		}

		public string[] FromContentTypes
		{
			get { return new string[] { "text/plain" }; }
		}

		public string[] ToContentTypes
		{
			get { return new string[] { "text/x-test1" }; }
		}

		public Grade ConversionGrade
		{
			get { return Grade.Ok; }
		}

		public void Convert(string FromContentType, Stream From, string FromFileName, string ResourceName, string ToContentType, 
            Stream To, Variables Session)
		{
			byte[] Data = new byte[From.Length];
			From.Read(Data, 0, (int)From.Length);
			string s = Encoding.UTF8.GetString(Data);

			s += "\r\nConverter 1 was here.";

			Data = Encoding.UTF8.GetBytes(s);
			To.Write(Data, 0, Data.Length);
		}
	}
}
