using System;
using StructureMap;

namespace MagiQL.Service.WebAPI.StructureMap
{
    /// <summary>
    /// Global services available throughout the lifetime of this application. 
    /// 
    /// These services are initialized on application pre-start.
    /// </summary>
    public class GlobalServices
    {
        /// <summary>
        /// The StructureMap container for this application.
        /// 
        /// Please use responsibly. This is not meant to be used willy-nilly
        /// as a service locator.
        /// 
        /// Only use this to resolve esoteric compositions roots that live outside
        /// of the WebAPI world.
        /// </summary>
        public static IContainer StructureMapContainer { get; private set; }

        public static void Initialize(IContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            StructureMapContainer = container;
        }
    }
}