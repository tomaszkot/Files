using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
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
    public class AppInstance : ObservableObject, IAppInstance
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

        public AppInstance(string header, IconSource icon, TabItemArguments args)
        {
            Header = header;
            IconSource = icon;
            AddAppInstanceByArguments(args);
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
            AppInstanceInfos.Insert(0, info);
            OnPropertyChanged(nameof(AppInstanceInfos));
        }

        public async void UpdateTabInfo()
        {
            AllowStorageItemDrop = true;
            (this.Header, this.IconSource) = await MainPageViewModel.GetSelectedTabInfoAsync(AppInstanceInfos[0].TabItemArguments.NavigationArg);
        }

        public async void UpdateTabInfo(List<TabItemArguments> args)
        {
            AppInstanceInfos[0].TabItemArguments = args[0];
            AllowStorageItemDrop = true;
            (this.Header, this.IconSource) = await MainPageViewModel.GetSelectedTabInfoAsync(args[0].NavigationArg);
        }

        public async Task<DataPackageOperation> TabItemDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.AvailableFormats.Contains(StandardDataFormats.StorageItems))
            {
                if (AppInstanceInfos[0].InstanceViewModel.IsPageTypeNotHome && !AppInstanceInfos[0].InstanceViewModel.IsPageTypeSearchResults)
                {
                    await AppInstanceInfos[0].FilesystemHelpers.PerformOperationTypeAsync(
                        DataPackageOperation.Move,
                        e.DataView,
                        AppInstanceInfos[0].FilesystemViewModel.WorkingDirectory,
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

    public class TabItemArguments
    {
        public TabItemArguments() { }

        public TabItemArguments(string navPath)
        {
            InitialPageType = typeof(ModernShellPage);
            NavigationArg = navPath;
        }

        static KnownTypesBinder TypesBinder = new KnownTypesBinder
        {
            KnownTypes = new List<Type> { typeof(string) }
        };
        public Type InitialPageType { get; set; }
        public string NavigationArg { get; set; }

        public Guid InstanceId { get; set; } = Guid.Empty;

        public string Serialize() => JsonConvert.SerializeObject(this, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = TypesBinder
        });

        public static TabItemArguments Deserialize(string obj) => JsonConvert.DeserializeObject<TabItemArguments>(obj, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = TypesBinder
        });
    }

    public class KnownTypesBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName)
        {
            if (!KnownTypes.Any(x => x.Name == typeName))
            {
                throw new ArgumentException();
            }
            else
            {
                return KnownTypes.SingleOrDefault(t => t.Name == typeName);
            }
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
