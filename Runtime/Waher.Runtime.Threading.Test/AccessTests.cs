using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Waher.Runtime.Threading.Test
{
    [TestClass]
    public class AccessTests
	{
        [TestMethod]
        public async Task Test_01_Read()
        {
			using (MultiReadSingleWriteObject obj = new MultiReadSingleWriteObject())
			{
				Assert.AreEqual(0, obj.NrReaders);
				await obj.BeginRead();
				Assert.AreEqual(1, obj.NrReaders);
				await obj.EndRead();
				Assert.AreEqual(0, obj.NrReaders);
			}
		}
    }
}
