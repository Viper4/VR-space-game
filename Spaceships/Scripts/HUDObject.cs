using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDObject : MonoBehaviour
{
    int ID;
    Radar parentRadar;

    [SerializeField] float borderSize = 100;
    [SerializeField] RectTransform canvasRectangle;
    [SerializeField] RectTransform leftBorder;
    [SerializeField] RectTransform rightBorder;
    [SerializeField] RectTransform topBorder;
    [SerializeField] RectTransform bottomBorder;

    [SerializeField] TextMeshProUGUI textUI;

    [SerializeField] float killTime = 0.5f;
    float killTimer;

    void Update()
    {
        killTimer -= Time.deltaTime;
        if (killTimer <= 0)
        {
            parentRadar.RemoveHUDObject(ID);
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        leftBorder.sizeDelta = new Vector2(borderSize, leftBorder.sizeDelta.y);
        rightBorder.sizeDelta = new Vector2(borderSize, rightBorder.sizeDelta.y);
        topBorder.sizeDelta = new Vector2(topBorder.sizeDelta.x, borderSize);
        bottomBorder.sizeDelta = new Vector2(bottomBorder.sizeDelta.x, borderSize);
    }

    public void Init(Radar radar, Vector3 position, Bounds bounds, int ID, string text = "")
    {
        parentRadar = radar;
        this.ID = ID;
        UpdateObject(position, bounds, text);
    }

    public void SetColor(Color color)
    {
        leftBorder.GetComponent<Image>().color = color;
        rightBorder.GetComponent<Image>().color = color;
        topBorder.GetComponent<Image>().color = color;
        bottomBorder.GetComponent<Image>().color = color;
    }

    public void UpdateObject(Vector3 position, Bounds bounds, string text = "")
    {
        killTimer = killTime;
        transform.SetPositionAndRotation(position, Quaternion.LookRotation(transform.position - FlatCamera.instance.transform.position, FlatCamera.instance.transform.up));
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, FlatCamera.instance.transform.eulerAngles.z);

        // left, top, right, and bottom are Panels whose pivots are set as follows:
        // top: (1, 0)
        // right: (0, 0)
        // bottom: (0, 1)
        // left: (1, 1)

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] worldCorners = new Vector3[] {
            min,
            max,
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
        };

        Vector2 firstScreenPosition = FlatCamera.instance.WorldToScreenPoint(worldCorners[0]);
        float maxX = firstScreenPosition.x;
        float minX = firstScreenPosition.x;
        float maxY = firstScreenPosition.y;
        float minY = firstScreenPosition.y;
        for (int i = 1; i < 8; i++)
        {
            Vector2 screenPosition = FlatCamera.instance.WorldToScreenPoint(worldCorners[i]);
            if (screenPosition.x > maxX)
                maxX = screenPosition.x;
            if (screenPosition.x < minX)
                minX = screenPosition.x;
            if (screenPosition.y > maxY)
                maxY = screenPosition.y;
            if (screenPosition.y < minY)
                minY = screenPosition.y;
        }

        if(RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRectangle, new Vector2(maxX, maxY), FlatCamera.instance, out Vector3 topRight) && RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRectangle, new Vector2(minX, minY), FlatCamera.instance, out Vector3 bottomLeft))
        {
            leftBorder.gameObject.SetActive(true);
            rightBorder.gameObject.SetActive(true);
            topBorder.gameObject.SetActive(true);
            bottomBorder.gameObject.SetActive(true);

            topBorder.position = topRight;
            bottomBorder.position = bottomLeft;
            Vector3 localTopRight = topBorder.localPosition;
            Vector3 localBottomLeft = bottomBorder.localPosition;
            Vector3 localTopLeft = new Vector2(localBottomLeft.x, localTopRight.y);
            Vector3 localBottomRight = new Vector2(localTopRight.x, localBottomLeft.y);
            leftBorder.localPosition = localTopLeft;
            rightBorder.localPosition = localBottomRight;

            float width = Mathf.Abs(localTopRight.x - localTopLeft.x);
            float height = Mathf.Abs(localTopRight.y - localBottomRight.y);
            leftBorder.sizeDelta = new Vector2(borderSize, height + borderSize);
            rightBorder.sizeDelta = new Vector2(borderSize, height + borderSize);
            topBorder.sizeDelta = new Vector2(width + borderSize, borderSize);
            bottomBorder.sizeDelta = new Vector2(width + borderSize, borderSize);

            if (textUI != null)
            {
                textUI.gameObject.SetActive(true);
                textUI.text = text;
                textUI.color = leftBorder.GetComponent<Image>().color;
                textUI.transform.position = topRight;
                textUI.transform.localPosition = new Vector3(textUI.transform.localPosition.x + borderSize, textUI.transform.localPosition.y, textUI.transform.localPosition.z);
            }
        }
        else
        {
            leftBorder.gameObject.SetActive(false);
            rightBorder.gameObject.SetActive(false);
            topBorder.gameObject.SetActive(false);
            bottomBorder.gameObject.SetActive(false);
            if (textUI != null)
                textUI.gameObject.SetActive(false);
        }
    }
}
