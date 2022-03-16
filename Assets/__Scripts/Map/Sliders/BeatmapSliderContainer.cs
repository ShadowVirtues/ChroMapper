using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BeatmapSliderContainer : BeatmapObjectContainer
{
    private static readonly Color unassignedColor = new Color(0.1544118f, 0.1544118f, 0.1544118f);

    [SerializeField] private TracksManager manager;
    [FormerlySerializedAs("sliderData")] public BeatmapSlider SliderData;
    [SerializeField] private GameObject indicatorMu;
    [SerializeField] private GameObject indicatorTmu;

    private MeshRenderer splineRenderer;
    internal MeshRenderer SplineRenderer { get => splineRenderer; set
        {
            splineRenderer = value;
            splineRenderer.SetPropertyBlock(MaterialPropertyBlock);
        } }
    [SerializeField] private List<MeshRenderer> noteRenderer;

    private const float partition = 0.25f;
    public override BeatmapObject ObjectData { get => SliderData; set => SliderData = (BeatmapSlider)value; }

    public static BeatmapSliderContainer SpawnSlider(BeatmapSlider data, ref GameObject prefab)
    {
        var container = Instantiate(prefab).GetComponent<BeatmapSliderContainer>();
        container.SliderData = data;
        return container;
    }

    public override void Setup()
    {
        base.Setup();
        MaterialPropertyBlock.SetFloat("_Lit", 1);
        MaterialPropertyBlock.SetFloat("_TranslucentAlpha", 1);
        UpdateMaterials();
    }

    public override void UpdateGridPosition()
    {
        transform.localPosition = new Vector3(-1.5f, 0.5f, SliderData.Time * EditorScaleController.EditorScale);
        RecomputePosition();
        UpdateCollisionGroups();
    }
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;

        //MaterialPropertyBlock.SetVector(shaderScale, scale);
        //UpdateMaterials();
    }

    /// <summary>
    /// Defer Bezier Curve based on slider data. Not the official algorithm I guess?
    /// </summary>
    /// <param name="sliderData"></param>
    public void RecomputePosition(BeatmapSlider sliderData = null)
    {
        if (sliderData != null) SliderData = sliderData;
        var spline = GetComponent<SplineMesh.Spline>();
        var n1 = spline.nodes[0];
        var n2 = spline.nodes[1];
        n1.Position = new Vector3(SliderData.X, SliderData.Y, 0);
        n2.Position = new Vector3(SliderData.TailX, SliderData.TailY, (SliderData.TailTime - SliderData.Time) * EditorScaleController.EditorScale);
        var distance = SliderData.TailTime - SliderData.Time;
        var d1 = Vector3.zero;
        d1.z += distance * partition;
        d1.y += distance * SliderData.HeadControlPointLengthMultiplier;
        var rot1 = Quaternion.Euler(0, 0, BeatmapNoteContainer.Directionalize(SliderData.Direction).z - 180);
        d1 = rot1 * d1;
        n1.Direction = n1.Position + d1;
        var d2 = Vector3.zero;
        d2.z += distance * partition;
        d2.y += distance * SliderData.TailControlPointLengthMultiplier;
        var rot2 = Quaternion.Euler(0, 0, BeatmapNoteContainer.Directionalize(SliderData.TailCutDirection).z - 180);
        d2 = rot2 * d2;
        n2.Direction = n2.Position + d2;
        spline.RefreshCurves();
        ResetIndicatorsPosition();
    }

    private void ResetIndicatorsPosition()
    {
        var spline = GetComponent<SplineMesh.Spline>();
        var n1 = spline.nodes[0];
        var n2 = spline.nodes[1];
        indicatorMu.transform.localPosition = n1.Direction;
        indicatorTmu.transform.localPosition = 2 * n2.Position - n2.Direction; // symetric to n2 to make it comprehensible
    }

    public void SetColor(Color color)
    {
        MaterialPropertyBlock.SetColor(BeatmapObjectContainer.color, color);
        UpdateMaterials();
    }

    internal override void UpdateMaterials()
    {
        foreach (var renderer in noteRenderer) renderer.SetPropertyBlock(MaterialPropertyBlock);
        if (SplineRenderer != null)
            SplineRenderer.SetPropertyBlock(MaterialPropertyBlock);
        foreach (var renderer in SelectionRenderers) renderer.SetPropertyBlock(MaterialPropertyBlock);
    }

    public void SetIndicatorBlocksActive(bool visible)
    {
        foreach (var renderer in noteRenderer) renderer.gameObject.SetActive(visible);
    }

    public void ChangeMu(float modifier)
    {
        SliderData.HeadControlPointLengthMultiplier += modifier;
    }

    public void ChangeTmu(float modifier)
    {
        SliderData.TailControlPointLengthMultiplier += modifier;
    }
}
