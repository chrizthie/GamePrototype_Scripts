using UnityEngine;

public class Readable : Interactable
{
    public enum ReadableType
    {
        Safe,
        Lore,
        Warning,
        Entity,
        Corrupted,
        StoryCritical
    }

    [Header("Psychological Settings")]
    [SerializeField] private ReadableType readableType;
    [SerializeField] private bool useCustomWord;
    [SerializeField] private string customWord;


    [Header("Readable Content")]
    [SerializeField] private string title;
    [TextArea(5, 15)]
    [SerializeField] private string bodyText;

    public string Title => title;
    public string BodyText => bodyText;

    public ReadableType Type => readableType;
    public bool UseCustomWord => useCustomWord;
    public string CustomWord => customWord;
}