#r "Microsoft.ServiceBus"

using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Text.RegularExpressions;
using System.Net.Http;
using static System.Environment;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

public static async Task Run(TimerInfo myTimer, TraceWriter log)
{
    var namespaceManager = NamespaceManager.CreateFromConnectionString(Env("ServiceBusConnectionString"));

    // foreach(var topic in await namespaceManager.GetTopicsAsync())
    // {
    // foreach(var subscription in await namespaceManager.GetSubscriptionsAsync(topic.Path))
    // {
    // await LogMessageCountsAsync(
    // $"{Escape(topic.Path)}.{Escape(subscription.Name)}",
    // subscription.MessageCountDetails, log);
    // }
    // }
	//check if changes deployed automatically

    foreach(var queue in await namespaceManager.GetQueuesAsync())
    {
        if(queue.MessageCountDetails.DeadLetterMessageCount>0)
        {
            await LogMessageCountsAsync(Escape(queue.Path),queue.MessageCountDetails, log);
        }
    }
}

private static async Task LogMessageCountsAsync(string entityName,MessageCountDetails details, TraceWriter log)
{
    var appinsightkey=Env("AppInsightInstrumentationKey");
    var telemetryClient = new TelemetryClient();
    telemetryClient.InstrumentationKey = appinsightkey;
    var telemetry = new TraceTelemetry(entityName);
    //telemetry.Properties.Add("Active Message Count", details.ActiveMessageCount.ToString());
    telemetry.Properties.Add("Dead Letter Count", details.DeadLetterMessageCount.ToString());
    //telemetryClient.TrackMetric(new MetricTelemetry(entityName+" Active Message Count", details.ActiveMessageCount));
    telemetryClient.TrackMetric(new MetricTelemetry(entityName+"_Dead_Letter_Count", details.DeadLetterMessageCount));
    telemetryClient.TrackTrace(telemetry);
}

private static string Escape(string input) => Regex.Replace(input, @"[^A-Za-z0-9]+", "_");
private static string Env(string name) => GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);