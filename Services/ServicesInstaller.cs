using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace EELauncher.Services; 

public class ServicesInstaller : IWindsorInstaller {
    public void Install(IWindsorContainer container, IConfigurationStore store) {
        container.Register(Classes.FromAssembly(Assembly.GetExecutingAssembly())
            .Where(Component.HasAttribute<ServiceAttribute>)
            .WithService.DefaultInterfaces()
            .LifestyleSingleton());
    }
}