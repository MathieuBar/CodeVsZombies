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

            throw new AssertInconclusiveException();
        }
    }
}
