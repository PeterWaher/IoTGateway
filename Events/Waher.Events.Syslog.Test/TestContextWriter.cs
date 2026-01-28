using System.Text;

namespace Waher.Events.Syslog.Test
{
	/// <summary>
	/// Writes to a <see cref="TestContext"/>
	/// </summary>
	public class TestContextWriter : TextWriter
	{
		private readonly TestContext context;

		public TestContextWriter(TestContext Context)
		{
			this.context = Context;
		}

		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(char[] buffer, int index, int count)
		{
			string s = new(buffer, index, count);
			this.context?.Write(s);
		}
	}
}
