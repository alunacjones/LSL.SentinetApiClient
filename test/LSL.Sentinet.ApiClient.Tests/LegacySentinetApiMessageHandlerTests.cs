using System;
using RichardSzalay.MockHttp;

namespace LSL.Sentinet.ApiClient.Tests;

public class LegacySentinetApiMessageHandlerTests
{
    [Test]
    public async Task GivenAnInvalidMessageHandler_ItShouldThrowTheExpectedException()
    {
        var client = CreateClient(authFails: true, isAlreadAuthorised: false);

        var act = async () => await client.GetFolderAsync(0);
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task GivenAValidMessageHandler_ItShouldAccessTheApi()
    {
        var client = CreateClient(authFails: false, isAlreadAuthorised: false);

        var response = await client.GetFolderAsync(0);
        response.Should().BeOfType<Folder>();
    }

    [Test]
    public async Task GivenAValidMessageHandlerAndWeAreAlreadyAuthorised_ItShouldAccessTheApi()
    {
        var client = CreateClient(authFails: false, isAlreadAuthorised: true);

        var response = await client.GetFolderAsync(0);
        response.Should().BeOfType<Folder>();
    }    

    private static ISentinetApiClient CreateClient(bool authFails, bool isAlreadAuthorised)
    {
        var mockHttpHandler = CreateMockHttpMessageHandler();

        var httpClient = new HttpClient(new LegacySentinetApiMessageHandler("username", "password")
        {
            InnerHandler = mockHttpHandler
        })
        {
            BaseAddress = new Uri("http://nowhere.com/RepositoryService.svc/")
        };

        mockHttpHandler.StubEndPointsAndAuth(authFails, isAlreadAuthorised);

        mockHttpHandler.When(HttpMethod.Get, "http://nowhere.com/RepositoryService.svc/LogOn?userName=username&password=password")
            .Respond(new StringContent(authFails ? "false" : "true"));

        return new SentinetApiClient(httpClient);
    }
}