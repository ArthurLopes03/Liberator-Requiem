using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeAnimations : MonoBehaviour
{
    public Texture[] animatedTextures;
    public int currentAnimation;

    private Material _currentMaterial;
    private bool _hasCorrectMaterial;
    private bool _hasTextures;
    void Start()
    {
        //get the material of the current gameobject
        _currentMaterial = gameObject.GetComponent<Renderer>().material;
        
        //flag set up so the code does give errors on every frame when gameobject is setup incorrectly for easier debugging
        _hasCorrectMaterial = false;
        _hasTextures = false;
        
        //check if the material is using the billboarding shader
        if (_currentMaterial.shader.name == "Unlit/BillboardingShader")
        {
            _hasCorrectMaterial = true;
        }
        else
        {
            Debug.LogError("Could not find material with shader 'Unlit/BillboardingShader'", gameObject.transform);
        }

        //check if the animated textures array isn't empty
        if (animatedTextures.Length > 0)
        {
            _hasTextures = true;
        }
        else
        {
            Debug.LogError("Textures missing in 'Animated Textures' array", gameObject.transform);
        }

        //force the current texture to be the first one
        currentAnimation = 0;
    }
    void Update()
    {
        transform.eulerAngles = new Vector3(0,0,0);
        //check for error flags
        if (_hasCorrectMaterial && _hasTextures)
        {
            int numberOfTextures = animatedTextures.Length;
            
            //modulus the current animation int when loading a texture just to make sure it won't give an out of bounds error 
            _currentMaterial.SetTexture("_MainTex", animatedTextures[currentAnimation % numberOfTextures]);
        }
    }
}
