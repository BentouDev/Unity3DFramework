namespace Framework.Utils
{
    public static class PairUtils
    {
        public static Pair<T, U> MakePair<T, U>(T first, U second)
        {
            return new Pair<T, U>(first, second);
        }
    }
    
    public struct Pair<T, U>
    {
        public Pair(T first, U second) 
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }

        public void Deconstruct(out T first, out U second)
        {
            first = First;
            second = Second;
        }
    };
}