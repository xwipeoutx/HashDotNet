namespace HashDotNet
{
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

    internal static class Pair
    {
        internal static Pair<T1, T2> Create<T1, T2>(T1 a, T2 b)
        {
            return new Pair<T1, T2>(a, b);
        }
    }
}