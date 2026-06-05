using UnityEngine.Assertions;
using UnityEngine;

public class BreadThrower : MonoBehaviour {
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject breadPrefab;
    [SerializeField] private Transform parent;
    private Rigidbody2D _breadRigidbody;
    private GameObject _breadInstance;


    private void Awake() {
        Assert.IsNotNull(spawnPoint);
        Assert.IsNotNull(breadPrefab);
        Assert.IsNotNull(parent);
    }

    public void SpawnBread() {
        // spawn bread in hand to move up with hand when swinging
        _breadInstance = Instantiate(breadPrefab, spawnPoint);
    }

    public void ThrowBread() {
        // replace bread in hand with new bread outside Haensel container
        Destroy(_breadInstance);
        _breadInstance = Instantiate(breadPrefab, spawnPoint.position, spawnPoint.rotation, parent);
        _breadRigidbody = _breadInstance.GetComponent<Rigidbody2D>();
        _breadRigidbody.gravityScale = 1;
        _breadRigidbody.AddForce(new(3, 2), ForceMode2D.Impulse);
    }
}
