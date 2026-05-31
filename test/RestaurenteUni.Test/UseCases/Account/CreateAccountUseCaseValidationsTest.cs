using Microsoft.EntityFrameworkCore;
using RestauranteUni.Application.UseCases.Account;
using RestauranteUni.Application.UseCases.Account.Validations;
using RestauranteUni.Data;
using RestauranteUni.Domain.Account.DTO;
using RestauranteUni.Domain.UseCases;

namespace RestaurenteUni.Test.UseCases.Account
{
    public class CreateAccountUseCaseValidationsTest
    {

        private IUseCaseHandler<CreateAccountDto, CreateAccountResponseDto> _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _handler = new CreateAccountUseCaseHandler(new ApplicationDbContext(options), new CreateAccountDtoValidation());
        }


        [TestCase("diego@gmail.com", "Abc12345")]
        [TestCase("contato@empresa.com.br", "MinhaSenha123")]
        [TestCase("usuario@outlook.com", "Senha2025")]
        [TestCase("teste@hotmail.com", "Password1")]
        [TestCase("admin@restauranteuni.com", "Admin123")]
        public async Task ShouldReturnSuccess_WhenCreateAccountDtoIsValid(
            string email,
            string password)
        {
            var createDto = new CreateAccountDto()
            {
                Email = email,
                Password = password,
                BirthDate = DateTime.Now.AddYears(-18)
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Validations, Is.Empty);
            });
        }

        [TestCase("diego@gmail.com")]
        [TestCase("contato@empresa.com.br")]
        [TestCase("teste@outlook.com")]
        [TestCase("admin@hotmail.com")]
        [TestCase("usuario@yahoo.com")]
        public async Task ShouldNotReturnEmailValidation_WhenEmailIsValid(string email)
        {
            var createDto = new CreateAccountDto()
            {
                Email = email,
                Password = "Abc12345",
                BirthDate = DateTime.Now
            };

            var result = await _handler.HandleAsync(createDto);

            var emailValidation = result.Validations
                .FirstOrDefault(x => x.Property == "Email");

            Assert.That(emailValidation, Is.Null);
        }


        [TestCase("Abc12345")]
        [TestCase("MinhaSenha123")]
        [TestCase("Password1")]
        [TestCase("Admin2025")]
        [TestCase("Teste123ABC")]
        public async Task ShouldNotReturnPasswordValidation_WhenPasswordIsValid(string password)
        {
            var createDto = new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = password,
                BirthDate = DateTime.Now
            };

            var result = await _handler.HandleAsync(createDto);

            var passwordValidation = result.Validations
                .FirstOrDefault(x => x.Property == "Password");

            Assert.That(passwordValidation, Is.Null);
        }

        [TestCase("")]
        [TestCase("diego")]
        [TestCase("diego.com")]
        [TestCase("@gmail.com")]
        [TestCase("diego@")]
        [TestCase("diego@gmail")]
        [TestCase("diego@@gmail.com")]
        public async Task ShouldReturnResultFailureWithEmailProperty_WhenEmailIsInvalid(string email)
        {
            var createDto = new CreateAccountDto()
            {
                Email = email,
                Password = "123456",
                BirthDate = DateTime.Now
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>            
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Empty);

                var emailValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Email");

                Assert.That(emailValidation, Is.Not.Null);
                Assert.That(emailValidation!.ErrorsMessage.Contains("Invalid e-mail"), Is.True);
            });
        }

        [Test]
        public async Task ShouldReturnMultipleEmailValidationErrors()
        {
            var createDto = new CreateAccountDto()
            {
                Email = "",
                Password = "123456",
                BirthDate = DateTime.Now
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var emailValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Email");

                Assert.That(emailValidation, Is.Not.Null);

                Assert.That(emailValidation!.ErrorsMessage.Count, Is.EqualTo(2));

                Assert.That(
                    emailValidation.ErrorsMessage,
                    Contains.Item("E-mail is required"));

                Assert.That(
                    emailValidation.ErrorsMessage,
                    Contains.Item("Invalid e-mail"));
            });
        }

        [Test]
        public async Task ShouldReturnMultiplePasswordValidationErrors_WhenPasswordIsEmpty()
        {
            var createDto = new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = "",
                BirthDate = DateTime.Now
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var passwordValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Password");

                Assert.That(passwordValidation, Is.Not.Null);

                Assert.That(
                    passwordValidation!.ErrorsMessage,
                    Contains.Item("Password is required"));

                Assert.That(
                    passwordValidation.ErrorsMessage,
                    Contains.Item("Password must have at least 8 characters"));

                Assert.That(
                    passwordValidation.ErrorsMessage,
                    Contains.Item("Password must contain at least one uppercase letter"));

                Assert.That(
                    passwordValidation.ErrorsMessage,
                    Contains.Item("Password must contain at least one lowercase letter"));

                Assert.That(
                    passwordValidation.ErrorsMessage,
                    Contains.Item("Password must contain at least one number"));
            });
        }

        [TestCase("12345678")]
        [TestCase("abcdefgh")]
        [TestCase("ABCDEFGH")]
        [TestCase("Abcdefgh")]
        [TestCase("ABC12345")]
        [TestCase("abc12345")]
        [TestCase("Abc123")]
        public async Task ShouldReturnPasswordValidationError_WhenPasswordIsInvalid(string password)
        {
            var createDto = new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = password,
                BirthDate = DateTime.Now
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var passwordValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Password");

                Assert.That(passwordValidation, Is.Not.Null);
                Assert.That(passwordValidation!.ErrorsMessage, Is.Not.Empty);
            });
        }
    }
}
