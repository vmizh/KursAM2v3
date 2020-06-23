namespace Core.Helper
{
    public class KursConvert
    {
        public static short? ToShort(bool? b)
        {
            return b == null ? null : (short?) (b.Value ? 1 : 0);
        }
    }
}