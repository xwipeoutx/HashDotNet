namespace HashDotNet.FarmHash
{
    public class FarmHash0To16BytesStrategy : IHashStrategy<ulong>
    {
        public ulong Hash(byte[] s)
        {
            int len = s.Length;

            if (len >= 8)
            {
                ulong mul = HashUtils.k2 + (uint)len * 2;
                ulong a = HashUtils.ReadULong(s, 0) + HashUtils.k2;
                ulong b = HashUtils.ReadULong(s, len - 8);
                ulong c = HashUtils.Rotate(b, 37) * mul + a;
                ulong d = (HashUtils.Rotate(a, 25) + b) * mul;
                return HashUtils.HashLen16(c, d, mul);
            }
            if (len >= 4)
            {
                ulong mul = HashUtils.k2 + (uint)len * 2;
                uint a = HashUtils.ReadUInt(s, 0);
                return HashUtils.HashLen16((uint)len + (a << 3), HashUtils.ReadUInt(s, len - 4), mul);
            }
            if (len > 0)
            {
                byte a = s[0];
                byte b = s[len >> 1];
                byte c = s[len - 1];
                uint y = (uint)(a) + ((uint)(b) << 8);
                uint z = (uint)len + ((uint)(c) << 2);
                return HashUtils.ShiftMix(y * HashUtils.k2 ^ z * HashUtils.k0) * HashUtils.k2;
            }
            return HashUtils.k2;
        }
    }
}