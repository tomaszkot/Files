using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Files.ViewModels.SettingsViewModels
{
    public class MultitaskingViewModel : ObservableObject
    {
        private bool isVerticalTabFlyoutEnabled = App.AppSettings.IsVerticalTabFlyoutEnabled;
        public bool IsVerticalTabFlyoutEnabled
        {
            get
            {
                return isVerticalTabFlyoutEnabled;
            }
            set
            {
                if (SetProperty(ref isVerticalTabFlyoutEnabled, value))
                {
                    App.AppSettings.IsVerticalTabFlyoutEnabled = value;
                }
            }
        }

        private bool isDualPaneEnabled = App.AppSettings.IsMultiPaneEnabled;
        private bool alwaysOpenDualPaneInNewTab = App.AppSettings.AlwaysOpenMaxPanesWithNewTab;

        public bool IsDualPaneEnabled
        {
            get
            {
                return isDualPaneEnabled;
            }
            set
            {
                if (SetProperty(ref isDualPaneEnabled, value))
                {
                    App.AppSettings.IsMultiPaneEnabled = value;
                }
            }
        }

        public bool AlwaysOpenDualPaneInNewTab
        {
            get
            {
                return alwaysOpenDualPaneInNewTab;
            }
            set
            {
                if (SetProperty(ref alwaysOpenDualPaneInNewTab, value))
                {
                    App.AppSettings.AlwaysOpenMaxPanesWithNewTab = value;
                }
            }
        }
    }
}