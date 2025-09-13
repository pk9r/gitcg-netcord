using HoyolabHttpClient.Configuration;
using HoyolabHttpClient.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Data = HoyolabHttpClient.Responses.GcgBasicInfo.Data;

namespace HoyolabHttpClient;

public class HoyolabHttpClientService
{
    private const string LoginKeyCert =
        """
        -----BEGIN PUBLIC KEY-----
        MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4PMS2JVMwBsOIrYWRluY
        wEiFZL7Aphtm9z5Eu/anzJ09nB00uhW+ScrDWFECPwpQto/GlOJYCUwVM/raQpAj
        /xvcjK5tNVzzK94mhk+j9RiQ+aWHaTXmOgurhxSp3YbwlRDvOgcq5yPiTz0+kSeK
        ZJcGeJ95bvJ+hJ/UMP0Zx2qB5PElZmiKvfiNqVUk8A8oxLJdBB5eCpqWV6CUqDKQ
        KSQP4sM0mZvQ1Sr4UcACVcYgYnCbTZMWhJTWkrNXqI8TMomekgny3y+d6NX/cFa6
        6jozFIF4HCX5aW8bp8C8vq2tFvFbleQ/Q3CU56EWWKMrOcpmFtRmC18s9biZBVR/
        8QIDAQAB
        -----END PUBLIC KEY-----
        """;

    private const string WebLoginByPasswordUri = "account/ma-passport/api/webLoginByPassword";

    private const string GetGameRecordCardUri = "event/game_record/card/wapi/getGameRecordCard";

    private const string DeckListUri = "event/game_record/genshin/api/gcg/deckList";
    private const string GcgBasicInfoUri = "event/game_record/genshin/api/gcg/basicInfo";
    private const string GcgCardListUri = "event/game_record/genshin/api/gcg/cardList";

    private const string RoleSkillUri = "event/cardsquare/role/skill";
    private const string ActionSkillUri = "event/cardsquare/action/skill";

    private const string CardRolesUri = "event/cardsquare/roles";
    private const string CardActionsUri = "event/cardsquare/actions";

    private const string EncodeCardCodeUri = "event/cardsquare/encode_card_code";
    private const string DecodeCardCodeUri = "event/cardsquare/decode_card_code";

    private readonly HoyolabAuthorize? _defaultAuthorize;
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public HoyolabHttpClientService(
        IOptions<HoyolabHttpClientOptions> options,
        HttpClient httpClient,
        IHttpClientFactory httpClientFactory
    )
    {
        _httpClient = httpClient;
        _httpClientFactory = httpClientFactory;
        _defaultAuthorize = options.Value.DefaultAuthorize;

        _httpClient.BaseAddress = new Uri(
            uriString: "https://sg-public-api.hoyolab.com"
        );

        if (_defaultAuthorize == null)
            return;

        ConfigAuthorizeClient(
            client: _httpClient,
            authorize: _defaultAuthorize
        );
    }

    //public async Task<IEnumerable<IGrouping<int, Role>>>
    //    GetRolesGroupByElement(string lang)
    //{
    //    var roles = await GetRolesAsync(lang);
    //    var rolesGroupByElement = roles
    //        .GroupBy(role => role.Element);
    //    return rolesGroupByElement;
    //}

    public async Task<Responses.WebLoginByPassword.Result>
        WebLoginByPasswordAsync(string account, string password)
    {
        var httpClient = CreateHttpClient();

        httpClient.DefaultRequestHeaders.Add(
            name: "x-rpc-app_id",
            value: "c9oqaq3s3gu8"
        );
        httpClient.DefaultRequestHeaders.Add(
            name: "x-rpc-device_id",
            value: "0876c024-b7bd-4c9f-9175-c35b8766ccb0"
        );
        httpClient.DefaultRequestHeaders.Add(
            name: "Referer",
            value: "https://account.hoyolab.com/"
        );

        var payload = new { account = LoginEncrypt(account), password = LoginEncrypt(password), token_type = 6 };

        using var response = await httpClient.PostAsJsonAsync(
            requestUri: WebLoginByPasswordUri,
            value: payload
        );

        response.EnsureSuccessStatusCode();

        var value = await response.Content.ReadFromJsonAsync<
            Responses.WebLoginByPassword.Response
        >();

        ThrowIfNotSuccess(value);

        var setCookie = response.Headers
            .GetValues(name: "Set-Cookie")
            .ToList();

        var uid = setCookie.First(x => x.StartsWith("ltuid_v2="));
        uid = uid[9..uid.IndexOf(';')];
        var token = setCookie.First(x => x.StartsWith("ltoken_v2="));
        token = token[10..token.IndexOf(';')];

        var authorize = new HoyolabAuthorize(uid, token);

        return new Responses.WebLoginByPassword.Result(authorize, value.Data!);
    }


    public async Task<Data>
        GetGcgBasicInfoAsync(
            string server, // Region, e.g. "os_asia", "os_europe", "os_america"
            string roleId, // Genshin Impact UID 
            HoyolabAuthorize? authorize = null
        )
    {
        var httpClient = _httpClient;
        if (authorize != null)
        {
            httpClient = CreateHttpClient();
            ConfigAuthorizeClient(httpClient, authorize);
        }

        var uri = GcgBasicInfoUri;
        uri = QueryHelpers.AddQueryString(uri, "server", server);
        uri = QueryHelpers.AddQueryString(uri, "role_id", roleId);

        using var response = await httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.GcgBasicInfo.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.GcgCardList.Data>
        GetGcgCardListAsync(
            string server, // Region, e.g. "os_asia", "os_europe", "os_america"
            string roleId, // Genshin Impact UID
            bool needAvatar = true,
            bool needAction = true,
            bool needStats = true,
            int offset = 0,
            int limit = 5000,
            HoyolabAuthorize? authorize = null
        )
    {
        var httpClient = _httpClient;
        if (authorize != null)
        {
            httpClient = CreateHttpClient();
            ConfigAuthorizeClient(httpClient, authorize);
        }

        var uri = GcgCardListUri;
        uri = QueryHelpers.AddQueryString(uri, "server", server);
        uri = QueryHelpers.AddQueryString(uri, "role_id", roleId);
        uri = QueryHelpers.AddQueryString(uri, "need_avatar",
            needAvatar.ToString().ToLowerInvariant());
        uri = QueryHelpers.AddQueryString(uri, "need_action",
            needAction.ToString().ToLowerInvariant());
        uri = QueryHelpers.AddQueryString(uri, "need_stats",
            needStats.ToString().ToLowerInvariant());
        uri = QueryHelpers.AddQueryString(uri, "offset", offset.ToString());
        uri = QueryHelpers.AddQueryString(uri, "limit", limit.ToString());

        using var response = await httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.GcgCardList.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.DeckList.Data>
        GetDeckListAsync(
            HoyolabAuthorize authorize,
            string roleId, // Genshin Impact UID 
            string server // Region, e.g. "os_asia", "os_europe", "os_america"
        )
    {
        var uri = DeckListUri;
        uri = QueryHelpers.AddQueryString(uri, "role_id", roleId);
        uri = QueryHelpers.AddQueryString(uri, "server", server);

        var httpClient = CreateHttpClient();

        ConfigAuthorizeClient(httpClient, authorize);

        using var response = await httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.DeckList.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.GameRecordCard.Data>
        GetGameRecordCardAsync(HoyolabAuthorize authorize)
    {
        var uri = GetGameRecordCardUri;
        uri = QueryHelpers.AddQueryString(uri, "uid", authorize.HoyolabUserId);

        var httpClient = CreateHttpClient();

        httpClient.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: GetCookieAuthorize(authorize)
        );

        using var response = await httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.GameRecordCard.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.Skills.Data>
        GetRoleSkillAsync(int id, string lang = "en-us")
    {
        var uri = RoleSkillUri;
        AddQueryStringToUri(ref uri, "id", id.ToString());
        AddLangQueryToUri(ref uri, lang);

        using var response = await _httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.Skills.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.ActionSkill.Data>
        GetActionSkillAsync(int id, string lang = "en-us")
    {
        var uri = ActionSkillUri;
        AddQueryStringToUri(ref uri, "id", id.ToString());
        AddQueryStringToUri(ref uri, "lang", lang);

        using var response = await _httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.ActionSkill.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.CardRoles.Data>
        GetCardRolesAsync(string lang = "en-us")
    {
        var uri = CardRolesUri;
        AddLangQueryToUri(ref uri, lang);

        using var response = await _httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.CardRoles.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.CardActions.Data>
        GetCardActionsAsync(
            IEnumerable<int> roleIds,
            string lang = "en-us"
        )
    {
        var uri = CardActionsUri;
        AddLangQueryToUri(ref uri, lang);

        var value = new { role_ids = roleIds };

        using var response = await _httpClient
            .PostAsJsonAsync(requestUri: uri, value: value);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<Responses.CardActions.Response>();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.EncodeCardCode.Data>
        EncodeCardCodeAsync(
            IEnumerable<int> roleCards,
            IEnumerable<int> actionCards,
            string lang = "en-us"
        )
    {
        var uri = EncodeCardCodeUri;
        AddLangQueryToUri(ref uri, lang);

        var value = new
        {
            role_cards = roleCards, //
            action_cards = actionCards //
        };

        using var response = await _httpClient
            .PostAsJsonAsync(uri, value);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<
                Responses.EncodeCardCode.Response
            >();

        ThrowIfNotSuccess(result);

        return result.Data!;
    }

    public async Task<Responses.DecodeCardCode.Result>
        DecodeCardCodeAsync(
            string code,
            string lang = "en-us"
        )
    {
        var resultBuilder = new ValidateOptionsResultBuilder();

        var uri = DecodeCardCodeUri;
        AddLangQueryToUri(ref uri, lang);

        var value = new { code };

        using var response = await _httpClient
            .PostAsJsonAsync(requestUri: uri, value: value);

        if (!response.IsSuccessStatusCode)
        {
            resultBuilder.AddError(
                error: $"Failed to decode card code: {response.ReasonPhrase}"
            );

            return new Responses.DecodeCardCode.Result(
                Validate: resultBuilder.Build()
            );
        }

        var result = (await response.Content
            .ReadFromJsonAsync<Responses.DecodeCardCode.Response>())!;

        // ThrowIfNotSuccess(result);
        if (result.Retcode != 0)
        {
            resultBuilder.AddError(
                error: $"Hoyolab API return non-zero Retcode: {result.Retcode}. Message: {result.Message}."
            );

            return new Responses.DecodeCardCode.Result(
                Validate: resultBuilder.Build()
            );
        }

        return new Responses.DecodeCardCode.Result(
            Validate: resultBuilder.Build(),
            Data: result.Data!
        );
    }

    private static void AddQueryStringToUri(
        ref string uri,
        string name,
        string value
    )
    {
        uri = QueryHelpers.AddQueryString(
            uri: uri,
            name: name,
            value: value
        );
    }

    private static void AddLangQueryToUri(
        ref string uri, string lang
    )
    {
        AddQueryStringToUri(ref uri, "lang", lang);
    }

    private static void ThrowIfNotSuccess(
        [NotNull] HoyolabResponseBase? response
    )
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.Retcode == 0)
            return;

        var message =
            $"""
             Hoyolab API return non-zero Retcode: {response.Retcode}.
             Message: {response.Message}.
             """;

        throw new Exception(message);
    }

    private static string LoginEncrypt(string source)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(LoginKeyCert);

        var data = Encoding.UTF8.GetBytes(source);
        var encryptedData = rsa.Encrypt(
            data: data,
            padding: RSAEncryptionPadding.Pkcs1
        );

        return Convert.ToBase64String(encryptedData);
    }

    private static string GetCookieAuthorize(HoyolabAuthorize authorize)
    {
        return $"ltuid_v2={authorize.HoyolabUserId}; ltoken_v2={authorize.Token}";
    }

    private static void ConfigAuthorizeClient(
        HttpClient client,
        HoyolabAuthorize authorize
    )
    {
        var cookieBuilder = new StringBuilder();
        cookieBuilder.Append(GetCookieAuthorize(authorize));

        client.DefaultRequestHeaders.Add(
            name: "Cookie",
            value: cookieBuilder.ToString()
        );
        client.DefaultRequestHeaders.Add(
            name: "x-rpc-language",
            value: "en-us"
        );
    }

    private HttpClient CreateHttpClient()
    {
        var httpClient = _httpClientFactory
            .CreateClient("HoyolabHttpClient");
        httpClient.BaseAddress = _httpClient.BaseAddress;

        return httpClient;
    }
}