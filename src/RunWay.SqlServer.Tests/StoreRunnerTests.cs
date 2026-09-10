using Kododo.RunWay.SqlServer.Tests.Fixtures;
using Kododo.RunWay.Storage.Tests;
using Xunit;

namespace Kododo.RunWay.SqlServer.Tests;

[Collection("SqlServer")]
public sealed class StoreRunnerTests(SqlServerFixture fixture)
    : StoreRunnerTestsBase<SqlServerFixture>(fixture);
