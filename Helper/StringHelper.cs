namespace Helper
{
    public static class StringHelper
    {
        /// <summary>
        /// Получить первые count символы из строки
        /// </summary>
        /// <param name="str"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string GetFirst(this string str, int count)
        {
            if (str.Length <= count) 
                return str;
            return str.Substring(0,count);
        }
    }
}