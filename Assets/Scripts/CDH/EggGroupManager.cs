using System.Collections.Generic;
using UnityEngine;

public class EggGroupManager : MonoBehaviour
{
    [Header("페이즈별 알 리스트")]
    public List<BreakOnXKey> phase1Eggs = new List<BreakOnXKey>();
    public List<BreakOnXKey> phase2Eggs = new List<BreakOnXKey>();
    public List<BreakOnXKey> phase3Eggs = new List<BreakOnXKey>();

    private BreakOnXKey phase1DropEgg;
    private BreakOnXKey phase2DropEgg;
    private BreakOnXKey phase3DropEgg;

    public void RegisterEgg(BreakOnXKey egg, int phase)
    {
        if (phase == 1) phase1Eggs.Add(egg);
        else if (phase == 2) phase2Eggs.Add(egg);
        else if (phase == 3) phase3Eggs.Add(egg);
    }

    public void PickDropEgg(int phase)
    {
        if (phase == 1 && phase1DropEgg == null && phase1Eggs.Count > 0)
            phase1DropEgg = phase1Eggs[Random.Range(0, phase1Eggs.Count)];
        else if (phase == 2 && phase2DropEgg == null && phase2Eggs.Count > 0)
            phase2DropEgg = phase2Eggs[Random.Range(0, phase2Eggs.Count)];
        else if (phase == 3 && phase3DropEgg == null && phase3Eggs.Count > 0)
            phase3DropEgg = phase3Eggs[Random.Range(0, phase3Eggs.Count)];
    }

    public bool IsDropEgg(BreakOnXKey egg, int phase)
    {
        if (phase == 1) return egg == phase1DropEgg;
        if (phase == 2) return egg == phase2DropEgg;
        if (phase == 3) return egg == phase3DropEgg;
        return false;
    }

    public void SetActiveEggs(int activePhase)
    {
        foreach (var egg in phase1Eggs)
            if (egg != null) egg.gameObject.SetActive(activePhase == 1);

        foreach (var egg in phase2Eggs)
            if (egg != null) egg.gameObject.SetActive(activePhase == 2);

        foreach (var egg in phase3Eggs)
            if (egg != null) egg.gameObject.SetActive(activePhase == 3);
    }

}
