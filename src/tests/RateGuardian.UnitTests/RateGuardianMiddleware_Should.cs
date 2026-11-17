using RateGuardian.Middleware;
using RateGuardian.Storage;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace RateGuardian.UnitTests;

[TestClass]
public class RateGuardianMiddleware_Should
{

  [TestMethod]
  public async Task TrackUnderIpAddressNone_WhenRemoteIpMissing()
  {
    // Arrange
    var storage = new InMemStorage();
    var middleware = new RateGuardianMiddleware(storage);

    // Act
    await middleware.InvokeAsync(new DefaultHttpContext(), _ => Task.CompletedTask);

    // Assert
    Assert.IsTrue(storage.TrackedRequests.ContainsKey(IPAddress.None));
  }
}


