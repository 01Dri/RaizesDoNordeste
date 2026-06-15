using Microsoft.EntityFrameworkCore;
using RestauranteUni.Application.UseCases.Login;
using RestauranteUni.Application.UseCases.Login.Validations;
using RestauranteUni.Data;
using RestauranteUni.Domain.UseCases;
using System.Net;
using Moq;
using RestauranteUni.Application.Services;
using RestauranteUni.Domain.ValuesObjects;
using RestauranteUni.Domain.Services;
using System.Security.Claims;
using RestauranteUni.Domain.Core.Login;

namespace RestaurenteUni.Test.UseCases.Login
{
    public class LoginUseCaseTest
    {
        private ApplicationDbContext _context;
        private IUseCaseHandler<LoginDto, LoginResponseDto> _handler;
        private static readonly Guid RestauranteUniversitarioId = Guid.Parse("9a88024d-2618-4e25-87f5-35217f7a7c8a");
        private Mock<IHasherService> _hashService;
        private Mock<ITokenService> _tokenService;
        private const string HashedPassword = "$2a$11$dXJ4VDEuVjFKSUlJSUlJSS5GVEV3VHJvMHB2cHYwMHB2cHYwMHB2"; // BCrypt hash of "senha123@"

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
            InsertAccount();

            _hashService = new Mock<IHasherService>();
            _hashService.Setup(x => x.VerifyPassword("senha123@", HashedPassword))
                .Returns(true);
            _hashService.Setup(x => x.VerifyPassword(It.IsNotIn("senha123@"), HashedPassword))
                .Returns(false);

            _tokenService = new Mock<ITokenService>();
            _tokenService.Setup(x => x.WriteToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<Claim>>(), It.IsAny<DateTime?>()))
                .Returns("mocked_jwt_token");

            _handler = new LoginUseCaseHandler(_context, new LoginUseCaseDtoValidation(), _hashService.Object, _tokenService.Object);

        }


        [Test]
        public async Task ShouldReturnBadRequest_WhenNotFoundRestaurantById()
        {
            var loginDto = CreateValidLoginDto(Guid.NewGuid());
            var result = await _handler.HandleAsync(loginDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations.Count, Is.EqualTo(1));
                Assert.That(result.Validations.First().Errors, Contains.Item("Restaurant not found"));
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            });

        } 
        // Account not found by e-mail, should return 401  with error message (Invalid credentials) 


            [Test]
        public async Task ShouldReturnUnauthorized_WhenNotFoundAccountByEmail()
        {
            var loginDto = new LoginDto("notfound@gmail.com", "senha123@", RestauranteUniversitarioId);
            var result = await _handler.HandleAsync(loginDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations.Count, Is.EqualTo(1));
                Assert.That(result.Validations.First().Errors, Contains.Item("Invalid credentials"));
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            });
        }

        [Test]
        public async Task ShouldReturnSuccess_WhenAccountAndRestaurantWereFound()
        {
            var loginDto = CreateValidLoginDto();
            var result = await _handler.HandleAsync(loginDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Validations, Is.Empty);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.Token, Is.Not.Empty);
            });
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("diego")]
        [TestCase("diego.com")]
        [TestCase("@gmail.com")]
        [TestCase("diego@")]
        [TestCase("diego@gmail")]
        [TestCase("diego@@gmail.com")]

        public async Task ShouldReturnBadRequest_WhenEmailIsInvalid(string email)
        {
            var loginDto = new LoginDto(email, "", RestauranteUniversitarioId); 

            var result = await _handler.HandleAsync(loginDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations.Count, Is.EqualTo(1));
                Assert.That(result.Validations.First().Errors, Contains.Item($"Invalid {nameof(LoginDto.Email)}"));
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            });
        }

        [TestCase("")]
        [TestCase(" ")]
        public async Task ShouldReturnBadRequest_WhenPasswordIsInvalid(string password)
        {
            var loginDto = new LoginDto("diego@gmail.com", password, RestauranteUniversitarioId);

            var result = await _handler.HandleAsync(loginDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations.Count, Is.EqualTo(1));
                Assert.That(result.Validations.First().Errors, Contains.Item($"Invalid {nameof(LoginDto.Password)}"));
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            });
        }

        [Test]
        public async Task ShouldReturnBadRequest_WhenRestaurantIdIsInvalid()
        {
            var loginDto = new LoginDto("diego@gmail.com", "senha123@", Guid.Empty);

            var result = await _handler.HandleAsync(loginDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations.Count, Is.EqualTo(1));
                Assert.That(result.Validations.First().Errors, Contains.Item($"Invalid {nameof(LoginDto.RestaurantId)}"));
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            });
        }

        [Test]
        public async Task ShouldNotReturnRestaurantIdValidation_WhenRestaurantIdIsValid()
        {
            var loginDto = CreateValidLoginDto();

            var result = await _handler.HandleAsync(loginDto);

            var restaurantIdValidation = result.Validations
                .FirstOrDefault(x => x.Property == nameof(LoginDto.RestaurantId));

            Assert.That(restaurantIdValidation, Is.Null);
        }

        [TestCase("", "")]
        [TestCase(" ", " ")]
        [TestCase("diego", " ")]
        [TestCase("diego.com", " ")]
        [TestCase("@gmail.com", " ")]
        [TestCase("diego@", " ")]
        [TestCase("diego@gmail", " ")]
        [TestCase("diego@@gmail.com", " ")]

        public async Task ShouldReturnBadRequest_WhenEmailAndPasswordIsInvalid(string email, string password)
        {
            var loginDto = new LoginDto(email, password, Guid.NewGuid());

            var result = await _handler.HandleAsync(loginDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations.Count, Is.EqualTo(1));
                Assert.That(result.Validations.First().Errors, Contains.Item($"Invalid {nameof(LoginDto.Email)}"));
                Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            });
        }


        private LoginDto CreateValidLoginDto(Guid? restaurantId = null)
        {
            return new LoginDto("diego@gmail.com", "senha123@", restaurantId ?? RestauranteUniversitarioId);
        }

        private void InsertAccount()
        {
            _context.Accounts.Add(new RestauranteUni.Domain.Core.Accounts.Account
            {
                Email = new Email("diego@gmail.com"),
                Password = HashedPassword
            });

            _context.SaveChanges();
        }




        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

    }



}
