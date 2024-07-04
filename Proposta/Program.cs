using FB.Common.Util;
using FB.Platforme.Business.Interface;
using FB.Platforme.Business;
using Proposta;
using Proposta.Transaction;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

ConfigurationAppSettings.ConfigureSettings(builder.Configuration);

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IBaseEventHandler, ExecuteTRA>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<PropostaCreditoRepository>();

var host = builder.Build();
host.Run();
