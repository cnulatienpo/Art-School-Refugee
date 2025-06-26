using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;
using UnityEngine.Networking;
using UnityEngine;

/// <summary>
/// Tracks player behaviour during a Mess Hall drawing session and
/// writes the data to a JSON file when the session ends.
/// This is intended only for internal analytics.
/// </summary>
public class MessHallSessionTracker : MonoBehaviour
{
    [Serializable]
    class SessionData
    {
        // Field name key:
        // p  - promptID
        // s  - shape codes (e.g. "cb" for cube)
        // m  - modifier tags
        // y  - symmetryEnabled
        // l  - layers used
        // t  - total drawing time
        // a  - abandoned flag
        // f  - canvas flip count
        // z  - [zoom, timestamp] pairs
        // u  - undo count
        // ts - [from, to] tool switch pairs (Tools enum)
        // d  - stroke density map
        // sp - average stroke speed
        // er - eraser volume
        // fz - form zones
        // sc - shape usage clusters

        public string p = "none";
        public List<string> s = new List<string>();
        public List<string> m = new List<string>();
        public bool y;
        public List<string> l = new List<string>();

        public float t;
        public bool a;
        public int f;
        public List<float[]> z = new List<float[]>();
        public int u;
        public List<int[]> ts = new List<int[]>();

        public int[][] d;
        public float sp;
        public float er;
        public List<string> fz = new List<string>();
        public List<string> sc = new List<string>();
    }

    // Tool enumeration used in compressed logs
    enum Tools { Pencil = 0, Ink = 1, Airbrush = 2, Eraser = 3 }

    // Map long shape names to compact two-letter codes for transmission
    static readonly Dictionary<string, string> ShapeCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        {"cube", "cb"},
        {"sphere", "sp"},
        {"cylinder", "cy"},
        {"cone", "cn"},
        {"pyramid", "py"},
        {"tetrahedron", "th"},
        {"octahedron", "oh"}
    };

    const float AbandonThreshold = 0.05f; // <5% drawing considered abandoned
    const string DataFolder = "messhall_data";

    [Header("Server Upload")]
    [Tooltip("Endpoint for the session upload API")]
    public string uploadUrl = "http://localhost:5000/api/messhall/upload"; //
        Set this to your remote server URL when deployed

    readonly SessionData data = new SessionData();
    DateTime startTime;
    bool consentGiven;
    Tools? lastTool;
    float activityRatio;
    // When true the JSON payload is GZip compressed before upload.
    // Backend example (Python/Flask):
    //   data = gzip.decompress(request.data).decode('utf-8')
    const bool UseGzip = true;

    void Start()
    {
        startTime = DateTime.UtcNow;
    }

    void Update()
    {
        data.t += Time.deltaTime;
    }

    /// <summary>
    /// Update the currently active prompt ID.
    /// </summary>
    public void SetPromptID(string id)
    {
        if (!string.IsNullOrEmpty(id))
            data.p = id;
    }

    /// <summary>
    /// Toggle symmetry tracking state.
    /// </summary>
    public void SetSymmetry(bool enabled) => data.y = enabled;

    /// <summary>Call once when the session begins.</summary>
    public void BeginSession(string promptId, bool symmetry, bool insightsConsent)
    {
        data.p = string.IsNullOrEmpty(promptId) ? "none" : promptId;
        data.y = symmetry;
        consentGiven = insightsConsent;
    }

    /// <summary>Record the use of a shape asset.</summary>
    public void AddShapeAsset(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;
        string code;
        if (!ShapeCodes.TryGetValue(name, out code))
            code = name.Length > 1 ? name.Substring(0, 2).ToLower() : name;
        if (!data.s.Contains(code))
            data.s.Add(code);
    }

    /// <summary>Record a modifier tag.</summary>
    public void AddModifierTag(string tag)
    {
        if (!string.IsNullOrEmpty(tag) && !data.m.Contains(tag))
            data.m.Add(tag);
    }

    /// <summary>Record that a layer was used.</summary>
    public void AddLayerUsed(string layer)
    {
        if (!string.IsNullOrEmpty(layer) && !data.l.Contains(layer))
            data.l.Add(layer);
    }

    /// <summary>Increment the canvas flip counter.</summary>
    public void RegisterCanvasFlip() => data.f++;

    /// <summary>Record a zoom level.</summary>
    public void LogZoom(float zoom)
    {
        float ts = (float)(DateTime.UtcNow - startTime).TotalSeconds;
        data.z.Add(new float[] { zoom, ts });
    }

    /// <summary>Increment undo count.</summary>
    public void RegisterUndo() => data.u++;

    /// <summary>Record a tool switch.</summary>
    public void SwitchTool(string newTool)
    {
        if (string.IsNullOrEmpty(newTool))
            return;
        if (!Enum.TryParse(newTool, out Tools parsed))
            return;
        if (lastTool.HasValue && parsed != lastTool.Value)
            data.ts.Add(new int[] { (int)lastTool.Value, (int)parsed });
        lastTool = parsed;
    }

    /// <summary>Update drawing activity ratio (0..1).</summary>
    public void UpdateDrawingActivity(float ratio)
    {
        activityRatio = Mathf.Clamp01(ratio);
    }

    /// <summary>Store drawing insight data if the user consented.</summary>
    public void SetDrawingInsights(int[][] densityMap, float avgSpeed, float erasePct,
        List<string> zones, List<string> clusters)
    {
        if (!consentGiven)
            return;
        data.d = densityMap;
        data.sp = avgSpeed;
        data.er = erasePct;
        if (zones != null) data.fz = new List<string>(zones);
        if (clusters != null) data.sc = new List<string>(clusters);
    }

    /// <summary>Call when the session ends to write the JSON file.</summary>
    public void EndSession()
    {
        data.a = activityRatio < AbandonThreshold;

        if (!consentGiven)
        {
            data.d = null;
            data.fz.Clear();
            data.sc.Clear();
            data.sp = 0f;
            data.er = 0f;
        }

        string dir = Path.Combine(Application.persistentDataPath, DataFolder);
        Directory.CreateDirectory(dir);
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(dir, $"{timestamp}.json");
        File.WriteAllText(path, JsonUtility.ToJson(data, true));

        StartCoroutine(SendSessionToServer(data));
    }

    IEnumerator SendSessionToServer(SessionData payload)
    {
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        if (UseGzip)
        {
            using (var ms = new MemoryStream())
            {
                using (var gz = new GZipStream(ms, CompressionLevel.Optimal))
                    gz.Write(body, 0, body.Length);
                body = ms.ToArray();
            }
        }

        using (UnityWebRequest req = UnityWebRequest.Post(uploadUrl, ""))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (UseGzip)
                req.SetRequestHeader("Content-Encoding", "gzip");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log("Session upload successful: " + req.downloadHandler.text);
            else
                Debug.LogError("Session upload failed: " + req.error);
        }
    }
}
