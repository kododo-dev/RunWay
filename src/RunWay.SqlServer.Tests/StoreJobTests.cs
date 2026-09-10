using Kododo.RunWay.SqlServer.Tests.Fixtures;
using Kododo.RunWay.Storage.Tests;
using Xunit;

namespace Kododo.RunWay.SqlServer.Tests;

[Collection("SqlServer")]
public sealed class StoreJobTests(SqlServerFixture fixture)
    : StoreJobTestsBase<SqlServerFixture>(fixture);

[CollectionDefinition("SqlServer")]
public sealed class SqlServerCollectionDefinition : ICollectionFixture<SqlServerFixture>;
