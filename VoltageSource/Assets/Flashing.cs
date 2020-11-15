using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashing : MonoBehaviour
{
    private MeshRenderer _renderer;
    
    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        if (_renderer)
            _renderer.enabled = true;
        
    }


    public void StartFlashing()
    {
        if (_renderer)
        {
            _renderer.enabled = false;
            StartCoroutine(IFlashing());
        }
    }

    private IEnumerator IFlashing()
    {
        yield return new WaitForSeconds(0.25f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.25f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.25f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.25f); // 1 second
        ToggleRenderers();
        yield return new WaitForSeconds(0.2f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.2f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.2f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.2f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.2f); // 2 second
        ToggleRenderers();
        yield return new WaitForSeconds(0.2f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.2f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f); // 3 second
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.15f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.1f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.1f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.1f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.1f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.1f);
        ToggleRenderers();
        yield return new WaitForSeconds(0.1f); // 4 seconds
        ToggleRenderers();
    }
    
    private void ToggleRenderers()
    {
        _renderer.enabled = !_renderer.enabled;
    }
    
}
