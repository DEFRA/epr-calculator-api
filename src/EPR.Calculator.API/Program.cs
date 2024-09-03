using EPR.Calculator.API.CommandHandlers;
using EPR.Calculator.API.Commands;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Queries;
using EPR.Calculator.API.QueryHandlers;
using EPR.Calculator.API.Validators;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<CreateDefaultParameterSettingValidator>());
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<ICommandHandler<CreateDefaultParameterCommand>, CreateDefaultParameterCommandHandler>();
builder.Services.AddScoped<IQueryHandler<DefaultParameterSettingDetailQuery, IEnumerable<DefaultSchemeParametersDto>>, DefaultParameterSettingDetailQueryHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
