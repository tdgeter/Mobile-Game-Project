using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    
    [Header("Skill Properties")]
    public int power = 10; 
    public int hitCount = 1;
    public float damageMultiplier = 1f;
    
    public int apCost = 0; 
}