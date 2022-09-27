namespace Waher.Networking.XMPP.HTTPX
{
	internal class Chunk
	{
		internal byte[] Data;
		internal int Nr;
		internal bool Last;

		internal Chunk(int Nr, bool Last, byte[] Data)
		{
			this.Data = Data;
			this.Nr = Nr;
			this.Last = Last;
		}
	}
}
