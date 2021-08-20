using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;

namespace Files.UserControls.MultitaskingControl
{

    public interface ITabItemContent
    {
        public event EventHandler<TabItemArguments> ContentChanged;
    }

    public interface ITabItem
    {
        public bool IsCurrentInstance { get; set; }

        public TabItemArguments TabItemArguments { get; }
    }
}