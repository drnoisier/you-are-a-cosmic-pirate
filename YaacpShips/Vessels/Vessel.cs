﻿using System;
using YaacpShips.Cannons;

namespace YaacpShips
{
    namespace Vessels
    {
        public abstract class Vessel
        {
            public string Name {get; protected set;}
            public int[] Crew {get; protected set;}
            public int[] CrewMax {get; protected set;}
            public string[] CrewTypes {get; protected set;}
            public int Health {get; protected set;}
            public int HealthMax {get; protected set;}
            public int Size {get; protected set;}
            public string Status {get; set;}
            
            private Cannon[] armament;

            public Cannon[] Armament
            {
                get
                {
                    return armament;
                }
                set
                {
                    armament = new Cannon[3 * this.Size];

                    for (var i = 0; i < armament.Length; i++)
                    {
                        if (value[i] != null)
                        {
                            if (value[i].Size > this.Size) value[i].Size = this.Size;

                            armament[i] = value[i];
                        }
                    }
                }
            }

            public Vessel(string shipName, string[] crewTypes, int size, int healthBase)
            {
                this.Name = shipName;
                this.CrewTypes = crewTypes;
                this.Status = "Nothing";

                if (size < 1) size = 1;
                else if (size > 3) size = 3;

                this.Size = size;
                this.HealthMax = healthBase * this.Size;
                this.Health = this.HealthMax;
                this.CrewMax = new int[this.CrewTypes.Length];
                this.Crew = new int[this.CrewTypes.Length];

                int basicAmount;

                for (var i = 0; i < this.CrewTypes.Length; i++)
                {
                    if (this.CrewTypes[i] == "troops") basicAmount = 40;
                    else if (this.CrewTypes[i] == "sailors") basicAmount = 30;
                    else basicAmount = 5;

                    this.CrewMax[i] = basicAmount * this.Size;
                    this.Crew[i] = 0;
                }
            }

            public void GetDamage(int value)
            {
                if (value > this.Health) this.Health = 0;
                else if (value > 0) this.Health -= value;

                if (this.Health == 0)
                {
                    for (var i = 0; i < this.Crew.Length; i++)
                    {
                        this.Crew[i] = 0;
                    }

                    for (var i = 0; i < this.Armament.Length; i++)
                    {
                        this.Armament[i].Working = false;
                    }
                }
                else this.UpdateCannonsWorkingStatus("damage");
            }

            public void Repair(int value)
            {
                if ((value > 0) && (this.Health > 0))
                {    
                    this.Health += value;

                    if (this.Health > this.HealthMax) this.Health = this.HealthMax;

                    this.UpdateCannonsWorkingStatus("repair");
                }
            }

            protected void UpdateCannonsWorkingStatus(string updateCause)
            {

                if (updateCause != "damage" && updateCause != "repair") return;

                int healthPercentage = (this.Health * 100) / this.HealthMax;
                int cannonsWorking = 0;

                for (var i = 0; i < this.Armament.Length; i++)
                {
                    if (this.Armament[i].Working) cannonsWorking += 1;
                }

                int cannonsWorkingPercentage = (cannonsWorking * 100) / this.Armament.Length;

                int a = healthPercentage;
                int b = cannonsWorkingPercentage;
                bool cannonsMustWorking = true;

                if (updateCause == "damage")
                {
                    a = cannonsWorkingPercentage;
                    b = healthPercentage;
                    cannonsMustWorking = false;
                }

                if (a > b)
                {
                    Random randomizer = new Random();
                    int cannonsNotWorking = (this.Armament.Length / 100) * healthPercentage;

                    for (int i = cannonsNotWorking; i > 0; i--)
                    {
                        int randomIndex;
                        bool cannonIsWorking;

                        do
                        {
                            randomIndex = randomizer.Next(0, this.Armament.Length - 1);
                            cannonIsWorking = this.Armament[randomIndex].Working;

                            if (cannonIsWorking != cannonsMustWorking) this.Armament[randomIndex].Working = cannonsMustWorking;   
                        }
                        while (cannonIsWorking == cannonsMustWorking);
                    }
                }
            }

            public void GetCrew(string type, int number)
            {
                int index = Array.IndexOf(this.CrewTypes, type);
                int spaceLeft = this.CrewMax[index] - this.Crew[index];

                if (spaceLeft >= number) this.Crew[index] += number;
                else this.Crew[index] += spaceLeft;

                if (this.Crew[index] <= 0) this.Crew[index] = 0;
            }

            public bool[] CannonsReadyToFire()
            {
                bool[] result = new bool[this.Armament.Length];

                for (var i = 0; i < this.Armament.Length; i++)
                {
                    result[i] = this.Armament[i].Working && this.Armament[i].Load;
                }

                return result;
            }

            public void Boarding(Vessel enemy)
            {
                int ourPower = BasicParameters.BoardingPower(this);
                int enemyPower = BasicParameters.BoardingPower(enemy);

                int winnerPower;
                int winnerPowerStart;
                Vessel winner;
                Vessel looser;
                int[] winningCrew;

                if (ourPower != enemyPower)
                {
                    if (ourPower > enemyPower)
                    {
                        winnerPower = ourPower - enemyPower;
                        winnerPowerStart = ourPower;
                        winner = this;
                        looser = enemy;
                    }
                    else
                    {
                        winnerPower = enemyPower - ourPower;
                        winnerPowerStart = enemyPower;
                        winner = enemy;
                        looser = this;
                    }

                    winningCrew = BasicParameters.CrewRest(winnerPowerStart, winnerPower, winner.CrewTypes, winner.Crew);

                    for (var i = 0; i < winner.CrewTypes.Length; i++)
                    {
                        winner.GetCrew(winner.CrewTypes[i], -(winner.CrewMax[i]));
                        winner.GetCrew(winner.CrewTypes[i], winningCrew[i]);
                    }
                }
                else
                {
                    if (this == enemy) looser = this;
                    else
                    {
                        Random randomizer = new Random();

                        if (randomizer.Next(0, 1) == 0)
                        {
                            winner = this;
                            looser = enemy;
                        }
                        else
                        {
                            winner = enemy;
                            looser = this;
                        }

                        winningCrew = BasicParameters.CrewRest(45, winner.CrewTypes, winner.Crew);

                        for (var i = 0; i < winner.CrewTypes.Length; i++)
                        {
                            winner.GetCrew(winner.CrewTypes[i], -(winner.CrewMax[i]));
                            winner.GetCrew(winner.CrewTypes[i], winningCrew[i]);
                        }
                    }
                }

                for (var i = 0; i < looser.CrewTypes.Length; i++)
                {
                    looser.GetCrew(looser.CrewTypes[i], -(looser.CrewMax[i]));
                }
            }

            public int Volley()
            {
                int result = 0;
                Random randomizer = new Random();

                for (var i = 0; i < this.Armament.Length; i++)
                {
                    if (this.Armament[i].Load) result += this.Armament[i].Fire(randomizer);
                }

                return result;
            }

            public void ArmamentReload()
            {
                for (var i = 0; i < this.Armament.Length; i++)
                {
                    if (this.Armament[i].Working && (this.Armament[i].CooldownCount > 0)) this.Armament[i].CooldownCount -= 1;
                }
            }
        }
    }
}
