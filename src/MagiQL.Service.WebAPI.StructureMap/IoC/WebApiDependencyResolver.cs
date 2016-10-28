using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using StructureMap;

namespace MagiQL.Service.WebAPI.StructureMap.IoC
{
    /// <summary>
    /// An implementation of the WebAPI IDependencyResolver that uses
    /// StructureMap to resolve the requested components.
    /// </summary>
    public class WebApiDependencyResolver : IDependencyResolver
    {
        private bool disposed;
        private readonly IContainer container;
        private readonly bool isNestedResolver;

        /// <summary>
        /// Creates a new instance of a WebAPI IDependencyResolver that uses
        /// StructureMap to resolve the requested components.
        /// </summary>
        public WebApiDependencyResolver(IContainer container, bool isNestedResolver)
        {
            if (container == null) throw new ArgumentNullException("container");

            this.container = container;
            this.isNestedResolver = isNestedResolver;
        }
          
        public object GetService(Type serviceType)
        {
            if (disposed) throw new ObjectDisposedException("WebApiDependencyResolver");
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            
            
            if (serviceType.IsAbstract || serviceType.IsInterface)
            {
                return container.TryGetInstance(serviceType);
            }

            
            return container.GetInstance(serviceType);
             
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return container.GetAllInstances(serviceType).Cast<object>();
        }

        public IDependencyScope BeginScope()
        {
            if (disposed)
                throw new ObjectDisposedException("WebApiDependencyResolver");

            return new WebApiDependencyResolver(container.GetNestedContainer(), true);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            // We only want to dispose the nested DI containers that we created ourselves.
            // The top-level DI container that was provided to us on first instantiation
            // should be disposed of by the application itself on shutdown. We should not
            // mess with (or dispose!) objects that have been created by someone else.
            if (!isNestedResolver)
                return;

            container.Dispose();
            disposed = true;
        }
    }
}