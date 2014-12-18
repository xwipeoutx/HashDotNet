namespace HashDotNet.FarmHash
{
    public class FarmHashOver64BytesStrategy : IHashStrategy<ulong>
    {
        public ulong Hash(byte[] s)
        {
            const ulong seed = 81;
            int len = s.Length;

            // For strings over 64 bytes we loop.  Internal state consists of
            // 56 bytes: v, w, x, y, and z.
            ulong x = seed;
            ulong y = unchecked(seed * HashUtils.k1 + 113);
            ulong z = HashUtils.ShiftMix(y * HashUtils.k2 + 113) * HashUtils.k2;
            Pair<ulong, ulong> v = Pair.Create(0UL, 0UL);
            Pair<ulong, ulong> w = Pair.Create(0UL, 0UL);
            x = x * HashUtils.k2 + HashUtils.ReadULong(s, 0);

            // Set end so that after the loop we have 1 to 64 bytes left to process.
            //tfsbad const char* end = s + ((len - 1) / 64) * 64;
            //tfsbad const char* last64 = end + ((len - 1) & 63) - 63;
            int currentOffset = 0;
            int endOffset = ((len - 1) / 64) * 64;
            int last64Offset = endOffset + ((len - 1) & 63) - 63;

            do
            {
                x = HashUtils.Rotate(x + y + v.First + HashUtils.ReadULong(s, currentOffset + 8), 37) * HashUtils.k1;
                y = HashUtils.Rotate(y + v.Second + HashUtils.ReadULong(s, currentOffset + 48), 42) * HashUtils.k1;
                x ^= w.Second;
                y += v.First + HashUtils.ReadULong(s, currentOffset + 40);
                z = HashUtils.Rotate(z + w.First, 33) * HashUtils.k1;
                v = WeakHashLen32WithSeeds(s, v.Second * HashUtils.k1, x + w.First, currentOffset);
                w = WeakHashLen32WithSeeds(s, z + w.Second, y + HashUtils.ReadULong(s, currentOffset + 16), currentOffset + 32);

                //tfsbadstd::swap(z, x);
                z = z ^ x;
                x = x ^ z;
                z = z ^ x;


                currentOffset += 64;
            } while (currentOffset != endOffset);

            ulong mul = HashUtils.k1 + ((z & 0xff) << 1);
            // Make s point to the last 64 bytes of input.
            currentOffset = last64Offset;

            w.First = w.First + (ulong)((len - 1) & 63);
            v.First += w.First;
            w.First += v.First;

            x = HashUtils.Rotate(x + y + v.First + HashUtils.ReadULong(s, currentOffset + 8), 37) * mul;
            y = HashUtils.Rotate(y + v.Second + HashUtils.ReadULong(s, currentOffset + 48), 42) * mul;
            x ^= w.Second * 9;
            y += v.First * 9 + HashUtils.ReadULong(s, currentOffset + 40);
            z = HashUtils.Rotate(z + w.First, 33) * mul;
            v = WeakHashLen32WithSeeds(s, v.Second * mul, x + w.First, currentOffset);
            w = WeakHashLen32WithSeeds(s, z + w.Second, y + HashUtils.ReadULong(s, currentOffset + 16), currentOffset + 32);

            //tfsbad std::swap(z, x);
            z = z ^ x;
            x = x ^ z;
            z = z ^ x;

            return HashUtils.HashLen16(HashUtils.HashLen16(v.First, w.First, mul) + HashUtils.ShiftMix(y) * HashUtils.k0 + z, HashUtils.HashLen16(v.Second, w.Second, mul) + x,
                mul);

        }

        private static Pair<ulong, ulong> WeakHashLen32WithSeeds(ulong w, ulong x, ulong y, ulong z, ulong a, ulong b)
        {
            a += w;
            b = HashUtils.Rotate(b + a + z, 21);
            ulong c = a;
            a += x;
            a += y;
            b += HashUtils.Rotate(a, 44);
            return Pair.Create(a + z, b + c);
        }

        // Return a 16-byte hash for s[0] ... s[31], a, and b.  Quick and dirty.
        private static Pair<ulong, ulong> WeakHashLen32WithSeeds(byte[] s, ulong a, ulong b, int offset)
        {
            return WeakHashLen32WithSeeds(HashUtils.ReadULong(s, offset), HashUtils.ReadULong(s, offset + 8), HashUtils.ReadULong(s, offset + 16), HashUtils.ReadULong(s, offset + 24),
                a,
                b);
        }
    }
}