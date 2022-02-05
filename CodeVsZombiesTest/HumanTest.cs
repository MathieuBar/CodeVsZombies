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

        [TestMethod]
        public void AddThreateningZombie_AddFirstThreat_OneThreatCount()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie = new Zombie(0, 400, 0, 0, 0);
            Hero hero = new Hero(3000, 0);
            zombie.UpdateTarget(hero, humans);

            bool added = human.AddThreateningZombie(zombie, hero, humans);

            Assert.IsTrue(added);
            Assert.AreEqual(1, human.ThreateningZombiesCount);
            Assert.AreEqual(1, human.TurnsBeforeBeingCaught);
            Assert.IsFalse(human.Doomed);
        }

        [TestMethod]
        public void AddThreateningZombie_Add2ndThreatNearest_GoodTurnsBeforeBeingCaught()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie1 = new Zombie(0, 800, 0, 400, 0);
            Zombie zombie2 = new Zombie(1, 400, 0, 0, 0);
            Hero hero = new Hero(4000, 0);
            zombie1.UpdateTarget(hero, humans);
            zombie2.UpdateTarget(hero, humans);

            human.AddThreateningZombie(zombie1, hero, humans);
            human.AddThreateningZombie(zombie2, hero, humans);

            Assert.AreEqual(2, human.ThreateningZombiesCount);
            Assert.AreEqual(1, human.TurnsBeforeBeingCaught);
            Assert.IsTrue(human.Doomed);
        }

        [TestMethod]
        public void AddThreateningZombie_Add2ndThreatFurthest_GoodTurnsBeforeBeingCaught()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie1 = new Zombie(0, 400, 0, 0, 0);
            Zombie zombie2 = new Zombie(1, 800, 0, 400, 0);
            Hero hero = new Hero(3000, 0);
            zombie1.UpdateTarget(hero, humans);
            zombie2.UpdateTarget(hero, humans);

            human.AddThreateningZombie(zombie1, hero, humans);
            human.AddThreateningZombie(zombie2, hero, humans);

            Assert.AreEqual(2, human.ThreateningZombiesCount);
            Assert.AreEqual(1, human.TurnsBeforeBeingCaught);
        }

        [TestMethod]
        public void AddThreateningZombie_AddZombieWithNoTargetSet_Added()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie = new Zombie(0, 400, 0, 0, 0);
            Hero hero = new Hero(3000, 0);
            zombie.UpdateTarget(hero, humans);

            bool result = human.AddThreateningZombie(zombie, hero, humans);

            Assert.IsTrue(result);
            Assert.AreEqual(1, human.ThreateningZombiesCount);
            Assert.AreEqual(1, human.TurnsBeforeBeingCaught);
        }

        [TestMethod]
        public void AddThreateningZombie_AddZombieWithHeroTarget_NotAdded()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie1 = new Zombie(0, 5400, 0, 5000, 0);
            Hero hero = new Hero(5000, 0);
            zombie1.UpdateTarget(hero, humans);

            bool result = human.AddThreateningZombie(zombie1, hero, humans);

            Assert.IsFalse(result);
            Assert.AreEqual(0, human.ThreateningZombiesCount);
            Assert.AreEqual(int.MaxValue, human.TurnsBeforeBeingCaught);
        }

        [TestMethod]
        public void AddThreateningZombie_AddZombieWithWrongTarget_NotAdded()
        {
            Human human1 = new Human(0, 0, 0);
            Human human2 = new Human(1, 1000, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human1,
                human2,
            };
            Zombie zombie1 = new Zombie(0, 1400, 0, 1000, 0);
            Hero hero = new Hero(5000, 0);
            // zombie1 target will be human2
            zombie1.UpdateTarget(hero, humans);

            bool result = human1.AddThreateningZombie(zombie1, hero, humans);

            Assert.IsFalse(result);
            Assert.AreEqual(0, human1.ThreateningZombiesCount);
            Assert.AreEqual(int.MaxValue, human1.TurnsBeforeBeingCaught);
        }

        [TestMethod]
        public void AddThreateningZombie_AddZombieAlreadyKnown_NotAdded()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie1 = new Zombie(0, 400, 0, 0, 0);
            Hero hero = new Hero(5000, 0);
            zombie1.UpdateTarget(hero, humans);

            human.AddThreateningZombie(zombie1, hero, humans);
            bool result = human.AddThreateningZombie(zombie1, hero, humans);

            Assert.IsFalse(result);
            Assert.AreEqual(1, human.ThreateningZombiesCount);
            Assert.AreEqual(1, human.TurnsBeforeBeingCaught);
        }        

        [TestMethod]
        public void ClearThreateningZombies_ClearThreats_0ThreatAndMaxValueTurnsBeforeCaught()
        {
            Human human = new Human(0, 0, 0);
            IEnumerable<Human> humans = new List<Human>(1)
            {
                human,
            };
            Zombie zombie = new Zombie(0, 400, 0, 0, 0);
            Hero hero = new Hero(5000, 0);
            zombie.UpdateTarget(hero, humans);
            human.AddThreateningZombie(zombie, hero, humans);

            human.ClearThreateningZombies();

            Assert.AreEqual(0, human.ThreateningZombiesCount);
            Assert.AreEqual(int.MaxValue, human.TurnsBeforeBeingCaught);
        }

        [TestMethod]
        public void OnNewTurnStarted_NewTurn_ThreatsDataCleared()
        {
            Inputs inputs = InputsGenerator.GenerateInputs(CodingGameTestCase.Simple);
            Player p = new Player(inputs);
            Hero hero = new Hero(5000, 0);
            Zombie zombie = new Zombie(0, 0, 400, 0, 0);
            Human human = new Human(0, 0, 0, p);
            List<Human> humans = new List<Human>() { human };
            // set zombie target to human and add threatening zombie to human threats
            zombie.UpdateTarget(hero, humans);
            human.AddThreateningZombie(zombie, hero, humans);
            // check that init is well done
            Assert.AreEqual(1, human.ThreateningZombiesCount);
            Assert.AreEqual(1, human.TurnsBeforeBeingCaught);
            Assert.IsTrue(human.Doomed);

            // send NewTurnStarted event
            p.UpdateFromNewInputs(inputs); 

            // check that Threats data is cleared
            Assert.AreEqual(0, human.ThreateningZombiesCount);
            Assert.AreEqual(int.MaxValue, human.TurnsBeforeBeingCaught);
            Assert.IsFalse(human.Doomed);
        }
    }
}
