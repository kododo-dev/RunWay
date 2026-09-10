using Kododo.RunWay;

namespace Kododo.RunWay.Storage.Tests;

public interface IStorageTestFixture
{
    void ConfigureStorage(IRunWayConfiguration config);

    Task ResetAsync();
}
