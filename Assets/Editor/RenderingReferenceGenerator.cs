using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;

public static class RenderingReferenceGenerator
{
    private const string SwatchFolder = "Assets/Resources/RenderingSwatches";
    private const string OutputFile = "rendering_reference.json";

    [MenuItem("Tools/Generate Rendering Reference")]
    public static void Generate()
    {
        if (!Directory.Exists(SwatchFolder))
        {
            UnityEngine.Debug.LogError($"Folder '{SwatchFolder}' does not exist.");
            return;
        }

        var entries = new SortedDictionary<string, string>();
        foreach (string path in Directory.GetFiles(SwatchFolder, "*.png"))
        {
            string fileName = Path.GetFileName(path);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(path);
            string displayName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                nameWithoutExt.Replace("_", " "));
            entries[displayName] = fileName;
        }

        var sb = new StringBuilder();
        sb.AppendLine("{");
        bool first = true;
        foreach (var kvp in entries)
        {
            if (!first)
                sb.AppendLine(",");
            sb.Append("  \"").Append(kvp.Key).Append("\": \"")
              .Append(kvp.Value).Append("\"");
            first = false;
        }
        sb.AppendLine();
        sb.AppendLine("}");

        string outputPath = Path.Combine(SwatchFolder, OutputFile);
        File.WriteAllText(outputPath, sb.ToString());
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log($"Rendering reference saved to {outputPath}");
    }
}
