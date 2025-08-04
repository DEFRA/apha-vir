using Amazon.CloudWatchLogs;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;


namespace Apha.VIR.Web.Extensions
{
    public static class SerilogExtensions
    {
        public static LoggerConfiguration UseAwsCloudWatch(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            var cloudWatchClient = new AmazonCloudWatchLogsClient();


            var cloudWatchOptions = new CloudWatchSinkOptions
            {
                LogGroupName = configuration["AwsLogging:LogGroupName"],
                LogStreamNameProvider = new DefaultLogStreamProvider(),
                TextFormatter = new RenderedCompactJsonFormatter(),
                MinimumLogEventLevel = LogEventLevel.Error,
                CreateLogGroup = true,
                BatchSizeLimit = 100,
                Period = TimeSpan.FromSeconds(10),
                //LogGroupRetentionPolicy The number of days to retain the log events in the specified log group.
                //BatchSizeLimit  The batch size to be used when uploading logs to AWS CloudWatch. Defaults to 100.
                //QueueSizeLimit  The queue size to be used when holding batched log events in memory. Defaults to 10000.
                //RetryAttempts  The number of attempts to retry in the case of a failure.
            };

            return loggerConfiguration
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .WriteTo.AmazonCloudWatch(cloudWatchOptions, cloudWatchClient);

        }
    }
}
