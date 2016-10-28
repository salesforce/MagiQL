namespace MagiQL.Framework.Repositories.Dapper
{
    public static class MapperRegistry
    {
        public static bool Initialized = false;

        public static void Initialize()
        {
            if (!Initialized)
            {
                DapperExtensions.DapperExtensions.SetMappingAssemblies(new[] {typeof (ReportColumnMappingMapper).Assembly});

                Initialized = true;
            }
        }
    }
}
