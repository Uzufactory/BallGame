using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageBtnStatus : MonoBehaviour
{
    [SerializeField]
    private string sceneName;

    private void Start()
    {
        var button = this.gameObject.GetComponent<Button>();
        button?.onClick.AddListener(() => GotoScene(sceneName));
    }

    private void GotoScene(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        SceneManager.LoadScene(name);
    }
}
