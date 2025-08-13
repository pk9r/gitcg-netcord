using Microsoft.Extensions.Options;

namespace HoyolabHttpClient.Responses.DecodeCardCode;

public record Result(
    ValidateOptionsResult Validate,
    Data Data = null!
);