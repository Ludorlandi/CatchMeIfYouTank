using UnityEngine;

/// <summary>
/// Componente che viene aggiunto automaticamente agli oggetti afferrabili
/// per tracciare se sono già stati afferrati da un player
/// </summary>
public class GrabbableObject : MonoBehaviour
{
    private bool isGrabbed = false;
    private GameObject grabbedBy = null;

    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    public GameObject GetGrabbedBy()
    {
        return grabbedBy;
    }

    public void SetGrabbed(bool grabbed, GameObject grabber)
    {
        isGrabbed = grabbed;
        grabbedBy = grabber;
    }
}