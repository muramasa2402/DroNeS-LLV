using UnityEngine;
using System.Collections.Generic;

public class EventLog : MonoBehaviour
{

    private List<string> _log = new List<string>();
    private string _guiText = "";
    private Font font;
    private float margin;
    public int maxLines = 40;
    public int fontSize = 24;

    private void Awake()
    {
        font = Resources.Load("Fonts/Courier New") as Font;
        margin = Screen.height * 0.01f;
    }

    void OnGUI()
    {
        GUIStyle logStyle = new GUIStyle(GUI.skin.textArea)
        {
            font = font,
            fontSize = fontSize
        };
        float height = logStyle.lineHeight * 1.25f;

        Rect r = new Rect(margin, Screen.height - height - margin, Screen.width / 3, height);
        GUI.Label(r, _guiText, logStyle);
    }

    public void AddEvent(string eventString)
    {
        _log.Insert(0,eventString);

        if (_log.Count >= maxLines) { _log.RemoveAt(_log.Count - 1); }

        _guiText = "";

        foreach (var logEvent in _log)
        {
            _guiText += logEvent + "\n";
        }
    }
}
