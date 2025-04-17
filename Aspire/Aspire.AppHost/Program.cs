using Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);


#region Infrastructure
var redis = builder.AddRedis("redis")
                   .WithLifetime(ContainerLifetime.Persistent)
                   .WithRedisInsight();

var postgres = builder.AddPostgres("postgres")
                      .WithLifetime(ContainerLifetime.Persistent);

var basketdb = postgres.AddDatabase("pg-basket");
var catalogdb = postgres.AddDatabase("pg-catalog");
var orderdb = postgres.AddDatabase("pg-order");
var paymentdb = postgres.AddDatabase("pg-payment");
var identitydb = postgres.AddDatabase("pg-identity");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithManagementPlugin();

#endregion Infrastructure


#region Add projects

//Маршрутизація
var apiGateway = builder.AddProject<ApiGateway_WebHost>("api-gateway")
                        .AddCommonConfiguration(builder.Configuration);

//
var auth = builder.AddProject<Auth_WebHost>("auth-api")
                  .AddCommonConfiguration(builder.Configuration);


//Сервіс управління кошиком
var basket = builder.AddProject<Basket_WebHost>("basket-api")
                    .AddCommonConfiguration(builder.Configuration);


//Сервіс каталогу товарів
var catalog = builder.AddProject<Catalog_WebHost>("catalog-api")
                     .AddCommonConfiguration(builder.Configuration);


//Сервіс сповіщень
var notification = builder.AddProject<Notification_WebHost>("notification-api")
                          .AddCommonConfiguration(builder.Configuration);


//Сервіс обробки замовлень
var order = builder.AddProject<Order_WebHost>("order-api")
                   .AddCommonConfiguration(builder.Configuration);


//Сервіс оплат
var payment = builder.AddProject<Payment_WebHost>("payment-api")
                     .AddCommonConfiguration(builder.Configuration);

#endregion Add projects



#region projects references

apiGateway.WithReference(redis).WaitFor(redis)
          .WithReference(order).WaitFor(order)
          .WithReference(basket).WaitFor(basket);

auth.WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(identitydb).WaitFor(identitydb);

basket.WithReference(basketdb).WaitFor(basketdb)
      .WithReference(rabbitmq).WaitFor(rabbitmq);


catalog.WithReference(catalogdb).WaitFor(catalogdb)
       .WithReference(rabbitmq).WaitFor(rabbitmq);


notification.WithReference(rabbitmq).WaitFor(rabbitmq);


order.WithReference(orderdb).WaitFor(orderdb)
     .WithReference(rabbitmq).WaitFor(rabbitmq);


payment.WithReference(paymentdb).WaitFor(paymentdb)
       .WithReference(rabbitmq).WaitFor(rabbitmq);

#endregion projects references


builder.Build().Run();

