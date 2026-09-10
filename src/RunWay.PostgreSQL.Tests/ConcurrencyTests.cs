using Kododo.RunWay.PostgreSQL.Tests.Fixtures;
using Kododo.RunWay.Storage.Tests;
using Xunit;

namespace Kododo.RunWay.PostgreSQL.Tests;

[Collection("PostgreSQL")]
public sealed class ConcurrencyTests(PostgreSqlFixture fixture)
    : ConcurrencyTestsBase<PostgreSqlFixture>(fixture);
