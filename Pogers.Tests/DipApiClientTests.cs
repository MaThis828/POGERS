using System.Net;
using Pogers.Infrastructure.Dip;

namespace Pogers.Tests
{
    public class DipApiClientTests
    {
        [Fact]
        public async Task Liefert_Personen_ueber_mehrere_Cursor_Seiten()
        {
            var handler = new FakeHttpMessageHandler()
                .RespondWithJson(TestFactory.PersonSeite("seite2", TestFactory.Person(1, "Lars", "Klingbeil")))
                .RespondWithJson(TestFactory.PersonSeite("seite2", TestFactory.Person(2, "Olaf", "Scholz")));

            var personen = await TestFactory.DipClient(handler.CreateClient()).GetPersonenAsync(21).ToListAsync();

            Assert.Equal(new[] { "Klingbeil", "Scholz" }, personen.Select(p => p.Nachname));
            Assert.Contains("cursor=seite2", handler.AngefragteUrls[1]);
        }

        [Fact]
        public async Task Endet_bei_Cursor_null()
        {
            var handler = new FakeHttpMessageHandler()
                .RespondWithJson(TestFactory.PersonSeite(null, TestFactory.Person(1, "Olaf", "Scholz")));

            var personen = await TestFactory.DipClient(handler.CreateClient()).GetPersonenAsync(21).ToListAsync();

            Assert.Single(personen);
            Assert.Single(handler.AngefragteUrls);
        }

        [Fact]
        public async Task Endet_bei_leerer_Seite()
        {
            var handler = new FakeHttpMessageHandler()
                .RespondWithJson(TestFactory.PersonSeite("weiter", TestFactory.Person(1, "Olaf", "Scholz")))
                .RespondWithJson(TestFactory.PersonSeite("nochweiter"));

            var personen = await TestFactory.DipClient(handler.CreateClient()).GetPersonenAsync(21).ToListAsync();

            Assert.Single(personen);
            Assert.Equal(2, handler.AngefragteUrls.Count);
        }

        [Fact]
        public async Task Wahlperiode_steht_in_der_Url_und_der_Key_nur_im_Header()
        {
            var handler = new FakeHttpMessageHandler().RespondWithJson(TestFactory.PersonSeite(null));

            await TestFactory.DipClient(handler.CreateClient()).GetPersonenAsync(21).ToListAsync();

            Assert.Contains("f.wahlperiode=21", handler.AngefragteUrls[0]);
            Assert.DoesNotContain("testkey", handler.AngefragteUrls[0]);
            Assert.Equal("ApiKey testkey", handler.AuthorizationHeader[0]);
        }

        [Fact]
        public async Task Abgelehnter_Key_wirft_mit_Hinweis_auf_user_secrets()
        {
            var handler = new FakeHttpMessageHandler().RespondWith(HttpStatusCode.Unauthorized);

            var fehler = await Assert.ThrowsAsync<DipApiException>(
                () => TestFactory.DipClient(handler.CreateClient()).GetPersonenAsync(21).ToListAsync());

            Assert.Contains("user-secrets", fehler.Message);
        }

        [Fact]
        public async Task Rate_Limit_wirft_mit_Statuscode()
        {
            var handler = new FakeHttpMessageHandler().RespondWith(HttpStatusCode.TooManyRequests);

            var fehler = await Assert.ThrowsAsync<DipApiException>(
                () => TestFactory.DipClient(handler.CreateClient()).GetPersonenAsync(21).ToListAsync());

            Assert.Contains("429", fehler.Message);
        }
    }
}
