using OmniAntiCheat.Windows;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Diagnostics;
using System.Windows;

namespace OmniAntiCheat {
	///<summary>Interaction logic for App.xaml</summary>
	public partial class App : PrismApplication {

		protected override Window CreateShell() {
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry) {
			containerRegistry.Register<MainWindow>();
		}

		protected override void ConfigureViewModelLocator() {
			base.ConfigureViewModelLocator();
			ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType => {
				string vmTypeStr = viewType.FullName + "VM";
				Type vmType = Type.GetType(vmTypeStr);
				if(vmType == null) {
					if(Debugger.IsAttached) {
						Debugger.Break();
					}
				}
				return vmType;
			});
		}

	}
}
