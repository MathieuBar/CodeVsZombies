using System;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class CodingGameTest
    {
        [TestMethod]
        public void TwoZombies()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.TwoZombies);
            Player player = new Player(inputs);
            Position resultPos = player.GetNextHeroTarget();

            Assert.AreEqual(resultPos.x, inputs.HumansInputs[0].X);
            Assert.AreEqual(resultPos.y, inputs.HumansInputs[0].Y);

            throw new AssertInconclusiveException();
        }
    }
}
