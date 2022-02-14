using src;
using src.Canvas;
using src.Model;
using TMPro;
using UnityEngine;

public class LandView : MonoBehaviour
{
    private Land land;

    public TextMeshProUGUI firstLabel;
    public TextMeshProUGUI secondLabel;
    public GameObject nftToggle;
    public ActionButton navigateInMap;

    void Start()
    {
        navigateInMap.AddListener(() => { GameManager.INSTANCE.NavigateInMap(land); });
    }

    public void SetLand(Land land)
    {
        this.land = land;
        firstLabel.SetText("Land #" + land.id);
        secondLabel.SetText("Size: " + GetLandSize(land) + " (" + land.x1 + ", " + land.y1 + ", " + land.x2 + ", " +
                            land.y2 + ")");
        nftToggle.SetActive(land.isNft);
    }

    private long GetLandSize(Land land1)
    {
        return (land1.x2 - land1.x1) * (land1.y2 - land1.y1);
    }
}