namespace HashDotNet.FarmHash
{
    public class FarmHash17To32BytesStrategy : IHashStrategy<ulong>
    {
        public ulong Hash(byte[] s)
        {
            int len = s.Length;

            ulong mul = HashUtils.k2 + (uint)len * 2;
            ulong a = HashUtils.ReadULong(s, 0) * HashUtils.k1;
            ulong b = HashUtils.ReadULong(s, 8);
            ulong c = HashUtils.ReadULong(s, len - 8) * mul;
            ulong d = HashUtils.ReadULong(s, len - 16) * HashUtils.k2;
            return HashUtils.HashLen16(HashUtils.Rotate(a + b, 43) + HashUtils.Rotate(c, 30) + d,
                a + HashUtils.Rotate(b + HashUtils.k2, 18) + c, mul);
        }
    }
}