using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Microsoft.AspNetCore.Cors;
using Question3aConsume.Model;
using Newtonsoft.Json.Linq;



public class ConsumerRabbitMQ : BackgroundService
    {

    private readonly taskItemContext _context;

    


    private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;

        public ConsumerRabbitMQ(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<ConsumerRabbitMQ>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {

                // HostName = "localhost" , 
                // Port = 30724
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))

            };
        Console.WriteLine(factory.HostName + ":" + factory.Port);
        // create connection  
        _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _channel.QueueDeclare("TaskQueue", true, false, false, null);
            // _channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            // _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<QueueItem>(content);
                var mydata = new
                {
                    ID = data.id,
                    TOKEN = data.token,
                    CREATEDTIME = data.time
                };
                Console.WriteLine("fangyuan", data);
                Console.WriteLine(mydata);
                // _context.taskitems.Add();
                // _context.SaveChanges();
                HandleMessage(content);
               //  AddtaskData()
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("TaskQueue", false, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessage(string content)
        {
        // we just print this message

        //  taskItemContext context = new();
        // _context.taskitems.Add();
        _logger.LogInformation($"consumer received {content}");
            Console.WriteLine($"consumer received {content}");
        }
 

    private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }


