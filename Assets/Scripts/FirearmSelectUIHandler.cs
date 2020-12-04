using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void FirearmSelectionChanged(AbstractFirearm firearm);

public class FirearmSelectUIHandler : MonoBehaviour
{
    
    private List<AbstractFirearm> firearms;
    private int selectedIndex;

    [SerializeField] private Image previewImage;

    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    private void Start()
    {

        previousButton.onClick.AddListener(OnPreviousClick);
        nextButton.onClick.AddListener(OnNextClick);

        firearms = Firearms.GetFirearms();

        //SelectFirearmFromCurrentIndex();
    }

    private void OnPreviousClick()
    {
        if (selectedIndex == 0)
        {
            selectedIndex = firearms.Count - 1;
        }
        else
        {
            selectedIndex--;
        }

        SelectFirearmFromCurrentIndex();
    }

    private void OnNextClick()
	{
        if (selectedIndex == firearms.Count-1)
		{
            selectedIndex = 0;
		}
        else
		{
            selectedIndex++;
		}

        SelectFirearmFromCurrentIndex();
    }

    private void SelectFirearmFromCurrentIndex()
	{
        GameManager.Instance.NetworkSafe_SelectLocalPlayerFirearm(firearms[selectedIndex].FirearmUniqueIdentifier);
        UpdateUI();
    }

    private void UpdateUI()
	{
        previewImage.sprite = firearms[selectedIndex].GetPreviewSprite;
	}
}
