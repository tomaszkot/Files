using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Files.ViewModels;
using Files.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Files.Filesystem;
using System.Threading;

namespace Files.UserControls.MultitaskingControl
{
    public class AppInstanceGroup : ObservableObject, IAppInstance
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string header;
        private IconSource iconSource;
        private bool allowStorageItemDrop;

        public List<AppInstanceInformation> AppInstanceInfos { get; } = new List<AppInstanceInformation>();

        public string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }

        public IconSource IconSource
        {
            get => iconSource;
            set => SetProperty(ref iconSource, value);
        }

        public bool AllowStorageItemDrop
        {
            get => allowStorageItemDrop;
            set => SetProperty(ref allowStorageItemDrop, value);
        }

        public AppInstanceGroup(string header, IconSource icon, List<TabItemArguments> args)
        {
            Header = header;
            IconSource = icon;
            foreach (TabItemArguments argument in args)
            {
                AddAppInstanceByArguments(argument);
            }
            UpdateTabInfo();
        }

        public AppInstanceGroup(string header, IconSource icon, TabItemArguments initialArg)
        {
            Header = header;
            IconSource = icon;
            AddAppInstanceByArguments(initialArg);
            UpdateTabInfo();
        }

        public void AddAppInstanceByArguments(TabItemArguments args)
        {
            var currentInstanceVM = new CurrentInstanceViewModel();
            var info = new AppInstanceInformation()
            {
                FilesystemViewModel = new ItemViewModel(currentInstanceVM.FolderSettings),
                InstanceViewModel = currentInstanceVM,
                TabItemArguments = args,
                ParentAppInstanceId = this.Id,
                CancellationTokenSource = new CancellationTokenSource()
            };
            info.FilesystemHelpers = new FilesystemHelpers(info, info.CancellationTokenSource.Token);
            AppInstanceInfos.Add(info);
            OnPropertyChanged(nameof(AppInstanceInfos));
        }

        public void RemoveAppInstanceFromGroup(AppInstanceInformation info)
        {
            AppInstanceInfos.Remove(info);
            OnPropertyChanged(nameof(AppInstanceInfos));
        }

        public async void UpdateTabInfo()
        {
            AllowStorageItemDrop = true;
            if (AppInstanceInfos.Count == 2)
            {
                this.Header = (await MainPageViewModel.GetSelectedTabInfoAsync(AppInstanceInfos[0].TabItemArguments.NavigationArg)).tabLocationHeader + " | " + (await MainPageViewModel.GetSelectedTabInfoAsync(AppInstanceInfos[1].TabItemArguments.NavigationArg)).tabLocationHeader;
            }
            else if (AppInstanceInfos.Count > 2)
            {
                // TODO: Localize this
                this.Header = AppInstanceInfos.Count + " Locations";
            }
            else
            {
                this.Header = (await MainPageViewModel.GetSelectedTabInfoAsync(AppInstanceInfos[0].TabItemArguments.NavigationArg)).tabLocationHeader;
            }

            this.IconSource = new FontIconSource()
            {
                Glyph = "\uF57C"
            };
        }

        public async void UpdateTabInfo(List<TabItemArguments> args)
        {
            foreach (TabItemArguments arg in args)
            {
                AddAppInstanceByArguments(arg);
            }

            AllowStorageItemDrop = true;
            if (args.Count == 2)
            {
                this.Header = (await MainPageViewModel.GetSelectedTabInfoAsync(args[0].NavigationArg)).tabLocationHeader + " | " + (await MainPageViewModel.GetSelectedTabInfoAsync(args[1].NavigationArg)).tabLocationHeader;
            }
            else if (args.Count > 2)
            {
                // TODO: Localize this
                this.Header = args.Count + " Locations";
            }
            else
            {
                this.Header = (await MainPageViewModel.GetSelectedTabInfoAsync(args[0].NavigationArg)).tabLocationHeader;
            }

            this.IconSource = new FontIconSource()
            {
                Glyph = "&#xF57C;"
            };
        }

        public async Task<DataPackageOperation> TabItemDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.AvailableFormats.Contains(StandardDataFormats.StorageItems))
            {
                var selectedPaneInfo = AppInstanceInfos.First(x => x.IsCurrentInstance);
                if (selectedPaneInfo.InstanceViewModel.IsPageTypeNotHome && !selectedPaneInfo.InstanceViewModel.IsPageTypeSearchResults)
                {
                    await selectedPaneInfo.FilesystemHelpers.PerformOperationTypeAsync(
                        DataPackageOperation.Move,
                        e.DataView,
                        selectedPaneInfo.FilesystemViewModel.WorkingDirectory,
                        false,
                        true);
                    return DataPackageOperation.Move;
                }
            }
            return DataPackageOperation.None;
        }

        public DataPackageOperation TabItemDragOver(object sender, DragEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

