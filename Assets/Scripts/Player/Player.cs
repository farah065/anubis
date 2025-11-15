using System;
using UnityEngine;

namespace GEM
{
    public class Player : MonoBehaviour
    {
        public int health = 100;

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {

            }
        }
    }
}