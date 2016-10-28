using System;
using System.Collections.Generic;
using System.Linq;
using MagiQL.Framework.Interfaces.Renderers;
using MagiQL.Framework.Renderers.RenderFilters;
using MagiQL.Framework.Renderers.RenderFilters.DataFormatRenderFilters;

namespace MagiQL.Framework.Renderers
{
    public class RenderFilterFactory : IRenderFilterFactory
    {
        public RenderFilterFactory()
        {
            if (!_registry.Any())
            {
                Initialize();
            }
        }

        private static List<IRenderFilter> _registry = new List<IRenderFilter>();

        public void Register<T>(T filter = default(T)) where T : IRenderFilter
        {
            if (!_registry.OfType<T>().Any())
            {
                if (filter == null)
                {
                    filter = Activator.CreateInstance<T>();
                }
                _registry.Add(filter);
            }
        }

        public void UnRegister<T>() where T : IRenderFilter
        {
            if (_registry.OfType<T>().Any())
            {
                _registry = _registry.Where(x => x.GetType() != typeof(T)).ToList();
            }
        }

        // The filters will be processed in the order they are returned
        public List<IRenderFilter> GetFilters()
        { 
            return _registry;
        }

        public void Initialize()
        {
            Register<PrecisionRenderFilter>();
            
            // data format
            Register<BooleanDataFormatRenderFilter>();
            Register<CurrencyDataFormatRenderFilter>();
            Register<PercentageDataFormatRenderFilter>();
            Register<UtcDateTimeDataFormatRenderFilter>();
        }
    }
}
