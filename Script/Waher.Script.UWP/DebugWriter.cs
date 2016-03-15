using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Script
{
	/// <summary>
	/// A <see cref="TextWriter"/> class that outputs any text written to it, to <see cref="System.Diagnostics.Debug"/>.
	/// </summary>
	public class DebugWriter : TextWriter
	{
		public override System.Text.Encoding Encoding
		{
			get { return System.Text.Encoding.UTF8; }
		}

		public override void Write(bool value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(char value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(char[] buffer)
		{
			System.Diagnostics.Debug.Write(buffer);
		}

		public override void Write(decimal value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(double value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(float value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(int value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(long value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(object value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(string value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(uint value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(ulong value)
		{
			System.Diagnostics.Debug.Write(value);
		}

		public override void Write(string format, object arg0)
		{
			System.Diagnostics.Debug.Write(string.Format(format, arg0));
		}

		public override void Write(string format, params object[] arg)
		{
			System.Diagnostics.Debug.Write(string.Format(format, arg));
		}

		public override void Write(char[] buffer, int index, int count)
		{
			System.Diagnostics.Debug.Write(new string(buffer, index, count));
		}

		public override void Write(string format, object arg0, object arg1)
		{
			System.Diagnostics.Debug.Write(string.Format(format, arg0, arg1));
		}

		public override void Write(string format, object arg0, object arg1, object arg2)
		{
			System.Diagnostics.Debug.Write(string.Format(format, arg0, arg1, arg2));
		}

		public override void WriteLine()
		{
			System.Diagnostics.Debug.WriteLine(string.Empty);
		}

		public override void WriteLine(bool value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(char value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(char[] buffer)
		{
			System.Diagnostics.Debug.WriteLine(buffer);
		}

		public override void WriteLine(decimal value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(double value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(float value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(int value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(long value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(object value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(string value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(uint value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(ulong value)
		{
			System.Diagnostics.Debug.WriteLine(value);
		}

		public override void WriteLine(string format, object arg0)
		{
			System.Diagnostics.Debug.WriteLine(string.Format(format, arg0));
		}

		public override void WriteLine(string format, params object[] arg)
		{
			System.Diagnostics.Debug.WriteLine(string.Format(format, arg));
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			System.Diagnostics.Debug.WriteLine(new string(buffer, index, count));
		}

		public override void WriteLine(string format, object arg0, object arg1)
		{
			System.Diagnostics.Debug.WriteLine(string.Format(format, arg0, arg1));
		}

		public override void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			System.Diagnostics.Debug.WriteLine(string.Format(format, arg0, arg1, arg2));
		}
	}
}