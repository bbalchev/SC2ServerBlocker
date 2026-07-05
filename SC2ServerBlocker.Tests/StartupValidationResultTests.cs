using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SC2ServerBlocker.Tests
{
    [TestClass]
    public class StartupValidationResultTests
    {
        [TestMethod]
        public void AllowsBlocking_IsFalse_OnlyForErrors()
        {
            Assert.IsTrue(StartupValidationResult.Ok().AllowsBlocking);
            Assert.IsTrue(StartupValidationResult.Warning("warn").AllowsBlocking);
            Assert.IsFalse(StartupValidationResult.Error("error").AllowsBlocking);
        }
    }
}
