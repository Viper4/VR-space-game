using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatSystem : MonoBehaviour
{
    [SerializeField] float health = 100;
    [SerializeField] float maxHealth = 100;
    [SerializeField] float armor = 0;
    [SerializeField] float maxArmor = 100;

    [SerializeField] bool player;
    [SerializeField] Slider healthBar;
    [SerializeField] Slider armorBar;
    [SerializeField] Animation damageAnimation;

    public void Damage(float amount, float armorPiercing)
    {
        float healthDamage = amount * armorPiercing;
        health -= healthDamage;
        armor -= amount - healthDamage;
        health = Mathf.Clamp(health, 0, maxHealth);
        armor = Mathf.Clamp(armor, 0, maxArmor);

        if (player)
        {
            damageAnimation.Play();
            healthBar.value = health;
            armorBar.value = armor;
        }
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public void AddArmor(float amount)
    {
        armor += amount;
        armor = Mathf.Clamp(armor, 0, maxArmor);
    }
}
