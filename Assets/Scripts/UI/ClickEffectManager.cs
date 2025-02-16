using UnityEngine;

public class ClickEffectManager : MonoBehaviour
{
    public ParticleSystem clickEffect;

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            clickEffect.gameObject.SetActive(false);
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 clickPosition = GetMouseWorldPosition();
                ShowClickEffect(clickPosition);
            }
        }

    }

    void ShowClickEffect(Vector3 position)
    {
        if (clickEffect == null)
        {
            Debug.LogError("Click Effect is not assigned!");
            return;
        }

        if (!clickEffect.gameObject.activeInHierarchy)
        {
            clickEffect.gameObject.SetActive(true);
        }

        clickEffect.transform.position = position;
        clickEffect.Stop();
        clickEffect.Play();
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane + 1f; // Ensure it's not at the camera's position
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}