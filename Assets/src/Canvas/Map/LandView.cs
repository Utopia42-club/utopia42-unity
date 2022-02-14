using src;
using src.Canvas;
using src.Model;
using TMPro;
using UnityEngine;

public class LandView : MonoBehaviour
{
    private Land land;

    public TextMeshProUGUI label;
    public ActionButton navigateInMap;

    void Start()
    {
        navigateInMap.AddListener(() => { GameManager.INSTANCE.NavigateInMap(land); });
    }

    public void SetLand(Land land)
    {
        this.land = land;
        label.SetText("Land " + land.id + " in  " + land.x1 + "," + land.y1 + "," + land.x2 + "," + land.y2);
    }
}