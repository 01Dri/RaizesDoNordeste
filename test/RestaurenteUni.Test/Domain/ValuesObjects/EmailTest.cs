using RestauranteUni.Domain.ValuesObjects;

namespace RestaurenteUni.Test.Domain.ValuesObjects
{
    public class EmailTest
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ShouldThrowArgumentNullExceptionWhenIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Email(null!));
        }

        [Test]
        public void ShouldThrowArgumentExceptionWhenIsEmpty()
        {
            Assert.Throws<ArgumentException>(() => new Email(""));
        }

        [Test]
        public void ShouldThrowArgumentExceptionWhenIsWhiteSpace()
        {
            Assert.Throws<ArgumentException>(() => new Email(" "));
        }

        [TestCase("diego")]
        [TestCase("diego!")]
        [TestCase("diego@")]
        [TestCase("@gmail.com")]
        [TestCase("diego.com")]
        public void ShouldThrowArgumentExceptionWhenIsInvalid(string value)
        {
            Assert.Throws<ArgumentException>(() => new Email(value));
        }
    }
}
