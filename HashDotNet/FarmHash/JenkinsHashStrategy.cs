
using System;

namespace HashDotNet.FarmHash
{
    public class Murmer32HashStrategy : IHashStrategy<uint>
    {
        private const uint c1 = 0xcc9e2d51;
        private const uint c2 = 0x1b873593;
        private const int r1 = 15;
        private const int r2 = 13;
        private const uint m = 5;
        private const uint n = 0xe6546b64;

        public uint Hash(byte[] key)
        {
            uint len = (uint)key.Length;
            uint hash = 0;

            uint nblocks = len / 4;


            int i;
            for (i = 0; i < nblocks; i++)
            {
                uint k = BitConverter.ToUInt32(key, i * 4);
                k *= c1;
                k = (k << r1) | (k >> (32 - r1));
                k *= c2;

                hash ^= k;
                hash = ((hash << r2) | (hash >> (32 - r2))) * m + n;
            }

            uint tail = BitConverter.ToUInt32(key, key.Length - 4); //tfsbad should be the actual tail, not just the last 4 bits.  0 out the others.

            uint k1 = 0;

            if (len - nblocks == 3)
                k1 ^= tail << 8;

            if (len - nblocks == 2)
                k1 ^= tail << 16;

            if (len - nblocks == 1)
                k1 ^= tail << 24;

            k1 *= c1;
            k1 = (k1 << r1) | (k1 >> (32 - r1));
            k1 *= c2;
            hash ^= k1;

            hash ^= len;
            hash ^= (hash >> 16);
            hash *= 0x85ebca6b;
            hash ^= (hash >> 13);
            hash *= 0xc2b2ae35;
            hash ^= (hash >> 16);

            return hash;
        }
    }

    public class OneAtATimeJenkinsHashStrategy : IHashStrategy<uint>
    {
        public uint Hash(byte[] data)
        {
            uint hash = 0;
            for (var i = 0; i < data.Length; i++)
            {
                hash += data[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }
    }
}