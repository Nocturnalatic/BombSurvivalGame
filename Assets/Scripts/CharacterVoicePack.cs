using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Voice Pack", menuName = "Character Voice Pack", order = 1)]
public class CharacterVoicePack : ScriptableObject
{
    public List<AudioSource> lightDamageTakenNoises;
    public List<AudioSource> heavyDamageTakenNoises;
    public List<AudioSource> deathNoises;
}
