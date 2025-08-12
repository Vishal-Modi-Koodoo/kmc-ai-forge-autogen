using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KMC_Forge_BTL_API.Services;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.ApiDescription.ParameterDescriptions
            .Where(x => x.ModelMetadata?.ModelType == typeof(IFormFile) || 
                       x.ModelMetadata?.ModelType == typeof(List<IFormFile>));

        if (fileParameters.Any())
        {
            var content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    "multipart/form-data", new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                {
                                    "files", new OpenApiSchema
                                    {
                                        Type = "array",
                                        Items = new OpenApiSchema
                                        {
                                            Type = "string",
                                            Format = "binary"
                                        },
                                        Description = "Multiple files to upload"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = content,
                Required = true
            };
        }
    }
}
