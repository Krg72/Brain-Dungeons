using UnityEngine;
using UnityEngine.UI;

public class ToggleItemUI : MonoBehaviour
{
    Toggle tgl;

    public Image backGroundimage;
    public GameObject tglOffHandle;

    void Start()
    {
        tgl = GetComponent<Toggle>();
    }

    void Update()
    {
        if(tgl.isOn)
        {
            backGroundimage.color = Color.green;
            tglOffHandle.SetActive(false);
        }
        else
        {
            backGroundimage.color = Color.white;
            tglOffHandle.SetActive(true);
        }
    }
}
