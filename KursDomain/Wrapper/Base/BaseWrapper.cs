﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Domain.Wrapper.Base;
using KursDomain.ICommon;
using Prism.Events;

namespace KursDomain.Wrapper.Base;

public class BaseWrapper<T> : NotifyDataErrorInfoBase
{
    public BaseWrapper(T model, IEventAggregator eventAggregator, IMessageDialogService messageDialogService) : base(
        eventAggregator, messageDialogService)
    {
        Model = model;
    }

    public BaseWrapper(T model) : base()
    {
        Model = model;
    }

    [Display(AutoGenerateField = false)] public T Model { get; set; }

    protected virtual void SetValue<TValue>(TValue value,
        [CallerMemberName] string propertyName = null)
    {
        typeof(T).GetProperty(propertyName)?.SetValue(Model, value);
        RaisePropertyChanged(propertyName);
        ValidatePropertyInternal(propertyName, value);
    }

    protected virtual TValue GetValue<TValue>([CallerMemberName] string propertyName = null)
    {
        return (TValue)typeof(T).GetProperty(propertyName)?.GetValue(Model);
    }

    private void ValidatePropertyInternal(string propertyName, object currentValue)
    {
        ClearErrors(propertyName);

        ValidateDataAnnotations(propertyName, currentValue);

        ValidateCustomErrors(propertyName);
    }

    private void ValidateDataAnnotations(string propertyName, object currentValue)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(Model) { MemberName = propertyName };
        Validator.TryValidateProperty(currentValue, context, results);

        foreach (var result in results) AddError(propertyName, result.ErrorMessage);
    }

    private void ValidateCustomErrors(string propertyName)
    {
        var errors = ValidateProperty(propertyName);
        if (errors == null) return;
        foreach (var error in errors)
            AddError(propertyName, error);
    }

    protected virtual IEnumerable<string> ValidateProperty(string propertyName)
    {
        return null;
    }

    public virtual void StartLoad(bool isFullLoad = true)
    {
    }
}
