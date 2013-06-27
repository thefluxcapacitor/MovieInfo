namespace MovieInfoCmd
{
    using System.Linq;
    using System.Text;

    public static class Extensions
    {
        public static void WriteElement(this StringBuilder builder, string element, string value)
        {
            builder.AppendFormat("<{0}>{1}</{0}>", element, value);
        }

        public static string CommaSeparated(this string[] array)
        {
            return array.Aggregate((curr, next) => curr + ", " + next);
        }
    }
}
