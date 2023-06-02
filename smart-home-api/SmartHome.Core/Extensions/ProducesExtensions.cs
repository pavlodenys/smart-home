namespace SmartHome.Core.Extensions
{
    public static class ProducesExtensions
    {
        public static IEnumerable<T> NotDeleted<T>(this IEnumerable<T> x) where T:class
        {
            return x.Where(x => !((IDeleted)x).IsDeleted);
        }
    }
}