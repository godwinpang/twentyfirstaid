using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class LineRendererSettings : MonoBehaviour
{
    [SerializeField] LineRenderer rend;
    Vector3[] points;

    public GameObject panel;
    public Image img;
    public Button btn;

    public XRNode handType;

    // Start is called before the first frame update
    void Start()
    {
        rend = gameObject.GetComponent<LineRenderer>();
        points = new Vector3[2];
        points[0] = Vector3.zero;
        points[1] = transform.position + new Vector3(0,0,20);
        rend.SetPositions(points);
        rend.enabled = true;
        rend.startColor = Color.red;
        rend.endColor = Color.red;

        img = panel.GetComponent<Image>();
    }

    bool AlignLineRenderer(LineRenderer rend)
    {
        Ray ray;
        ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool didHit = false;

        if (Physics.Raycast(ray, out hit))
        {
            btn = hit.collider.gameObject.GetComponent<Button>();
            didHit = true;

            rend.startColor = Color.green;
            rend.endColor = Color.green;
        }
        else
        {
            rend.startColor = Color.red;
            rend.endColor = Color.red;
        }

        rend.SetPositions(points);
        rend.material.color = rend.startColor;
        return didHit;
    }

    // Update is called once per frame
    void Update()
    {
        bool gripDown = false;
        InputDevice hand = InputDevices.GetDeviceAtXRNode(handType);
        hand.TryGetFeatureValue(CommonUsages.triggerButton, out gripDown);

        Debug.Log(gripDown);
        if (AlignLineRenderer(rend) && gripDown) {
            ChangeColor();
        }
    }

    public void ChangeColor()
    {
        Debug.Log("hi");
        if (btn != null) 
        {
            if (img.color == Color.red)
            {
                img.color = Color.green;
            } 
            else 
            {
                img.color = Color.red;
            }
        }
    }
}
