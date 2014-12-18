using System;

namespace HashDotNet
{
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
}