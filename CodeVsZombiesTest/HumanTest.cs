using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class HumanTest
    {

        [TestMethod]
        public void ComputeNextPos_NoSpeed_ReturnSourcePos()
        {
            Human h = new Human(0, -1, -1);
            (Position p1, Position p2)[] testCases = new[]
            {
                (new Position(0, 0), new Position(-1, -1)),
                (new Position(0, 0), new Position(0, 0)),
                (new Position(1000, 2000), new Position(0, 0)),
            };

            foreach((Position p1, Position p2) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p1, p2);
                Assert.AreEqual(p1.X, nextPos.X);
                Assert.AreEqual(p1.Y, nextPos.Y);
            }
        }

        [TestMethod]
        public void ComputeNextPos_NoSpeed_ReturnPresentPos()
        {
            (Human h, Position p)[] testCases = new[]
            {
                (new Human(0, 0, 0), new Position(-1, -1)),
                (new Human(0, 0, 0), new Position(0, 0)),
                (new Human(0, 1000, 2000), new Position(0, 0)),
            };

            foreach((Human h, Position p) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p);
                Assert.AreEqual(h.Pos.X, nextPos.X);
                Assert.AreEqual(h.Pos.Y, nextPos.Y);
            }
        }
    }
}
