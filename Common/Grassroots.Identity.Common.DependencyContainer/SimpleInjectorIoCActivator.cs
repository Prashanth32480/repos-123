using ServiceStack.Configuration;
using SimpleInjector;

namespace Grassroots.Identity.Common.DependencyContainer
{
    public class SimpleInjectorIoCActivator : IContainerAdapter
    {
        private readonly Container _container;

        public SimpleInjectorIoCActivator(Container container)
        {
            _container = container;
        }

        public T Resolve<T>()
        {
            return (T)_container.GetInstance(typeof(T));
        }

        public T TryResolve<T>()
        {
            var registration = _container.GetRegistration(typeof(T));
            return registration == null ? default(T) : (T)registration.GetInstance();
        }
    }
}