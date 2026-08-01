using RaizesDoNordeste.Application.Services;
using RaizesDoNordeste.Domain.Services;

namespace RaizesDoNordeste.Test.Services
{
    public sealed class HasherServiceTest
    {

        private IHasherService _hasherService;
        [SetUp]
        public void Setup()
        {
            _hasherService = new HasherService();
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        public void ShouldThrowArgumentException_WhenPasswordIsInvalid(string? password)
        {
            Assert.Throws(Is.TypeOf<ArgumentException>().Or.TypeOf<ArgumentNullException>(),
                () => _hasherService.HashPassword(password!));
        }

        [TestCase("myPassword123")]
        [TestCase("senha123@")]
        [TestCase("ComplexPassword!@#$%")]
        [TestCase("a")]
        public void ShouldReturnValidHash_WhenPasswordIsValid(string password)
        {
            var hash = _hasherService.HashPassword(password);

            Assert.Multiple(() =>
            {
                Assert.That(hash, Is.Not.Null);
                Assert.That(hash, Is.Not.Empty);
                Assert.That(hash.Length, Is.GreaterThan(0));
                Assert.That(hash, Is.Not.EqualTo(password));
            });
        }

        [TestCase("myPassword123")]
        [TestCase("senha123@")]
        public void ShouldReturnTrue_WhenPasswordMatchesHash(string password)
        {
            var hash = _hasherService.HashPassword(password);
            var result = _hasherService.VerifyPassword(password, hash);

            Assert.That(result, Is.True);
        }

        [TestCase("correctPassword", "wrongPassword")]
        [TestCase("senha123@", "senha123")]
        [TestCase("ComplexPassword!@#$%", "ComplexPassword!@#$")]
        public void ShouldReturnFalse_WhenPasswordDoesNotMatchHash(string correctPassword, string wrongPassword)
        {
            var hash = _hasherService.HashPassword(correctPassword);
            var result = _hasherService.VerifyPassword(wrongPassword, hash);

            Assert.That(result, Is.False);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        public void ShouldThrowArgumentException_WhenVerifyPasswordWithInvalidPassword(string? password)
        {
            var validHash = _hasherService.HashPassword("validPassword");

            Assert.Throws(Is.TypeOf<ArgumentException>().Or.TypeOf<ArgumentNullException>(),
                () => _hasherService.VerifyPassword(password!, validHash));
        }

        [Test]
        public void ShouldProduceDifferentHashesForSamePassword()
        {
            var password = "myPassword123";
            var hash1 = _hasherService.HashPassword(password);
            var hash2 = _hasherService.HashPassword(password);

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }
    }
}

