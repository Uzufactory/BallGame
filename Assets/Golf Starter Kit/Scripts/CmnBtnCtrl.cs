using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.SceneManagement.SceneManager;

public class CmnBtnCtrl : MonoBehaviour
{
    [SerializeField]
    private Button btn_ReStart;

    [SerializeField]
    private Button btn_Back;

    private void Start()
    {
        btn_ReStart?.onClick.AddListener(() => LoadScene(GetActiveScene().name));
        btn_Back?.onClick.AddListener(() => LoadScene("Splash"));
    }
}
