using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Core;
using Core.EntityViewModel.CommonReferences;

namespace KursAM2.Managers.Base
{
    public static class AttributeHelper
    {
        public static string GetDisplayName(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;
            var attribute = (DisplayAttribute) fieldInfo.GetCustomAttribute(typeof(DisplayAttribute));
            return attribute.Name;
        }

        public static DocumentType GetDocumentType(this string value)
        {
            return Enum.GetValues(typeof(DocumentType))
                .Cast<DocumentType>()
                .FirstOrDefault(en => value == GetDisplayName(en));
        }
    }
}