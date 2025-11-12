using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPageSwitch : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private PageType pageType;

    [SerializeField]
    private bool isChanging = true;
    public bool isSwitchLastPage = false;

    [SerializeField]
    private ScreenManager uIManager;

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(SwitchPage);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SwitchPage);
        }
    }

    private void OnValidate()
    {
        uIManager = FindFirstObjectByType<ScreenManager>();

#if UNITY_EDITOR
        if (!AssetDatabase.Contains(this))
        {
            if (uIManager == null)
            {
                Debug.LogError("Need UiManager");
            }
        }
#endif
        if (isChanging)
        {
            if (isSwitchLastPage)
            {
                pageType = PageType.None;
            }
        }
        else
        {
            isSwitchLastPage = false;
        }

        button = GetComponent<Button>();
    }

    private void OnMouseDown()
    {
        if (button == null)
        {
            SwitchPage();
        }
    }

    private void SwitchLastPage() =>
        uIManager.SwitchLastPage();

    public void SwitchPage()
    {
        print("switchPage - " + pageType);

        if (isChanging)
        {
            if (isSwitchLastPage)
            {
                SwitchLastPage();
            }
            else
            {
                uIManager.ChangePage(pageType);
            }
        }
        else
        {
            uIManager.SetPage(pageType);
        }        
    }   
}
