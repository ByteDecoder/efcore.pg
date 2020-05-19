using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindDbFunctionsQueryNpgsqlTest : NorthwindDbFunctionsQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindDbFunctionsQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Like / ILike

        public override async Task Like_literal(bool async)
        {
            // PostgreSQL like is case-sensitive, while the EF Core "default" (i.e. SqlServer) is insensitive.
            // So we override and assert only 19 matches unlike the default's 34.
            await AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Like(c.ContactName, "%M%"),
                c => c.ContactName.Contains("M"));

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE '%M%'");
        }

        public override async Task Like_identity(bool async)
        {
            await base.Like_identity(async);

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE c.""ContactName"" ESCAPE ''");
        }

        public override async Task Like_literal_with_escape(bool async)
        {
            await base.Like_literal_with_escape(async);

            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE '!%' ESCAPE '!'");
        }

        [Fact]
        public void String_Like_Literal_With_Backslash()
        {
            using var context = CreateContext();
            var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "\\"));

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" LIKE '\' ESCAPE ''");
        }

        [Fact]
        public void String_ILike_Literal()
        {
            using var context = CreateContext();
            var count = context.Customers.Count(c => EF.Functions.ILike(c.ContactName, "%M%"));

            Assert.Equal(34, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" ILIKE '%M%'");
        }

        [Fact]
        public void String_ILike_Literal_With_Escape()
        {
            using var context = CreateContext();
            var count = context.Customers.Count(c => EF.Functions.ILike(c.ContactName, "!%", "!"));

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""Customers"" AS c
WHERE c.""ContactName"" ILIKE '!%' ESCAPE '!'");
        }

        #endregion

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}