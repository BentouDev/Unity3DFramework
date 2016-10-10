using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuildNumber : MonoBehaviour
{
    public Text VersionText;
    public string Prefix;

    void Awake()
    {
        VersionText.text = Prefix + Application.version;
    }
}
