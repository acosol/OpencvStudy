using System.Windows;
using System.Windows.Threading;
using OpenCvRecognition;
using OpencvStudy.Components;
using Prism.DryIoc;
using Prism.Ioc;

namespace OpencvStudy;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<Dispatcher>(r => Current.Dispatcher);
        containerRegistry.RegisterSingleton<Recognition>();
    }

    protected override Window CreateShell() => Container.Resolve<MainWindow>();
}
