using StructureMap.Graph;

namespace MagiQL.Service.WebAPI.StructureMap.IoC
{
    public class Registration
    {
        public static void UseDefaultConventions<T>(IAssemblyScanner scan)
        {
            scan.AssemblyContainingType<T>(); 
            scan.WithDefaultConventions();
        }  
    }
}