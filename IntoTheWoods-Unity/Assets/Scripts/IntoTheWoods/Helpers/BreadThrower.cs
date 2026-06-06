using System;
using IntoTheWoods.Characters;
using UnityEngine.Assertions;
using UnityEngine;

public class BreadThrower : MonoBehaviour {
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject breadPrefab;
    /// <summary>
    /// this one needs another physics layer
    /// </summary>
    [SerializeField] private GameObject breadPrefabBackLane;
    [SerializeField] private Transform parent;
    [SerializeField] private Walker walker;
    private GameObject _breadInstance;

    private void Awake() {
        Assert.IsNotNull(spawnPoint);
        Assert.IsNotNull(breadPrefab);
        Assert.IsNotNull(parent);
        Assert.IsNotNull(walker);
    }

    public void SpawnBread() {
        // spawn bread in hand to move up with hand when swinging
        _breadInstance = Instantiate(breadPrefab, spawnPoint);
        if (walker.BackLane) {
            _breadInstance.transform.localScale *= 0.8f;
        }
    }

    public void ThrowBread() {
        // replace bread in hand with new bread outside Haensel container
        Destroy(_breadInstance);
        if (walker.BackLane) {
            _breadInstance = Instantiate(breadPrefabBackLane, spawnPoint.position, spawnPoint.rotation, parent);
            _breadInstance.transform.localScale *= 0.8f;
        }
        else {
            _breadInstance = Instantiate(breadPrefab, spawnPoint.position, spawnPoint.rotation, parent);
        }

        Rigidbody2D breadRigidbody = _breadInstance.GetComponent<Rigidbody2D>();
        breadRigidbody.gravityScale = 1;
        breadRigidbody.AddForce(new(3 * Math.Sign(transform.parent.transform.localScale.x), 2), ForceMode2D.Impulse);
    }
}
