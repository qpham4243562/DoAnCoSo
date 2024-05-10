namespace WebApplication2.Models
{
    public static class ImageExtensions
    {
        public static IEnumerable<byte[]> ToImages(this byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return Enumerable.Empty<byte[]>();
            }

            return new[] { data };
        }
    }
}
