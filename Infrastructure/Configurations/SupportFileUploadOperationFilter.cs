using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SupportFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.GetCustomAttributes(typeof(HttpPostAttribute), false).Any() &&
            context.MethodInfo.GetParameters().Any(p => p.ParameterType == typeof(IFormFile)))
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = { ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = { ["file"] = new OpenApiSchema { Type = "string", Format = "binary" } }
                    }
                } }
            };
        }
    }
}