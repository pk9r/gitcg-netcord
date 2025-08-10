namespace HoyolabHttpClient.Responses.WebLoginByPassword;

public record Result(
    HoyolabAuthorize Authorize,
    Data Data
);
