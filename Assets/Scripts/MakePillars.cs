using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MakePillars : MonoBehaviour
{
    float[] pillarHeights;
    [SerializeField] int pillarCount = 20;
    public ClickToReveal[] hiddenPillars;
    public float pillarScale = 4;
    [SerializeField] GameObject canonicalPillar;
    [SerializeField] TMP_Text costText;
    [SerializeField] Material winMaterial;
    [SerializeField] GameObject fireworks;

    static float MIN_HEIGHT = 0.01f;
    static float MAX_HEIGHT = 1f;
    
    int cost;
    public float target = 0.5f;
    void Start()
    {
        canonicalPillar.SetActive(false);
        Vector3 ft = canonicalPillar.transform.position;
        Quaternion fr = canonicalPillar.transform.rotation;
        Vector3 pillarOffset = new Vector3(1, 0, 0);

        cost = 0;
        UpdateCost();
        won = false;

        GameObject cco = GameObject.FindGameObjectWithTag("ComparisonCubeOffset");
        cco.transform.localScale = new Vector3(pillarCount, target * pillarScale, cco.transform.localScale.z);
        
        pillarHeights = new float[pillarCount + 2];
        for (int i = 0; i < pillarHeights.Length; i++)
            pillarHeights[i] = float.NaN;
        pillarHeights[0] = MIN_HEIGHT;
        pillarHeights[pillarHeights.Length - 1] = MAX_HEIGHT;
        
        hiddenPillars = new ClickToReveal[pillarCount];
        for (int i = 0; i < hiddenPillars.Length; i++)
        {
            GameObject newObject = Object.Instantiate(canonicalPillar, ft + pillarOffset * i, fr, transform);
            
            TMP_Text text = newObject.GetComponentInChildren<TMP_Text>();
            text.text = i.ToString();

            ClickToReveal hiddenPillar = newObject.GetComponentInChildren<ClickToReveal>();
            hiddenPillars[i] = hiddenPillar;
            hiddenPillar.index = i;
            
            newObject.SetActive(true);
        }
    }

    void UpdateCost() { costText.text = "Cost: " + cost; }

    int SeekCollapsed(int index, int direction)
    {
        while (pillarHeights[index].Equals(float.NaN))
        {
            index += direction;
        }
        return index;
    }

    public float GetHeight(int index)
    {
        int leftRegionBoundary = SeekCollapsed(index, -1);
        int rightRegionBoundary = SeekCollapsed(index, 1);

        float left = pillarHeights[leftRegionBoundary];
        float right = pillarHeights[rightRegionBoundary];
        if (target <= left || target >= right)
        {
            Debug.Log("Not an option");
            float verticalSpan = right - left;
            float ratio = (index - leftRegionBoundary) / (float) (rightRegionBoundary - leftRegionBoundary);
            return left + ratio * verticalSpan;
        }

        if (rightRegionBoundary - leftRegionBoundary == 2)
            return target;
        
        int spanLeft = index - leftRegionBoundary;
        int spanRight = rightRegionBoundary - index;
        
        if (spanRight > spanLeft)
        {
            Debug.Log("Select right");
            float verticalSpan = target - left;
            float ratio = (float) spanLeft / (float) (rightRegionBoundary - leftRegionBoundary - 1);
            return left + ratio * verticalSpan;
        } else
        {
            Debug.Log("Select left");
            float verticalSpan = right - target;
            float ratio = (float) (spanLeft - 1) / (float) (rightRegionBoundary - leftRegionBoundary - 1);
            return target + ratio * verticalSpan;
        }
    }
    
    public void Collapse(int index)
    {
        index += 1;

        float height = GetHeight(index);

        pillarHeights[index] = height;
        index -= 1;
        
        ClickToReveal hiddenPillar = hiddenPillars[index];
        GameObject visiblePillar = hiddenPillar.visiblePillar;
        Vector3 s = visiblePillar.transform.localScale;
        visiblePillar.transform.localScale = new Vector3(s.x, height * pillarScale, s.z);
        visiblePillar.transform.localPosition = new Vector3(0.5f, height * pillarScale / 2, 0.5f);
        hiddenPillar.Reveal();
    }

    public void Click(int index)
    {
        Collapse(index);
        if (ClickToReveal.revealPairs)
        {
            if (index > 0 && hiddenPillars[index - 1].gameObject.activeSelf)
                Collapse(index - 1);
            else if (index + 1 < hiddenPillars.Length)
                Collapse(index + 1);
        }
    }

    public void StopFireworks() { SetFireworks(false); }
    public void SetFireworks(bool enabled)
    {
        foreach(ParticleSystem p in fireworks.GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.EmissionModule em = p.emission;
            em.enabled = enabled; // Why is this not a one-liner???
        }
    }

    bool won = false;
    public void CheckWin()
    {
        if (won) return;
        for (int i = 0; i < pillarHeights.Length; i++)
        {
            if (pillarHeights[i] == target)
            {
                won = true;
                GameObject visiblePillar = hiddenPillars[i - 1].visiblePillar;
                visiblePillar.GetComponent<MeshRenderer>().material = winMaterial;

                fireworks.transform.position = visiblePillar.transform.position + Vector3.forward + Vector3.down * 2 + Vector3.left * 0.5f;
                SetFireworks(true);
                Invoke(nameof(StopFireworks), 7);
                return;
            }
        }
    }

    public void Increment()
    {
        cost++;
        UpdateCost();
        CheckWin();
    }
}
