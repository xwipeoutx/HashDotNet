namespace HashDotNet.FarmHash
{
    public class FarmHash33To64BytesStrategy : IHashStrategy<ulong>
    {
        public ulong Hash(byte[] s)
        {
            int len = s.Length;

            ulong mul = HashUtils.k2 + (uint)len * 2;
            ulong a = HashUtils.ReadULong(s, 0) * HashUtils.k2;
            ulong b = HashUtils.ReadULong(s, 8);
            ulong c = HashUtils.ReadULong(s, len - 8) * mul;
            ulong d = HashUtils.ReadULong(s, len - 16) * HashUtils.k2;
            ulong y = HashUtils.Rotate(a + b, 43) + HashUtils.Rotate(c, 30) + d;
            ulong z = HashUtils.HashLen16(y, a + HashUtils.Rotate(b + HashUtils.k2, 18) + c, mul);
            ulong e = HashUtils.ReadULong(s, 16) * mul;
            ulong f = HashUtils.ReadULong(s, 24);
            ulong g = (y + HashUtils.ReadULong(s, len - 32)) * mul;
            ulong h = (z + HashUtils.ReadULong(s, len - 24)) * mul;
            return HashUtils.HashLen16(HashUtils.Rotate(e + f, 43) + HashUtils.Rotate(g, 30) + h,
                e + HashUtils.Rotate(f + a, 18) + g, mul);
        }
    }
}