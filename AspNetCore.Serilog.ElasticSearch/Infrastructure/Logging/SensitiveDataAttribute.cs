using Microsoft.Extensions.Compliance.Classification;

namespace AspNetCore.Serilog.ElasticSearch.Infrastructure.Logging;

public sealed class SensitiveDataAttribute() : DataClassificationAttribute(LoggingTaxonomy.SensitiveData);