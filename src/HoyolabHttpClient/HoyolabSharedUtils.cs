using System.Collections.Generic;

namespace HoyolabHttpClient;

public static class HoyolabSharedUtils
{
    public const int RoleCardCount = 3;
    public const int ActionCardCount = 30;

    // Dictionary mapping URLs to emoji keys
    public static readonly Dictionary<string, string> UrlToEmojis = new()
    {
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/e25f04745615df9e779831a1c1354e38.png"] = "food",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/d92127a21b942663e6ed0717cef1086e.png"] = "location",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/377c4198e3072e9c68066736be5b790c.png"] = "item",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/1643452964b58e2e69e64e2b5d3b5878.png"] = "catalyst",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/49acb071b0d634ded17cb788d9f520ed.png"] = "weapon",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/8abebfa3ce62ff97769616f606b9a7f4.png"] = "artifact",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/c77ba0b2ea2bf428b8414c9f9bc86035.png"] = "technique",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/e1430a8775eb864ed0617b7ef516e608.png"] = "physical_dmg",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/"] = "aligned",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/8b295c3fed21771dcced6055cdf2f2ce.png"] = "aligned",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/39de35b178b080a090ac95622bbbf0df.png"] = "unaligned",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/8401f5013a7c381edb6cd088b207bb9f.png"] = "omni",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/54f12c0eadb091b3d564408471387021.png"] = "energy",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/5bdbe64fa0b51912abf046d26f735401.png"] = "arcane_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/412134fa7fc8a73b5e20c177a3ae0046.png"] = "cryo_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/aebd3583ff696429f2e8d83527149ce3.png"] = "hydro_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/fe4517db495f28fdbcb44607d94640c3.png"] = "pyro_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/0a7a50d15a7580c4de07a41f87a95787.png"] = "electro_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/bff8d36c5fdb0df001e05624aa7b7e97.png"] = "anemo_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/680e755181a0b8c29e87d0570815ac29.png"] = "geo_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/848248aa7c40784633446db172ee4678.png"] = "dendro_die",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/ad05fb6424e58a2a5685647b6240555a.png"] = "arcane",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/924fba80b9137b4e8fdf5871aa22d666.png"] = "cryo",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/b357f624efdec0c1cc0cc4f180e07db4.png"] = "hydro",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/54e95d5fad102409daf191d692c92adc.png"] = "pyro",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/0ff87d31d2858dc563d4a441cdd42f95.png"] = "electro",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/c89ca75b4aef48091d59bf71aaff2b8b.png"] = "anemo",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/98c267c74a3efdcfed4131a054ff4b82.png"] = "geo",
        ["https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/0bfb3be161357095eefb85b88c1267a8.png"] = "dendro",
    };
}
