using System;
using System.Collections.Generic;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class PositionTest
    {
        [TestMethod]
        public void UndefinedPosition_CheckUndefinedPosition_MinusOne()
        {
            Position expected = new Position(-1, -1);

            Position res = Position.UndefinedPos;

            Assert.AreEqual(expected, res);
        }

        [TestMethod]
        public void FindBarycentre_NullInputs_ThrowsNullArgumentException()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => Position.FindBarycentre(null)
            );
        }
        
        [TestMethod]
        public void FindBarycentre_0Inputs_ThrowsArgumentOutOfRangeException()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => Position.FindBarycentre(new List<Position>())
            );
        }

        [TestMethod]
        public void FindBarycentre_1Pos_SamePosition()
        {
            Position expectedPosition = new Position(1, 1);
            IEnumerable<Position> positions = new List<Position>{
                expectedPosition,
            };
            
            Position res = Position.FindBarycentre(positions);

            Assert.AreEqual(expectedPosition, res);
        }

        [TestMethod]
        public void FindBarycentre_SeveralPositions_GoodBarycentre()
        {
            IEnumerable<Position> positions = new List<Position>{
                new Position(10, 100),
                new Position(30, 100),
                new Position(40, 200),
            };
            double expectedX = (10+30+40)/3.0;
            double expectedY = (100+100+200)/3.0;
            Position expectedPosition = new Position(expectedX, expectedY);
            
            Position res = Position.FindBarycentre(positions);

            Assert.AreEqual(expectedPosition, res);
        }


        [TestMethod]
        public void Postion_CreateFromInts_SameValuesThanInput()
        {
            (int, int)[] testCases = new[]
            {
                (0, 0),
                (-1, -1),
                (6999, 4500),
            };

            foreach((int, int) testValues in testCases)
            {
                Position pos = new Position(testValues.Item1, testValues.Item2);
                Assert.AreEqual(testValues.Item1, pos.X);
                Assert.AreEqual(testValues.Item2, pos.Y);
            }
        }

        [TestMethod]
        public void Postion_CreateFromDoubles_WellRoundedValues()
        {
            (double, double, int, int)[] testCases = new[]
            {
                (0.0, 0.0, 0, 0),
                (-1.1, -0.9, -2, -1),
                (0.9, 1.1, 0, 1),
            };

            foreach((double x, double y, int resX, int resY) in testCases)
            {
                Position pos = new Position(x, y);
                Assert.AreEqual(resX, pos.X);
                Assert.AreEqual(resY, pos.Y);
            }
        }

        [TestMethod]
        public void Equals_EqualsPosition_ReturnTrue()
        {
            (Position, Position)[] testCases = new[]
            {
                (new Position(0, 0), new Position(0, 0)),
                (new Position(0, -1), new Position(0, -1)),
                (new Position(1000, 2000), new Position(1000, 2000)),
            };

            foreach((Position p1, Position p2) in testCases)
            {
                Assert.IsTrue(p1.Equals(p2));
                Assert.IsTrue(p2.Equals(p1));
            }            
        }

        [TestMethod]
        public void DistanceTo_NegativeValues_ReturnsNaN()
        {
            (Position, Position, double)[] testCases = new[]
            {
                (new Position(-1, 0), new Position(0, 0), double.NaN),
                (new Position(0, -1), new Position(0, 0), double.NaN),
            };

            foreach((Position, Position, int) tc in testCases)
            {
                Assert.IsTrue(double.IsNaN(tc.Item1.DistanceTo(tc.Item2)));
                Assert.IsTrue(double.IsNaN(tc.Item2.DistanceTo(tc.Item1)));
            }
        }

        [TestMethod]
        public void DistanceTo_ValidPositions_ReturnsDistance()
        {
            (Position, Position, double)[] testCases = new (Position, Position, double)[]
            {
                (new Position(0, 0), new Position(0, 0), 0),
                (new Position(0, 56), new Position(0, 678), 678-56),
                (new Position(56, 0), new Position(678, 0), 678-56),
                (new Position(0, 0), new Position(3, 4), 5),
                (new Position(0, 0), new Position(1, 1), Math.Sqrt(2)),
            };

            foreach((Position, Position, double) tc in testCases)
            {
                Assert.AreEqual(tc.Item1.DistanceTo(tc.Item2), tc.Item3);
                Assert.AreEqual(tc.Item2.DistanceTo(tc.Item1), tc.Item3);
            }
        }
    }
}
