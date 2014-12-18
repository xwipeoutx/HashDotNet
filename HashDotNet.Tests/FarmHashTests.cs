using System;
using EasyAssertions;
using HashDotNet.FarmHash;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HashDotNet.Tests
{
    [TestClass]
    public class FarmHashTests
    {
        private FarmHashStrategy _sut = new FarmHashStrategy(new FarmHash0To16BytesStrategy(), new FarmHash17To32BytesStrategy(), new FarmHash33To64BytesStrategy(), new FarmHashOver64BytesStrategy());

        [TestInitialize]
        public void Initialize()
        {
            _sut = new FarmHashStrategy(new FarmHash0To16BytesStrategy(), new FarmHash17To32BytesStrategy(), new FarmHash33To64BytesStrategy(), new FarmHashOver64BytesStrategy());
        }

        [TestMethod]
        public void SmallByteCount_DifferentHash()
        {
            byte _1 = 0x0001;
            byte _2 = 0x0002;
            var hash1 = _sut.Hash(new[] { _1 });
            var hash2 = _sut.Hash(new[] { _2 });
            hash1.ShouldNotBe(hash2);
        }
    }

    public interface IBitOperator<T>
    {
        bool BitAt(T input, int position);
    }

    public class ULongBitOperator : IBitOperator<ulong>
    {
        public bool BitAt(ulong input, int position)
        {
            return ((input & (1ul << position)) != 0);
        }
    }
    public class UIntBitOperator : IBitOperator<uint>
    {
        public bool BitAt(uint input, int position)
        {
            return ((input & (1u << position)) != 0);
        }
    }

    public class AvalancheTester<T>
        where T : struct
    {
        private readonly IHashStrategy<T> _hashStrategy;
        private readonly IBitOperator<T> _bitOperator;
        private readonly int _numInputBytes;
        private readonly int _numOutputBytes;
        private readonly Random _random = new Random();

        public AvalancheTester(IHashStrategy<T> hashStrategy, IBitOperator<T> bitOperator, int numInputBytes, int numOutputBytes)
        {
            _hashStrategy = hashStrategy;
            _bitOperator = bitOperator;
            _numInputBytes = numInputBytes;
            _numOutputBytes = numOutputBytes;
        }

        public float[,] PerformAvalancheTest(int numSamples)
        {
            var flipCounts = new int[NumInputBits, NumOutputBits];

            for (var runNumber = 0; runNumber < numSamples; runNumber++)
            {
                byte[] initial = new byte[_numInputBytes];
                _random.NextBytes(initial);

                T hash1 = _hashStrategy.Hash(initial);

                for (var i = 0; i < NumInputBits; i++)
                {
                    byte bitMaskForByteFlip = (byte)(0x1 << (i % 8));

                    initial[i / 8] ^= bitMaskForByteFlip;
                    T hash2 = _hashStrategy.Hash(initial);
                    initial[i / 8] ^= bitMaskForByteFlip;

                    for (var j = 0; j < NumOutputBits; j++)
                    {
                        if (_bitOperator.BitAt(hash1, j) != _bitOperator.BitAt(hash2, j))
                            flipCounts[i, j]++;
                    }
                }
            }

            var flipRanges = new float[NumInputBits, NumOutputBits];
            for (var i = 0; i < NumInputBits; i++)
                for (var j = 0; j < NumOutputBits; j++)
                    flipRanges[i, j] = (float)flipCounts[i, j] / numSamples;

            return flipRanges;
        }

        private int NumInputBits { get { return _numInputBytes * 8; } }
        private int NumOutputBits { get { return _numOutputBytes * 8; } }
    }
}
