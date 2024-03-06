using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVfxManager : MonoBehaviour
{
    [System.Serializable]
    public struct VfxEntry
    {
        public string name;
        public GameObject prefab;
    }
    public VfxEntry[] _effects;
    private Dictionary<string, GameObject> _vfxPrefabs = new Dictionary<string, GameObject>();
    private Transform _playerRender;
    private Dictionary<string, ParticleSystem> _localEffects = new Dictionary<string, ParticleSystem>();

    void Awake()
    {
        _playerRender = transform.parent.GetChild(0);

        foreach(VfxEntry e in _effects)
        {
            _vfxPrefabs[e.name] = e.prefab;
        }
    }

    public void SpawnEffect(string effectName, bool inGlobalSpace = false, float yOffset = 0)
    {
        Vector3 globalPos = new Vector3(_playerRender.position.x + _vfxPrefabs[effectName].transform.position.x, _playerRender.position.y + _vfxPrefabs[effectName].transform.position.y + yOffset, _playerRender.position.z + _vfxPrefabs[effectName].transform.position.z);
        try
        {
            if(inGlobalSpace) Instantiate(_vfxPrefabs[effectName], globalPos, _vfxPrefabs[effectName].transform.rotation);
            else Instantiate(_vfxPrefabs[effectName], transform);
        }
        catch
        {
            Debug.LogError("Failed to spawn effect \"" + effectName + "\". Double-check that string is correct.");
        }
    }

    public void PlayEffect(string effectName)
    {
        if(!_localEffects.ContainsKey(effectName))
        {
            try
            {
                _localEffects.Add(effectName, transform.Find(effectName).GetComponent<ParticleSystem>());
            }
            catch
            {
                Debug.LogError("No particle system with name \"" + effectName + "\" as child of player VFX object.");
                return;
            }
        }

        if(!_localEffects[effectName].isPlaying) _localEffects[effectName].Play();
    }

    public void StopEffect(string effectName)
    {
        if(_localEffects.ContainsKey(effectName)) _localEffects[effectName].Stop();
    }
}
