using UnityEngine;

[CreateAssetMenu(
    fileName = "ReadableWordDatabase",
    menuName = "Psychological Readable/Word Database"
)]
public class ReadableWordDatabase : ScriptableObject
{
    [Header("Safe Words")]
    public string[] safeWords;

    [Header("Lore Words")]
    public string[] loreWords;

    [Header("Warning Words")]
    public string[] warningWords;

    [Header("Entity Words")]
    public string[] entityWords;

    [Header("Corrupted Words")]
    public string[] corruptedWords;
}