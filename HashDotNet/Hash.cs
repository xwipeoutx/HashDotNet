using System.Text;
using HashDotNet.FarmHash;


namespace HashDotNet
{
    public static class Hash
    {
        private static readonly FarmHash0To16BytesStrategy FarmHash0To16BytesStrategy = new FarmHash0To16BytesStrategy();
        private static readonly FarmHash17To32BytesStrategy FarmHash17To32BytesStrategy = new FarmHash17To32BytesStrategy();
        private static readonly FarmHash33To64BytesStrategy FarmHash33To64BytesStrategy = new FarmHash33To64BytesStrategy();
        private static readonly FarmHashOver64BytesStrategy FarmHashOver64BytesStrategy = new FarmHashOver64BytesStrategy();

        public static readonly FarmHashStrategy FarmHashStrategy = new FarmHashStrategy(FarmHash0To16BytesStrategy, FarmHash17To32BytesStrategy, FarmHash33To64BytesStrategy, FarmHashOver64BytesStrategy);

        public static ulong FarmHash64(string data)
        {
            var bytes = Encoding.Unicode.GetBytes(data);
            return FarmHashStrategy.Hash(bytes);
        }
    }
}