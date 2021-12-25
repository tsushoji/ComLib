namespace ComLibDemo.Models
{
    public class ParseHelper
    {
        public static bool TryParsePositeviNumStr(string str, out int result)
        {
            if (!int.TryParse(str, out result))
            {
                return false;
            }

            if (result < 0)
            {
                result = 0;
                return false;
            }

            return true;
        }
    }
}
