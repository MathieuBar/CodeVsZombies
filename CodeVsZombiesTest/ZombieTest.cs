using System;
using System.Collections.Generic;
using System.Linq;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeVsZombiesTest
{
    [TestClass]
    public class ZombieTest
    {
        public static Inputs GenerateInputs(ZombieTestCase unitTestCase) => unitTestCase switch
        {
            ZombieTestCase.OneZombieOneHuman => new Inputs(
                0,
                0,
                new List<HumanInputs>(1){
                    new HumanInputs(0, 1000, 0),
                },
                new List<ZombieInputs>(1){
                    new ZombieInputs(0, 2400, 0, 2000, 0),
                }
            ),

            ZombieTestCase.ZombieClearyNearerToOneOfTwoHumans => new Inputs(
                0,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1000, 0),
                    new HumanInputs(1, 2000, 0),
                },
                new List<ZombieInputs>(1){
                    new ZombieInputs(0, 3000, 0, 3000, 0),
                }
            ),

            ZombieTestCase.ZombieBarelyNearerToOneOfTwoHumans => new Inputs(
                0,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1000, 0),
                    new HumanInputs(1, 2000, 0),
                },
                new List<ZombieInputs>(1){
                    new ZombieInputs(0, 1499, 8000, 1499, 8000),
                }
            ),

            ZombieTestCase.ZombieAtExactSameDistanceOfTwoHumans => new Inputs(
                0,
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 1000, 0),
                    new HumanInputs(1, 2000, 0),
                },
                new List<ZombieInputs>(1){
                    new ZombieInputs(0, 1500, 0, 1500, 0),
                }
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(unitTestCase), $"Not expected unit test name value: {unitTestCase}"),
        };


        [TestMethod]
        public void GetNearestHuman_OneHuman_GoodTarget()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));

            Human result = zombie.GetNearestHuman(humans);

            Assert.AreEqual(0, result.Id);
        }


        [TestMethod]
        public void GetNearestHuman_OneClearlyNearest_GoodTarget()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.ZombieClearyNearerToOneOfTwoHumans);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));

            Human result = zombie.GetNearestHuman(humans);

            Assert.AreEqual(1, result.Id);
        }

        [TestMethod]
        public void GetNearestHuman_NearlySameDistance_AlwaysTheNearest()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.ZombieBarelyNearerToOneOfTwoHumans);
            Zombie zombie1 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Zombie zombie2 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humansReverse = humans.Reverse();

            Human result1 = zombie1.GetNearestHuman(humans);
            Human result2 = zombie2.GetNearestHuman(humansReverse);

            Assert.AreEqual(0, result1.Id);
            Assert.AreEqual(0, result2.Id);
        }

        [TestMethod]
        public void GetNearestHuman_ExactSameDistance_LowestId()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.ZombieAtExactSameDistanceOfTwoHumans);
            Zombie zombie1 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Zombie zombie2 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humansReverse = humans.Reverse();

            Human result1 = zombie1.GetNearestHuman(humans);
            Human result2 = zombie2.GetNearestHuman(humansReverse);

            Assert.AreEqual(0, result1.Id);
            Assert.AreEqual(0, result2.Id);
        }

        [TestMethod]
        public void GetDistToNearestHuman_OneHuman_GoodDistance()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));

            double result = zombie.GetDistToNearestHuman(humans);

            Assert.AreEqual(1400.0, result);           
        }

        [TestMethod]
        public void GetTurnsToNearestHuman_OneHuman_GoodDistance()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));

            int result = zombie.GetTurnsToNearestHuman(humans);

            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void GetNextTargetIsHero_OneHuman_HumanIsNearest()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Hero hero = new Hero(inputs);

            zombie.UpdateTarget(hero, humans);

            Assert.IsFalse(zombie.GetNextTargetIsHero(hero, humans));
        }

        [TestMethod]
        public void GetNextTargetIsHero_OneHuman_HeroIsNearest()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Hero hero = new Hero(zombie.Pos.X, zombie.Pos.Y + 1);

            zombie.UpdateTarget(hero, humans);

            Assert.IsTrue(zombie.GetNextTargetIsHero(hero, humans));
        }

        [TestMethod]
        public void GetNextPosition_GivenNextPos_GivenNextPos()
        {
            Hero hero = new Hero(0, 0);
            IEnumerable<Human> humans = new List<Human>{
                new Human(0, 3000, 3000),
            };
            Zombie zombie = new Zombie(0, 2000, 2000, 5000, 5000);
            Position expected = new Position(5000, 5000);

            zombie.UpdateTarget(hero, humans);
            Position res = zombie.GetNextPosition();

            Assert.AreEqual(expected.X, res.X);
        }

        [TestMethod]
        public void GetNextPosition_NoGivenNextPosAndTowardsHuman_GoodComputedNextPos()
        {
            Hero hero = new Hero(0, 0);
            IEnumerable<Human> humans = new List<Human>{
                new Human(0, 3000, 3000),
            };
            Zombie zombie = new Zombie(0, 2000, 2000);
            Position expected = new Position(2282, 2282);

            zombie.UpdateTarget(hero, humans);
            Position res = zombie.GetNextPosition();

            Assert.AreEqual(expected.X, res.X);
        }

        [TestMethod]
        public void GetNextPosition_NoGivenNextPosAndTowardsHero_GoodComputedNextPos()
        {
            Hero hero = new Hero(0, 0);
            IEnumerable<Human> humans = new List<Human>{
                new Human(0, 3000, 3000),
            };
            Zombie zombie = new Zombie(0, 1000, 1000);
            Position expected = new Position(717, 717);

            zombie.UpdateTarget(hero, humans);
            Position res = zombie.GetNextPosition();

            Assert.AreEqual(expected.X, res.X);
        }
    }
}
