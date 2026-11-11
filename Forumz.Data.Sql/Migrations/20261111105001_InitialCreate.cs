using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forumz.Data.Sql.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "forumz");

        migrationBuilder.CreateTable(
            name: "Groups",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Groups", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Members",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Signature = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                PostCount = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Members", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Messages",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                FromUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                ToUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Subject = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsRead = table.Column<bool>(type: "bit", nullable: false),
                IsDeletedByAuthor = table.Column<bool>(type: "bit", nullable: false),
                IsDeletedByRecipient = table.Column<bool>(type: "bit", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Messages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Preferences",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                AllowUsersToEditPosts = table.Column<bool>(type: "bit", nullable: false),
                AllowUsersToDeletePosts = table.Column<bool>(type: "bit", nullable: false),
                AllowUsersToManageSubscriptions = table.Column<bool>(type: "bit", nullable: false),
                AllowPrivateMessages = table.Column<bool>(type: "bit", nullable: false),
                RelativeDates = table.Column<bool>(type: "bit", nullable: false),
                FeedsEnabled = table.Column<bool>(type: "bit", nullable: false),
                SignaturesEnabled = table.Column<bool>(type: "bit", nullable: false),
                TopicsPageSize = table.Column<int>(type: "int", nullable: false),
                PostsPageSize = table.Column<int>(type: "int", nullable: false),
                SearchPageSize = table.Column<int>(type: "int", nullable: false),
                ActivePageSize = table.Column<int>(type: "int", nullable: false),
                MessagesPageSize = table.Column<int>(type: "int", nullable: false),
                SubscriptionsPageSize = table.Column<int>(type: "int", nullable: false),
                SubjectMaxLength = table.Column<int>(type: "int", nullable: false),
                PostMaxLength = table.Column<int>(type: "int", nullable: false),
                MessageSubjectMaxLength = table.Column<int>(type: "int", nullable: false),
                MessageTextMaxLength = table.Column<int>(type: "int", nullable: false),
                SearchMinimumLength = table.Column<int>(type: "int", nullable: false),
                FeedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Preferences", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Subscriptions",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                ForumId = table.Column<int>(type: "int", nullable: false),
                TopicId = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Subscriptions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Forums",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ForumGroupId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                NumTopics = table.Column<int>(type: "int", nullable: false),
                NumPosts = table.Column<int>(type: "int", nullable: false),
                LastTopicId = table.Column<int>(type: "int", nullable: false),
                LastPostId = table.Column<int>(type: "int", nullable: false),
                LastPostUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                LastPostTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Forums", x => x.Id);
                table.ForeignKey(
                    name: "FK_Forums_Groups_ForumGroupId",
                    column: x => x.ForumGroupId,
                    principalSchema: "forumz",
                    principalTable: "Groups",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Topics",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ForumId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                TopicType = table.Column<byte>(type: "tinyint", nullable: false),
                Subject = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                NumPosts = table.Column<int>(type: "int", nullable: false),
                Views = table.Column<int>(type: "int", nullable: false),
                LastPostId = table.Column<int>(type: "int", nullable: false),
                LastPostUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                LastPostTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Topics", x => x.Id);
                table.ForeignKey(
                    name: "FK_Topics_Forums_ForumId",
                    column: x => x.ForumId,
                    principalSchema: "forumz",
                    principalTable: "Forums",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Posts",
            schema: "forumz",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TopicId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IpAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Posts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Posts_Topics_TopicId",
                    column: x => x.TopicId,
                    principalSchema: "forumz",
                    principalTable: "Topics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Forums_ForumGroupId_DisplayOrder",
            schema: "forumz",
            table: "Forums",
            columns: new[] { "ForumGroupId", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Groups_TenantId_DisplayOrder",
            schema: "forumz",
            table: "Groups",
            columns: new[] { "TenantId", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Members_TenantId_UserId",
            schema: "forumz",
            table: "Members",
            columns: new[] { "TenantId", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Messages_TenantId_FromUserId_IsDeletedByAuthor",
            schema: "forumz",
            table: "Messages",
            columns: new[] { "TenantId", "FromUserId", "IsDeletedByAuthor" });

        migrationBuilder.CreateIndex(
            name: "IX_Messages_TenantId_ToUserId_IsDeletedByRecipient_CreatedOnUtc",
            schema: "forumz",
            table: "Messages",
            columns: new[] { "TenantId", "ToUserId", "IsDeletedByRecipient", "CreatedOnUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_Posts_TopicId_CreatedOnUtc",
            schema: "forumz",
            table: "Posts",
            columns: new[] { "TopicId", "CreatedOnUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_Preferences_TenantId",
            schema: "forumz",
            table: "Preferences",
            column: "TenantId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Subscriptions_TenantId_UserId_ForumId_TopicId",
            schema: "forumz",
            table: "Subscriptions",
            columns: new[] { "TenantId", "UserId", "ForumId", "TopicId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Topics_ForumId_TopicType_LastPostTime",
            schema: "forumz",
            table: "Topics",
            columns: new[] { "ForumId", "TopicType", "LastPostTime" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Members",
            schema: "forumz");

        migrationBuilder.DropTable(
            name: "Messages",
            schema: "forumz");

        migrationBuilder.DropTable(
            name: "Posts",
            schema: "forumz");

        migrationBuilder.DropTable(
            name: "Preferences",
            schema: "forumz");

        migrationBuilder.DropTable(
            name: "Subscriptions",
            schema: "forumz");

        migrationBuilder.DropTable(
            name: "Topics",
            schema: "forumz");

        migrationBuilder.DropTable(
            name: "Forums",
            schema: "forumz");

        migrationBuilder.DropTable(
            name: "Groups",
            schema: "forumz");
    }
}
