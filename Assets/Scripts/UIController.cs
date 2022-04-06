using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public Slider weaponTempSlider;
    public TMP_Text overheatedMessage;

    public Slider healthSlider;


    public GameObject deathScreen;
    public TMP_Text deathText;


    public TMP_Text killsText;
    public TMP_Text deathsText;

    void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
