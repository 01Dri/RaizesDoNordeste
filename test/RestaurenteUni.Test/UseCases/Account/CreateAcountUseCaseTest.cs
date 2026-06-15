using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestauranteUni.Application.UseCases.Accounts;
using RestauranteUni.Data;
using RestauranteUni.Domain.Core.Accounts.DTO;
using RestauranteUni.Domain.Core.Accounts.Roles;
using RestauranteUni.Domain.Services;
using RestauranteUni.Domain.UseCases;

namespace RestaurenteUni.Test.UseCases.Account
{
    public class CreateAcountUseCaseTest
    {
        private IUseCaseHandler<CreateAccountDto, CreateAccountUseCaseResponseDto> _handler;
        private Mock<IHasherService> _passwordEncrypter;
        private Mock<IValidator<CreateAccountDto>> _validator;
        private ApplicationDbContext _context;


        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _passwordEncrypter = new Mock<IHasherService>();
            _validator = new Mock<IValidator<CreateAccountDto>>();
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
            _handler = new CreateAccountUseCaseHandler(_context, _validator.Object, _passwordEncrypter.Object);

        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task ShouldCallHashPasswordMethod()
        {
            var createDto = CreateValidDto();

            SetupValidValidation(createDto);
            SetupPasswordHash();

            await _handler.HandleAsync(createDto);

            _validator.Verify(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()), Times.Once);
            _passwordEncrypter.Verify(x => x.HashPassword(createDto.Password), Times.Once);
        }

        [Test]
        public async Task ShouldReturnSuccessWithCreatedAccountData()
        {
            var createDto = CreateValidDto();

            SetupValidValidation(createDto);
            SetupPasswordHash();

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Validations, Is.Empty);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.Id, Is.GreaterThan(0));
                Assert.That(result.Data.Email.Value, Is.EqualTo(createDto.Email));
            });
        }

        [Test]
        public async Task ShouldPersistAccountWithHashedPassword()
        {
            var createDto = CreateValidDto();

            SetupValidValidation(createDto);
            SetupPasswordHash();

            var result = await _handler.HandleAsync(createDto);

            var account = await _context.Accounts
                .FirstAsync(x => x.Id == result.Data!.Id);

            Assert.Multiple(() =>
            {
                Assert.That(account.Email.Value, Is.EqualTo(createDto.Email));
                Assert.That(account.Password, Is.EqualTo("hash_password"));
                Assert.That(account.Password, Is.Not.EqualTo(createDto.Password));
            });
        }

        [Test]
        public async Task ShouldCreateAccountWithCustomerRoleEnabled()
        {
            var createDto = CreateValidDto();

            SetupValidValidation(createDto);
            SetupPasswordHash();

            var result = await _handler.HandleAsync(createDto);

            var account = await _context.Accounts
                .Include(x => x.RoleAccounts)
                .ThenInclude(x => x.Role)
                .FirstAsync(x => x.Id == result.Data!.Id);

            Assert.Multiple(() =>
            {
                Assert.That(account.RoleAccounts, Has.Count.EqualTo(1));
                Assert.That(account.RoleAccounts.First().Role!.Id, Is.EqualTo(RoleType.Customer));
                Assert.That(account.RoleAccounts.First().RoleStatus, Is.EqualTo(RoleStatus.Enable));
            });
        }

        [Test]
        public async Task ShouldReuseExistingRole_WhenCreateAccountWithCustomerRole()
        {
            var createDto = CreateValidDto();

            SetupValidValidation(createDto);
            SetupPasswordHash();

            var customerRole = await _context.Roles
                .SingleAsync(x => x.Id == RoleType.Customer);

            await _handler.HandleAsync(createDto);

            var roleAccount = await _context.RoleAccounts
                .SingleAsync();

            Assert.Multiple(() =>
            {
                //Assert.That(roleAccount.RoleId, Is.EqualTo(customerRole.Id));
                Assert.That(_context.Roles.Count(), Is.EqualTo(5));
            });
        }

        [Test]
        public async Task ShouldReturnCreatedRoleInResponse()
        {
            var createDto = CreateValidDto();

            SetupValidValidation(createDto);
            SetupPasswordHash();

            var result = await _handler.HandleAsync(createDto);

            Assert.That(result.Data!.Roles, Is.EquivalentTo(new[] { RoleType.Customer }));
        }

        [Test]
        public async Task ShouldReturnFailure_WhenValidationFails()
        {
            var createDto = CreateValidDto();

            _validator.Setup(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(
                [
                    new ValidationFailure(nameof(CreateAccountDto.Email), "Invalid e-mail")
                ]));

            var result = await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Data, Is.Null);
                Assert.That(result.Validations, Is.Not.Empty);
                Assert.That(result.Validations.First().Property, Is.EqualTo(nameof(CreateAccountDto.Email)));
                Assert.That(result.Validations.First().Errors, Contains.Item("Invalid e-mail"));
            });
        }

        [Test]
        public async Task ShouldNotPersistAccountOrRoleAccount_WhenValidationFails()
        {
            var createDto = CreateValidDto();

            _validator.Setup(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(
                [
                    new ValidationFailure(nameof(CreateAccountDto.Email), "Invalid e-mail")
                ]));

            await _handler.HandleAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(_context.Accounts.Count(), Is.EqualTo(0));
                Assert.That(_context.RoleAccounts.Count(), Is.EqualTo(0));
                Assert.That(_context.Roles.Count(), Is.EqualTo(5));
            });
        }

        [Test]
        public async Task ShouldNotCallHashPassword_WhenValidationFails()
        {
            var createDto = CreateValidDto();

            _validator.Setup(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(
                [
                    new ValidationFailure(nameof(CreateAccountDto.Password), "Password is required")
                ]));

            await _handler.HandleAsync(createDto);

            _passwordEncrypter.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        }

        private void SetupValidValidation(CreateAccountDto createDto)
        {
            _validator.Setup(x => x.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        private void SetupPasswordHash()
        {
            _passwordEncrypter.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hash_password");
        }

        private static CreateAccountDto CreateValidDto()
        {
            return new CreateAccountDto()
            {
                Email = "diego@gmail.com",
                Password = "MinhaSenha123",
                BirthDate = DateTime.Now.AddYears(-18)
            };
        }

    }
}
