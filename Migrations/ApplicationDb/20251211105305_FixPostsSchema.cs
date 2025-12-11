using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLOGAURA.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class FixPostsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 1. Add ImagePath if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Posts]') AND name = N'ImagePath')
                BEGIN
                    ALTER TABLE Posts ADD ImagePath nvarchar(max) NULL;
                END

                -- 2. Add VideoPath if it doesn't exist
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Posts]') AND name = N'VideoPath')
                BEGIN
                    ALTER TABLE Posts ADD VideoPath nvarchar(max) NULL;
                END

                -- 3. Make CategoryId nullable if it exists (legacy column from BlogContext)
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Posts]') AND name = N'CategoryId')
                BEGIN
                    -- Remove default constraint if exists
                    DECLARE @ConstraintName nvarchar(200);
                    SELECT @ConstraintName = name FROM sys.default_constraints 
                    WHERE parent_object_id = OBJECT_ID('Posts') 
                    AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('Posts') AND name = 'CategoryId');

                    IF @ConstraintName IS NOT NULL
                        EXEC('ALTER TABLE Posts DROP CONSTRAINT ' + @ConstraintName);

                    ALTER TABLE Posts ALTER COLUMN CategoryId int NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
