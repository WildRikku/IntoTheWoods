using System;
using UnityEngine;

public class ParticleSystemEndedNotifier : MonoBehaviour {
    public event Action ParticleSystemDone;

    private void OnParticleSystemStopped() {
        ParticleSystemDone?.Invoke();
    }
}
