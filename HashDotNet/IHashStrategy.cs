namespace HashDotNet
{
    public interface IHashStrategy<out T> where T : struct // Ideally just bitly stuff, like itns.
    {
        T Hash(byte[] bytes);
    }
}