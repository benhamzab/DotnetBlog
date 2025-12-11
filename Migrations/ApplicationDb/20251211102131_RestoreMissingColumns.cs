using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLOGAURA.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class RestoreMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manually add ProfilePictureUrl
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'ProfilePictureUrl') ALTER TABLE Users ADD ProfilePictureUrl nvarchar(max) NULL");

            // Manually add Username
            // 1. Add as nullable first
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'Username') BEGIN ALTER TABLE Users ADD Username nvarchar(50) NULL; END");
            
            // 2. Populate Username (Try to use Name if exists, else Default)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'Username')
                BEGIN
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'Name')
                        EXEC('UPDATE Users SET Username = Name WHERE Username IS NULL');
                    
                    UPDATE Users SET Username = 'User' + CAST(Id AS nvarchar(20)) WHERE Username IS NULL OR Username = '';
                END
            ");

            // 3. Make not null (as per model)
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Users]') AND name = N'Username') ALTER TABLE Users ALTER COLUMN Username nvarchar(50) NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
