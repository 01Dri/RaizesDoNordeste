using Microsoft.Extensions.Configuration;
using RestauranteUni.Application.Services;
using RestauranteUni.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Moq;

namespace RestaurenteUni.Test.Services
{
    public class TokenServiceTest
    {
        private ITokenService _tokenService;
        private Mock<IConfiguration> _configurationMock;
        private const string TestKey = "this_is_a_very_long_secret_key_for_jwt_token_generation_12345";
        private const string TestIssuer = "RestauranteUniAPI";
        private const string TestAudience = "RestauranteUniClients";

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x["Jwt:Key"]).Returns(TestKey);
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(TestIssuer);
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(TestAudience);

            _tokenService = new TokenService(_configurationMock.Object);
        }

        [Test]
        public void ShouldReturnValidToken_WhenCalledWithValidParameters()
        {
            var userId = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim> 
            { 
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = _tokenService.WriteToken(userId, sub, claims);

            Assert.Multiple(() =>
            {
                Assert.That(token, Is.Not.Null);
                Assert.That(token, Is.Not.Empty);
                Assert.That(token, Is.TypeOf<string>());
            });
        }

        [Test]
        public void ShouldContainCorrectClaims_WhenTokenIsDecoded()
        {
            var userId = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim> 
            { 
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("custom", "value")
            };

            var token = _tokenService.WriteToken(userId, sub, claims);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.Multiple(() =>
            {
                Assert.That(jwtToken, Is.Not.Null);
                Assert.That(jwtToken!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value, Is.EqualTo(userId.ToString()));
                Assert.That(jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value, Is.EqualTo(sub));
                Assert.That(jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value, Is.EqualTo("Admin"));
                Assert.That(jwtToken.Claims.FirstOrDefault(x => x.Type == "custom")?.Value, Is.EqualTo("value"));
            });
        }

        [Test]
        public void ShouldHaveCorrectIssuerAndAudience_WhenTokenIsDecoded()
        {
            var userId = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim>();

            var token = _tokenService.WriteToken(userId, sub, claims);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.Multiple(() =>
            {
                Assert.That(jwtToken!.Issuer, Is.EqualTo(TestIssuer));
                Assert.That(jwtToken.Audiences.FirstOrDefault(), Is.EqualTo(TestAudience));
            });
        }

        [Test]
        public void ShouldHaveDefaultExpiration_WhenExpirationIsNotProvided()
        {
            var userId = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim>();

            var token = _tokenService.WriteToken(userId, sub, claims);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var expirationTime = jwtToken!.ValidTo;
            var now = DateTime.UtcNow;

            // Allow for some time difference (should be around 30 minutes)
            Assert.That(expirationTime.Subtract(now).TotalMinutes, Is.GreaterThan(25).And.LessThan(35));
        }

        [Test]
        public void ShouldHaveCustomExpiration_WhenExpirationIsProvided()
        {
            var userId = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim>();
            var customExpiration = DateTime.UtcNow.AddHours(2);

            var token = _tokenService.WriteToken(userId, sub, claims, customExpiration);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            // Allow for small time differences
            var timeDifference = Math.Abs((jwtToken!.ValidTo - customExpiration).TotalSeconds);
            Assert.That(timeDifference, Is.LessThan(5));
        }

        [Test]
        public void ShouldDifferentTokensForDifferentUsers()
        {
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim>();

            var token1 = _tokenService.WriteToken(userId1, sub, claims);
            var token2 = _tokenService.WriteToken(userId2, sub, claims);

            Assert.That(token1, Is.Not.EqualTo(token2));
        }

        [Test]
        public void ShouldHandleMultipleClaims_WhenProvidingVariousClaimTypes()
        {
            var userId = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Manager"),
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim("department", "IT"),
                new Claim("level", "5")
            };

            var token = _tokenService.WriteToken(userId, sub, claims);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var roleClaims = jwtToken!.Claims.Where(x => x.Type == ClaimTypes.Role).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(roleClaims, Has.Count.EqualTo(2));
                Assert.That(jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value, Is.EqualTo("user@example.com"));
                Assert.That(jwtToken.Claims.FirstOrDefault(x => x.Type == "department")?.Value, Is.EqualTo("IT"));
            });
        }

        [Test]
        public void ShouldBeValidJwt_WhenTokenIsGenerated()
        {
            var userId = Guid.NewGuid();
            var sub = "user@example.com";
            var claims = new List<Claim>();

            var token = _tokenService.WriteToken(userId, sub, claims);
            var handler = new JwtSecurityTokenHandler();

            var canRead = handler.CanReadToken(token);

            Assert.That(canRead, Is.True);
        }
    }
}
