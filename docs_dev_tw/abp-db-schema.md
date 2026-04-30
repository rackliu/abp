# ABP DB Schema 文件

## 文件說明

本文件依據此 repo 內的 EF Core ModelBuilder、Entity 類別、SQL Server migration 與 ABP 慣例欄位整理資料庫結構。

- 本文件只涵蓋此 repo 可見的開源模組資料表。
- 此 repo 不含 ABP Commercial 專屬模組原始碼。
- 下方逐欄位表格仍保留 EF / CLR 邏輯型態，方便對照程式碼與 Entity 定義。
- 若要以 DBA 角度閱讀實體欄位型別、長度、索引、唯一鍵、外鍵與 migration 來源，請併讀本文件新增的 DBA 補充章節。
- 若欄位是由 ABP ConfigureByConvention 自動帶入，仍視為實際 schema 欄位並列入。
- 預設值欄位只列 EF 映射可明確確認的值；若只是建構子初始值或執行階段邏輯，則不視為資料庫預設值。

## DBA 補充說明

### SQL Server 型別與長度判讀規則

本 repo 的 migration 主要以 SQL Server provider 產生；若沒有特別標示，以下 SQL 型別/長度規則可直接用來把下方 CLR 欄位表換讀成 DBA 視角。

| EF / CLR 映射 | SQL Server 型別 | 長度 / 精度規則 | 常見欄位 |
|---|---|---|---|
| Guid | uniqueidentifier | 固定 16 byte | Id、TenantId、CreatorId |
| string + HasMaxLength(n) | nvarchar(n) | 以 HasMaxLength 為準 | UserName、Name、Slug |
| string 無 HasMaxLength | nvarchar(max) | 無固定上限 | ExtraProperties、Content、Data |
| bool | bit | 0 / 1 | IsDeleted、IsActive |
| int | int | 32-bit | AccessFailedCount、ExecutionDuration |
| short | smallint | 16-bit | StarCount |
| long | bigint | 64-bit | Size |
| DateTime | datetime2 | 7 位小數精度 | CreationTime、DeletionTime |
| DateTimeOffset | datetimeoffset | 含時區位移 | LockoutEnd、LastPasswordChangeTime |
| byte[] | varbinary(max) | 二進位大型欄位 | Blob Content |
| JSON / ExtraProperties | nvarchar(max) | 文字儲存 JSON | ExtraProperties、DisplayNames |

### 實務使用原則

1. 欄位 SQL 型別與長度以 migration 檔中的 CreateTable 定義為最終依據。
2. 索引、唯一鍵與外鍵以 migration 中的 CreateIndex / ForeignKey 與 ModelBuilder 的 HasIndex / HasForeignKey 交叉驗證。
3. 同一張表若出現在多個 host 或 template migration，優先看最接近該模組正式 demo / host 的 migration。
4. PostgreSQL、MySQL 等其他 provider 會轉譯成不同實體型別；本文件的 DBA 補充以 SQL Server 為基準。

### Migration 對照矩陣

| 模組範圍 | 主要資料表 | 代表 migration 檔案 | 說明 |
|---|---|---|---|
| Framework / Core / Identity / Permission / Setting / Tenant / OpenIddict | AbpUsers、AbpPermissionGrants、AbpSettings、AbpTenants、OpenIddictApplications 等 | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs | 目前 repo 內最完整、最容易一次看到核心開源表 SQL Server DDL 的整合 migration。 |
| Framework / Core / Background Jobs | AbpBackgroundJobs、AbpAuditLogs 等 | templates/app/aspnet-core/src/MyCompanyName.MyProjectName.EntityFrameworkCore/Migrations/20260220031648_Initial.cs | 典型分層模板的 SQL Server 初始 migration。 |
| Docs | DocsProjects、DocsDocuments、DocsDocumentContributors、DocsProjectPdfFiles | modules/docs/app/VoloDocs.EntityFrameworkCore/Migrations/20250411070040_initial.cs | Docs 模組初始表結構。 |
| Docs 補充 | DocsProjectPdfFiles | modules/docs/app/VoloDocs.EntityFrameworkCore/Migrations/20250425082043_add_pdf_file.cs | PDF 檔案表後續增補。 |
| Docs 後續同步 | Docs 模組整體 | modules/docs/app/VoloDocs.EntityFrameworkCore/Migrations/20251031015447_ABP10.cs、20251031064909_ABP10_1.cs、20260227074745_ABP10_2.cs | 用於追蹤 ABP 10.x 後的 schema 差異。 |
| Blogging | BlgBlogs、BlgPosts | modules/blogging/app/Volo.BloggingTestApp.EntityFrameworkCore/Migrations/20180621080811_Added_Blog_And_Post.cs | Blogging 初始主表。 |
| Blogging 補充 | BlgComments、BlgTags、BlgPostTags | modules/blogging/app/Volo.BloggingTestApp.EntityFrameworkCore/Migrations/20180816103331_Added_Tags_And_Comments.cs | 留言與標籤表。 |
| Blogging 補充 | BlgUsers | modules/blogging/app/Volo.BloggingTestApp.EntityFrameworkCore/Migrations/20180912113852_Added_BlogUsers.cs | 作者資料表。 |
| Blogging 後續欄位 | CoverImage、Url、Description、Blob 等 | modules/blogging/app/Volo.BloggingTestApp.EntityFrameworkCore/Migrations/*.cs | 此模組透過多支 migration 演進，DBA 若要追版本差異需逐支閱讀。 |
| CMS Kit | CmsBlogs、CmsBlogPosts、CmsPages、CmsComments、CmsRatings 等 | modules/cms-kit/host/Volo.CmsKit.HttpApi.Host/Migrations/20220504032110_Initial.cs | CMS Kit 初始表結構。 |
| CMS Kit 後續變更 | CmsPages.Status | modules/cms-kit/host/Volo.CmsKit.HttpApi.Host/Migrations/20251024065316_Status_Field_Added_To_Pages.cs | Pages 狀態欄位後續加入。 |
| Blob Storing Database | AbpBlobContainers、AbpBlobs | modules/blob-storing-database/host/BlobStoring.Database.Host.ConsoleApp/src/BlobStoring.Database.Host.ConsoleApp.ConsoleApp/Migrations/20201013055337_Initial.cs | Blob Database provider 的代表 migration。 |
| Background Jobs | AbpBackgroundJobs | modules/background-jobs/app/Volo.Abp.BackgroundJobs.DemoApp/Migrations/20260320082618_Initial.cs | Background Jobs 模組獨立 migration。 |
| IdentityServer Legacy | IdentityServerClients、IdentityServerApiResources、IdentityServerPersistedGrants 等 | modules/cms-kit/host/Volo.CmsKit.IdentityServer/Migrations/20220504032505_Initial.cs | repo 內可直接查到 IdentityServer legacy 表結構的代表 SQL Server migration。 |
| OpenIddict 模組獨立驗證 | OpenIddictApplications、OpenIddictScopes、OpenIddictTokens | modules/openiddict/app/OpenIddict.Demo.Server/Migrations/20260311061448_Initial.cs | 若要只看 OpenIddict 模組本身，這支 migration 最直接。 |

### 索引 / 唯一鍵 / 外鍵快查

下表只列顯式定義或 DBA 最常關心的 PK / UK / FK / Index。未列者通常只有主鍵或沿用 ABP / ASP.NET Core Identity 慣例索引，請回看對應 migration。

若你要產出可直接交付 DBA 的最終 DDL 文件，仍建議以該模組的 SQL Server migration CreateTable / CreateIndex 內容為主，本表定位是「快查與回溯入口」，不是完整 DDL 轉抄。

| 資料表 | 索引 / 唯一鍵 | 外鍵 | 主要來源 |
|---|---|---|---|
| AbpEventOutbox | IX_AbpEventOutbox_CreationTime | 無 | framework/src/Volo.Abp.EntityFrameworkCore/Volo/Abp/EntityFrameworkCore/DistributedEvents/EventOutboxDbContextModelBuilderExtensions.cs |
| AbpEventInbox | IX_AbpEventInbox_Status_CreationTime、IX_AbpEventInbox_MessageId | 無 | framework/src/Volo.Abp.EntityFrameworkCore/Volo/Abp/EntityFrameworkCore/DistributedEvents/EventInboxDbContextModelBuilderExtensions.cs |
| AbpAuditLogActions | PK_AbpAuditLogActions | FK_AbpAuditLogActions_AbpAuditLogs_AuditLogId | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpEntityChanges | PK_AbpEntityChanges | FK_AbpEntityChanges_AbpAuditLogs_AuditLogId | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpEntityPropertyChanges | PK_AbpEntityPropertyChanges | FK_AbpEntityPropertyChanges_AbpEntityChanges_EntityChangeId | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpBlobs | IX_AbpBlobs_ContainerId_Name 通常為唯一查找組合，實際以 migration 為準 | FK_AbpBlobs_AbpBlobContainers_ContainerId | modules/blob-storing-database/host/BlobStoring.Database.Host.ConsoleApp/src/BlobStoring.Database.Host.ConsoleApp.ConsoleApp/Migrations/20201013055337_Initial.cs |
| AbpFeatureValues | UX(Name, ProviderName, ProviderKey) | 無 | modules/feature-management/src/Volo.Abp.FeatureManagement.EntityFrameworkCore/Volo/Abp/FeatureManagement/EntityFrameworkCore/FeatureManagementDbContextModelCreatingExtensions.cs |
| AbpFeatureGroups | UX(Name) | 無 | modules/feature-management/src/Volo.Abp.FeatureManagement.EntityFrameworkCore/Volo/Abp/FeatureManagement/EntityFrameworkCore/FeatureManagementDbContextModelCreatingExtensions.cs |
| AbpFeatures | UX(Name)、IX(GroupName) | 無 | modules/feature-management/src/Volo.Abp.FeatureManagement.EntityFrameworkCore/Volo/Abp/FeatureManagement/EntityFrameworkCore/FeatureManagementDbContextModelCreatingExtensions.cs |
| AbpPermissionGrants | UX(TenantId, Name, ProviderName, ProviderKey) | 無 | modules/permission-management/src/Volo.Abp.PermissionManagement.EntityFrameworkCore/Volo/Abp/PermissionManagement/EntityFrameworkCore/AbpPermissionManagementDbContextModelBuilderExtensions.cs |
| AbpResourcePermissionGrants | UX(TenantId, Name, ResourceName, ResourceKey, ProviderName, ProviderKey) | 無 | modules/permission-management/src/Volo.Abp.PermissionManagement.EntityFrameworkCore/Volo/Abp/PermissionManagement/EntityFrameworkCore/AbpPermissionManagementDbContextModelBuilderExtensions.cs |
| AbpPermissionGroups | UX(Name) | 無 | modules/permission-management/src/Volo.Abp.PermissionManagement.EntityFrameworkCore/Volo/Abp/PermissionManagement/EntityFrameworkCore/AbpPermissionManagementDbContextModelBuilderExtensions.cs |
| AbpPermissions | UX(ResourceName, Name)、IX(GroupName) | 無 | modules/permission-management/src/Volo.Abp.PermissionManagement.EntityFrameworkCore/Volo/Abp/PermissionManagement/EntityFrameworkCore/AbpPermissionManagementDbContextModelBuilderExtensions.cs |
| AbpSettings | UX(Name, ProviderName, ProviderKey) | 無 | modules/setting-management/src/Volo.Abp.SettingManagement.EntityFrameworkCore/Volo/Abp/SettingManagement/EntityFrameworkCore/SettingManagementDbContextModelBuilderExtensions.cs |
| AbpSettingDefinitions | UX(Name) | 無 | modules/setting-management/src/Volo.Abp.SettingManagement.EntityFrameworkCore/Volo/Abp/SettingManagement/EntityFrameworkCore/SettingManagementDbContextModelBuilderExtensions.cs |
| AbpTenants | IX(Name)、IX(NormalizedName) | AbpTenantConnectionStrings.TenantId -> AbpTenants.Id | modules/tenant-management/src/Volo.Abp.TenantManagement.EntityFrameworkCore/Volo/Abp/TenantManagement/EntityFrameworkCore/AbpTenantManagementDbContextModelCreatingExtensions.cs |
| AbpTenantConnectionStrings | PK(TenantId, Name) 類型組合主鍵 | FK_AbpTenantConnectionStrings_AbpTenants_TenantId | modules/tenant-management/src/Volo.Abp.TenantManagement.EntityFrameworkCore/Volo/Abp/TenantManagement/EntityFrameworkCore/AbpTenantManagementDbContextModelCreatingExtensions.cs |
| AbpUsers | IX_Email、IX_NormalizedEmail、IX_NormalizedUserName、IX_UserName | 供 AbpUserClaims、AbpUserLogins、AbpUserRoles、AbpUserTokens、AbpUserOrganizationUnits、AbpUserPasskeys、AbpUserPasswordHistories 參照 | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpRoles | IX_Name、IX_NormalizedName 通常在整合 migration 內可見 | 供 AbpUserRoles、AbpRoleClaims、AbpOrganizationUnitRoles 參照 | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpOrganizationUnitRoles | PK(OrganizationUnitId, RoleId) | FK -> AbpOrganizationUnits、FK -> AbpRoles | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpUserOrganizationUnits | PK(UserId, OrganizationUnitId) | FK -> AbpUsers、FK -> AbpOrganizationUnits | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpUserClaims | PK(Id) | FK -> AbpUsers | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpUserLogins | PK(UserId, LoginProvider) 或依 migration 組合鍵為準 | FK -> AbpUsers | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpUserRoles | PK(UserId, RoleId) | FK -> AbpUsers、FK -> AbpRoles | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| AbpUserTokens | PK(UserId, LoginProvider, Name) 類型組合鍵 | FK -> AbpUsers | templates/app-nolayers/aspnet-core/MyCompanyName.MyProjectName.Mvc/Migrations/20260220031620_Initial.cs |
| OpenIddictApplications | IX_OpenIddictApplications_ClientId | 供 OpenIddictAuthorizations、OpenIddictTokens 參照 | modules/openiddict/src/Volo.Abp.OpenIddict.EntityFrameworkCore/Volo/Abp/OpenIddict/EntityFrameworkCore/OpenIddictDbContextModelCreatingExtensions.cs |
| OpenIddictAuthorizations | 複合 IX(ApplicationId, Status, Subject, Type) | FK -> OpenIddictApplications | modules/openiddict/src/Volo.Abp.OpenIddict.EntityFrameworkCore/Volo/Abp/OpenIddict/EntityFrameworkCore/OpenIddictDbContextModelCreatingExtensions.cs |
| OpenIddictScopes | IX_OpenIddictScopes_Name | 無 | modules/openiddict/src/Volo.Abp.OpenIddict.EntityFrameworkCore/Volo/Abp/OpenIddict/EntityFrameworkCore/OpenIddictDbContextModelCreatingExtensions.cs |
| OpenIddictTokens | IX_ReferenceId、複合 IX(ApplicationId, Status, Subject, Type) | FK -> OpenIddictApplications、FK -> OpenIddictAuthorizations | modules/openiddict/src/Volo.Abp.OpenIddict.EntityFrameworkCore/Volo/Abp/OpenIddict/EntityFrameworkCore/OpenIddictDbContextModelCreatingExtensions.cs |
| DocsProjects | PK(Id) | 供 DocsProjectPdfFiles.ProjectId 參照 | modules/docs/src/Volo.Docs.EntityFrameworkCore/Volo/Docs/EntityFrameworkCore/DocsDbContextModelBuilderExtensions.cs |
| DocsDocuments | PK(Id) | 供 DocsDocumentContributors.DocumentId 參照 | modules/docs/src/Volo.Docs.EntityFrameworkCore/Volo/Docs/EntityFrameworkCore/DocsDbContextModelBuilderExtensions.cs |
| DocsDocumentContributors | PK(DocumentId, Username) | FK -> DocsDocuments | modules/docs/src/Volo.Docs.EntityFrameworkCore/Volo/Docs/EntityFrameworkCore/DocsDbContextModelBuilderExtensions.cs |
| DocsProjectPdfFiles | PK(ProjectId, FileName) | FK -> DocsProjects | modules/docs/src/Volo.Docs.EntityFrameworkCore/Volo/Docs/EntityFrameworkCore/DocsDbContextModelBuilderExtensions.cs |
| BlgBlogs | PK(Id) | 供 BlgPosts.BlogId 參照 | modules/blogging/app/Volo.BloggingTestApp.EntityFrameworkCore/Migrations/20180621080811_Added_Blog_And_Post.cs |
| BlgPosts | PK(Id) | FK -> BlgBlogs | modules/blogging/src/Volo.Blogging.EntityFrameworkCore/Volo/Blogging/EntityFrameworkCore/BloggingDbContextModelBuilderExtensions.cs |
| BlgComments | PK(Id) | FK -> BlgPosts、自我 FK -> BlgComments(RepliedCommentId) | modules/blogging/src/Volo.Blogging.EntityFrameworkCore/Volo/Blogging/EntityFrameworkCore/BloggingDbContextModelBuilderExtensions.cs |
| BlgTags | PK(Id) | 供 BlgPostTags.TagId 參照 | modules/blogging/src/Volo.Blogging.EntityFrameworkCore/Volo/Blogging/EntityFrameworkCore/BloggingDbContextModelBuilderExtensions.cs |
| BlgPostTags | PK(PostId, TagId) | FK -> BlgPosts、FK -> BlgTags | modules/blogging/src/Volo.Blogging.EntityFrameworkCore/Volo/Blogging/EntityFrameworkCore/BloggingDbContextModelBuilderExtensions.cs |
| CmsUsers | IX(TenantId, UserName)、IX(TenantId, Email) | 無 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsUserReactions | IX(TenantId, EntityType, EntityId, ReactionName)、IX(TenantId, CreatorId, EntityType, EntityId, ReactionName) | 無 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsComments | IX(TenantId, EntityType, EntityId)、IX(TenantId, RepliedCommentId) | 無顯式 FK；回覆鏈與實體關聯以應用邏輯辨識 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsRatings | IX(TenantId, EntityType, EntityId, CreatorId) | 無 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsTags | IX(TenantId, Name) | 供 CmsEntityTags.TagId 參照 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsEntityTags | PK(EntityId, TagId)、IX(TenantId, EntityId, TagId) | TagId 在 host migration 內通常可看到對 CmsTags 的 FK | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsPages | IX(TenantId, Slug) | 無 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsBlogs | PK(Id) | 供 CmsBlogPosts.BlogId、CmsBlogFeatures.BlogId 參照 | modules/cms-kit/host/Volo.CmsKit.HttpApi.Host/Migrations/20220504032110_Initial.cs |
| CmsBlogPosts | IX(Slug, BlogId) | BlogId 在 host migration 內可追到 CmsBlogs | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsBlogFeatures | PK(Id) | BlogId 在 host migration 內可追到 CmsBlogs | modules/cms-kit/host/Volo.CmsKit.HttpApi.Host/Migrations/20220504032110_Initial.cs |
| CmsMediaDescriptors | PK(Id) | 無 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsMenuItems | PK(Id) | ParentId / PageId 關聯請以 host migration 為準 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsGlobalResources | PK(Id) | 無 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| CmsUserMarkedItems | IX(TenantId, EntityType, EntityId)、IX(TenantId, CreatorId, EntityType, EntityId) | 無 | modules/cms-kit/src/Volo.CmsKit.EntityFrameworkCore/Volo/CmsKit/EntityFrameworkCore/CmsKitDbContextModelCreatingExtensions.cs |
| IdentityServerClients | UX(ClientId) 通常為核心唯一索引 | 供多個 Client 子表參照 | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerClientGrantTypes | PK(ClientId, GrantType) | FK -> IdentityServerClients | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerClientRedirectUris | PK(ClientId, RedirectUri) | FK -> IdentityServerClients | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerClientPostLogoutRedirectUris | PK(ClientId, PostLogoutRedirectUri) | FK -> IdentityServerClients | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerClientScopes | PK(ClientId, Scope) | FK -> IdentityServerClients | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerClientSecrets | PK(Id) | FK -> IdentityServerClients | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerClientClaims | PK(Id) | FK -> IdentityServerClients | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerClientCorsOrigins | PK(ClientId, Origin) | FK -> IdentityServerClients | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerIdentityResources | UX(Name) | 子表 Claims / Properties 參照 | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerApiResources | UX(Name) | 子表 Secrets / Claims / Scopes / Properties 參照 | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerApiScopes | UX(Name) | 子表 Claims / Properties 參照 | modules/identityserver/src/Volo.Abp.IdentityServer.EntityFrameworkCore/Volo/Abp/IdentityServer/EntityFrameworkCore/IdentityServerDbContextModelCreatingExtensions.cs |
| IdentityServerPersistedGrants | IX(Expiration)、IX(SubjectId, ClientId, Type) 類型索引請以 migration 為準 | 無傳統 FK | modules/cms-kit/host/Volo.CmsKit.IdentityServer/Migrations/20220504032505_Initial.cs |
| IdentityServerDeviceFlowCodes | UX(DeviceCode)、UX(UserCode) 類型索引請以 migration 為準 | 無傳統 FK | modules/cms-kit/host/Volo.CmsKit.IdentityServer/Migrations/20220504032505_Initial.cs |

## 共通欄位規則

下列欄位會反覆出現在多數資料表：

| 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|
| Id | 主鍵 | Guid | N | - | 單一主鍵。 |
| TenantId | 租戶識別碼 | Guid? | Y | - | 多租戶資料隔離欄位。 |
| ExtraProperties | 擴充屬性 | ExtraPropertyDictionary / JSON | Y | - | ABP 物件擴充系統的延伸欄位。 |
| ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖並行控制欄位。 |
| CreationTime | 建立時間 | DateTime | N | - | 建立資料的時間。 |
| CreatorId | 建立者 | Guid? | Y | - | 建立資料的使用者 Id。 |
| LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後一次修改時間。 |
| LastModifierId | 最後修改者 | Guid? | Y | - | 最後一次修改者 Id。 |
| IsDeleted | 是否軟刪除 | bool | N | false | ABP 軟刪除欄位。 |
| DeleterId | 刪除者 | Guid? | Y | - | 執行刪除的使用者 Id。 |
| DeletionTime | 刪除時間 | DateTime? | Y | - | 軟刪除時間。 |

## Framework / Core Tables

### Table: AbpEventOutbox

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 外送事件主鍵。 |
| 2 | ExtraProperties | 擴充屬性 | JSON | Y | - | 額外事件 metadata。 |
| 3 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 4 | EventName | 事件名稱 | string | N | - | 分散式事件名稱。 |
| 5 | EventData | 事件資料 | byte[] | N | - | 序列化後的事件內容。 |
| 6 | CreationTime | 建立時間 | DateTime | N | - | 事件寫入 outbox 的時間。 |

### Table: AbpEventInbox

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 收件事件主鍵。 |
| 2 | ExtraProperties | 擴充屬性 | JSON | Y | - | 額外事件 metadata。 |
| 3 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 4 | MessageId | 訊息識別碼 | string | N | - | 外部訊息唯一識別。 |
| 5 | EventName | 事件名稱 | string | N | - | 分散式事件名稱。 |
| 6 | EventData | 事件資料 | byte[] | N | - | 序列化事件內容。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 寫入 inbox 的時間。 |
| 8 | Status | 狀態 | IncomingEventStatus / int | N | 0 | 處理狀態，預設 Pending。 |
| 9 | HandledTime | 處理時間 | DateTime? | Y | - | 完成或丟棄時間。 |
| 10 | RetryCount | 重試次數 | int | N | 0 | 已重試次數。 |
| 11 | NextRetryTime | 下次重試時間 | DateTime? | Y | - | 排程重試時間。 |

### Table: AbpAuditLogs

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 稽核紀錄主鍵。 |
| 2 | ApplicationName | 應用程式名稱 | string | Y | - | 來源應用程式名稱。 |
| 3 | UserId | 使用者 Id | Guid? | Y | - | 發出請求的使用者。 |
| 4 | UserName | 使用者名稱 | string | Y | - | 發出請求的使用者帳號。 |
| 5 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 6 | TenantName | 租戶名稱 | string | Y | - | 所屬租戶名稱。 |
| 7 | ImpersonatorUserId | 模擬登入使用者 Id | Guid? | Y | - | 代理登入者 Id。 |
| 8 | ImpersonatorUserName | 模擬登入使用者名稱 | string | Y | - | 代理登入者帳號。 |
| 9 | ImpersonatorTenantId | 模擬登入租戶 Id | Guid? | Y | - | 代理登入者租戶。 |
| 10 | ImpersonatorTenantName | 模擬登入租戶名稱 | string | Y | - | 代理登入者租戶名稱。 |
| 11 | ExecutionTime | 執行時間 | DateTime | N | - | 請求開始執行時間。 |
| 12 | ExecutionDuration | 執行耗時 | int | N | - | 執行毫秒數。 |
| 13 | ClientIpAddress | Client IP | string | Y | - | 呼叫端 IP。 |
| 14 | ClientName | Client 名稱 | string | Y | - | 呼叫端名稱。 |
| 15 | ClientId | Client 識別碼 | string | Y | - | OAuth/OpenId client id 或自訂 client id。 |
| 16 | CorrelationId | 關聯識別碼 | string | Y | - | 串接追蹤識別碼。 |
| 17 | BrowserInfo | 瀏覽器資訊 | string | Y | - | User-Agent 或裝置描述。 |
| 18 | HttpMethod | HTTP 方法 | string | Y | - | GET、POST 等。 |
| 19 | Url | URL | string | Y | - | 被呼叫的 URL。 |
| 20 | HttpStatusCode | HTTP 狀態碼 | int? | Y | - | 回應狀態碼。 |
| 21 | Comments | 備註 | string | Y | - | 附加稽核註記。 |
| 22 | Exceptions | 例外內容 | string | Y | - | 執行期間例外資訊。 |
| 23 | ExtraProperties | 擴充屬性 | JSON | Y | - | 自訂稽核延伸資料。 |
| 24 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 25 | CreationTime | 建立時間 | DateTime | N | - | 稽核資料建立時間。 |
| 26 | CreatorId | 建立者 | Guid? | Y | - | 通常無特定值。 |

### Table: AbpBackgroundJobs

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 背景工作主鍵。 |
| 2 | JobName | 工作名稱 | string | N | - | 背景工作名稱。 |
| 3 | JobArgs | 工作參數 | string | N | - | 序列化後參數。 |
| 4 | TryCount | 嘗試次數 | short | N | 0 | 已執行嘗試次數。 |
| 5 | NextTryTime | 下次執行時間 | DateTime | N | - | 排程下一次執行時間。 |
| 6 | LastTryTime | 上次執行時間 | DateTime? | Y | - | 最近一次嘗試執行時間。 |
| 7 | IsAbandoned | 是否放棄 | bool | N | false | 是否已停止重試。 |
| 8 | Priority | 優先序 | byte / enum | N | Normal | 背景工作優先序。 |
| 9 | ApplicationName | 應用程式名稱 | string | Y | - | 來源應用名稱。 |
| 10 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 11 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 12 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 13 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 14 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |
| 15 | ExtraProperties | 擴充屬性 | JSON | Y | - | 自訂欄位。 |
| 16 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

### Table: AbpFeatureValues

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Feature 值主鍵。 |
| 2 | Name | 功能名稱 | string | N | - | Feature name。 |
| 3 | Value | 功能值 | string | N | - | 功能設定值。 |
| 4 | ProviderName | 提供者名稱 | string | N | - | 例如 T、R、U。 |
| 5 | ProviderKey | 提供者鍵值 | string | Y | - | 例如 TenantId、RoleName、UserId。 |

### Table: AbpPermissionGrants

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 權限授與主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 租戶隔離。 |
| 3 | Name | 權限名稱 | string | N | - | 被授與的 permission。 |
| 4 | ProviderName | 提供者名稱 | string | N | - | Role、User、Client 等。 |
| 5 | ProviderKey | 提供者鍵值 | string | N | - | provider 對應值。 |

### Table: AbpSettings

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 設定值主鍵。 |
| 2 | Name | 設定名稱 | string | N | - | Setting 名稱。 |
| 3 | Value | 設定值 | string | N | - | 設定內容。 |
| 4 | ProviderName | 提供者名稱 | string | Y | - | Global、Tenant、User 等。 |
| 5 | ProviderKey | 提供者鍵值 | string | Y | - | 對應 provider value。 |

### Table: AbpTenants

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 租戶主鍵。 |
| 2 | Name | 租戶名稱 | string | N | - | 使用者可識別租戶名稱。 |
| 3 | NormalizedName | 正規化租戶名稱 | string | N | - | 搜尋與比對使用。 |
| 4 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 5 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 6 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 7 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 8 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 9 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 10 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 11 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 12 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

## Identity Tables

### Table: AbpUsers

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 使用者主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | UserName | 使用者帳號 | string | N | - | 登入帳號。 |
| 4 | NormalizedUserName | 正規化帳號 | string | N | - | 搜尋與唯一比對使用。 |
| 5 | Name | 名字 | string | Y | - | 使用者名字。 |
| 6 | Surname | 姓氏 | string | Y | - | 使用者姓氏。 |
| 7 | Email | 電子郵件 | string | N | - | 電子郵件。 |
| 8 | NormalizedEmail | 正規化郵件 | string | N | - | 搜尋與比對用。 |
| 9 | EmailConfirmed | 郵件已驗證 | bool | N | false | 電子郵件是否已驗證。 |
| 10 | PasswordHash | 密碼雜湊 | string | Y | - | 密碼雜湊值。 |
| 11 | SecurityStamp | 安全戳記 | string | N | - | 認證憑證變更追蹤。 |
| 12 | IsExternal | 外部帳號 | bool | N | false | 是否為外部來源帳號。 |
| 13 | PhoneNumber | 電話號碼 | string | Y | - | 電話號碼。 |
| 14 | PhoneNumberConfirmed | 電話已驗證 | bool | N | false | 電話是否已驗證。 |
| 15 | IsActive | 是否啟用 | bool | N | - | 使用者是否可用。 |
| 16 | TwoFactorEnabled | 啟用雙因素 | bool | N | false | 是否啟用 2FA。 |
| 17 | LockoutEnd | 鎖定截止時間 | DateTimeOffset? | Y | - | 鎖定到何時。 |
| 18 | LockoutEnabled | 允許鎖定 | bool | N | false | 是否參與 lockout 機制。 |
| 19 | AccessFailedCount | 登入失敗次數 | int | N | 0 | 失敗登入次數。 |
| 20 | ShouldChangePasswordOnNextLogin | 下次登入需改密碼 | bool | N | - | 首次登入或管理員重設密碼時使用。 |
| 21 | EntityVersion | 實體版本 | int | N | - | 使用者版本遞增值。 |
| 22 | LastPasswordChangeTime | 最後改密碼時間 | DateTimeOffset? | Y | - | 密碼最近更新時間。 |
| 23 | LastSignInTime | 最後登入時間 | DateTimeOffset? | Y | - | 最近成功登入時間。 |
| 24 | Leaved | 已離開租戶 | bool | N | false | 共用帳號模式下是否離開租戶。 |
| 25 | ExtraProperties | 擴充屬性 | JSON | Y | - | 使用者擴充欄位。 |
| 26 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 27 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 28 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 29 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 30 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 31 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 32 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 33 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: AbpRoles

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 角色主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | Name | 角色名稱 | string | N | - | 角色名稱。 |
| 4 | NormalizedName | 正規化名稱 | string | N | - | 搜尋與唯一比對。 |
| 5 | IsDefault | 預設角色 | bool | N | - | 新使用者預設授與。 |
| 6 | IsStatic | 靜態角色 | bool | N | - | 是否為系統靜態角色。 |
| 7 | IsPublic | 公開角色 | bool | N | - | 是否可公開可見。 |
| 8 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 9 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 10 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 11 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 12 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 13 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 14 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 15 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 16 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: AbpUserClaims

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Claim 主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | UserId | 使用者 Id | Guid | N | - | 對應使用者。 |
| 4 | ClaimType | Claim 類型 | string | N | - | Claim type。 |
| 5 | ClaimValue | Claim 值 | string | Y | - | Claim value。 |

### Table: AbpUserRoles

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 2 | UserId | 使用者 Id | Guid | N | - | 使用者。 |
| 3 | RoleId | 角色 Id | Guid | N | - | 角色。 |

### Table: AbpUserLogins

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 2 | UserId | 使用者 Id | Guid | N | - | 使用者。 |
| 3 | LoginProvider | 登入提供者 | string | N | - | 外部登入 provider 名稱。 |
| 4 | ProviderKey | 提供者鍵值 | string | N | - | 外部帳號鍵值。 |
| 5 | ProviderDisplayName | 提供者顯示名稱 | string | Y | - | UI 顯示文字。 |

### Table: AbpUserTokens

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 2 | UserId | 使用者 Id | Guid | N | - | 使用者。 |
| 3 | LoginProvider | 登入提供者 | string | N | - | Provider 名稱。 |
| 4 | Name | Token 名稱 | string | N | - | Token 名稱。 |
| 5 | Value | Token 值 | string | Y | - | Token 內容。 |

### Table: AbpRoleClaims

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Claim 主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | RoleId | 角色 Id | Guid | N | - | 對應角色。 |
| 4 | ClaimType | Claim 類型 | string | N | - | Claim type。 |
| 5 | ClaimValue | Claim 值 | string | Y | - | Claim value。 |

### Table: AbpClaimTypes

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Claim type 主鍵。 |
| 2 | Name | 名稱 | string | N | - | Claim type 名稱。 |
| 3 | Required | 必填 | bool | N | - | 是否為必要 Claim。 |
| 4 | IsStatic | 靜態 | bool | N | - | 是否為系統靜態 Claim type。 |
| 5 | Regex | 正則規則 | string | Y | - | Claim 值驗證規則。 |
| 6 | RegexDescription | 規則說明 | string | Y | - | Regex 說明。 |
| 7 | Description | 說明 | string | Y | - | Claim type 說明。 |
| 8 | ValueType | 值型別 | IdentityClaimValueType / int | N | - | 值型別列舉。 |
| 9 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 10 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 11 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

### Table: AbpUserPasskeys

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | CredentialId | 憑證 Id | string | N | - | Passkey credential 唯一鍵。 |
| 2 | UserId | 使用者 Id | Guid | N | - | 對應使用者。 |
| 3 | Name | 名稱 | string | Y | - | 裝置或 passkey 名稱。 |
| 4 | Data | Passkey 資料 | JSON | Y | - | WebAuthn 序列化資料。 |
| 5 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 6 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |

### Table: AbpOrganizationUnits

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 組織單位主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | ParentId | 父節點 Id | Guid? | Y | - | 樹狀結構父節點。 |
| 4 | Code | 組織代碼 | string | N | - | 階層排序代碼。 |
| 5 | DisplayName | 顯示名稱 | string | N | - | UI 顯示名稱。 |
| 6 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 7 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 8 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 9 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 10 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 11 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 12 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 13 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 14 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: AbpOrganizationUnitRoles

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 2 | OrganizationUnitId | 組織單位 Id | Guid | N | - | 組織單位。 |
| 3 | RoleId | 角色 Id | Guid | N | - | 角色。 |

### Table: AbpUserOrganizationUnits

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 2 | OrganizationUnitId | 組織單位 Id | Guid | N | - | 組織單位。 |
| 3 | UserId | 使用者 Id | Guid | N | - | 使用者。 |

### Table: AbpUserPasswordHistories

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 2 | UserId | 使用者 Id | Guid | N | - | 使用者。 |
| 3 | Password | 歷史密碼雜湊 | string | N | - | 舊密碼雜湊。 |

### Table: AbpSecurityLogs

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 安全日誌主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | TenantName | 租戶名稱 | string | Y | - | 所屬租戶名稱。 |
| 4 | ApplicationName | 應用程式名稱 | string | Y | - | 發生事件的應用程式。 |
| 5 | Identity | 身分標識 | string | Y | - | 事件身分類別。 |
| 6 | Action | 動作 | string | Y | - | 事件動作，例如 LoginSucceeded。 |
| 7 | UserId | 使用者 Id | Guid? | Y | - | 使用者。 |
| 8 | UserName | 使用者名稱 | string | Y | - | 使用者帳號。 |
| 9 | ClientId | Client Id | string | Y | - | Client id。 |
| 10 | ClientIpAddress | Client IP | string | Y | - | 呼叫端 IP。 |
| 11 | BrowserInfo | 瀏覽器資訊 | string | Y | - | 裝置/瀏覽器資訊。 |
| 12 | CorrelationId | 關聯識別碼 | string | Y | - | 分散式追蹤碼。 |
| 13 | ExtraProperties | 擴充屬性 | JSON | Y | - | 自訂延伸資料。 |
| 14 | CreationTime | 建立時間 | DateTime | N | - | 事件建立時間。 |
| 15 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |

### Table: AbpLinkUsers

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 連結帳號主鍵。 |
| 2 | SourceUserId | 來源使用者 Id | Guid | N | - | 來源帳號。 |
| 3 | SourceTenantId | 來源租戶 Id | Guid? | Y | - | 來源租戶。 |
| 4 | TargetUserId | 目標使用者 Id | Guid | N | - | 目標帳號。 |
| 5 | TargetTenantId | 目標租戶 Id | Guid? | Y | - | 目標租戶。 |
| 6 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 7 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

### Table: AbpUserDelegations

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 委派主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | SourceUserId | 委派來源使用者 Id | Guid | N | - | 授權他人代理登入的使用者。 |
| 4 | TargetUserId | 委派目標使用者 Id | Guid | N | - | 可代理登入的使用者。 |
| 5 | StartTime | 開始時間 | DateTime | N | - | 委派起始時間。 |
| 6 | EndTime | 結束時間 | DateTime | N | - | 委派結束時間。 |

### Table: AbpSessions

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Session 主鍵。 |
| 2 | SessionId | Session 識別碼 | string | N | - | 工作階段唯一碼。 |
| 3 | Device | 裝置類型 | string | N | - | Web、Mobile 等。 |
| 4 | DeviceInfo | 裝置資訊 | string | Y | - | 裝置與瀏覽器描述。 |
| 5 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 6 | UserId | 使用者 Id | Guid | N | - | 對應使用者。 |
| 7 | ClientId | Client Id | string | Y | - | OIDC client id。 |
| 8 | IpAddresses | IP 位址列表 | string | Y | - | 使用逗號分隔的 IP 列表。 |
| 9 | SignedIn | 登入時間 | DateTime | N | - | Session 建立時間。 |
| 10 | LastAccessed | 最後存取時間 | DateTime? | Y | - | 最近存取時間。 |
| 11 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 12 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

## OpenIddict Tables

### Table: OpenIddictApplications

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | OpenIddict client 主鍵。 |
| 2 | ApplicationType | 應用程式類型 | string | Y | - | Native、Web、SPA 等。 |
| 3 | ClientId | Client Id | string | Y | - | OAuth/OIDC client id。 |
| 4 | ClientSecret | Client Secret | string | Y | - | client secret，可能已雜湊或加密。 |
| 5 | ClientType | Client 類型 | string | Y | - | public 或 confidential。 |
| 6 | ConsentType | 同意類型 | string | Y | - | explicit、implicit 等。 |
| 7 | DisplayName | 顯示名稱 | string | Y | - | UI 顯示名稱。 |
| 8 | DisplayNames | 多語顯示名稱 | string / JSON | Y | - | 多語顯示文字。 |
| 9 | JsonWebKeySet | JWK Set | string / JSON | Y | - | 公開金鑰資訊。 |
| 10 | Permissions | 權限清單 | string / JSON | Y | - | client 權限。 |
| 11 | PostLogoutRedirectUris | 登出轉址 URI | string / JSON | Y | - | 登出後回呼 URI。 |
| 12 | Properties | 屬性 | string / JSON | Y | - | 額外自訂屬性。 |
| 13 | RedirectUris | 轉址 URI | string / JSON | Y | - | redirect uri 清單。 |
| 14 | Requirements | 需求條件 | string / JSON | Y | - | OpenIddict requirement 設定。 |
| 15 | Settings | 設定 | string / JSON | Y | - | OpenIddict 設定物件。 |
| 16 | FrontChannelLogoutUri | 前通道登出 URI | string | Y | - | front-channel logout uri。 |
| 17 | ClientUri | Client URI | string | Y | - | client 網址。 |
| 18 | LogoUri | Logo URI | string | Y | - | client logo 網址。 |

### Table: OpenIddictAuthorizations

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 授權主鍵。 |
| 2 | ApplicationId | 應用程式 Id | Guid? | Y | - | 所屬 client。 |
| 3 | CreationDate | 建立日期 | DateTime? | Y | - | 授權建立時間。 |
| 4 | Properties | 屬性 | string / JSON | Y | - | 授權額外屬性。 |
| 5 | Scopes | Scope 清單 | string / JSON | Y | - | 已授權 scope。 |
| 6 | Status | 狀態 | string | Y | - | valid、revoked 等。 |
| 7 | Subject | 主體 | string | Y | - | 使用者 subject。 |
| 8 | Type | 類型 | string | Y | - | permanent、ad-hoc 等。 |
| 9 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 10 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

### Table: OpenIddictScopes

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Scope 主鍵。 |
| 2 | Description | 說明 | string | Y | - | scope 說明。 |
| 3 | Descriptions | 多語說明 | string / JSON | Y | - | 多語說明內容。 |
| 4 | DisplayName | 顯示名稱 | string | Y | - | UI 顯示名稱。 |
| 5 | DisplayNames | 多語顯示名稱 | string / JSON | Y | - | 多語顯示名稱。 |
| 6 | Name | Scope 名稱 | string | Y | - | scope code。 |
| 7 | Properties | 屬性 | string / JSON | Y | - | 自訂屬性。 |
| 8 | Resources | 資源清單 | string / JSON | Y | - | 關聯 API resource。 |
| 9 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 10 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 11 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 12 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 13 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 14 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 15 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 16 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 17 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: OpenIddictTokens

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Token 主鍵。 |
| 2 | ApplicationId | 應用程式 Id | Guid? | Y | - | 所屬 client。 |
| 3 | AuthorizationId | 授權 Id | Guid? | Y | - | 所屬 authorization。 |
| 4 | CreationDate | 建立日期 | DateTime? | Y | - | Token 建立時間。 |
| 5 | ExpirationDate | 到期日期 | DateTime? | Y | - | Token 到期時間。 |
| 6 | Payload | 負載內容 | string | Y | - | 參考型 token 的 payload。 |
| 7 | Properties | 屬性 | string / JSON | Y | - | 額外屬性。 |
| 8 | RedemptionDate | 兌換日期 | DateTime? | Y | - | Token 被使用時間。 |
| 9 | ReferenceId | 參考 Id | string | Y | - | 參考型 token 的識別碼。 |
| 10 | Status | 狀態 | string | Y | - | valid、redeemed、revoked。 |
| 11 | Subject | 主體 | string | Y | - | Token 對應 subject。 |
| 12 | Type | 類型 | string | Y | - | access_token、refresh_token 等。 |
| 13 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 14 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

## Additional Core Tables

### Table: AbpAuditLogActions

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 稽核動作主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | AuditLogId | 稽核主表 Id | Guid | N | - | 對應 AbpAuditLogs。 |
| 4 | ServiceName | 服務名稱 | string | Y | - | 應用服務或類別名稱。 |
| 5 | MethodName | 方法名稱 | string | Y | - | 被執行的方法名稱。 |
| 6 | Parameters | 參數 | string | Y | - | 序列化後參數內容。 |
| 7 | ExecutionTime | 執行時間 | DateTime | N | - | 動作執行時間。 |
| 8 | ExecutionDuration | 執行耗時 | int | N | - | 執行毫秒數。 |
| 9 | ExtraProperties | 擴充屬性 | JSON | Y | - | 自訂延伸資料。 |
| 10 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 11 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 12 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |

### Table: AbpEntityChanges

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 實體變更主鍵。 |
| 2 | AuditLogId | 稽核主表 Id | Guid | N | - | 對應稽核主表。 |
| 3 | TenantId | 租戶 Id | Guid? | Y | - | 稽核租戶。 |
| 4 | ChangeTime | 變更時間 | DateTime | N | - | 實體變更時間。 |
| 5 | ChangeType | 變更類型 | EntityChangeType / int | N | - | 新增、更新、刪除。 |
| 6 | EntityTenantId | 實體租戶 Id | Guid? | Y | - | 被變更實體的租戶。 |
| 7 | EntityId | 實體 Id | string | Y | - | 被變更實體主鍵的字串表示。 |
| 8 | EntityTypeFullName | 實體完整型別名稱 | string | N | - | 被變更實體的 .NET 型別。 |
| 9 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸欄位。 |
| 10 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 11 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 12 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |

### Table: AbpEntityPropertyChanges

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 屬性變更主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | EntityChangeId | 實體變更 Id | Guid | N | - | 對應 AbpEntityChanges。 |
| 4 | NewValue | 新值 | string | Y | - | 變更後內容。 |
| 5 | OriginalValue | 舊值 | string | Y | - | 變更前內容。 |
| 6 | PropertyName | 屬性名稱 | string | N | - | 欄位名稱。 |
| 7 | PropertyTypeFullName | 屬性型別名稱 | string | N | - | 欄位 .NET 型別。 |
| 8 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 9 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 10 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 11 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |

### Table: AbpAuditLogExcelFiles

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 匯出檔案主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | FileName | 檔名 | string | Y | - | 匯出檔案名稱。 |
| 4 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 5 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 6 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 7 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 8 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 9 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: AbpBlobContainers

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Blob 容器主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | Name | 容器名稱 | string | N | - | Blob 容器名稱。 |
| 4 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 5 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

### Table: AbpBlobs

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Blob 主鍵。 |
| 2 | ContainerId | 容器 Id | Guid | N | - | 對應 Blob 容器。 |
| 3 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 4 | Name | Blob 名稱 | string | N | - | Blob 唯一名稱。 |
| 5 | Content | 內容 | byte[] | Y | - | 實際二進位內容。 |
| 6 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 7 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

### Table: AbpFeatureGroups

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Feature 群組主鍵。 |
| 2 | Name | 群組名稱 | string | N | - | Feature group name。 |
| 3 | DisplayName | 顯示名稱 | string | N | - | 群組顯示文字。 |
| 4 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |

### Table: AbpFeatures

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Feature 定義主鍵。 |
| 2 | GroupName | 群組名稱 | string | N | - | 所屬群組。 |
| 3 | Name | 功能名稱 | string | N | - | Feature 名稱。 |
| 4 | ParentName | 父功能名稱 | string | Y | - | 父節點 feature。 |
| 5 | DisplayName | 顯示名稱 | string | N | - | UI 顯示文字。 |
| 6 | Description | 說明 | string | Y | - | 功能描述。 |
| 7 | DefaultValue | 預設值 | string | Y | - | 預設 feature 值。 |
| 8 | IsVisibleToClients | 用戶端可見 | bool | N | - | 是否可回傳到 client。 |
| 9 | IsAvailableToHost | Host 可用 | bool | N | - | Host 端是否可用。 |
| 10 | AllowedProviders | 允許提供者 | string | Y | - | 可用 provider 清單。 |
| 11 | ValueType | 值型別 | string | Y | - | Feature 值型別序列化資訊。 |
| 12 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |

### Table: AbpResourcePermissionGrants

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 資源型權限主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 租戶隔離。 |
| 3 | Name | 權限名稱 | string | N | - | Permission 名稱。 |
| 4 | ResourceName | 資源名稱 | string | N | - | 資源類型名稱。 |
| 5 | ResourceKey | 資源鍵值 | string | N | - | 指定資料列或資源鍵。 |
| 6 | ProviderName | 提供者名稱 | string | N | - | 授權提供者。 |
| 7 | ProviderKey | 提供者鍵值 | string | N | - | provider key。 |

### Table: AbpPermissionGroups

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 權限群組主鍵。 |
| 2 | Name | 群組名稱 | string | N | - | 權限群組代碼。 |
| 3 | DisplayName | 顯示名稱 | string | N | - | UI 顯示文字。 |
| 4 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |

### Table: AbpPermissions

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 權限定義主鍵。 |
| 2 | GroupName | 群組名稱 | string | Y | - | 所屬權限群組。 |
| 3 | Name | 權限名稱 | string | N | - | Permission 名稱。 |
| 4 | ResourceName | 資源名稱 | string | Y | - | Resource-based authorization 使用。 |
| 5 | ManagementPermissionName | 管理權限名稱 | string | Y | - | 管理該資源權限所需的 permission。 |
| 6 | ParentName | 父權限名稱 | string | Y | - | 權限樹父節點。 |
| 7 | DisplayName | 顯示名稱 | string | N | - | UI 顯示文字。 |
| 8 | IsEnabled | 是否啟用 | bool | N | - | 權限是否啟用。 |
| 9 | MultiTenancySide | 多租戶側別 | MultiTenancySides / int | N | - | Host、Tenant、Both。 |
| 10 | Providers | 提供者清單 | string | Y | - | 可用 provider 名稱清單。 |
| 11 | StateCheckers | 狀態檢查器 | string | Y | - | 權限狀態檢查設定。 |
| 12 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |

### Table: AbpSettingDefinitions

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 設定定義主鍵。 |
| 2 | Name | 設定名稱 | string | N | - | 唯一設定代碼。 |
| 3 | DisplayName | 顯示名稱 | string | N | - | 設定 UI 名稱。 |
| 4 | Description | 說明 | string | Y | - | 設定用途說明。 |
| 5 | DefaultValue | 預設值 | string | Y | - | 預設設定值。 |
| 6 | IsVisibleToClients | 用戶端可見 | bool | N | - | 是否可回傳至 client。 |
| 7 | Providers | 提供者清單 | string | Y | - | 可用 provider 列表。 |
| 8 | IsInherited | 是否繼承 | bool | N | - | 是否可由上層繼承。 |
| 9 | IsEncrypted | 是否加密 | bool | N | - | 是否以加密方式儲存。 |
| 10 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |

### Table: AbpTenantConnectionStrings

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | TenantId | 租戶 Id | Guid | N | - | 對應租戶。 |
| 2 | Name | 連線名稱 | string | N | - | Connection string 名稱。 |
| 3 | Value | 連線字串 | string | N | - | 實際 connection string。 |

## Optional Open Source Module Tables

### Table: DocsProjects

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 文件專案主鍵。 |
| 2 | Name | 專案名稱 | string | N | - | 文件專案名稱。 |
| 3 | ShortName | 簡稱 | string | N | - | 專案簡稱。 |
| 4 | DefaultDocumentName | 預設文件名稱 | string | N | - | 預設首頁文件。 |
| 5 | NavigationDocumentName | 導航文件名稱 | string | N | - | 導航設定文件。 |
| 6 | ParametersDocumentName | 參數文件名稱 | string | N | - | 參數設定文件。 |
| 7 | LatestVersionBranchName | 最新版本分支 | string | Y | - | Git 分支名稱。 |

### Table: DocsDocuments

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 文件主鍵。 |
| 2 | ProjectId | 專案 Id | Guid | N | - | 對應 DocsProjects。 |
| 3 | Name | 文件名稱 | string | N | - | 文件名稱。 |
| 4 | Version | 版本 | string | N | - | 文件版本。 |
| 5 | LanguageCode | 語系代碼 | string | N | - | 語系代碼。 |
| 6 | FileName | 檔名 | string | N | - | 原始檔名。 |
| 7 | Content | 內容 | string | N | - | 文件內文。 |
| 8 | Format | 格式 | string | Y | - | Markdown 等格式。 |
| 9 | EditLink | 編輯連結 | string | Y | - | 原始碼編輯連結。 |
| 10 | RootUrl | 根網址 | string | Y | - | 文件根網址。 |
| 11 | RawRootUrl | 原始根網址 | string | Y | - | Raw 文件根網址。 |
| 12 | LocalDirectory | 本機目錄 | string | Y | - | 匯入來源目錄。 |
| 13 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 14 | LastUpdatedTime | 最後更新時間 | DateTime | N | - | 最近同步更新時間。 |
| 15 | LastSignificantUpdateTime | 最後重大更新時間 | DateTime? | Y | - | 內容顯著更新時間。 |
| 16 | LastCachedTime | 最後快取時間 | DateTime | N | - | 快取更新時間。 |
| 17 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 18 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |

### Table: DocsDocumentContributors

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | DocumentId | 文件 Id | Guid | N | - | 對應文件。 |
| 2 | Username | 使用者名稱 | string | N | - | 貢獻者帳號。 |
| 3 | CommitCount | Commit 次數 | int | N | - | 貢獻次數。 |
| 4 | UserProfileUrl | 使用者頁面 | string | Y | - | 使用者個人頁 URL。 |
| 5 | AvatarUrl | 頭像 URL | string | Y | - | 頭像網址。 |

### Table: DocsProjectPdfFiles

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ProjectId | 專案 Id | Guid | N | - | 對應文件專案。 |
| 2 | FileName | 檔名 | string | N | - | PDF 檔名。 |
| 3 | Version | 版本 | string | Y | - | 文件版本。 |
| 4 | LanguageCode | 語系代碼 | string | Y | - | 語系。 |
| 5 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 6 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |

### Table: BlgUsers

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 部落格作者主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | UserName | 帳號 | string | N | - | 使用者帳號。 |
| 4 | Email | 電子郵件 | string | N | - | 電子郵件。 |
| 5 | Name | 名字 | string | Y | - | 名字。 |
| 6 | Surname | 姓氏 | string | Y | - | 姓氏。 |
| 7 | IsActive | 是否啟用 | bool | N | - | 是否啟用。 |
| 8 | EmailConfirmed | 郵件已驗證 | bool | N | false | 郵件是否驗證。 |
| 9 | PhoneNumber | 電話號碼 | string | Y | - | 電話號碼。 |
| 10 | PhoneNumberConfirmed | 電話已驗證 | bool | N | false | 電話是否驗證。 |
| 11 | WebSite | 網站 | string | Y | - | 個人網站。 |
| 12 | Twitter | Twitter | string | Y | - | Twitter 帳號。 |
| 13 | Github | GitHub | string | Y | - | GitHub 帳號。 |
| 14 | Linkedin | LinkedIn | string | Y | - | LinkedIn 帳號。 |
| 15 | Company | 公司 | string | Y | - | 所屬公司。 |
| 16 | JobTitle | 職稱 | string | Y | - | 職稱。 |
| 17 | Biography | 自我介紹 | string | Y | - | 個人簡介。 |

### Table: BlgPosts

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 文章主鍵。 |
| 2 | BlogId | Blog Id | Guid | N | - | 所屬 Blog。 |
| 3 | Url | URL | string | N | - | 文章 URL slug。 |
| 4 | CoverImage | 封面圖 | string | N | - | 封面圖片路徑或名稱。 |
| 5 | Title | 標題 | string | N | - | 文章標題。 |
| 6 | Content | 內容 | string | Y | - | 文章內容。 |
| 7 | Description | 摘要 | string | Y | - | 簡短描述。 |
| 8 | ReadCount | 閱讀次數 | int | N | - | 被讀取次數。 |
| 9 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 10 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 11 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 12 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 13 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 14 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: BlgComments

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 留言主鍵。 |
| 2 | PostId | 文章 Id | Guid | N | - | 所屬文章。 |
| 3 | RepliedCommentId | 回覆留言 Id | Guid? | Y | - | 若為回覆留言，指向父留言。 |
| 4 | Text | 內容 | string | N | - | 留言文字。 |
| 5 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 6 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 8 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 9 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 10 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: BlgTags

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Tag 主鍵。 |
| 2 | BlogId | Blog Id | Guid | N | - | 所屬 Blog。 |
| 3 | Name | 名稱 | string | N | - | 標籤名稱。 |
| 4 | Description | 說明 | string | Y | - | 標籤說明。 |
| 5 | UsageCount | 使用次數 | int | N | - | 被文章引用的次數。 |
| 6 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 7 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 8 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 9 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 10 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 11 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: BlgPostTags

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | PostId | 文章 Id | Guid | N | - | 對應文章。 |
| 2 | TagId | 標籤 Id | Guid | N | - | 對應標籤。 |

### Table: CmsUsers

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | CMS 使用者主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | UserName | 帳號 | string | N | - | 使用者帳號。 |
| 4 | Email | 電子郵件 | string | N | - | 電子郵件。 |
| 5 | Name | 名字 | string | Y | - | 名字。 |
| 6 | Surname | 姓氏 | string | Y | - | 姓氏。 |
| 7 | IsActive | 是否啟用 | bool | N | - | 是否啟用。 |
| 8 | EmailConfirmed | 郵件已驗證 | bool | N | false | 郵件是否驗證。 |
| 9 | PhoneNumber | 電話號碼 | string | Y | - | 電話號碼。 |
| 10 | PhoneNumberConfirmed | 電話已驗證 | bool | N | false | 電話是否驗證。 |

### Table: CmsUserReactions

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 反應主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | EntityType | 目標實體類型 | string | N | - | 反應對象的實體型別。 |
| 4 | EntityId | 目標實體 Id | string | N | - | 反應對象識別碼。 |
| 5 | ReactionName | 反應名稱 | string | N | - | Like、Love 等。 |
| 6 | CreatorId | 建立者 | Guid | N | - | 發出反應的使用者。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |

### Table: CmsComments

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 留言主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | EntityType | 目標實體類型 | string | N | - | 留言掛載的實體類型。 |
| 4 | EntityId | 目標實體 Id | string | N | - | 留言掛載的實體鍵值。 |
| 5 | Text | 留言內容 | string | N | - | 留言文字。 |
| 6 | RepliedCommentId | 回覆留言 Id | Guid? | Y | - | 父留言。 |
| 7 | CreatorId | 建立者 | Guid | N | - | 留言作者。 |
| 8 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 9 | Url | 來源網址 | string | Y | - | 建立留言時的頁面網址。 |
| 10 | IdempotencyToken | 去重 Token | string | Y | - | 避免重複送出的 token。 |
| 11 | IsApproved | 是否核准 | bool? | Y | - | 留言審核狀態。 |

### Table: CmsRatings

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 評分主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | EntityType | 目標實體類型 | string | N | - | 被評分的實體類型。 |
| 4 | EntityId | 目標實體 Id | string | N | - | 被評分的實體鍵值。 |
| 5 | StarCount | 星等 | short | N | - | 星等分數。 |
| 6 | CreatorId | 建立者 | Guid | N | - | 評分者。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |

### Table: CmsTags

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 標籤主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | EntityType | 目標實體類型 | string | N | - | 標籤適用的實體類型。 |
| 4 | Name | 名稱 | string | N | - | 標籤名稱。 |
| 5 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 6 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 8 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |

### Table: CmsEntityTags

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | EntityId | 目標實體 Id | string | N | - | 被標記實體的鍵值。 |
| 2 | TagId | 標籤 Id | Guid | N | - | 對應 CmsTags。 |
| 3 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |

### Table: CmsPages

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 頁面主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | Title | 標題 | string | N | - | 頁面標題。 |
| 4 | Slug | Slug | string | N | - | 頁面網址 slug。 |
| 5 | Content | 內容 | string | Y | - | 頁面 HTML / 內容。 |
| 6 | Script | 指令碼 | string | Y | - | 頁面自訂 script。 |
| 7 | Style | 樣式 | string | Y | - | 頁面自訂 CSS。 |
| 8 | IsHomePage | 是否首頁 | bool | N | - | 是否為首頁。 |
| 9 | EntityVersion | 實體版本 | int | N | - | 版本遞增值。 |
| 10 | LayoutName | 版面名稱 | string | Y | - | 使用的 layout 名稱。 |
| 11 | Status | 狀態 | PageStatus / int | N | - | Draft、Published 等。 |
| 12 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 13 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 14 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 15 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 16 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 17 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 18 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 19 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 20 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: CmsBlogs

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | CMS Blog 主鍵。 |
| 2 | Name | 名稱 | string | N | - | Blog 名稱。 |
| 3 | Slug | Slug | string | N | - | Blog 網址 slug。 |
| 4 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 5 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 6 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 8 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 9 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 10 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 11 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 12 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 13 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: CmsBlogPosts

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | CMS 文章主鍵。 |
| 2 | BlogId | Blog Id | Guid | N | - | 所屬 Blog。 |
| 3 | Title | 標題 | string | N | - | 文章標題。 |
| 4 | Slug | Slug | string | N | - | 文章網址 slug。 |
| 5 | ShortDescription | 短描述 | string | Y | - | 文章摘要。 |
| 6 | Content | 內容 | string | Y | - | 文章內容。 |
| 7 | CoverImageMediaId | 封面媒體 Id | Guid? | Y | - | 封面媒體資源。 |
| 8 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 9 | AuthorId | 作者 Id | Guid | N | - | 作者使用者 Id。 |
| 10 | Status | 狀態 | BlogPostStatus / int | N | - | Draft、Published 等。 |
| 11 | EntityVersion | 實體版本 | int | N | - | 版本遞增值。 |
| 12 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 13 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 14 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 15 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 16 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 17 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 18 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 19 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 20 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: CmsBlogFeatures

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Blog 功能主鍵。 |
| 2 | BlogId | Blog Id | Guid | N | - | 所屬 Blog。 |
| 3 | FeatureName | 功能名稱 | string | N | - | 功能代碼。 |
| 4 | IsEnabled | 是否啟用 | bool | N | - | 功能是否啟用。 |
| 5 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 6 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 8 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |

### Table: CmsMediaDescriptors

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 媒體主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | EntityType | 實體類型 | string | N | - | 掛載實體類型。 |
| 4 | Name | 名稱 | string | N | - | 媒體名稱。 |
| 5 | MimeType | MIME 類型 | string | N | - | 檔案 MIME type。 |
| 6 | Size | 大小 | long | Y | - | 檔案大小。 |
| 7 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 8 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 9 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 10 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 11 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 12 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 13 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 14 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 15 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: CmsMenuItems

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 選單項目主鍵。 |
| 2 | ParentId | 父節點 Id | Guid? | Y | - | 父選單項目。 |
| 3 | DisplayName | 顯示名稱 | string | N | - | 選單顯示文字。 |
| 4 | IsActive | 是否啟用 | bool | N | - | 是否啟用。 |
| 5 | Url | 連結 | string | N | - | 目標連結。 |
| 6 | Icon | 圖示 | string | Y | - | 圖示名稱。 |
| 7 | Order | 排序 | int | N | - | 顯示順序。 |
| 8 | Target | 目標視窗 | string | Y | - | _blank 等。 |
| 9 | ElementId | 元素 Id | string | Y | - | 前端元素識別碼。 |
| 10 | CssClass | CSS 類別 | string | Y | - | 前端樣式類別。 |
| 11 | PageId | 頁面 Id | Guid? | Y | - | 對應 CmsPages。 |
| 12 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 13 | RequiredPermissionName | 必要權限名稱 | string | Y | - | 顯示此選單所需權限。 |

### Table: CmsGlobalResources

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 全域資源主鍵。 |
| 2 | Name | 名稱 | string | N | - | 資源名稱。 |
| 3 | Value | 值 | string | N | - | 資源內容。 |
| 4 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 5 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 6 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 8 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 9 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 10 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |
| 11 | IsDeleted | 是否軟刪除 | bool | N | false | 軟刪除欄位。 |
| 12 | DeleterId | 刪除者 | Guid? | Y | - | 刪除者。 |
| 13 | DeletionTime | 刪除時間 | DateTime? | Y | - | 刪除時間。 |

### Table: CmsUserMarkedItems

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | 收藏主鍵。 |
| 2 | TenantId | 租戶 Id | Guid? | Y | - | 所屬租戶。 |
| 3 | CreatorId | 建立者 | Guid | N | - | 收藏者。 |
| 4 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 5 | EntityId | 目標實體 Id | string | N | - | 被收藏實體鍵值。 |
| 6 | EntityType | 目標實體類型 | string | N | - | 被收藏實體類型。 |

## IdentityServer Legacy Module Tables

### Table: IdentityServerClients

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Client 主鍵。 |
| 2 | ClientId | Client Id | string | N | - | OAuth/OIDC client id。 |
| 3 | ClientName | Client 名稱 | string | Y | - | 顯示名稱。 |
| 4 | Description | 說明 | string | Y | - | Client 說明。 |
| 5 | ClientUri | Client URI | string | Y | - | Client 網址。 |
| 6 | LogoUri | Logo URI | string | Y | - | Logo 網址。 |
| 7 | Enabled | 是否啟用 | bool | N | - | 是否啟用。 |
| 8 | ProtocolType | 協定類型 | string | N | - | 通常為 oidc。 |
| 9 | RequireClientSecret | 需要 Secret | bool | N | - | 是否要求 client secret。 |
| 10 | RequireConsent | 需要同意 | bool | N | - | 是否要求使用者同意。 |
| 11 | AllowRememberConsent | 記住同意 | bool | N | - | 是否允許記住同意。 |
| 12 | AlwaysIncludeUserClaimsInIdToken | ID Token 內永遠含使用者 Claims | bool | N | - | 是否將 claims 直接帶入 id token。 |
| 13 | RequirePkce | 需要 PKCE | bool | N | - | 是否要求 PKCE。 |
| 14 | AllowPlainTextPkce | 允許明文 PKCE | bool | N | - | 是否允許 plain text challenge。 |
| 15 | RequireRequestObject | 需要 Request Object | bool | N | - | 是否要求 request object。 |
| 16 | AllowAccessTokensViaBrowser | 瀏覽器取得 Access Token | bool | N | - | 是否允許透過瀏覽器接收 token。 |
| 17 | FrontChannelLogoutUri | 前通道登出 URI | string | Y | - | front-channel logout uri。 |
| 18 | FrontChannelLogoutSessionRequired | 前通道登出需 Session | bool | N | - | 是否要求 session id。 |
| 19 | BackChannelLogoutUri | 後通道登出 URI | string | Y | - | back-channel logout uri。 |
| 20 | BackChannelLogoutSessionRequired | 後通道登出需 Session | bool | N | - | 是否要求 session id。 |
| 21 | AllowOfflineAccess | 允許離線存取 | bool | N | - | 是否允許 refresh token。 |
| 22 | IdentityTokenLifetime | ID Token 存活秒數 | int | N | - | id token lifetime。 |
| 23 | AllowedIdentityTokenSigningAlgorithms | 可用簽章演算法 | string | Y | - | 可用簽章演算法。 |
| 24 | AccessTokenLifetime | Access Token 存活秒數 | int | N | - | access token lifetime。 |
| 25 | AuthorizationCodeLifetime | 授權碼存活秒數 | int | N | - | authorization code lifetime。 |
| 26 | ConsentLifetime | 同意有效秒數 | int? | Y | - | consent lifetime。 |
| 27 | AbsoluteRefreshTokenLifetime | Refresh Token 絕對有效秒數 | int | N | - | absolute refresh token lifetime。 |
| 28 | SlidingRefreshTokenLifetime | Refresh Token 滑動有效秒數 | int | N | - | sliding refresh token lifetime。 |
| 29 | RefreshTokenUsage | Refresh Token 使用策略 | int | N | - | reuse/one-time。 |
| 30 | UpdateAccessTokenClaimsOnRefresh | Refresh 時更新 Claims | bool | N | - | 是否更新 claims。 |
| 31 | RefreshTokenExpiration | Refresh Token 到期策略 | int | N | - | absolute/sliding。 |
| 32 | AccessTokenType | Access Token 類型 | int | N | - | JWT/reference。 |
| 33 | EnableLocalLogin | 啟用本機登入 | bool | N | - | 是否允許本機登入。 |
| 34 | IncludeJwtId | 包含 JTI | bool | N | - | 是否包含 jti。 |
| 35 | AlwaysSendClientClaims | 永遠傳送 Client Claims | bool | N | - | 是否總是傳送 client claims。 |
| 36 | ClientClaimsPrefix | Client Claims 前綴 | string | Y | - | client claims prefix。 |
| 37 | PairWiseSubjectSalt | PairWise Subject Salt | string | Y | - | PairWise subject salt。 |
| 38 | UserSsoLifetime | 使用者 SSO 存活秒數 | int? | Y | - | user SSO lifetime。 |
| 39 | UserCodeType | 使用者驗證碼類型 | string | Y | - | device flow user code type。 |
| 40 | DeviceCodeLifetime | Device Code 存活秒數 | int | N | - | device code lifetime。 |
| 41 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 42 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 43 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 44 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 45 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 46 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: IdentityServerClientGrantTypes

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | GrantType | Grant Type | string | N | - | client 允許的授權流程。 |

### Table: IdentityServerClientRedirectUris

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | RedirectUri | Redirect URI | string | N | - | 允許的 redirect uri。 |

### Table: IdentityServerClientPostLogoutRedirectUris

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | PostLogoutRedirectUri | 登出 Redirect URI | string | N | - | 登出後允許的 redirect uri。 |

### Table: IdentityServerClientScopes

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | Scope | Scope 名稱 | string | N | - | 允許的 scope。 |

### Table: IdentityServerClientSecrets

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | Type | Secret 類型 | string | N | - | secret 類型。 |
| 3 | Value | Secret 值 | string | N | - | secret 值。 |
| 4 | Description | 說明 | string | Y | - | secret 說明。 |
| 5 | Expiration | 到期時間 | DateTime? | Y | - | secret 到期時間。 |

### Table: IdentityServerClientClaims

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | Type | Claim 類型 | string | N | - | Claim type。 |
| 3 | Value | Claim 值 | string | N | - | Claim value。 |

### Table: IdentityServerClientIdPRestrictions

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | Provider | IdP 提供者 | string | N | - | 限制可用的外部身份提供者。 |

### Table: IdentityServerClientCorsOrigins

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | Origin | CORS 來源 | string | N | - | 允許跨網域來源。 |

### Table: IdentityServerClientProperties

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ClientId | Client 主鍵 | Guid | N | - | 對應 Client。 |
| 2 | Key | 鍵 | string | N | - | 自訂屬性鍵。 |
| 3 | Value | 值 | string | N | - | 自訂屬性值。 |

### Table: IdentityServerIdentityResources

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Identity resource 主鍵。 |
| 2 | Name | 名稱 | string | N | - | Resource 名稱。 |
| 3 | DisplayName | 顯示名稱 | string | Y | - | UI 顯示名稱。 |
| 4 | Description | 說明 | string | Y | - | Resource 說明。 |
| 5 | Enabled | 是否啟用 | bool | N | - | 是否啟用。 |
| 6 | Required | 是否必要 | bool | N | - | 是否必要。 |
| 7 | Emphasize | 是否強調 | bool | N | - | UI 是否強調。 |
| 8 | ShowInDiscoveryDocument | 顯示於 Discovery | bool | N | - | 是否顯示於 discovery document。 |
| 9 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 10 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 11 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 12 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 13 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 14 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: IdentityServerIdentityResourceClaims

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | IdentityResourceId | Resource Id | Guid | N | - | 對應 Identity resource。 |
| 2 | Type | Claim 類型 | string | N | - | Claim type。 |

### Table: IdentityServerIdentityResourceProperties

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | IdentityResourceId | Resource Id | Guid | N | - | 對應 Identity resource。 |
| 2 | Key | 鍵 | string | N | - | 自訂屬性鍵。 |
| 3 | Value | 值 | string | N | - | 自訂屬性值。 |

### Table: IdentityServerApiResources

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | API resource 主鍵。 |
| 2 | Name | 名稱 | string | N | - | Resource 名稱。 |
| 3 | DisplayName | 顯示名稱 | string | Y | - | UI 顯示名稱。 |
| 4 | Description | 說明 | string | Y | - | Resource 說明。 |
| 5 | Enabled | 是否啟用 | bool | N | - | 是否啟用。 |
| 6 | AllowedAccessTokenSigningAlgorithms | 可用簽章演算法 | string | Y | - | Access token 簽章演算法。 |
| 7 | ShowInDiscoveryDocument | 顯示於 Discovery | bool | N | - | 是否顯示於 discovery document。 |
| 8 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 9 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 10 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 11 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 12 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 13 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: IdentityServerApiResourceSecrets

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ApiResourceId | API Resource Id | Guid | N | - | 對應 API resource。 |
| 2 | Type | Secret 類型 | string | N | - | secret 類型。 |
| 3 | Value | Secret 值 | string | N | - | secret 值。 |
| 4 | Description | 說明 | string | Y | - | secret 說明。 |
| 5 | Expiration | 到期時間 | DateTime? | Y | - | secret 到期時間。 |

### Table: IdentityServerApiResourceClaims

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ApiResourceId | API Resource Id | Guid | N | - | 對應 API resource。 |
| 2 | Type | Claim 類型 | string | N | - | Claim type。 |

### Table: IdentityServerApiResourceScopes

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ApiResourceId | API Resource Id | Guid | N | - | 對應 API resource。 |
| 2 | Scope | Scope 名稱 | string | N | - | 對應 API scope。 |

### Table: IdentityServerApiResourceProperties

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ApiResourceId | API Resource Id | Guid | N | - | 對應 API resource。 |
| 2 | Key | 鍵 | string | N | - | 自訂屬性鍵。 |
| 3 | Value | 值 | string | N | - | 自訂屬性值。 |

### Table: IdentityServerApiScopes

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | API scope 主鍵。 |
| 2 | Enabled | 是否啟用 | bool | N | - | 是否啟用。 |
| 3 | Name | 名稱 | string | N | - | Scope 名稱。 |
| 4 | DisplayName | 顯示名稱 | string | Y | - | UI 顯示名稱。 |
| 5 | Description | 說明 | string | Y | - | Scope 說明。 |
| 6 | Required | 是否必要 | bool | N | - | 是否必要。 |
| 7 | Emphasize | 是否強調 | bool | N | - | UI 是否強調。 |
| 8 | ShowInDiscoveryDocument | 顯示於 Discovery | bool | N | - | 是否顯示於 discovery document。 |
| 9 | ExtraProperties | 擴充屬性 | JSON | Y | - | 延伸資料。 |
| 10 | ConcurrencyStamp | 並行戳記 | string | Y | - | 樂觀鎖控制。 |
| 11 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 12 | CreatorId | 建立者 | Guid? | Y | - | 建立者。 |
| 13 | LastModificationTime | 最後修改時間 | DateTime? | Y | - | 最後修改時間。 |
| 14 | LastModifierId | 最後修改者 | Guid? | Y | - | 最後修改者。 |

### Table: IdentityServerApiScopeClaims

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ApiScopeId | API Scope Id | Guid | N | - | 對應 API scope。 |
| 2 | Type | Claim 類型 | string | N | - | Claim type。 |

### Table: IdentityServerApiScopeProperties

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | ApiScopeId | API Scope Id | Guid | N | - | 對應 API scope。 |
| 2 | Key | 鍵 | string | N | - | 自訂屬性鍵。 |
| 3 | Value | 值 | string | N | - | 自訂屬性值。 |

### Table: IdentityServerPersistedGrants

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Key | 主鍵鍵值 | string | N | - | 授權資料主鍵。 |
| 2 | Type | 類型 | string | N | - | grant 類型。 |
| 3 | SubjectId | Subject Id | string | Y | - | 使用者 subject。 |
| 4 | SessionId | Session Id | string | Y | - | 工作階段識別碼。 |
| 5 | ClientId | Client Id | string | N | - | 所屬 client。 |
| 6 | Description | 說明 | string | Y | - | 說明。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 8 | Expiration | 到期時間 | DateTime? | Y | - | 到期時間。 |
| 9 | ConsumedTime | 已使用時間 | DateTime? | Y | - | 被消耗時間。 |
| 10 | Data | 資料 | string | N | - | 序列化 grant 資料。 |

### Table: IdentityServerDeviceFlowCodes

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | DeviceCode | 裝置碼 | string | N | - | Device code 主鍵。 |
| 2 | UserCode | 使用者碼 | string | N | - | 使用者輸入驗證碼。 |
| 3 | SubjectId | Subject Id | string | Y | - | 使用者 subject。 |
| 4 | SessionId | Session Id | string | Y | - | 工作階段識別碼。 |
| 5 | ClientId | Client Id | string | N | - | 所屬 client。 |
| 6 | Description | 說明 | string | Y | - | 說明。 |
| 7 | CreationTime | 建立時間 | DateTime | N | - | 建立時間。 |
| 8 | Expiration | 到期時間 | DateTime? | Y | - | 到期時間。 |
| 9 | Data | 資料 | string | N | - | 序列化裝置流程資料。 |

### Table: BlgBlogs

| 序 | 欄位名稱 | 中文名稱 | 資料型態 | Null | 預設值 | 說明 |
|---|---|---|---|---|---|---|
| 1 | Id | 主鍵 | Guid | N | - | Blog 主鍵。 |
| 2 | Name | 名稱 | string | N | - | Blog 名稱。 |
| 3 | ShortName | 簡稱 | string | N | - | 短名稱。 |
| 4 | Description | 說明 | string | Y | - | Blog 描述。 |

## 補充說明

1. 本文件已移除商業版欄位，因為此 repo 只包含開源模組原始碼。
2. 這一版已改成逐欄位 schema 形式，不再使用上一版的摘要式欄位集合表示。
3. 如果你要真正可交給 DBA 的版本，下一步應再補 SQL 型別、長度、索引、唯一鍵、外鍵與 migration 對照。