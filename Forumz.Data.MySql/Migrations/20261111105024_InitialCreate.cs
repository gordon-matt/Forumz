using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Forumz.Data.MySql.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Groups",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Forumz_Groups", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Members",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                DisplayName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                Signature = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                PostCount = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Forumz_Members", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Messages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                FromUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                ToUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                Subject = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                Text = table.Column<string>(type: "longtext", nullable: false),
                IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                IsDeletedByAuthor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                IsDeletedByRecipient = table.Column<bool>(type: "tinyint(1)", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Forumz_Messages", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Preferences",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                AllowUsersToEditPosts = table.Column<bool>(type: "tinyint(1)", nullable: false),
                AllowUsersToDeletePosts = table.Column<bool>(type: "tinyint(1)", nullable: false),
                AllowUsersToManageSubscriptions = table.Column<bool>(type: "tinyint(1)", nullable: false),
                AllowPrivateMessages = table.Column<bool>(type: "tinyint(1)", nullable: false),
                RelativeDates = table.Column<bool>(type: "tinyint(1)", nullable: false),
                FeedsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                SignaturesEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
            constraints: table => table.PrimaryKey("PK_Forumz_Preferences", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Subscriptions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                TenantId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                ForumId = table.Column<int>(type: "int", nullable: false),
                TopicId = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Forumz_Subscriptions", x => x.Id))
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Forums",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                ForumGroupId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                NumTopics = table.Column<int>(type: "int", nullable: false),
                NumPosts = table.Column<int>(type: "int", nullable: false),
                LastTopicId = table.Column<int>(type: "int", nullable: false),
                LastPostId = table.Column<int>(type: "int", nullable: false),
                LastPostUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                LastPostTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                DisplayOrder = table.Column<int>(type: "int", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Forumz_Forums", x => x.Id);
                table.ForeignKey(
                    name: "FK_Forumz_Forums_Forumz_Groups_ForumGroupId",
                    column: x => x.ForumGroupId,
                    principalTable: "Forumz_Groups",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Topics",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                ForumId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                TopicType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                Subject = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                NumPosts = table.Column<int>(type: "int", nullable: false),
                Views = table.Column<int>(type: "int", nullable: false),
                LastPostId = table.Column<int>(type: "int", nullable: false),
                LastPostUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                LastPostTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Forumz_Topics", x => x.Id);
                table.ForeignKey(
                    name: "FK_Forumz_Topics_Forumz_Forums_ForumId",
                    column: x => x.ForumId,
                    principalTable: "Forumz_Forums",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Forumz_Posts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                TopicId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false),
                Text = table.Column<string>(type: "longtext", nullable: false),
                IpAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Forumz_Posts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Forumz_Posts_Forumz_Topics_TopicId",
                    column: x => x.TopicId,
                    principalTable: "Forumz_Topics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Forums_ForumGroupId_DisplayOrder",
            table: "Forumz_Forums",
            columns: new[] { "ForumGroupId", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Groups_TenantId_DisplayOrder",
            table: "Forumz_Groups",
            columns: new[] { "TenantId", "DisplayOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Members_TenantId_UserId",
            table: "Forumz_Members",
            columns: new[] { "TenantId", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Messages_TenantId_FromUserId_IsDeletedByAuthor",
            table: "Forumz_Messages",
            columns: new[] { "TenantId", "FromUserId", "IsDeletedByAuthor" });

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Messages_TenantId_ToUserId_IsDeletedByRecipient_Creat~",
            table: "Forumz_Messages",
            columns: new[] { "TenantId", "ToUserId", "IsDeletedByRecipient", "CreatedOnUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Posts_TopicId_CreatedOnUtc",
            table: "Forumz_Posts",
            columns: new[] { "TopicId", "CreatedOnUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Preferences_TenantId",
            table: "Forumz_Preferences",
            column: "TenantId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Subscriptions_TenantId_UserId_ForumId_TopicId",
            table: "Forumz_Subscriptions",
            columns: new[] { "TenantId", "UserId", "ForumId", "TopicId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Forumz_Topics_ForumId_TopicType_LastPostTime",
            table: "Forumz_Topics",
            columns: new[] { "ForumId", "TopicType", "LastPostTime" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Forumz_Members");

        migrationBuilder.DropTable(
            name: "Forumz_Messages");

        migrationBuilder.DropTable(
            name: "Forumz_Posts");

        migrationBuilder.DropTable(
            name: "Forumz_Preferences");

        migrationBuilder.DropTable(
            name: "Forumz_Subscriptions");

        migrationBuilder.DropTable(
            name: "Forumz_Topics");

        migrationBuilder.DropTable(
            name: "Forumz_Forums");

        migrationBuilder.DropTable(
            name: "Forumz_Groups");
    }
}
