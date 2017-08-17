using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace UtilityDelta.EFCore.Database
{
    public class SqlLoggerProvider : ILoggerProvider
    {
        private List<string> m_sql;

        public SqlLoggerProvider(List<string> sql)
        {
            m_sql = sql;
        }

        public void Dispose()
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(m_sql);
        }

        private class MyLogger : ILogger
        {
            private List<string> m_sql;

            public MyLogger(List<string> sql)
            {
                m_sql = sql;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (eventId.Name != "Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")
                {
                    return;
                }

                var data = state as IEnumerable<KeyValuePair<string, object>>;
                if (data != null)
                {
                    m_sql.Add(data.Single(p => p.Key == "commandText").Value.ToString());
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}
