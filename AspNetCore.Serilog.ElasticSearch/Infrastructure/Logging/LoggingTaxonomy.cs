using Microsoft.Extensions.Compliance.Classification;

namespace AspNetCore.Serilog.ElasticSearch.Infrastructure.Logging;

public static class LoggingTaxonomy
{
    private static readonly string Name = "CustomTaxonomy";

    public static DataClassification SensitiveData => new(Name, nameof(SensitiveData));
}