using UnityEngine;

[System.Serializable]
public struct PlayerInputState
{
    public Vector2 move;
    public Vector2 look;

    public bool run;
    public bool crouch;
    public bool flashlight;
}
