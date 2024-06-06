using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class DemoController : MonoBehaviour
{
    public SplineAnimate _pointLightAnimate;

    public GameObject _spotLight;

    private bool camAnimating = false;
    public SplineAnimate _vCamAnimate;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_pointLightAnimate.gameObject.activeSelf)
            {
                _pointLightAnimate.Pause();
                _pointLightAnimate.gameObject.SetActive(false);
            }
            else
            {
                _pointLightAnimate.gameObject.SetActive(true);
                _pointLightAnimate.Play();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (_spotLight.activeSelf)
            {
                _spotLight.SetActive(false);
            }
            else
            {
                _spotLight.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (camAnimating)
            {
                _vCamAnimate.Pause();
            }
            else
            {
                _vCamAnimate.Play();
            }
            camAnimating = !camAnimating;
        }
    }

}
