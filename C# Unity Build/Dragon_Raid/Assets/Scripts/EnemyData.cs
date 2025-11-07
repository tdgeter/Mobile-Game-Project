using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public int attack;  // (ATK)
    public int defense; // (DEF)
    public int luck;    // (LCK)
    public int power;   // (PWR) 
}