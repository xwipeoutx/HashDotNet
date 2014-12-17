using System;
using System.Text;

namespace HashDotNet
{
    internal static class Pair
    {
        internal static Pair<T1, T2> Create<T1, T2>(T1 a, T2 b)
        {
            return new Pair<T1, T2>(a, b);
        }
    }

    internal struct Pair<T1, T2>
    {
        internal Pair(T1 a, T2 b)
        {
            First = a;
            Second = b;
        }

        internal T1 First;
        internal T2 Second;

    }

    internal static class HashUtils
    {
        // Some primes between 2^63 and 2^64 for various uses.
        internal const ulong k0 = 0xc3a5c85c97cb3127ul;
        internal const ulong k1 = 0xb492b66fbe98f273ul;
        internal const ulong k2 = 0x9ae16a3b2f90404ful;

        // Magic numbers for 32-bit hashing.  Copied from Murmur3.
        internal const uint c1 = 0xcc9e2d51;
        internal const uint c2 = 0x1b873593;

        internal static ulong ShiftMix(ulong val)
        {
            return val ^ (val >> 47);
        }

        internal static ulong ReadULong(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt64(bytes, offset);
        }

        internal static uint ReadUInt(byte[] bytes, int offset)
        {
            return BitConverter.ToUInt32(bytes, offset);
        }

        internal static ulong Rotate(ulong val, int shift)
        {
            // Avoid shifting by 64: doing so yields an undefined result.
            return shift == 0
                ? val
                : ((val >> shift) | (val << (64 - shift)));
        }

        internal static ulong HashLen16(ulong u, ulong v, ulong mul) //tfsbad find nice name
        {
            // Murmur-inspired hashing.
            ulong a = ShiftMix((u ^ v) * mul);
            ulong b = ShiftMix((v ^ a) * mul);
            return b * mul;
        }
    }

    public interface IHashStrategy<out T> where T : struct // Ideally just bitly stuff, like itns.
    {
        T Hash(byte[] bytes);
    }

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

    public class FarmHashStrategy : IHashStrategy<ulong>
    {
        private readonly FarmHash0To16BytesStrategy _farmHash0To16Bytes;
        private readonly FarmHash17To32BytesStrategy _farmHash17To32Bytes;
        private readonly FarmHash33To64BytesStrategy _farmHash33To64Bytes;
        private readonly FarmHashOver64BytesStrategy _farmHashOver64Bytes;

        public FarmHashStrategy(FarmHash0To16BytesStrategy farmHash0To16Bytes, FarmHash17To32BytesStrategy farmHash17To32Bytes, FarmHash33To64BytesStrategy farmHash33To64Bytes, FarmHashOver64BytesStrategy farmHashOver64Bytes)
        {
            _farmHash0To16Bytes = farmHash0To16Bytes;
            _farmHash17To32Bytes = farmHash17To32Bytes;
            _farmHash33To64Bytes = farmHash33To64Bytes;
            _farmHashOver64Bytes = farmHashOver64Bytes;
        }

        public ulong Hash(byte[] s)
        {
            int len = s.Length;

            return len <= 16 ? _farmHash0To16Bytes.Hash(s)
                : len <= 32 ? _farmHash17To32Bytes.Hash(s)
                : len <= 64 ? _farmHash33To64Bytes.Hash(s)
                : _farmHashOver64Bytes.Hash(s);
        }
    }

    public static class FarmHash
    {
        private static readonly FarmHash0To16BytesStrategy FarmHash0To16BytesStrategy = new FarmHash0To16BytesStrategy();
        private static readonly FarmHash17To32BytesStrategy FarmHash17To32BytesStrategy = new FarmHash17To32BytesStrategy();
        private static readonly FarmHash33To64BytesStrategy FarmHash33To64BytesStrategy = new FarmHash33To64BytesStrategy();
        private static readonly FarmHashOver64BytesStrategy FarmHashOver64BytesStrategy = new FarmHashOver64BytesStrategy();

        public static readonly FarmHashStrategy Strategy = new FarmHashStrategy(FarmHash0To16BytesStrategy, FarmHash17To32BytesStrategy, FarmHash33To64BytesStrategy, FarmHashOver64BytesStrategy);

        public static ulong Hash(string data)
        {
            var bytes = Encoding.Unicode.GetBytes(data);
            return Strategy.Hash(bytes);
        }
    }
}