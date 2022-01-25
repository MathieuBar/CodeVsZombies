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
        public void UpdateTarget_OneHuman_HumanIsNearest()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Hero hero = new Hero(inputs);

            zombie.UpdateTarget(hero, humans);

            Assert.IsFalse(zombie.NextTargetIsHero);
        }

        [TestMethod]
        public void UpdateTarget_OneHuman_HeroIsNearest()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Hero hero = new Hero(zombie.Pos.x, zombie.Pos.y + 1);

            zombie.UpdateTarget(hero, humans);

            Assert.IsTrue(zombie.NextTargetIsHero);
        }

        [TestMethod]
        public void UpdateTarget_OneHuman_GoodTargetGoodDistance()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.OneZombieOneHuman);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Hero hero = new Hero(inputs);

            zombie.UpdateTarget(hero, humans);

            Assert.AreEqual(zombie.NextNearestHuman.Id, 0);
            Assert.AreEqual(zombie.TurnsToNearestHuman, 4);
        }

        [TestMethod]
        public void UpdateTarget_OneClearlyNearest_GoodTarget()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.ZombieClearyNearerToOneOfTwoHumans);
            Zombie zombie = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Hero hero = new Hero(inputs);

            zombie.UpdateTarget(hero, humans);

            Assert.AreEqual(zombie.NextNearestHuman.Id, 1);
        }

        [TestMethod]
        public void UpdateTarget_NearlySameDistance_AlwaysTheNearest()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.ZombieBarelyNearerToOneOfTwoHumans);
            Zombie zombie1 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Zombie zombie2 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humansReverse = humans.Reverse();
            Hero hero = new Hero(inputs);

            zombie1.UpdateTarget(hero, humans);
            zombie2.UpdateTarget(hero, humansReverse);

            Assert.AreEqual(zombie1.NextNearestHuman.Id, 0);
            Assert.AreEqual(zombie2.NextNearestHuman.Id, 0);
        }

        [TestMethod]
        public void UpdateTarget_ExactSameDistance_LowestId()
        {
            Inputs inputs = ZombieTest.GenerateInputs(ZombieTestCase.ZombieAtExactSameDistanceOfTwoHumans);
            Zombie zombie1 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humans = inputs.HumansInputs.Select(hi => new Human(hi));
            Zombie zombie2 = new Zombie(inputs.ZombieInputs[0]);
            IEnumerable<Human> humansReverse = humans.Reverse();
            Hero hero = new Hero(inputs);

            zombie1.UpdateTarget(hero, humans);
            zombie2.UpdateTarget(hero, humansReverse);

            Assert.AreEqual(zombie1.NextNearestHuman.Id, 0);
            Assert.AreEqual(zombie2.NextNearestHuman.Id, 0);
        }
    }
}
