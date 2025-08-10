CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "DiscordUsers" (
    "Id" numeric(20,0) NOT NULL,
    CONSTRAINT "PK_DiscordUsers" PRIMARY KEY ("Id")
);

CREATE TABLE "HoyolabAccounts" (
    "Id" uuid NOT NULL,
    "DiscordUserId" numeric(20,0) NOT NULL,
    "HoyolabUserId" character varying(10) NOT NULL,
    "Token" character varying(255) NOT NULL,
    "GameRoleId" character varying(10) NOT NULL,
    "Region" character varying(10) NOT NULL,
    CONSTRAINT "PK_HoyolabAccounts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HoyolabAccounts_DiscordUsers_DiscordUserId" FOREIGN KEY ("DiscordUserId") REFERENCES "DiscordUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ActiveHoyolabAccounts" (
    "DiscordUserId" numeric(20,0) NOT NULL,
    "HoyolabAccountId" uuid NOT NULL,
    CONSTRAINT "PK_ActiveHoyolabAccounts" PRIMARY KEY ("DiscordUserId"),
    CONSTRAINT "FK_ActiveHoyolabAccounts_DiscordUsers_DiscordUserId" FOREIGN KEY ("DiscordUserId") REFERENCES "DiscordUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ActiveHoyolabAccounts_HoyolabAccounts_HoyolabAccountId" FOREIGN KEY ("HoyolabAccountId") REFERENCES "HoyolabAccounts" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ActiveHoyolabAccounts_HoyolabAccountId" ON "ActiveHoyolabAccounts" ("HoyolabAccountId");

CREATE INDEX "IX_HoyolabAccounts_DiscordUserId" ON "HoyolabAccounts" ("DiscordUserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250810071314_Initial', '9.0.8');

COMMIT;

