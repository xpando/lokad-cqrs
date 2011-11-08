using System;
using Lokad.Cqrs.Build.Engine;

namespace Lokad.Cqrs
{
    public static class ExtendCqrsEngineBuilder
    {
        public static void SqlServer(this CqrsEngineBuilder @this, Action<SqlEngineModule> config)
        {
            var module = new SqlEngineModule();
            config(module);
            @this.Advanced.RegisterModule(module);
        }
    }
}