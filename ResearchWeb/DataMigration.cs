using Microsoft.Data.Sqlite;
using Npgsql;

namespace ResearchWeb
{
    public static class DataMigration
    {
        public static async Task RunAsync(string postgresConnectionString)
        {
            string sqlitePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "App_Data",
                "research.db"
            );

            if (!File.Exists(sqlitePath))
            {
                Console.WriteLine("SQLite database not found:");
                Console.WriteLine(sqlitePath);
                return;
            }

            string sqliteConnectionString =
                $"Data Source={sqlitePath}";

            Console.WriteLine("=================================");
            Console.WriteLine("STARTING DATA TRANSFER");
            Console.WriteLine("SQLite → PostgreSQL");
            Console.WriteLine("=================================");

            await using var sqlite =
                new SqliteConnection(sqliteConnectionString);

            await using var postgres =
                new NpgsqlConnection(postgresConnectionString);

            await sqlite.OpenAsync();
            await postgres.OpenAsync();

            // =========================================
            // 1. Research
            // =========================================

            Console.WriteLine("Reading Research records...");

            await using var researchCommand = sqlite.CreateCommand();

            researchCommand.CommandText = """
                SELECT
                    ID,
                    اسم_الباحث,
                    تاريخ_الاجتماع,
                    عنوان_البحث,
                    رقم_البحث,
                    رقم_الاجتماع,
                    نتيجة_البحث,
                    رقم_الهاتف,
                    توصيات_اللجنة
                FROM "الابحاث العلمية 2026"
                ORDER BY ID
                """;

            var researchRecords = new List<ResearchRecord>();

            await using (var reader =
                await researchCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    researchRecords.Add(
                        new ResearchRecord
                        {
                            ID = reader.GetInt32(0),

                            اسم_الباحث =
                                reader.IsDBNull(1)
                                    ? null
                                    : reader.GetString(1),

                            تاريخ_الاجتماع =
                                reader.IsDBNull(2)
                                    ? null
                                    : reader.GetDateTime(2),

                            عنوان_البحث =
                                reader.IsDBNull(3)
                                    ? null
                                    : reader.GetString(3),

                            رقم_البحث =
                                reader.IsDBNull(4)
                                    ? null
                                    : reader.GetString(4),

                            رقم_الاجتماع =
                                reader.IsDBNull(5)
                                    ? null
                                    : reader.GetString(5),

                            نتيجة_البحث =
                                reader.IsDBNull(6)
                                    ? null
                                    : reader.GetString(6),

                            رقم_الهاتف =
                                reader.IsDBNull(7)
                                    ? null
                                    : reader.GetString(7),

                            توصيات_اللجنة =
                                reader.IsDBNull(8)
                                    ? null
                                    : reader.GetString(8)
                        });
                }
            }

            Console.WriteLine(
                $"SQLite Research Count = {researchRecords.Count}");

            foreach (var r in researchRecords)
            {
                await using var cmd =
                    postgres.CreateCommand();

                cmd.CommandText = """
                    INSERT INTO "الابحاث العلمية 2026"
                    (
                        "ID",
                        "اسم_الباحث",
                        "تاريخ_الاجتماع",
                        "عنوان_البحث",
                        "رقم_البحث",
                        "رقم_الاجتماع",
                        "نتيجة_البحث",
                        "رقم_الهاتف",
                        "توصيات_اللجنة"
                    )
                    VALUES
                    (
                        @id,
                        @researcher,
                        @meetingDate,
                        @title,
                        @researchNumber,
                        @meetingNumber,
                        @result,
                        @phone,
                        @recommendations
                    )
                    ON CONFLICT ("ID") DO NOTHING;
                    """;

                cmd.Parameters.AddWithValue(
                    "@id",
                    r.ID);

                cmd.Parameters.AddWithValue(
                    "@researcher",
                    (object?)r.اسم_الباحث ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@meetingDate",
                    (object?)r.تاريخ_الاجتماع ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@title",
                    (object?)r.عنوان_البحث ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@researchNumber",
                    (object?)r.رقم_البحث ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@meetingNumber",
                    (object?)r.رقم_الاجتماع ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@result",
                    (object?)r.نتيجة_البحث ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@phone",
                    (object?)r.رقم_الهاتف ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@recommendations",
                    (object?)r.توصيات_اللجنة ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine(
                $"Transferred Research records = {researchRecords.Count}");

            // =========================================
            // 2. Users
            // =========================================

            Console.WriteLine("Reading Users...");

            await using var usersCommand =
                sqlite.CreateCommand();

            usersCommand.CommandText = """
                SELECT
                    ID,
                    Username,
                    Password,
                    FullName,
                    Role
                FROM "Users"
                ORDER BY ID
                """;

            var users = new List<UserRecord>();

            await using (var reader =
                await usersCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    users.Add(
                        new UserRecord
                        {
                            ID = reader.GetInt32(0),

                            Username =
                                reader.IsDBNull(1)
                                    ? null
                                    : reader.GetString(1),

                            Password =
                                reader.IsDBNull(2)
                                    ? null
                                    : reader.GetString(2),

                            FullName =
                                reader.IsDBNull(3)
                                    ? null
                                    : reader.GetString(3),

                            Role =
                                reader.IsDBNull(4)
                                    ? null
                                    : reader.GetString(4)
                        });
                }
            }

            Console.WriteLine(
                $"SQLite Users Count = {users.Count}");

            foreach (var u in users)
            {
                await using var cmd =
                    postgres.CreateCommand();

                cmd.CommandText = """
                    INSERT INTO "Users"
                    (
                        "ID",
                        "Username",
                        "Password",
                        "FullName",
                        "Role"
                    )
                    VALUES
                    (
                        @id,
                        @username,
                        @password,
                        @fullname,
                        @role
                    )
                    ON CONFLICT ("ID") DO NOTHING;
                    """;

                cmd.Parameters.AddWithValue(
                    "@id",
                    u.ID);

                cmd.Parameters.AddWithValue(
                    "@username",
                    (object?)u.Username ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@password",
                    (object?)u.Password ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@fullname",
                    (object?)u.FullName ?? DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@role",
                    (object?)u.Role ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine(
                $"Transferred Users = {users.Count}");

            // =========================================
            // 3. Visitors
            // =========================================

            Console.WriteLine("Reading Visitors...");

            await using var visitorsCommand =
                sqlite.CreateCommand();

            visitorsCommand.CommandText = """
                SELECT
                    ID,
                    VisitDate
                FROM "Visitors"
                ORDER BY ID
                """;

            var visitors = new List<VisitorRecord>();

            await using (var reader =
                await visitorsCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    visitors.Add(
                        new VisitorRecord
                        {
                            ID = reader.GetInt32(0),

                            VisitDate =
                                reader.GetDateTime(1)
                        });
                }
            }

            Console.WriteLine(
                $"SQLite Visitors Count = {visitors.Count}");

            foreach (var v in visitors)
            {
                await using var cmd =
                    postgres.CreateCommand();

                cmd.CommandText = """
                    INSERT INTO "Visitors"
                    (
                        "ID",
                        "VisitDate"
                    )
                    VALUES
                    (
                        @id,
                        @visitDate
                    )
                    ON CONFLICT ("ID") DO NOTHING;
                    """;

                cmd.Parameters.AddWithValue(
                    "@id",
                    v.ID);

                cmd.Parameters.AddWithValue(
                    "@visitDate",
                    v.VisitDate);

                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine(
                $"Transferred Visitors = {visitors.Count}");

            // =========================================
            // 4. Reset PostgreSQL sequences
            // =========================================

            Console.WriteLine(
                "Resetting PostgreSQL sequences...");

            await ResetSequenceAsync(
                postgres,
                "الابحاث العلمية 2026");

            await ResetSequenceAsync(
                postgres,
                "Users");

            await ResetSequenceAsync(
                postgres,
                "Visitors");

            Console.WriteLine(
                "PostgreSQL sequences reset successfully.");

            // =========================================
            // Completed
            // =========================================

            Console.WriteLine("=================================");
            Console.WriteLine("DATA TRANSFER COMPLETED");
            Console.WriteLine("=================================");
        }

        // =========================================
        // Reset one PostgreSQL sequence
        // =========================================

        private static async Task ResetSequenceAsync(
            NpgsqlConnection postgres,
            string tableName)
        {
            string sql = $"""
                SELECT setval(
                    pg_get_serial_sequence(
                        '"{tableName}"',
                        'ID'
                    ),
                    COALESCE(
                        (SELECT MAX("ID") FROM "{tableName}"),
                        1
                    ),
                    true
                );
                """;

            await using var cmd =
                new NpgsqlCommand(sql, postgres);

            await cmd.ExecuteScalarAsync();
        }

        // =========================================
        // Research Record
        // =========================================

        private class ResearchRecord
        {
            public int ID { get; set; }

            public string? اسم_الباحث { get; set; }

            public DateTime? تاريخ_الاجتماع { get; set; }

            public string? عنوان_البحث { get; set; }

            public string? رقم_البحث { get; set; }

            public string? رقم_الاجتماع { get; set; }

            public string? نتيجة_البحث { get; set; }

            public string? رقم_الهاتف { get; set; }

            public string? توصيات_اللجنة { get; set; }
        }

        // =========================================
        // User Record
        // =========================================

        private class UserRecord
        {
            public int ID { get; set; }

            public string? Username { get; set; }

            public string? Password { get; set; }

            public string? FullName { get; set; }

            public string? Role { get; set; }
        }

        // =========================================
        // Visitor Record
        // =========================================

        private class VisitorRecord
        {
            public int ID { get; set; }

            public DateTime VisitDate { get; set; }
        }
    }
}