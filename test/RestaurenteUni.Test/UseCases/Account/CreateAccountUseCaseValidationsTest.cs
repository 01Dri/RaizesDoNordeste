using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestauranteUni.Application.UseCases.Accounts;
using RestauranteUni.Application.UseCases.Accounts.Validations;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Accounts.DTO;
using RestauranteUni.Domain.Services;
using RestauranteUni.Domain.UseCases;

namespace RestaurenteUni.Test.UseCases.Account
{
    public class CreateAccountUseCaseValidationsTest
    {

        private IUseCaseHandler<CreateAccountDto, CreateAccountUseCaseResponseDto> _handler;
        private Mock<IHasherService> _passwordEncrypter;
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _passwordEncrypter = new Mock<IHasherService>();
            _passwordEncrypter.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hash_password");
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
            _handler = new CreateAccountUseCaseHandler(_context, new CreateAccountDtoValidation(), _passwordEncrypter.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
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
                BirthDate = DateTime.Now.AddYears(-18)
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
                BirthDate = DateTime.Now.AddYears(-18)
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
                BirthDate = DateTime.Now.AddYears(-18)
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>            
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Validations, Is.Not.Empty);

                var emailValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Email");

                Assert.That(emailValidation, Is.Not.Null);
                Assert.That(emailValidation!.Errors.Contains("E-mail inválido"), Is.True);
            });
        }

        [Test]
        public async Task ShouldReturnMultipleEmailValidationErrors()
        {
            var createDto = new CreateAccountDto()
            {
                Email = "",
                Password = "123456",
                BirthDate = DateTime.Now.AddYears(-18)
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var emailValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Email");

                Assert.That(emailValidation, Is.Not.Null);

                Assert.That(emailValidation!.Errors.Count, Is.EqualTo(2));

                Assert.That(
                    emailValidation.Errors,
                    Contains.Item("O e-mail é obrigatório"));

                Assert.That(
                    emailValidation.Errors,
                    Contains.Item("E-mail inválido"));
            });
        }

        [Test]
        public async Task ShouldReturnMultiplePasswordValidationErrors_WhenPasswordIsEmpty()
        {
            var createDto = new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = "",
                BirthDate = DateTime.Now.AddYears(-18)
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var passwordValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Password");

                Assert.That(passwordValidation, Is.Not.Null);

                Assert.That(
                    passwordValidation!.Errors,
                    Contains.Item("A senha é obrigatória"));

                Assert.That(
                    passwordValidation.Errors,
                    Contains.Item("A senha deve ter pelo menos 8 caracteres"));

                Assert.That(
                    passwordValidation.Errors,
                    Contains.Item("A senha deve conter pelo menos uma letra maiúscula"));

                Assert.That(
                    passwordValidation.Errors,
                    Contains.Item("A senha deve conter pelo menos uma letra minúscula"));

                Assert.That(
                    passwordValidation.Errors,
                    Contains.Item("A senha deve conter pelo menos um número"));
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
                BirthDate = DateTime.Now.AddYears(-18)
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var passwordValidation = result.Validations
                    .FirstOrDefault(x => x.Property == "Password");

                Assert.That(passwordValidation, Is.Not.Null);
                Assert.That(passwordValidation!.Errors, Is.Not.Empty);
            });
        }

        [TestCaseSource(nameof(ValidBirthDates))]
        public async Task ShouldNotReturnBirthDateValidation_WhenBirthDateIsValid(DateTime birthDate)
        {
            var createDto = new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = "Abc12345",
                BirthDate = birthDate
            };

            var result = await _handler.HandleAsync(createDto);

            var birthDateValidation = result.Validations
                .FirstOrDefault(x => x.Property == nameof(CreateAccountDto.BirthDate));

            Assert.That(birthDateValidation, Is.Null);
        }

        [Test]
        public async Task ShouldReturnBirthDateValidation_WhenBirthDateIsEmpty()
        {
            var createDto = new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = "Abc12345",
                BirthDate = default
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var birthDateValidation = result.Validations
                    .FirstOrDefault(x => x.Property == nameof(CreateAccountDto.BirthDate));

                Assert.That(birthDateValidation, Is.Not.Null);
                Assert.That(
                    birthDateValidation!.Errors,
                    Contains.Item("A data de nascimento é obrigatória"));
            });
        }

        [Test]
        public async Task ShouldReturnBirthDateValidation_WhenBirthDateIsInTheFuture()
        {
            var createDto = new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = "Abc12345",
                BirthDate = DateTime.Today.AddDays(1)
            };

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);

                var birthDateValidation = result.Validations
                    .FirstOrDefault(x => x.Property == nameof(CreateAccountDto.BirthDate));

                Assert.That(birthDateValidation, Is.Not.Null);
                Assert.That(
                    birthDateValidation!.Errors,
                    Contains.Item("A data de nascimento não pode ser no futuro"));
            });
        }

        private static IEnumerable<DateTime> ValidBirthDates()
        {
            yield return DateTime.Today;
            yield return DateTime.Today.AddYears(-18);
            yield return DateTime.Today.AddYears(-100);
        }
    }
}
