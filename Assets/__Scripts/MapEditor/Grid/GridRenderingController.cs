﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class GridRenderingController : MonoBehaviour
{
    [SerializeField] private AudioTimeSyncController atsc;
    [SerializeField] private Renderer[] oneBeat;
    [SerializeField] private Renderer[] smallBeatSegment;
    [SerializeField] private Renderer[] detailedBeatSegment;
    [SerializeField] private Renderer[] preciseBeatSegment;
    [SerializeField] private Renderer[] gridsToDisableForHighContrast;

    public readonly List<Renderer> AllRenderers = new List<Renderer>();

    private static readonly int Offset = Shader.PropertyToID("_Offset");
    private static readonly int GridSpacing = Shader.PropertyToID("_GridSpacing");
    private static readonly int MainAlpha = Shader.PropertyToID("_BaseAlpha");
    private static readonly float MainAlphaDefault = 0.1f;

    private Settings settings;

    [Inject]
    private void Construct(Settings settings)
    {
        this.settings = settings;
    }

    private void Awake()
    {
        atsc.GridMeasureSnappingChanged += GridMeasureSnappingChanged;
        AllRenderers.AddRange(oneBeat);
        AllRenderers.AddRange(smallBeatSegment);
        AllRenderers.AddRange(detailedBeatSegment);
        AllRenderers.AddRange(preciseBeatSegment);
        Settings.NotifyBySettingName(nameof(Settings.HighContrastGrids), UpdateHighContrastGrids);
    }

    public void UpdateOffset(float offset)
    {
        foreach (Renderer g in AllRenderers)
        {
            g.material.SetFloat(Offset, offset);
        }
        if (!atsc.IsPlaying)
        {
            GridMeasureSnappingChanged(atsc.gridMeasureSnapping);
        }
    }

    private void GridMeasureSnappingChanged(int snapping)
    {
        float gridSeparation = GetLowestDenominator(snapping);
        if (gridSeparation < 3) gridSeparation = 4;
        
        foreach (Renderer g in oneBeat)
        {
            g.enabled = true;
            g.material.SetFloat(GridSpacing, EditorScaleController.EditorScale / 4f);
        }

        foreach (Renderer g in smallBeatSegment)
        {
            g.enabled = true;
            g.material.SetFloat(GridSpacing, EditorScaleController.EditorScale / 4f / gridSeparation);
        }

        bool useDetailedSegments = gridSeparation < snapping;
        gridSeparation *= GetLowestDenominator(Mathf.FloorToInt(snapping / gridSeparation));
        foreach (Renderer g in detailedBeatSegment)
        {
            g.enabled = useDetailedSegments;
            g.material.SetFloat(GridSpacing, EditorScaleController.EditorScale / 4f / gridSeparation);
        }

        bool usePreciseSegments = gridSeparation < snapping;
        gridSeparation *= GetLowestDenominator(Mathf.FloorToInt(snapping / gridSeparation));
        foreach (Renderer g in preciseBeatSegment)
        {
            g.enabled = usePreciseSegments;
            g.material.SetFloat(GridSpacing, EditorScaleController.EditorScale / 4f / gridSeparation);
        }

        UpdateHighContrastGrids();
    }

    private void UpdateHighContrastGrids(object _ = null)
    {
        var alpha = settings.HighContrastGrids ? 0 : MainAlphaDefault;

        foreach (Renderer g in gridsToDisableForHighContrast)
        {
            g.material.SetFloat(MainAlpha, alpha);
        }
    }

    private int GetLowestDenominator(int a)
    {
        if (a <= 1) return 2;

        IEnumerable<int> factors = PrimeFactors(a);

        if (factors.Any())
        {
            return factors.Max();
        }
        return a;
    }

    public static List<int> PrimeFactors(int a)
    {
        List<int> retval = new List<int>();
        for (int b = 2; a > 1; b++)
        {
            while (a % b == 0)
            {
                a /= b;
                retval.Add(b);
            }
        }
        return retval;
    }

    private void OnDestroy()
    {
        atsc.GridMeasureSnappingChanged -= GridMeasureSnappingChanged;
        Settings.ClearSettingNotifications(nameof(Settings.HighContrastGrids));
    }
}
