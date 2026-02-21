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

var minio = builder.AddContainer("minio", "minio/minio")
                   .WithArgs("server", "/data", "--console-address", ":9001")
                   .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
                   .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
                   .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
                   .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "console")
                   .WithLifetime(ContainerLifetime.Persistent);

#endregion Infrastructure


#region Add projects

var apiGateway = builder.AddProject<ApiGateway_WebHost>("api-gateway")
                        .AddCommonConfiguration(builder.Configuration)
                        .WithHttpHealthCheck("/health");


var auth = builder.AddProject<Auth_WebHost>("auth-api")
                  .AddCommonConfiguration(builder.Configuration)
                  .WithHttpHealthCheck("/health");


var basket = builder.AddProject<Basket_WebHost>("basket-api")
                    .AddCommonConfiguration(builder.Configuration)
                    .WithHttpHealthCheck("/health");


var catalog = builder.AddProject<Catalog_WebHost>("catalog-api")
                     .AddCommonConfiguration(builder.Configuration)
                     .WithHttpHealthCheck("/health");


var notification = builder.AddProject<Notification_WebHost>("notification-api")
                          .AddCommonConfiguration(builder.Configuration)
                          .WithHttpHealthCheck("/health");


var order = builder.AddProject<Order_WebHost>("order-api")
                   .AddCommonConfiguration(builder.Configuration)
                   .WithHttpHealthCheck("/health");


var payment = builder.AddProject<Payment_WebHost>("payment-api")
                     .AddCommonConfiguration(builder.Configuration)
                     .WithHttpHealthCheck("/health");

#endregion Add projects



#region projects references

apiGateway.WithReference(redis).WaitFor(redis)
          .WithReference(auth).WaitFor(auth)
          .WithReference(catalog).WaitFor(catalog)
          .WithReference(basket).WaitFor(basket)
          .WithReference(order).WaitFor(order)
          .WithReference(payment).WaitFor(payment)
          .WithReference(notification).WaitFor(notification)
          .WithReference(minio.GetEndpoint("api")).WaitFor(minio);

auth.WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(identitydb).WaitFor(identitydb);

basket.WithReference(redis).WaitFor(redis)
      .WithReference(rabbitmq).WaitFor(rabbitmq);


catalog.WithReference(redis).WaitFor(redis)
       .WithReference(catalogdb).WaitFor(catalogdb)
       .WithReference(rabbitmq).WaitFor(rabbitmq)
       .WithReference(minio.GetEndpoint("api")).WaitFor(minio);


notification.WithReference(rabbitmq).WaitFor(rabbitmq);


order.WithReference(orderdb).WaitFor(orderdb)
     .WithReference(rabbitmq).WaitFor(rabbitmq)
     .WithReference(catalog).WaitFor(catalog);


payment.WithReference(paymentdb).WaitFor(paymentdb)
       .WithReference(rabbitmq).WaitFor(rabbitmq);

#endregion projects references


#region Frontend

var web = builder.AddViteApp("web", "../../Web/marinelacespace-web", "dev")
                .WithReference(apiGateway)
                //.WaitFor(apiGateway)
                .WithExternalHttpEndpoints();

#endregion Frontend


await builder.Build().RunAsync();

