using UnityEngine;

public class PlayerZoneState : MonoBehaviour
{
    public int currentZone = -1;
    public bool inPuzzleMode = false;

    public void SetZone(int zone)
    {
        currentZone = zone;
        inPuzzleMode = true;
    }

    public void ExitZone()
    {
        currentZone = -1;
        inPuzzleMode = false;
    }

    public int GetZone()
    {
        return currentZone;
    }

    public bool IsInPuzzleMode()
    {
        return inPuzzleMode;
    }

    public void ExitPuzzleMode()
    {
        inPuzzleMode = false;
    }
}
