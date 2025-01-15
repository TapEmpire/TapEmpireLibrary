using UnityEngine;

public class PrivacyPolicy : MonoBehaviour
{
    [SerializeField] GameObject PolicyPanel;

    private static bool IsPolicyAccepted => PlayerPrefs.GetInt("PrivacyPolicy", 0) == 1;

    void Start()
    {
        if (IsPolicyAccepted)
            OnPolicyAccepted();
        else
        {
            PolicyPanel.SetActive(true);
        }
    }

    public void Accept()
    {
        PlayerPrefs.SetInt("PrivacyPolicy", 1);
        PlayerPrefs.Save();
        OnPolicyAccepted();
    }

    public void VisitWebsite()
    {
        AdsManager.Instance.VisitPrivacyPolicy();
    }

    void OnPolicyAccepted()
    {
        PolicyPanel.SetActive(false);
    }
}
