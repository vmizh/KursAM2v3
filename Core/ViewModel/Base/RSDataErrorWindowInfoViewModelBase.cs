using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Core.ViewModel.Base
{
    public abstract class RSDataErrorWindowInfoViewModelBase : RSWindowViewModelBase, IDataErrorInfo
    {
        #region Data Validation

        string IDataErrorInfo.Error => throw new NotSupportedException(
            "IDataErrorInfo.Error is not supported, use IDataErrorInfo.this[propertyName] instead.");

        private readonly Dictionary<string, object> myValues = new Dictionary<string, object>();

        protected void SetValue<T>(Expression<Func<T>> propertySelector, T value)
        {
            var propertyName = GetPropertyName(propertySelector);
            SetValue(propertyName, value);
        }

        private string GetPropertyName(LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new InvalidOperationException();
            return memberExpression.Member.Name;
        }

        /// <summary>
        ///     Sets the value of a property.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The property value.</param>
        protected void SetValue<T>(string propertyName, T value)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Invalid property name", propertyName);
            myValues[propertyName] = value;
            RaisePropertyChanged(propertyName);
        }

        private object GetValue(string propertyName)
        {
            object value;
            if (!myValues.TryGetValue(propertyName, out value))
            {
                var propertyDescriptor = TypeDescriptor.GetProperties(GetType()).Find(propertyName, false);
                if (propertyDescriptor == null)
                    throw new ArgumentException("Invalid property name", propertyName);
                value = propertyDescriptor.GetValue(this);
                myValues.Add(propertyName, value);
            }

            return value;
        }

        protected virtual string OnValidate(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Invalid property name", propertyName);
            var error = string.Empty;
            var value = GetValue(propertyName);
            var results = new List<ValidationResult>(1);
            var result = Validator.TryValidateProperty(
                value,
                new ValidationContext(this, null, null)
                {
                    MemberName = propertyName
                },
                results);
            if (!result)
            {
                var validationResult = results.First();
                error = validationResult.ErrorMessage;
            }

            return error;
        }

        string IDataErrorInfo.this[string propertyName] => OnValidate(propertyName);

        #endregion
    }
}