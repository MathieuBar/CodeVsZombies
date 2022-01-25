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
                
            _ => throw new ArgumentOutOfRangeException(nameof(gameTestCase), $"Not expected coding game test value: {gameTestCase}"),
        };
    }
}
