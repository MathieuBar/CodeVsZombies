using System;
using System.Collections.Generic;
using CodeVsZombiesLibrary;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace CodeVsZombiesTest
{
    public class InputsGenerator
    {
        public static Inputs GenerateInputs(CodingGameTestCase gameTestCase) => gameTestCase switch
        {
            CodingGameTestCase.Simple => new Inputs(
                0, 
                0,
                new List<HumanInputs>(1){
                    new HumanInputs(0, 8250, 4500),
                },
                new List<ZombieInputs>(1){
                    new ZombieInputs(0, 8250, 8999, 8250, 8599)
                }
            ),
                
            CodingGameTestCase.TwoZombies => new Inputs(
                5000, 
                0,
                new List<HumanInputs>(2){
                    new HumanInputs(0, 950, 6000),
                    new HumanInputs(1, 8000, 6100),
                },
                new List<ZombieInputs>(2){
                    new ZombieInputs(0, 3100, 7000, 2737, 6831),
                    new ZombieInputs(1, 11500, 7100, 11115, 6990),
                }
            ),
            CodingGameTestCase.Dilemme => new Inputs(
                3989, 
                3259,
                new List<HumanInputs>(){
                    new HumanInputs(0, 647, 384),
                    new HumanInputs(1, 60, 1262),
                    new HumanInputs(2, 1391, 1601),
                    new HumanInputs(3, 1363, 422),
                    new HumanInputs(4, 15470, 384),
                    new HumanInputs(5, 15060, 1262),
                    new HumanInputs(6, 11391, 1601),
                    new HumanInputs(7, 11363, 422),
                },
                new List<ZombieInputs>(){
                    new ZombieInputs(0, 7900, 1579, 8299, 1581),
                    new ZombieInputs(1, 8500, 2470, 8883, 2354),
                    new ZombieInputs(2, 7500, 3798, 7104, 3737),
                    new ZombieInputs(3, 6500, 4682, 6151, 4484),
                    new ZombieInputs(4, 9000, 5664, 9202, 5319),
                    new ZombieInputs(5, 7500, 6319, 7198, 6056),
                    new ZombieInputs(6, 8500, 7094, 8195, 6834),
                    new ZombieInputs(7, 7800, 8447, 7563, 8124),
                    new ZombieInputs(8, 8100, 8847, 7862, 8524),
                    new ZombieInputs(9, 0, 7000, 291, 6726),
                    new ZombieInputs(10, 1000, 7900, 1216, 7563),
                    new ZombieInputs(11, 3000, 8500, 3074, 8106),
                    new ZombieInputs(12, 5000, 7500, 4907, 7110),
                    new ZombieInputs(13, 7000, 6500, 6727, 6206),
                    new ZombieInputs(14, 9000, 7000, 9161, 6634),
                    new ZombieInputs(15, 11000, 7500, 11026, 7100),
                    new ZombieInputs(16, 13000, 8500, 12909, 8110),
                    new ZombieInputs(17, 15000, 7800, 15003, 7400),
                }
            ), 
            CodingGameTestCase.Reflexe => new Inputs(
                8000, 
                4500,
                new List<HumanInputs>(){
                    new HumanInputs(0, 3000, 4500),
                    new HumanInputs(1, 14000, 4500),
                },
                new List<ZombieInputs>(){
                    new ZombieInputs(0, 2500, 4500, 2900, 4500),
                    new ZombieInputs(1, 15500, 6500, 15260, 6180),
                }
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(gameTestCase), $"Not expected coding game test value: {gameTestCase}"),
        };
    }
}
