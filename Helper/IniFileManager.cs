using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Helper
{
    public class IniFileManager
    {
        private readonly string Path; //Имя файла.

        // С помощью конструктора записываем пусть до файла и его имя.
        public IniFileManager(string IniPath)
        {
            Path = new FileInfo(IniPath).FullName;
            if (!File.Exists(Path))
            {
                File.Create(Path);
            }
        }

        [DllImport("kernel32")] // Подключаем kernel32.dll и описываем его функцию WritePrivateProfilesString
        private static extern long WritePrivateProfileString(string Section, string Key, string Value,
            string FilePath);

        [DllImport("kernel32")] // Еще раз подключаем kernel32.dll, а теперь описываем функцию GetPrivateProfileString
        private static extern int GetPrivateProfileString(string Section, string Key, string Default,
            StringBuilder RetVal, int Size, string FilePath);

        //Читаем ini-файл и возвращаем значение указного ключа из заданной секции.
        public string ReadINI(string Section, string Key)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        //Записываем в ini-файл. Запись происходит в выбранную секцию в выбранный ключ.
        public void Write(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, Path);
        }

        //Удаляем ключ из выбранной секции.
        public void DeleteKey(string Key, string Section = null)
        {
            Write(Section, Key, null);
        }

        //Удаляем выбранную секцию
        public void DeleteSection(string Section = null)
        {
            Write(Section, null, null);
        }

        //Проверяем, есть ли такой ключ, в этой секции
        public bool KeyExists(string Key, string Section = null)
        {
            return ReadINI(Section, Key).Length > 0;
        }
    }
}
