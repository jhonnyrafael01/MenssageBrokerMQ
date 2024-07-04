using Cartao;
using Cartao.Transaction;
using FB.Common.Util;
using FB.Platforme.Business;
using FB.Platforme.Business.Interface;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
ConfigurationAppSettings.ConfigureSettings(builder.Configuration);

builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IBaseEventHandler, ExecuteTRA>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<CartaoRepository>();

var host = builder.Build();
host.Run();
