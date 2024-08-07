﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.ViewModel.Base;
using KursDomain.ICommon;
using KursDomain.Services;
using Prism.Events;

namespace Domain.Wrapper.Base;

public class NotifyDataErrorInfoBase : RSViewModelBase2, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errorsByPropertyName
        = new Dictionary<string, List<string>>();
    //protected IEventAggregator EventAggregator;
    //protected IMessageDialogService MessageDialogService;

    public NotifyDataErrorInfoBase(IEventAggregator eventAggregator, IMessageDialogService messageDialogService)
    {
        EventAggregator = eventAggregator;
        MessageDialogService = messageDialogService;
    }

    public NotifyDataErrorInfoBase()
    {
        EventAggregator = new EventAggregator();
        MessageDialogService = new MessageDialogService();
    }

    [Display(AutoGenerateField = false)] public bool HasErrors => _errorsByPropertyName.Any();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public IEnumerable GetErrors(string propertyName)
    {
        if (propertyName == null) return null;
        return _errorsByPropertyName.TryGetValue(propertyName, out var val)
            ? val
            : null;
    }

    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        base.RaisePropertyChanged(nameof(HasErrors));
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errorsByPropertyName.ContainsKey(propertyName))
            _errorsByPropertyName[propertyName] = new List<string>();
        if (_errorsByPropertyName[propertyName].Contains(error)) return;
        _errorsByPropertyName[propertyName].Add(error);
        OnErrorsChanged(propertyName);
    }

    protected void ClearErrors(string propertyName)
    {
        if (!_errorsByPropertyName.ContainsKey(propertyName)) return;
        _errorsByPropertyName.Remove(propertyName);
        OnErrorsChanged(propertyName);
    }
}
