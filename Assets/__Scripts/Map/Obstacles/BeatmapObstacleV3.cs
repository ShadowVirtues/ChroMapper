using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Serialization;

public class BeatmapObstacleV3 : BeatmapObstacle
{
    public const int ValueUnknownBarrier = -1;
    //private float b;
    //private int x;
    private int lineLayer;
    // private float d;
    // private int w;
    private int height = 5;

    //public float Time { get => base.Time; set => base.Time = value; }
    //public int LineIndex { get => base.LineIndex; set => base.LineIndex = value; }
    public int LineLayer
    {
        get
        {
            return Type switch
            {
                ValueFullBarrier => 0,
                ValueHighBarrier => 2,
                _ => lineLayer,
            };
        }
        set
        {
            lineLayer = value;
            CheckTypeCompatibility();
        }
    }
    //public float Duration { get => base.Duration; set => base.Duration = value; }
    //public int Width { get => base.Width; set => base.Width = value; }
    public int Height { get {
            return Type switch
            {
                ValueFullBarrier => 5,
                ValueHighBarrier => 3,
                _ => height,
            };
        }
        set {
            height = value;
            CheckTypeCompatibility();
        } 
    }

    internal static readonly float[] startHeightMap = { 0.0f, 0.6f, 1.5f }; // may not be the correct height
    internal static readonly float[,] heightMap = new float[3, 5] 
    {
        { 0.9f, 1.8f, 2.7f, 3.6f, 4.2f},
        { 0.9f, 1.8f, 2.7f, 3.6f, 3.6f},
        { 0.9f, 1.8f, 2.7f, 2.7f, 2.7f},
    };

    public BeatmapObstacleV3(JSONNode node)
    {
        Time = RetrieveRequiredNode(node, "b").AsFloat;
        LineIndex = RetrieveRequiredNode(node, "x").AsInt;
        LineLayer = RetrieveRequiredNode(node, "y").AsInt;
        Duration = RetrieveRequiredNode(node, "d").AsFloat;
        Width = RetrieveRequiredNode(node, "w").AsInt;
        Height = RetrieveRequiredNode(node, "h").AsInt;
        CustomData = node[BeatmapObjectV3CustomDataKey];
    }

    public BeatmapObstacleV3(BeatmapObstacle o)
    {
        Time = o.Time;
        LineIndex = o.LineIndex;
        Duration = o.Duration;
        Width = o.Width;
        CustomData = o.CustomData;
        Type = o.Type;
        if (o is BeatmapObstacleV3)
        {
            var ov3 = o as BeatmapObstacleV3;
            LineLayer = ov3.LineLayer;
            Height = ov3.Height;
        }
    }


    public override JSONNode ConvertToJson()
    {
        if (!Settings.Instance.Load_MapV3) return base.ConvertToJson();
        JSONNode node = new JSONObject();
        node["b"] = Math.Round(Time, DecimalPrecision);
        node["x"] = LineIndex;
        node["y"] = LineLayer;
        node["d"] = Math.Round(Duration, DecimalPrecision); //Get rid of float precision errors
        node["w"] = Width;
        node["h"] = Height;
        if (CustomData != null) node[BeatmapObjectV3CustomDataKey] = CustomData;
        return node;
    }

    public override void Apply(BeatmapObject originalData)
    {
        base.Apply(originalData);

        if (originalData is BeatmapObstacleV3 obs)
        {
            Time = obs.Time;
            LineIndex = obs.LineIndex;
            LineLayer = obs.LineLayer;
            Duration = obs.Duration;
            Width = obs.Width;
            Height = obs.Height;
        }
    }

    private void CheckTypeCompatibility()
    {
        if (lineLayer == 2 && height == 3) Type = ValueHighBarrier;
        else if (lineLayer == 0 && height == 5) Type = ValueFullBarrier;
        else if (Type <= ValueHighBarrier) Type = ValueUnknownBarrier;
    }

    public void GetHeights(ref float height, ref float startHeight)
    {
        if (Type == ValueUnknownBarrier || Type == ValueFullBarrier || Type == ValueHighBarrier)
        {
            startHeight = startHeightMap[Mathf.Clamp(LineLayer, 0, 2)];
            height = heightMap[Mathf.Clamp(LineLayer, 0, 2), Mathf.Clamp(Height, 1, 5) - 1];
        }
    }
}
