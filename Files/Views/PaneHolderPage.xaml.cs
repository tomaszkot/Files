using Files.Filesystem;
using Files.UserControls.MultitaskingControl;
using Files.ViewModels;
using Microsoft.Toolkit.Uwp;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI;

namespace Files.Views
{
    public sealed partial class PaneHolderPage : Page, IPaneHolder, ITabItemContent
    {
        public AppInstanceGroup Instances => MainPageViewModel.AppInstances.OfType<AppInstanceGroup>().FirstOrDefault(x => x.AppInstanceInfos == TabItemArguments);

        public event EventHandler<TabItemArguments> ContentChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsViewModel AppSettings => App.AppSettings;
        public IFilesystemHelpers FilesystemHelpers => ActivePane?.FilesystemHelpers;

        public ObservableCollection<AppInstanceInformation> TabItemArguments { get; set; }

        private bool _windowIsCompact = Window.Current.Bounds.Width <= 750;

        private bool windowIsCompact
        {
            get
            {
                return _windowIsCompact;
            }
            set
            {
                if (value != _windowIsCompact)
                {
                    _windowIsCompact = value;
                    NotifyPropertyChanged(nameof(IsMultiPaneEnabled));
                }
            }
        }

        public bool IsMultiPaneActive => Instances.AppInstanceInfos.Count > 1;

        public bool IsMultiPaneEnabled
        {
            get => AppSettings.IsMultiPaneEnabled && !(Window.Current.Bounds.Width <= 750);
        }

        private IShellPage activePane;

        public IShellPage ActivePane
        {
            get => activePane;
            set
            {
                if (activePane != value)
                {
                    activePane = value;
                    
                    if (ActivePane != null)
                    {
                        foreach (AppInstanceInformation info in Instances.AppInstanceInfos)
                        {
                            info.IsCurrentInstance = false;
                        }
                        ActivePane.AppInstanceInfo.IsCurrentInstance = true;
                    }
                    NotifyPropertyChanged(nameof(ActivePane));
                    NotifyPropertyChanged(nameof(FilesystemHelpers));
                }
            }
        }

        public bool IsCurrentInstance { get; set; }

        public PaneHolderPage()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += Current_SizeChanged;
            App.AppSettings.PropertyChanged += AppSettings_PropertyChanged;

            // TODO: fallback / error when failed to get NavigationViewCompactPaneLength value?
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            windowIsCompact = Window.Current.Bounds.Width <= 750;
        }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            //if (eventArgs.Parameter is string navPath)
            //{
            //    NavParamsLeft = navPath;
            //    NavParamsRight = "NewTab".GetLocalized();
            //}
            //if (eventArgs.Parameter is PaneNavigationArguments paneArgs)
            //{
            //    NavParamsLeft = paneArgs.LeftPaneNavPathParam;
            //    NavParamsRight = paneArgs.RightPaneNavPathParam;
            //    IsRightPaneVisible = IsMultiPaneEnabled && paneArgs.RightPaneNavPathParam != null;
            //}

            //TabItemArguments = new TabItemArguments()
            //{
            //    InitialPageType = typeof(PaneHolderPage),
            //    NavigationArg = new PaneNavigationArguments()
            //    {
            //        LeftPaneNavPathParam = NavParamsLeft,
            //        RightPaneNavPathParam = IsRightPaneVisible ? NavParamsRight : null
            //    }
            //};
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(App.AppSettings.IsMultiPaneEnabled):
                    NotifyPropertyChanged(nameof(IsMultiPaneEnabled));
                    break;
            }
        }

        private void Pane_ContentChanged(object sender, TabItemArguments arg)
        {
            var changedInstance = sender as IShellPage;
            changedInstance.AppInstanceInfo.TabItemArguments = arg;
        }

        public DataPackageOperation TabItemDragOver(object sender, DragEventArgs e)
        {
            return ActivePane?.AppInstanceInfo.AppInstance.TabItemDragOver(sender, e) ?? DataPackageOperation.None;
        }

        public async Task<DataPackageOperation> TabItemDrop(object sender, DragEventArgs e)
        {
            if (ActivePane != null)
            {
                return await ActivePane.AppInstanceInfo.AppInstance.TabItemDrop(sender, e);
            }
            return DataPackageOperation.None;
        }

        public void OpenPathInNewPane(string path)
        {
            Instances.AddAppInstanceByArguments(new TabItemArguments()
            {
                InitialPageType = typeof(ModernShellPage),
                NavigationArg = path
            });
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            var ctrl = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control);
            var shift = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Shift);
            var menu = args.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Menu);

            switch (c: ctrl, s: shift, m: menu, k: args.KeyboardAccelerator.Key)
            {
                case (true, true, false, VirtualKey.Left): // ctrl + shift + "<-" select left pane
                    if (AppSettings.IsMultiPaneEnabled)
                    {
                        ActivePane = ((GridViewItem)PanesControl.ContainerFromIndex(0)).FindDescendant<ModernShellPage>();
                    }
                    break;

                case (true, true, false, VirtualKey.Right): // ctrl + shift + "->" select right pane
                    if (AppSettings.IsMultiPaneEnabled)
                    {
                        if (Instances.AppInstanceInfos.Count < 2)
                        {
                            OpenPathInNewPane("NewTab".GetLocalized());
                        }
                        ActivePane = ((GridViewItem)PanesControl.ContainerFromIndex(1)).FindDescendant<ModernShellPage>();
                    }
                    break;

                case (true, true, false, VirtualKey.W): // ctrl + shift + "W" close right pane
                    if (Instances.AppInstanceInfos.Count > 1)
                    {
                        Instances.AppInstanceInfos.Remove(Instances.AppInstanceInfos.Last());
                    }
                    break;

                case (false, true, true, VirtualKey.Add): // alt + shift + "+" open pane
                    if (AppSettings.IsMultiPaneEnabled)
                    {
                        if (Instances.AppInstanceInfos.Count < Constants.UI.MaxPanesCount)
                        {
                            OpenPathInNewPane("NewTab".GetLocalized());
                        }
                    }
                    break;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            App.AppSettings.PropertyChanged -= AppSettings_PropertyChanged;
            Window.Current.SizeChanged -= Current_SizeChanged;
            foreach(AppInstanceInformation info in Instances.AppInstanceInfos)
            {
                ((GridViewItem)PanesControl.ContainerFromIndex(Instances.AppInstanceInfos.IndexOf(info))).FindDescendant<ModernShellPage>().Dispose();
            }
        }

        public void CloseActivePane()
        {
            if (Instances.AppInstanceInfos.Count > 1)
            {
                Instances.AppInstanceInfos.Remove(Instances.AppInstanceInfos.First(x => x.IsCurrentInstance));
            }
        }

        private void Pane_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ActivePane = sender as IShellPage;
            e.Handled = false;
        }
    }
}