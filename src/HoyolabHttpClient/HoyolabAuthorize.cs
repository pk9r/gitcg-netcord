namespace HoyolabHttpClient;

public record HoyolabAuthorize(
    string HoyolabUserId, // ltuid_v2 
    string Token // ltoken_v2
);
