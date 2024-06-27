using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IronPdf.License.LicenseKey = "IRONSUITE.TUENXSE150551.FPT.EDU.VN.26983-385B17840D-CA4A2NU-7OWVH2YJBNFK-ZV3PIF25B4QG-U3VMLL27KBVY-SR6ICHLWLSPI-SOIYZA47QTHT-B6NOUDEQSUWJ-YOLONT-TDVIDHFN4B6NEA-DEPLOYMENT.TRIAL-P74X2C.TRIAL.EXPIRES.25.JUL.2024";
builder.Services.AddControllers();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    }
);
builder.Services.AddSwaggerGenNewtonsoftSupport();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddDependenceInjection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseJwt();

app.UseAuthorization();

app.MapControllers();

    app.Run();
