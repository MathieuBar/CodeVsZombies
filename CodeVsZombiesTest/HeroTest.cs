using System;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class HeroTest
    {
        [TestMethod]
        public void ComputeNextPos_NegativeTargetValue_ThrowArgumentOutOfRangeException()
        {
            (Hero h, Position p)[] testCases = new[]
            {
                (new Hero(0, 0), new Position(-1, 0)),
                (new Hero(0, 0), new Position(0, -1)),
            };

            foreach((Hero h, Position p) in testCases)
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(
                    () => h.ComputeNextPos(p));
            }
        }

        [TestMethod]
        public void ComputeNextPos_AlreadyOnTarget_TargetPos()
        {
            (Hero h, Position p)[] testCases = new[]
            {
                (new Hero(0, 0), new Position(0, 0)),
                (new Hero(1000, 2000), new Position(1000, 2000)),
            };

            foreach ((Hero h, Position p) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p);
                Assert.AreEqual(p.x, nextPos.x);
                Assert.AreEqual(p.y, nextPos.y);
            }
        }

        [TestMethod]
        public void ComputeNextPos_InOneMoveRange_TargetPos()
        {
            (Hero h, Position p)[] testCases = new[]
            {
                (new Hero(0, 0), new Position(1000, 0)),
                (new Hero(0, 0), new Position(0, 1000)),
                (new Hero(0, 0), new Position(500, 600)),
                (new Hero(1000, 2000), new Position(1500, 2500)),
                (new Hero(1000, 2000), new Position(500, 2500)),
                (new Hero(1000, 2000), new Position(500, 1500)),
                (new Hero(1000, 2000), new Position(1500, 1500)),
            };

            foreach ((Hero h, Position p) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p);
                Assert.AreEqual(p.x, nextPos.x);
                Assert.AreEqual(p.y, nextPos.y);
            }
        }

        [TestMethod]
        public void ComputeNextPos_SeveralMoves_GoodRoundedNewPos()
        {
            (Hero h, Position p, Position res)[] testCases = new[]
            {
                // cardinal directions
                (new Hero(5000, 5000), new Position(7000, 5000), new Position(6000, 5000)),
                (new Hero(5000, 5000), new Position(5000, 7000), new Position(5000, 6000)),
                (new Hero(5000, 5000), new Position(0, 5000), new Position(4000, 5000)),
                (new Hero(5000, 5000), new Position(5000, 0), new Position(5000, 4000)),
                // non cardinal directions
                (new Hero(5000, 5000), new Position(6000, 5200), new Position(5980, 5196)),
                (new Hero(5000, 5000), new Position(4000, 5200), new Position(4019, 5196)),
                (new Hero(5000, 5000), new Position(4000, 4800), new Position(4019, 4803)),
                (new Hero(5000, 5000), new Position(6000, 4800), new Position(5980, 4803)),
            };

            foreach ((Hero h, Position p, Position res) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p);
                Assert.AreEqual(res.x, nextPos.x);
                Assert.AreEqual(res.y, nextPos.y);
            }
        }

        [TestMethod]
        public void ComputeNextPos_VeryCloseUnderOneMoveRange_TargetReached()
        {
            (Hero h, Position p)[] testCases = new[]
            {
                // cases with real distance between 999 and 1000
                (new Hero(5000, 5000), new Position(5000+707, 5000+707)),
                (new Hero(5000, 5000), new Position(5000-707, 5000+707)),
                (new Hero(5000, 5000), new Position(5000-707, 5000-707)),
                (new Hero(5000, 5000), new Position(5000+707, 5000-707)),
            };

            foreach ((Hero h, Position p) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p);
                Assert.AreEqual(p.x, nextPos.x);
                Assert.AreEqual(p.y, nextPos.y);
            }            
        }

        [TestMethod]
        public void ComputeNextPos_VeryCloseOverOneMoveRange_TargetReached()
        {
            (Hero h, Position p)[] testCases = new[]
            {
                // cases with real distance between 1000 and 1001
                (new Hero(5000, 5000), new Position(5000-708, 5000-707)),
            };

            foreach ((Hero h, Position p) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p);
                Assert.AreEqual(p.x, nextPos.x);
                Assert.AreEqual(p.y, nextPos.y);
            }            
        }

        [TestMethod]
        public void ComputeNextPos_VeryCloseOverOneMoveRange_TargetNotReached()
        {
            (Hero h, Position p, Position res)[] testCases = new[]
            {
                // cases with real distance between 1000 and 1001
                (new Hero(5000, 5000), new Position(5000+708, 5000+707), new Position(5707, 5706)),
                (new Hero(5000, 5000), new Position(5000-708, 5000+707), new Position(4292, 5706)),
                (new Hero(5000, 5000), new Position(5000+708, 5000-707), new Position(5707, 4293)),
            };

            foreach ((Hero h, Position p, Position res) in testCases)
            {
                Position nextPos = h.ComputeNextPos(p);
                Assert.AreEqual(res.x, nextPos.x);
                Assert.AreEqual(res.y, nextPos.y);
            }            
        }

        [TestMethod]
        public void TurnsToBeInRange_NegativeTargetValue_ThrowArgumentOutOfRangeException()
        {
            (Hero h, Position p)[] testCases = new[]
            {
                (new Hero(0, 0), new Position(-1, 0)),
                (new Hero(0, 0), new Position(0, -1)),
            };

            foreach((Hero h, Position p) in testCases)
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(
                    () => h.TurnsToBeInRange(p, 0));
            }
        }

        [TestMethod]
        public void TurnsToBeInRange_AlreadyInRange_Return0()
        {
            (Hero h, Position p, int rng)[] testCases = new[]
            {
                (new Hero(0, 0), new Position(0, 0), 0),
                (new Hero(0, 0), new Position(0, 0), 2000),
                (new Hero(0, 0), new Position(0, 1000), 2000),
                (new Hero(0, 0), new Position(0, 2000), 2000),
            };

            foreach((Hero h, Position p, int rng) in testCases)
            {
                int result = h.TurnsToBeInRange(p, rng);
                Assert.AreEqual(0, result);
            }            
        }

        [TestMethod]
        public void TurnsToBeInRange_HasToMove_ReturnCorrectValue()
        {
            (Hero h, Position p, int rng, int res)[] testCases = new[]
            {
                (new Hero(0, 0), new Position(0, 500), 0, 1),
                (new Hero(0, 0), new Position(0, 3000), 0, 3),
                (new Hero(0, 0), new Position(0, 3500), 0, 4),
                (new Hero(0, 0), new Position(0, 16000), 0, 16),
                (new Hero(0, 0), new Position(0, 2500), 2000, 1),
                (new Hero(0, 0), new Position(0, 3000), 2000, 1),
                (new Hero(0, 0), new Position(0, 3500), 2000, 2),
                (new Hero(0, 0), new Position(0, 16000), 2000, 14),
            };

            foreach((Hero h, Position p, int rng, int res) in testCases)
            {
                int result = h.TurnsToBeInRange(p, rng);
                Assert.AreEqual(res, result);
            }            
        }

        [TestMethod]
        public void TurnsToBeInRange_RoundingTricks_ReturnCorrectValue()
        {
            (Hero h, Position p, int rng, int res)[] testCases = new[]
            {
                // 2 turns thanks to rounding rules, 3 turns if we consider real distance
                (new Hero(5000, 5000), new Position(5000-1416, 5000-1414), 0, 2),
            };

            foreach((Hero h, Position p, int rng, int res) in testCases)
            {
                int result = h.TurnsToBeInRange(p, rng);
                Assert.AreEqual(res, result);
            }                 
        }

        [TestMethod]
        public void UpdateDistancesToHumans_InitWithTwoHumans_GoodDistances()
        {
            throw new AssertInconclusiveException();            
        }

        [TestMethod]
        public void UpdateDistancesToHumans_UpdateDistances_GoodDistances()
        {
            throw new AssertInconclusiveException();            
        }

        [TestMethod]
        public void GetTurnsToGetInRangeToHuman_DistanceNotSet_ThrowsArgumentOutOfRangeException()
        {
            throw new AssertInconclusiveException();
        }
    }
}
