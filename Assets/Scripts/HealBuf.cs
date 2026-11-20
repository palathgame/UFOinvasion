
using UnityEngine;

public class HealBuf : BuffScript
{
 
        protected override string Label()
        {
            return positiv ? "HP +15" : "HP -15";
        }

        public int getStatHeal()
        {

            if (positiv)
            {

                return 15;
            }

            return -15;

        }



    }
