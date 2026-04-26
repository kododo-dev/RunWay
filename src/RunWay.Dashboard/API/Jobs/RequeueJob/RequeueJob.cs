using Kododo.Reiho.AspNetCore.API;

namespace Kododo.RunWay.Dashboard.API.Jobs.RequeueJob;

internal sealed record RequeueJob(string Id) : IRequest;
