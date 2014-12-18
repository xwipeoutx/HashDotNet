namespace HashDotNet.FarmHash
{
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
}