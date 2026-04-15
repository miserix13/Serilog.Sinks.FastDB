using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Stellar.Collections;

namespace Serilog.Sinks.FastDB
{
    public class FastdbSink : ILogEventSink, IAsyncDisposable
    {
        private readonly IFormatProvider provider;
        private readonly Stellar.Collections.FastDB dB;
        private readonly IFastDBCollection<Guid, string> logs;

        public FastdbSink(IFormatProvider formatProvider, FastDBOptions options, string collectionName = "logs") :
            base()
        {
            this.provider = formatProvider;
            this.dB = new(options);
            this.logs = this.dB.GetCollection<Guid, string>(collectionName);
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await this.logs.CloseAsync();
            await this.dB.DisposeAsync();
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(this.provider);
            _ = this.logs.Add(Guid.NewGuid(), message);
        }
    }

    public static class FastdbSinkExtensions
    {
        public static LoggerConfiguration FastdbSink(this LoggerConfiguration loggerConfiguration, FastDBOptions options, string collectionName = "logs", IFormatProvider provider = null)
        {
            return loggerConfiguration.FastdbSink(options, collectionName, provider);
        }
    }
}
