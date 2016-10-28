using System;
using System.Linq;

namespace MagiQL.DataAdapters.Infrastructure.Sql.Functions
{
    public abstract class InlineSqlFunction
    {
        public abstract string Name { get; }
        protected abstract int ArgumentCount { get; }
        protected abstract string FunctionFormat { get; }

        public virtual string Parse(params object[] args)
        {
            if (args == null || args.Count() != ArgumentCount)
            {
                throw new ArgumentException("Function " + Name + " expects " + ArgumentCount + " arguments");
            }

            return string.Format(FunctionFormat, args);
        }
    }
}