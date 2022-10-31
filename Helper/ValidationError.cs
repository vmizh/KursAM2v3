using System.Collections.Generic;

namespace Core.Helper
{
    public static class ValidationError
    {
        public static string FieldNotNull = "Поле не может быть пустым";

        public static string GetErrorWomen(string name)
        {
            return $"{name} всегда быть заполнена обязательно";
        }

        public static string GetErrorMan(string name)
        {
            return $"{name} всегда быть заполнен обязательно";
        }

        public static string GetNotNullFieldsError(string fname, Dictionary<string, object> fieldName)
        {
            if (!fieldName.ContainsKey(fname)) return null;
            return fieldName[fname] == null ? "Поле не может быть пустым" : null;
        }
    }
}