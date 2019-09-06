using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Documents;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps
{
    /// <summary>
    /// This recipe step creates custom content definition.
    /// </summary>
    public class ContentDefinitionStep : IRecipeStepHandler
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentDefinitionStep(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "ContentDefinition", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var step = context.Step.ToObject<ContentDefinitionStepModel>();

            foreach (var contentType in step.ContentTypes)
            {
                var newType = _contentDefinitionManager.GetTypeDefinition(contentType.Name)
                    ?? new ContentTypeDefinition(contentType.Name, contentType.DisplayName);

                UpdateContentType(newType, contentType);
            }

            foreach (var contentPart in step.ContentParts)
            {
                var newPart = _contentDefinitionManager.GetPartDefinition(contentPart.Name)
                    ?? new ContentPartDefinition(contentPart.Name);

                UpdateContentPart(newPart, contentPart);
            }

            return Task.CompletedTask;
        }

        private void UpdateContentType(ContentTypeDefinition type, ContentTypeDefinitionDocument record)
        {
            _contentDefinitionManager.AlterTypeDefinition(type.Name, builder =>
            {
                if (!String.IsNullOrEmpty(record.DisplayName))
                {
                    builder.DisplayedAs(record.DisplayName);
                    builder.MergeSettings(record.Settings);
                }

                foreach (var part in record.ContentTypePartDefinitions)
                {
                    builder.WithPart(part.Name, part.PartName, partBuilder => partBuilder.MergeSettings(part.Settings));
                }
            });
        }

        private void UpdateContentPart(ContentPartDefinition part, ContentPartDefinitionDocument record)
        {
            _contentDefinitionManager.AlterPartDefinition(part.Name, builder =>
            {
                builder.MergeSettings(record.Settings);

                foreach (var field in record.ContentPartFieldDefinitions)
                {
                    builder.WithField(field.Name, fieldBuilder =>
                    {
                        fieldBuilder.OfType(field.FieldName);
                        fieldBuilder.MergeSettings(field.Settings);
                    });
                }
            });
        }

        private class ContentDefinitionStepModel
        {
            public ContentTypeDefinitionDocument[] ContentTypes { get; set; } = Array.Empty<ContentTypeDefinitionDocument>();
            public ContentPartDefinitionDocument[] ContentParts { get; set; } = Array.Empty<ContentPartDefinitionDocument>();
        }
    }
}
