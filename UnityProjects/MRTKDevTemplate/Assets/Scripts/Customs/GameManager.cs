using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private RAGChatManager chatManager;
    private float msgTotalVertSize;

    private void Start()
    {
        chatManager = GetComponent<RAGChatManager>();
        msgTotalVertSize = 10f; // Initial tolerance panel size
    }

    #region PANELS
    public List<GameObject> menuPanels;

    public void OnChangePanel(GameObject clickedPanel)
    {
        // Check if the clicked panel is valid and exists in the list
        if (clickedPanel != null && menuPanels.Contains(clickedPanel))
        {
            // Deactivate all panels
            foreach (var panel in menuPanels)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }

            // Activate the clicked panel
            clickedPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Clicked panel is not valid or not found in the menuPanels list.");
        }
    }
    #endregion


    #region AI ASSISTANCE

    [SerializeField] private MRTKTMPInputField MRTKInputField;
    [SerializeField] private TMP_Text userInputField;

    [SerializeField] private GameObject msgUserBubble;
    [SerializeField] private GameObject msgAIBubble;
    [SerializeField] private GameObject msgTextConversation;

    private string SendBubbleMessage(string query, int indexMsg, GameObject BubbleMsg)
    {
        // Instantiate a msg bubble and fills its text value
        GameObject newMsgBubble = Instantiate(BubbleMsg, msgTextConversation.transform);
        newMsgBubble.transform.SetAsFirstSibling();

        // Assign proper variables
        TMP_Text bubbleText = newMsgBubble.GetComponentInChildren<TMP_Text>();
        RectTransform messageRT = newMsgBubble.transform.GetChild(indexMsg).GetComponent<RectTransform>();
        RectTransform contentRT = msgTextConversation.GetComponent<RectTransform>();

        if (bubbleText != null)
        {

            bubbleText.text = query;
            bubbleText.ForceMeshUpdate(true);

            // Updata the layout system
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
            LayoutRebuilder.ForceRebuildLayoutImmediate(newMsgBubble.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleText.gameObject.GetComponent<RectTransform>());

            // Resize the msg bubble
            messageRT.sizeDelta = new Vector2(messageRT.sizeDelta.x, bubbleText.gameObject.GetComponent<RectTransform>().sizeDelta.y);
            Debug.Log(bubbleText.gameObject.GetComponent<RectTransform>().sizeDelta.y + " and " + bubbleText.gameObject.name);
            msgTotalVertSize += messageRT.sizeDelta.y; 

            // If the msg size exceeds the panel, resize the panel
            if (contentRT.sizeDelta.y <= msgTotalVertSize)
            {
                contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, contentRT.sizeDelta.y + messageRT.sizeDelta.y);
            }
            
            // Updata the whole layout system again
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

            // Clear the input text
            MRTKInputField.text = string.Empty;
            userInputField.text = string.Empty;

            Debug.Log(query);
            return query;
            
        }
        else
        {
            Debug.Log("No TMP_Text component found in the msgTextBubble prefab.");
            return null;
        }
    }

    [SerializeField] private PressableButton unRecord;

    public async void OnButtonSendPress()
    {
        // Check if input empty
        if (userInputField != null && !string.IsNullOrEmpty(userInputField.text))
        {
            unRecord.OnClicked.Invoke();
            string query = SendBubbleMessage(userInputField.text, 1, msgUserBubble);
            string response = await chatManager.SendQuery(query);
            string holder = SendBubbleMessage(response, 0, msgAIBubble);
            holder = string.Empty;
        }
        else
        {
            Debug.Log("Input text is empty or null.");
        }
    }

    public async void OnFrequentInquiriesPress(string inquiries)
    {
        // Check if input empty
        if (inquiries != null && !string.IsNullOrEmpty(inquiries))
        {
            string query = SendBubbleMessage(inquiries, 1, msgUserBubble);
            string response = await chatManager.SendQuery(query);
            string holder = SendBubbleMessage(response, 0, msgAIBubble);
            holder = string.Empty;
        }
        else
        {
            Debug.Log("Input text is empty or null.");
        }
    }
    #endregion
}
