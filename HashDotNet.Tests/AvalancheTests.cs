using System;
using EasyAssertions;
using HashDotNet.FarmHash;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HashDotNet.Tests
{
    [TestClass]
    public class AvalancheTests
    {
        readonly int numInputBytes = 128;
        readonly int sampleSize = 1000;

        [TestMethod]
        public void DoAvalancheTestForFarm()
        {
            FarmHashStrategy sut = new FarmHashStrategy(new FarmHash0To16BytesStrategy(), new FarmHash17To32BytesStrategy(), new FarmHash33To64BytesStrategy(), new FarmHashOver64BytesStrategy());

            var numOutputBytes = sizeof(ulong);
            var tester = new AvalancheTester<ulong>(sut, new ULongBitOperator(), numInputBytes, numOutputBytes);
            var avalancheResult = tester.PerformAvalancheTest(sampleSize);

            ConfirmResults(numInputBytes, numOutputBytes, avalancheResult);
        }

        [TestMethod]
        public void DoAvalancheTestForJenkins()
        {
            OneAtATimeJenkinsHashStrategy sut = new OneAtATimeJenkinsHashStrategy();

            var numOutputBytes = sizeof(uint);
            var tester = new AvalancheTester<uint>(sut, new UIntBitOperator(), numInputBytes, numOutputBytes);
            var avalancheResult = tester.PerformAvalancheTest(sampleSize);

            ConfirmResults(numInputBytes, numOutputBytes, avalancheResult);
        }

        [TestMethod]
        public void DoAvalancheTestForMurmer()
        {
            Murmer32HashStrategy sut = new Murmer32HashStrategy();

            var numOutputBytes = sizeof(uint);
            var tester = new AvalancheTester<uint>(sut, new UIntBitOperator(), numInputBytes, numOutputBytes);
            var avalancheResult = tester.PerformAvalancheTest(sampleSize);

            ConfirmResults(numInputBytes, numOutputBytes, avalancheResult);
        }

        private static void ConfirmResults(int numInputBytes, int numOutputBytes, float[,] avalancheResult)
        {
            float min = 1f, max = 0f;

            for (var i = 0; i < numInputBytes * 8; i++)
            {
                for (var j = 0; j < numOutputBytes * 8; j++)
                {
                    var volatility = avalancheResult[i, j];

                    min = Math.Min(volatility, min);
                    max = Math.Max(volatility, max);
                }
            }

            Console.WriteLine("Min={0}, Max={1}", min, max);

            for (var i = 0; i < numInputBytes * 8; i++)
            {
                for (var j = 0; j < numOutputBytes * 8; j++)
                {
                    var volatility = avalancheResult[i, j];
                    volatility.ShouldBeGreaterThan(0.3f).And.ShouldBeLessThan(0.7f);
                }
            }
        }
    }
}