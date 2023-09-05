namespace KahaAPI.Test
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestApiCall()
        {
            // Arrange
            using (var httpClient = new HttpClient())
            {
                var apiUrl = "https://restcountries.com/v2/all";

                // Act
                var response = await httpClient.GetAsync(apiUrl);

                // Assert
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Assert.NotEmpty(content);
            }
        }
    }
}
