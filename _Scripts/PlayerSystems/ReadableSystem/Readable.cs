using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("Already Read Behavior")]
    [SerializeField] private bool alreadyReadBehavior;
    public bool AlreadyReadBehavior => alreadyReadBehavior;
    private bool hasBeenRead;
    public bool HasBeenRead => hasBeenRead;
    public void MarkAsRead()
    {
        hasBeenRead = true;
    }

    [Header("Readable Content")]
    [SerializeField] private string title;

    [TextArea(5, 15)]
    [SerializeField] private string bodyText;

    [Header("Readable Appearance")]
    [SerializeField] private TMP_FontAsset titleFont;
    [SerializeField] private TMP_FontAsset bodyFont;

    [Header("Readable Audio")]
    [SerializeField]
    private List<ReadableAudioData> audioStates =
    new List<ReadableAudioData>();

    public List<ReadableAudioData> AudioStates =>
        audioStates;


    public string Title => title;
    public string BodyText => bodyText;

    public TMP_FontAsset TitleFont => titleFont;
    public TMP_FontAsset BodyFont => bodyFont;

    public ReadableType Type => readableType;
    public bool UseCustomWord => useCustomWord;
    public string CustomWord => customWord;

}