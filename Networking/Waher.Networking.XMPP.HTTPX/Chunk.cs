namespace Waher.Networking.XMPP.HTTPX
{
	internal class Chunk
	{
		internal bool ConstantBuffer;
		internal byte[] Data;
		internal int Nr;
		internal bool Last;

		internal Chunk(int Nr, bool Last, bool ConstantBuffer, byte[] Data)
		{
			this.ConstantBuffer = ConstantBuffer;
			this.Data = Data;
			this.Nr = Nr;
			this.Last = Last;
		}
	}
}
