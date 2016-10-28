using MagiQL.DataAdapters.Infrastructure.Sql;
using MagiQL.Framework.Interfaces;
using MagiQL.Framework.Renderers.SpreadsheetGenerator;
using MagiQL.Framework.Repositories.Repositories;
using MagiQL.Framework.Services;
using MagiQL.Service.Interfaces;
using StructureMap.Configuration.DSL;

namespace MagiQL.Service.WebAPI.StructureMap.IoC.MagiQL
{
    public class MagiQlRegistry : Registry
    {
        public MagiQlRegistry()
        { 
            IncludeRegistry<FrameworkRegistry>();
            IncludeRegistry<Interfaces>();
            IncludeRegistry<Repositories>();
            IncludeRegistry<SpreadsheetGenerator>();
            //IncludeRegistry<MagiQlDataSourcesRegistry>(); // this is dynamically loaded

            For<ISqlQueryExecutor>().Use<SqlQueryExecutor>();
        }
    }

    class FrameworkRegistry : Registry
    {
        public FrameworkRegistry()
        {
            Scan(Registration.UseDefaultConventions<ReportsService>);
            For<IReportsService>().Use<ReportsService>();
            For<IColumnProvider>().Use<ManagedColumnProvider>();
        }
    }

    class Interfaces : Registry
    {
        public Interfaces()
        {
            Scan(Registration.UseDefaultConventions<ReportsService>);
        }
    }

    class Repositories : Registry
    {
        public Repositories()
        {
            Scan(Registration.UseDefaultConventions<ReportStatusRepository>);
        }
    }

    public class SpreadsheetGenerator : Registry
    {
        public SpreadsheetGenerator()
        {
            Scan(Registration.UseDefaultConventions<SpreadsheetWriter>);
        }

    }
}