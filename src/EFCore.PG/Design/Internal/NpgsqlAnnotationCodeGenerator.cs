﻿using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class NpgsqlAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        public NpgsqlAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema
                && string.Equals("public", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override string GenerateFluentApi(IModel model, IAnnotation annotation, string language)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(language, nameof(language));

            if (language != "CSharp")
                return null;

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix))
            {
                var extension = new PostgresExtension(model, annotation.Name);
                return $".{nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension)}(\"{extension.Name}\")";
            }
            return null;
        }

        // TODO: Implement GenerateFluentApi for Npgsql-specific stuff (index method)
    }
}