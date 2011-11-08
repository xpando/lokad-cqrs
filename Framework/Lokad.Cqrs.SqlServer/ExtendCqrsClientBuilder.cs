using System;
using Lokad.Cqrs.Build;
using Lokad.Cqrs.Build.Client;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsClientBuilder
    {
        public static void SqlServer(this CqrsClientBuilder @this, Action<SqlClientModule> config)
        {
            var module = new SqlClientModule();
            config(module);
            @this.Advanced.RegisterModule(module);
        }
    }
}