﻿using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Dapr;
using Volo.Abp.EventBus.Dapr;

namespace Volo.Abp.AspNetCore.Mvc.Dapr.EventBus.Controllers;

[Area("abp")]
[RemoteService(Name = "abp")]
public class AbpAspNetCoreMvcDaprEventsController : AbpController
{
    [HttpPost(AbpAspNetCoreMvcDaprPubSubConsts.DaprEventCallbackUrl)]
    public virtual async Task<IActionResult> EventAsync()
    {
        HttpContext.ValidateDaprAppApiToken();

        var daprSerializer = HttpContext.RequestServices.GetRequiredService<IDaprSerializer>();
        var body = (await JsonDocument.ParseAsync(HttpContext.Request.Body));

        var id = body.RootElement.GetProperty("id").GetString();
        var pubSubName = body.RootElement.GetProperty("pubsubname").GetString();
        var topic = body.RootElement.GetProperty("topic").GetString();
        var data = body.RootElement.GetProperty("data").GetRawText();
        if (pubSubName.IsNullOrWhiteSpace() || topic.IsNullOrWhiteSpace() || data.IsNullOrWhiteSpace())
        {
            Logger.LogError("Invalid Dapr event request.");
            return BadRequest();
        }

        var distributedEventBus = HttpContext.RequestServices.GetRequiredService<DaprDistributedEventBus>();

        if (data.Contains("Data") && data.Contains("CorrelationId")) //TODO: Check the json with JSON Schema.
        {
            var abpDaprEventData = daprSerializer.Deserialize(data, typeof(AbpDaprEventData<>).MakeGenericType(distributedEventBus.GetEventType(topic)));
            var eventData = abpDaprEventData.GetType().GetProperties().First(x => x.Name == "Data").GetValue(abpDaprEventData);
            var correlationId = abpDaprEventData.GetType().GetProperties().First(x => x.Name == "CorrelationId").GetValue(abpDaprEventData) as string;
            await distributedEventBus.TriggerHandlersAsync(id, distributedEventBus.GetEventType(topic), eventData, correlationId);
        }
        else
        {
            var eventData = daprSerializer.Deserialize(data, distributedEventBus.GetEventType(topic));
            await distributedEventBus.TriggerHandlersAsync(id, distributedEventBus.GetEventType(topic), eventData, null);
        }
        return Ok();
    }
}
