using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Files.UserControls.MultitaskingControl
{
    public interface IMultitaskingControl
    {
        public event EventHandler<CurrentInstanceChangedEventArgs> TabSelectionChanged;

        public ObservableCollection<IAppInstance> Items { get; }

        public ITabItemContent GetCurrentSelectedTabInstance();

        public void CloseTab(IAppInstance IAppInstance);
        public void SetLoadingIndicatorStatus(IAppInstance item, bool loading);
    }

    public class CurrentInstanceChangedEventArgs : EventArgs
    {
        public ITabItemContent CurrentInstance { get; set; }
    }
}