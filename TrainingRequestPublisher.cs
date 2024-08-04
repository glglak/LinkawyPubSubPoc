using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.Sql;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace TrainingRequestPublisher
{
    public static class SqlTriggerBinding
    {

        private static readonly string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        private static readonly string TopicName = Environment.GetEnvironmentVariable("TopicName");
        private static readonly IQueueClient queueClient = new QueueClient(ServiceBusConnectionString, TopicName);
 

        [FunctionName("TrainingRequestPublisher")]
        public static async Task RunAsync(
                [SqlTrigger("[dbo].[TrainingRequests]", "SqlConnectionString")] IReadOnlyList<SqlChange<TrainingRequest>> changes,
                ILogger log)
        {
            log.LogInformation("SQL Changes: " + JsonConvert.SerializeObject(changes));
            foreach (var change in changes)
            {
                var trainingRequest = new TrainingRequest
                {
                     
                    RequestID = change.Item.RequestID,
                    EmployeeID = change.Item.EmployeeID,
                    CourseID = change.Item.CourseID,
                    Status = change.Item.Status,
                    Progress = change.Item.Progress,
                    CreatedAt = change.Item.CreatedAt,
                    UpdatedAt = change.Item.UpdatedAt
                };

                var messageBody = JsonConvert.SerializeObject(trainingRequest);
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                await queueClient.SendAsync(message);

                log.LogInformation($"Published training request update: {trainingRequest.RequestID}");
            }
        }
        //could be a shared nugget package
        public class TrainingRequest
        {
            public int RequestID { get; set; }
            public int EmployeeID { get; set; }
            public int CourseID { get; set; }
            public string Status { get; set; }
            public string Progress { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }

   
}
