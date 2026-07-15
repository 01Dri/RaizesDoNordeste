using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using RaizesDoNordeste.Application.UseCases.Accounts;
using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.Core.Accounts;
using RaizesDoNordeste.Domain.Core.Accounts.Roles;
using RaizesDoNordeste.Domain.Core.Users;
using RaizesDoNordeste.Domain.ValuesObjects;

namespace RaizesDoNordeste.Test.UseCases.Account
{
    [TestFixture]
    public class GetUserProfileUseCaseTest
    {
        private ApplicationDbContext _context;
        private Mock<ICurrentUser> _currentUserMock;
        private GetUserProfileUseCase _useCase;
        private readonly long _accountId = 12;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _currentUserMock = new Mock<ICurrentUser>();
            _currentUserMock.Setup(x => x.AccountId).Returns(_accountId);

            _useCase = new GetUserProfileUseCase(_context, _currentUserMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ShouldReturnUserProfile_WhenAccountExists()
        {
            // Arrange
            var account = new RaizesDoNordeste.Domain.Core.Accounts.Account
            {
                Id = _accountId,
                Email = new Email("profile@user.com"),
                Password = "hashedpassword",
                RoleAccounts = new List<RoleAccount>
                {
                    RoleAccount.Create(RoleType.Customer, RoleStatus.Enable),
                    RoleAccount.Create(RoleType.Professional, RoleStatus.Enable)
                }
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _useCase.HandleAsync(CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data.Id, Is.EqualTo(_accountId));
                Assert.That(result.Data.Email, Is.EqualTo("profile@user.com"));
                Assert.That(result.Data.Roles, Contains.Item(RoleType.Customer.ToString()));
                Assert.That(result.Data.Roles, Contains.Item(RoleType.Professional.ToString()));
                Assert.That(result.Data.Active, Is.True);
            });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnFailure_WhenAccountNotFoundInDatabase()
        {
            // Arrange
            _currentUserMock.Setup(x => x.AccountId).Returns(999L);

            // Act
            var result = await _useCase.HandleAsync(CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.ErrorData.Message, Contains.Substring("Usuário não encontrado"));
            });
        }
    }
}
