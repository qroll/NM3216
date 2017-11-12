using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPageWizard : MonoBehaviour {

    public List<GameObject> pages;
    public GameObject leftArrow;
    public GameObject rightArrow;
    public Text pageNumber;

    private int maxPageIndex;
    private int currPageIndex;
    private Button left;
    private Button right;

    void Start()
    {
        maxPageIndex = pages.Count - 1;
        left = leftArrow.GetComponent<Button>();
        right = rightArrow.GetComponent<Button>();
        left.onClick.AddListener(OnPrevPage);
        right.onClick.AddListener(OnNextPage);

        Reset();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            right.onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            left.onClick.Invoke();
        }
    }

    void OnEnable()
    {
        Reset();
    }

    void Reset()
    {
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }

        currPageIndex = 0;
        pages[0].SetActive(true);
        UpdatePagination();
    }

	public void OnNextPage()
    {
        if (currPageIndex == maxPageIndex)
        {
            return;
        }

        pages[currPageIndex].SetActive(false);
        currPageIndex++;
        pages[currPageIndex].SetActive(true);

        UpdatePagination();
    }

    public void OnPrevPage()
    {
        if (currPageIndex == 0)
        {
            return;
        }

        pages[currPageIndex].SetActive(false);
        currPageIndex--;
        pages[currPageIndex].SetActive(true);

        UpdatePagination();
    }

    void UpdatePagination()
    {
        if (currPageIndex == 0)
        {
            leftArrow.SetActive(false);
            rightArrow.SetActive(true);
        }
        else if (currPageIndex == maxPageIndex)
        {
            leftArrow.SetActive(true);
            rightArrow.SetActive(false);
        }
        else
        {
            leftArrow.SetActive(true);
            rightArrow.SetActive(true);
        }

        pageNumber.text = (currPageIndex + 1) + " / " + (maxPageIndex + 1);
    }

}
