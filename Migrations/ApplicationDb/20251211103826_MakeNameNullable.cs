using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLOGAURA.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class MakeNameNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 1. Drop the index that depends on the column (if it exists)
                DROP INDEX IF EXISTS IX_Users_Username ON Users;

                -- 2. Drop any default constraint on 'Name' (if it exists)
                DECLARE @ConstraintName nvarchar(200);
                SELECT @ConstraintName = name FROM sys.default_constraints 
                WHERE parent_object_id = OBJECT_ID('Users') 
                AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Name');

                IF @ConstraintName IS NOT NULL
                    EXEC('ALTER TABLE Users DROP CONSTRAINT ' + @ConstraintName);

                -- 3. Alter the 'Name' column to be NULLABLE
                ALTER TABLE Users ALTER COLUMN Name nvarchar(MAX) NULL;

                -- 4. Recreate the unique index on Username
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('Users') AND name='Username')
                BEGIN
                    CREATE UNIQUE INDEX IX_Users_Username ON Users(Username);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
