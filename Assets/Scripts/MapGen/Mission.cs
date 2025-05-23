using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    public string missionTitle;
    public string missionDescription;

    public string missionTag;

    public int pointsGainedFromMission;

    public bool afterFire;
    public Sprite markerSprite;
    public GameObject missionObj;
    public Image missionIconImage;
    public TMP_Text missionTitleText;
    public TMP_Text missionDescriptionText;
}
