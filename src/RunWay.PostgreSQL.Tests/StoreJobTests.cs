using Kododo.RunWay.PostgreSQL.Tests.Fixtures;
using Kododo.RunWay.Storage.Tests;
using Xunit;

namespace Kododo.RunWay.PostgreSQL.Tests;

[Collection("PostgreSQL")]
public sealed class StoreJobTests(PostgreSqlFixture fixture)
    : StoreJobTestsBase<PostgreSqlFixture>(fixture);

[CollectionDefinition("PostgreSQL")]
public sealed class PostgreSqlCollectionDefinition : ICollectionFixture<PostgreSqlFixture>;
